using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.Homeownership.Domain.Resales.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.Testing;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.Homeownership.Tests.Application.Commands;

public class RecordBuybackClosedHandlerTests
{
    public class TheHandleMethod
    {
        private static readonly DateOnly Today = new(2026, 3, 10);

        [Fact]
        public void GivenExercisedOption_WhenClosed_ThenClosesTheResaleAndEndsTheLeaseWithItsResale()
        {
            var staff = UserId.Generate();
            var resaleId = ResaleId.Generate();
            var lease = EventReplay.Replay<GroundLease>(GroundLease.Establish(
                GroundLeaseId.Generate(), "land-parcel-1234", "home-5678", Homeowner.From("Dana Whitfield", "dana@example.com"),
                new DateOnly(2020, 1, 1), 99, 60, LeaseFeeTerms.None, MortgageDebtCap.Create(90), Money.Create(200_000m), Money.Create(250_000m), "Covenants", ResalePolicyTerms.AppraisalBased(0.25m),
                HomeCommitments.None, staff));
            var resale = EventReplay.Replay<Resale>(
                Resale.RecordIntentToSell(resaleId, lease, Today, NoticeChannel.Phone, null, Today, staff));
            resale = resale.Decide(r => r.RecordAppraisal(lease, Money.Create(270_000), Today, "Hollis", null, null, Today, staff)).Aggregate;
            resale = resale.Decide(r => r.ExerciseOption(Today, null, Today, staff)).Aggregate;
            var resaleStream = new StubEventStream<Resale>(resale);
            var leaseStream = new StubEventStream<GroundLease>(lease.Evolve(new ResaleStarted(lease.Id.Value, resaleId.Value))!);

            RecordBuybackClosedHandler.Handle(
                new RecordBuybackClosed(resaleId, Today, Money.Create(205_000), null, true, null, staff),
                new BuybackClosing(resaleStream, leaseStream, Today));

            resaleStream.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<BuybackClosed>();
            leaseStream.EventsAppended.Should().HaveCount(2);
            leaseStream.EventsAppended[0].Should().BeOfType<GroundLeaseTerminated>()
                .Which.Reason.Should().Be("Bought by the trust");
            leaseStream.EventsAppended[1].Should().BeOfType<ResaleClosed>();
        }
    }
}
