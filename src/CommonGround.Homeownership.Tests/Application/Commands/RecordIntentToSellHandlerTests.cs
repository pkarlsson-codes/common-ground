using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.Homeownership.Domain.Resales.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;
using JasperFx.Events;

namespace CommonGround.Homeownership.Tests.Application.Commands;

public class RecordIntentToSellHandlerTests
{
    public class TestBed
    {
        public static readonly DateOnly Today = new(2026, 3, 10);

        public GroundLease Lease { get; } = EventReplay.Replay<GroundLease>(GroundLease.Establish(
            GroundLeaseId.Generate(), "land-parcel-1234", "home-5678", Homeowner.From("Dana Whitfield", "dana@example.com"),
            new DateOnly(2020, 1, 1), 99, 60, 200_000m, 250_000m, "Covenants", ResalePolicyTerms.AppraisalBased(0.25m), HomeCommitments.None,
            UserId.Generate()));

        public RecordIntentToSell Command() =>
            new(ResaleId.Generate(), Lease.Id, Today, NoticeChannel.Phone, null, UserId.Generate());
    }

    public class TheHandleMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenLeaseWithNoResale_WhenRecorded_ThenStartsTheResaleOnTheLeaseAndOpensIt()
        {
            var lease = new StubEventStream<GroundLease>(_testBed.Lease);
            var command = _testBed.Command();

            var started = RecordIntentToSellHandler.Handle(command, new IntentToSellStart(lease, TestBed.Today));

            lease.EventsAppended.Should().ContainSingle().Which.Should().BeOfType<ResaleStarted>()
                .Which.ResaleId.Should().Be(command.ResaleId.Value);
            started.Events.Should().ContainSingle().Which.Should().BeOfType<IntentToSellRecorded>();
        }

        [Fact]
        public void GivenLeaseWithOpenResale_WhenRecorded_ThenThrowsBusinessRuleViolation()
        {
            var open = _testBed.Lease.Evolve(new ResaleStarted(_testBed.Lease.Id.Value, ResaleId.Generate().Value))!;
            var lease = new StubEventStream<GroundLease>(open);

            var act = () => RecordIntentToSellHandler.Handle(_testBed.Command(), new IntentToSellStart(lease, TestBed.Today));

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.ground_lease.start_resale.resale_already_open");
        }
    }
}
