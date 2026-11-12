using CommonGround.API.PortfolioImport;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.API.Tests.PortfolioImport;

public class PortfolioImportPlannerTests
{
    internal class TestBed
    {
        private static readonly string[] Columns =
        [
            "Parcel Number", "Parcel Address", "Parcel Acres", "Acquired On", "Acquired By",
            "Unit", "Home Type", "Bedrooms", "Bathrooms", "Accessible", "Income Limit", "Square Feet", "Year Built", "Assessed Value", "Assessed On",
            "Homeowner Name", "Homeowner Email", "Homeowner Phone", "Lease Template", "Lease Start", "Base Price", "Appraised Value at Purchase",
            "Land Use Fee", "Fees Tracked From", "Amount Owed Then",
        ];

        public DateOnly Today { get; } = new(2026, 11, 11);

        public UserId Staff { get; } = UserId.Generate();

        public RegisteredProperties Properties { get; set; } = new([], []);

        public LeasingForImport Leasing { get; set; } = new(
            [new GroundLeaseTemplateDetailView
            {
                Id = "template-1",
                TemplateName = "Lease 2005",
                TermYears = 99,
                OptionWindowDays = 90,
                LandUseFee = 25m,
                RepairReserveFee = 10m,
                CovenantTerms = "Covenant",
                Status = "Active",
                FormulaKind = "FixedPercentage",
                AnnualPercentage = 0.015m,
            }],
            []);

        /// <summary>A valid row: a new parcel and a single-family home leased to Ana Lee since 2005.</summary>
        public Dictionary<string, string> Row(string parcelNumber = "APN-1", string unit = "") => new()
        {
            ["Parcel Number"] = parcelNumber,
            ["Parcel Address"] = "12 Elm St",
            ["Parcel Acres"] = "0.25",
            ["Acquired On"] = "3/15/2004",
            ["Acquired By"] = "Donation",
            ["Unit"] = unit,
            ["Home Type"] = "Single Family",
            ["Bedrooms"] = "3",
            ["Bathrooms"] = "1.5",
            ["Accessible"] = "no",
            ["Square Feet"] = "1,200",
            ["Year Built"] = "1998",
            ["Assessed Value"] = "$180,000",
            ["Assessed On"] = "2026-01-01",
            ["Homeowner Name"] = "Ana Lee",
            ["Homeowner Email"] = "ana@example.com",
            ["Lease Template"] = "lease 2005",
            ["Lease Start"] = "2005-06-01",
            ["Base Price"] = "120000",
            ["Appraised Value at Purchase"] = "150000",
        };

        public static CsvTable Csv(params Dictionary<string, string>[] rows)
        {
            var lines = new List<string> { string.Join(",", Columns) };
            lines.AddRange(rows.Select(r => string.Join(",", Columns.Select(c => Quote(r.GetValueOrDefault(c, string.Empty))))));
            return CsvTable.Parse(string.Join("\n", lines));
        }

        public PortfolioImportPlan Plan(CsvTable table) => PortfolioImportPlanner.Plan(table, Properties, Leasing, Today, Staff);

        private static string Quote(string value) => value.Contains(',', StringComparison.Ordinal) ? $"\"{value}\"" : value;
    }

    public class ThePlanMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenTwoHomesOnOneNewParcelAndOneLease_WhenPlanned_ThenCreatesTheParcelOnceThenEachHomeAndTheLease()
        {
            var leased = _testBed.Row(unit: "A");
            var vacant = _testBed.Row(unit: "B");
            vacant["Homeowner Name"] = string.Empty;

            var plan = _testBed.Plan(TestBed.Csv(leased, vacant));

            plan.Errors.Should().BeEmpty();
            plan.Steps.Select(s => s.Command.GetType()).Should().Equal(
                typeof(RegisterLandParcel), typeof(RegisterHome), typeof(CreateGroundLease), typeof(RegisterHome));
            plan.Steps.Select(s => s.Row).Should().Equal(2, 2, 2, 3);
            (plan.ParcelsToCreate, plan.HomesToCreate, plan.LeasesToCreate).Should().Be((1, 2, 1));
        }

        [Fact]
        public void GivenAnOldLeaseWithNoTrackingColumns_WhenPlanned_ThenTracksFeesFromTheFirstOfThisMonthWithNothingOwed()
        {
            var plan = _testBed.Plan(TestBed.Csv(_testBed.Row()));

            var lease = plan.Steps.Select(s => s.Command).OfType<CreateGroundLease>().Single();
            lease.LeaseFeeTracking!.TrackedFrom.Should().Be(new DateOnly(2026, 11, 1));
            lease.LeaseFeeTracking.OpeningBalance.Should().Be(Money.Zero);
        }

        [Fact]
        public void GivenNoFeeColumns_WhenPlanned_ThenTheLeaseTakesItsTermsAndFeesFromTheTemplate()
        {
            var row = _testBed.Row();
            row["Land Use Fee"] = "$20.00";

            var plan = _testBed.Plan(TestBed.Csv(row));

            var lease = plan.Steps.Select(s => s.Command).OfType<CreateGroundLease>().Single();
            (lease.TermYears, lease.OptionWindowDays, lease.CovenantTerms).Should().Be((99, 90, "Covenant"));
            lease.LeaseFeeTerms.LandUseFee.Should().Be(Money.Create(20m));
            lease.LeaseFeeTerms.RepairReserveFee.Should().Be(Money.Create(10m));
        }

        [Fact]
        public void GivenUnreadableCells_WhenPlanned_ThenReportsEachWithItsRowAndColumnAndPlansNothing()
        {
            var good = _testBed.Row("APN-1");
            var bad = _testBed.Row("APN-2");
            bad["Bedrooms"] = "two";
            bad["Lease Start"] = "June 2005";
            bad["Home Type"] = "Castle";

            var plan = _testBed.Plan(TestBed.Csv(good, bad));

            plan.Errors.Select(e => (e.Row, e.Column)).Should().BeEquivalentTo(new (int, string?)[]
            {
                (3, "Home Type"), (3, "Bedrooms"), (3, "Lease Start"),
            });
            plan.Steps.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", 80)]
        [InlineData("60", 60)]
        [InlineData("120%", 120)]
        [InlineData("none", null)]
        public void GivenAnIncomeLimitCell_WhenPlanned_ThenTheHomeGetsThatLimit(string cell, int? percentOfAmi)
        {
            var row = _testBed.Row();
            row["Income Limit"] = cell;

            var plan = _testBed.Plan(TestBed.Csv(row));

            plan.Errors.Should().BeEmpty();
            plan.Steps.Select(s => s.Command).OfType<RegisterHome>().Single().IncomeLimitPercentOfAmi.Should().Be(percentOfAmi);
        }

        [Theory]
        [InlineData("eighty")]
        [InlineData("250")]
        public void GivenAnIncomeLimitThatIsNotAPercentOrIsOutOfRange_WhenPlanned_ThenReportsItOnTheIncomeLimitColumn(string cell)
        {
            var row = _testBed.Row();
            row["Income Limit"] = cell;

            var plan = _testBed.Plan(TestBed.Csv(row));

            var error = plan.Errors.Should().ContainSingle().Subject;
            (error.Row, error.Column).Should().Be((2, "Income Limit"));
        }

        [Fact]
        public void GivenATemplateNameTheTrustDoesNotHave_WhenPlanned_ThenSaysWhichTemplatesExist()
        {
            var row = _testBed.Row();
            row["Lease Template"] = "Lease 1999";

            var plan = _testBed.Plan(TestBed.Csv(row));

            plan.Errors.Should().ContainSingle().Which.Message.Should().Contain("Lease 2005");
        }

        [Fact]
        public void GivenAValueADomainRuleRefuses_WhenPlanned_ThenReportsTheRuleOnItsRowAndColumn()
        {
            var row = _testBed.Row();
            row["Parcel Acres"] = "0";

            var plan = _testBed.Plan(TestBed.Csv(row));

            var error = plan.Errors.Should().ContainSingle().Subject;
            (error.Row, error.Column).Should().Be((2, "Parcel Acres"));
        }

        [Fact]
        public void GivenAPhoneNumberWithoutAnAreaCode_WhenPlanned_ThenReportsItOnTheHomeownerPhoneColumn()
        {
            var row = _testBed.Row();
            row["Homeowner Phone"] = "555-0100";

            var plan = _testBed.Plan(TestBed.Csv(row));

            var error = plan.Errors.Should().ContainSingle().Subject;
            (error.Row, error.Column, error.Message).Should().Be((2, "Homeowner Phone", "Enter a valid phone number."));
        }

        [Fact]
        public void GivenThePhoneNumberInTheDownloadableTemplate_WhenPlanned_ThenAcceptsIt()
        {
            var row = _testBed.Row();
            row["Homeowner Phone"] = "(217) 555-0123";

            var plan = _testBed.Plan(TestBed.Csv(row));

            plan.Errors.Should().BeEmpty();
        }

        [Fact]
        public void GivenTheParcelHomeAndLeaseAlreadyExist_WhenPlanned_ThenCreatesNothingAndCountsThemAsExisting()
        {
            _testBed.Properties = new RegisteredProperties([new RegisteredParcel("parcel-1", "apn-1")], [new RegisteredHome("home-1", "parcel-1", null)]);
            _testBed.Leasing = _testBed.Leasing with { LeasedHomeIds = ["home-1"] };

            var plan = _testBed.Plan(TestBed.Csv(_testBed.Row("APN-1")));

            plan.Errors.Should().BeEmpty();
            plan.Steps.Should().BeEmpty();
            (plan.ParcelsExisting, plan.HomesExisting, plan.LeasesExisting).Should().Be((1, 1, 1));
        }

        [Fact]
        public void GivenAnExistingHomeWithoutALease_WhenPlanned_ThenCreatesOnlyTheLease()
        {
            _testBed.Properties = new RegisteredProperties([new RegisteredParcel("parcel-1", "APN-1")], [new RegisteredHome("home-1", "parcel-1", null)]);

            var plan = _testBed.Plan(TestBed.Csv(_testBed.Row("APN-1")));

            var lease = plan.Steps.Should().ContainSingle().Which.Command.Should().BeOfType<CreateGroundLease>().Subject;
            (lease.LandParcelId, lease.UnitId).Should().Be(("parcel-1", "home-1"));
        }

        [Fact]
        public void GivenTwoRowsForTheSameHome_WhenPlanned_ThenReportsTheSecondRow()
        {
            var plan = _testBed.Plan(TestBed.Csv(_testBed.Row(), _testBed.Row()));

            plan.Errors.Should().ContainSingle().Which.Row.Should().Be(3);
        }

        [Fact]
        public void GivenAFileWithoutARequiredColumn_WhenPlanned_ThenReportsTheColumnOnTheHeaderRow()
        {
            var plan = _testBed.Plan(CsvTable.Parse("Parcel Number\nAPN-1\n"));

            plan.Errors.Should().Contain(e => e.Row == 1 && e.Column == "Home Type");
        }
    }
}
