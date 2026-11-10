using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
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

public class WithdrawIntentToSellHandlerTests
{
    public class TheHandleMethod
    {
        private static readonly DateOnly Today = new(2026, 3, 10);

        [Fact]
        public void GivenOpenResale_WhenWithdrawn_ThenWithdrawsItAndClosesItOnTheLease()
        {
            var resaleId = ResaleId.Generate();
            var staff = UserId.Generate();
            var lease = EventReplay.Replay<GroundLease>(GroundLease.Establish(
                GroundLeaseId.Generate(), "land-parcel-1234", "home-5678", Homeowner.From("Dana Whitfield", "dana@example.com"),
                new DateOnly(2020, 1, 1), 99, 60, LeaseFeeTerms.None, 200_000m, 250_000m, "Covenants", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None, staff));
            var resale = EventReplay.Replay<Resale>(
                Resale.RecordIntentToSell(resaleId, lease, Today, NoticeChannel.Phone, null, Today, staff));
            var resaleStream = new StubEventStream<Resale>(resale);
            var leaseStream = new StubEventStream<GroundLease>(lease.Evolve(new ResaleStarted(lease.Id.Value, resaleId.Value))!);

            WithdrawIntentToSellHandler.Handle(
                new WithdrawIntentToSell(resaleId, Today, "Staying put", staff),
                new IntentToSellWithdrawal(resaleStream, leaseStream, Today));

            resaleStream.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ResaleWithdrawn>();
            leaseStream.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ResaleClosed>();
        }
    }
}
