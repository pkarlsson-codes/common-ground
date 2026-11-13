using CommonGround.API.Reports;
using CommonGround.Homeownership.Application.Queries;
using FluentAssertions;

namespace CommonGround.API.Tests.Reports;

public class FunderReportBuilderTests
{
    public class TestBed
    {
        public DateOnly From { get; } = new(2025, 1, 1);

        public DateOnly To { get; } = new(2025, 12, 31);

        public Dictionary<string, DateOnly> Registered { get; } = [];

        public List<FunderReportLease> Leases { get; } = [];

        public List<FunderReportBuyback> Buybacks { get; } = [];

        public static FunderReportLease Lease(
            string homeId,
            DateOnly start,
            DateOnly? endedOn = null,
            decimal basePrice = 200_000m,
            decimal? appraised = 300_000m,
            FunderReportEligibility? eligibility = null) =>
            new($"gl-{homeId}-{start:yyyyMMdd}", homeId, $"Home {homeId}", start, endedOn, basePrice, appraised, eligibility);

        public static FunderReportEligibility Band(int? over, int? upTo) => new(3, 48_000m, over, upTo);

        public FunderReportBuyback Buyback(string homeId, DateOnly closing) =>
            new($"resale-{homeId}", homeId, $"Home {homeId}", closing, OriginalPrice: 150_000m, AppraisedValue: 280_000m, PurchaseOptionPrice: 175_000m, PricePaid: 172_000m);

        public Contracts.Reports.Responses.FunderReportResponse Build() =>
            FunderReportBuilder.Build(From, To, Registered, new FunderReportRecords(Leases, Buybacks));
    }

    public class TheBuildMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHomesLeasedEndedAndUnregistered_WhenBuilt_ThenPortfolioCountsHomesOnTheBooksOnTo()
        {
            _testBed.Registered["leased"] = new(2020, 1, 1);
            _testBed.Registered["bought-back"] = new(2020, 1, 1);
            _testBed.Registered["held"] = new(2024, 6, 1);
            _testBed.Registered["registered-later"] = new(2026, 2, 1);
            _testBed.Registered["imported"] = new(2026, 3, 1);
            _testBed.Leases.Add(TestBed.Lease("leased", new(2021, 5, 1)));
            _testBed.Leases.Add(TestBed.Lease("bought-back", new(2019, 5, 1), endedOn: new(2025, 12, 31)));
            _testBed.Leases.Add(TestBed.Lease("imported", new(2012, 5, 1)));

            var report = _testBed.Build();

            report.Portfolio.Homes.Should().Be(4);
            report.Portfolio.UnderGroundLease.Should().Be(2);
            report.Portfolio.HeldOrForSale.Should().Be(2);
        }

        [Fact]
        public void GivenLeasesInsideAndOutsideThePeriod_WhenBuilt_ThenSalesAreTheLeasesStartedInItEarliestFirst()
        {
            _testBed.Leases.Add(TestBed.Lease("before", new(2024, 12, 31)));
            _testBed.Leases.Add(TestBed.Lease("late", new(2025, 12, 31)));
            _testBed.Leases.Add(TestBed.Lease("early", new(2025, 1, 1)));
            _testBed.Leases.Add(TestBed.Lease("after", new(2026, 1, 1)));

            var report = _testBed.Build();

            report.Sales.Select(s => s.HomeId).Should().Equal("early", "late");
        }

        [Fact]
        public void GivenASaleWithEligibility_WhenBuilt_ThenTheRowCarriesPriceAffordabilityAndBand()
        {
            _testBed.Leases.Add(TestBed.Lease("a", new(2025, 4, 1), basePrice: 218_000m, appraised: 300_500m, eligibility: TestBed.Band(null, 50)));

            var sale = _testBed.Build().Sales.Single();

            sale.Price.Should().Be(218_000m);
            sale.AffordabilityGiven.Should().Be(82_500m);
            sale.HouseholdSize.Should().Be(3);
            sale.AnnualIncome.Should().Be(48_000m);
            sale.IncomeBandOverPercentOfAmi.Should().BeNull();
            sale.IncomeBandUpToPercentOfAmi.Should().Be(50);
        }

        [Fact]
        public void GivenALeaseWithNoPurchaseOrAppraisal_WhenBuilt_ThenSizeIncomeBandAndAffordabilityAreNull()
        {
            _testBed.Leases.Add(TestBed.Lease("manual", new(2025, 4, 1), appraised: null));

            var report = _testBed.Build();

            var sale = report.Sales.Single();
            sale.AffordabilityGiven.Should().BeNull();
            sale.HouseholdSize.Should().BeNull();
            sale.AnnualIncome.Should().BeNull();
            report.TotalAffordabilityGiven.Should().Be(0m);
        }

        [Fact]
        public void GivenSalesInSeveralBands_WhenBuilt_ThenBandsAreCountedLowestFirstAndNotRecordedLast()
        {
            _testBed.Leases.Add(TestBed.Lease("none", new(2025, 2, 1)));
            _testBed.Leases.Add(TestBed.Lease("over-highest", new(2025, 3, 1), eligibility: TestBed.Band(120, null)));
            _testBed.Leases.Add(TestBed.Lease("mid-1", new(2025, 4, 1), eligibility: TestBed.Band(50, 80)));
            _testBed.Leases.Add(TestBed.Lease("lowest", new(2025, 5, 1), eligibility: TestBed.Band(null, 50)));
            _testBed.Leases.Add(TestBed.Lease("mid-2", new(2025, 6, 1), eligibility: TestBed.Band(50, 80)));
            _testBed.Leases.Add(TestBed.Lease("no-schedule", new(2025, 7, 1), eligibility: TestBed.Band(null, null)));

            var bands = _testBed.Build().IncomeBands;

            bands.Select(b => (b.OverPercentOfAmi, b.UpToPercentOfAmi, b.Count)).Should().Equal(
                (null, 50, 1),
                (50, 80, 2),
                (120, null, 1),
                (null, null, 2));
        }

        [Fact]
        public void GivenBuybacksInsideAndOutsideThePeriod_WhenBuilt_ThenResalesHoldThoseClosedInItWithKeptAndGain()
        {
            _testBed.Buybacks.Add(_testBed.Buyback("in", new(2025, 11, 14)));
            _testBed.Buybacks.Add(_testBed.Buyback("out", new(2026, 1, 2)));

            var report = _testBed.Build();

            var resale = report.Resales.Single();
            resale.HomeId.Should().Be("in");
            resale.AffordabilityKept.Should().Be(108_000m);
            resale.SellerGain.Should().Be(22_000m);
            report.TotalAffordabilityKept.Should().Be(108_000m);
        }

        [Fact]
        public void GivenSalesWithAppraisals_WhenBuilt_ThenTotalAffordabilityGivenAddsThemUp()
        {
            _testBed.Leases.Add(TestBed.Lease("a", new(2025, 4, 1), basePrice: 200_000m, appraised: 260_000m));
            _testBed.Leases.Add(TestBed.Lease("b", new(2025, 5, 1), basePrice: 150_000m, appraised: 190_000m));

            _testBed.Build().TotalAffordabilityGiven.Should().Be(100_000m);
        }
    }
}
