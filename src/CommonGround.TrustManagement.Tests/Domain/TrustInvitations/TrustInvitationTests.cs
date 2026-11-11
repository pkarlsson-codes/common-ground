using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using CommonGround.TrustManagement.Domain.TrustInvitations;
using CommonGround.TrustManagement.Domain.TrustInvitations.Events;
using FluentAssertions;
using Xunit;

namespace CommonGround.TrustManagement.Tests.Domain.TrustInvitations;

public class TrustInvitationTests
{
    public class TestBed
    {
        public TrustId TrustId { get; } = TrustId.Generate();

        public UserId Member { get; } = UserId.Generate();

        public UserId Colleague { get; } = UserId.Generate();

        public DateTimeOffset IssuedAt { get; } = new(2026, 11, 1, 15, 0, 0, TimeSpan.Zero);

        public TrustInvitationIssued Issued() =>
            TrustInvitation.Issue(TrustInvitationId.Generate(), TrustId, EmailAddress.Create("colleague@example.org"), "hash", Member, IssuedAt);

        public TrustInvitation IssuedInvitation() => EventReplay.Replay<TrustInvitation>(Issued());

        public TrustInvitation AcceptedInvitation()
        {
            var invitation = IssuedInvitation();
            return invitation.Evolve(invitation.Accept(Colleague, IssuedAt.AddHours(1)));
        }

        public TrustInvitation CancelledInvitation()
        {
            var invitation = IssuedInvitation();
            return invitation.Evolve(invitation.Cancel(TrustId, Member)!);
        }
    }

    public class TheIssueMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAnInvitation_WhenIssued_ThenExpiresSevenDaysLater()
        {
            var issued = _testBed.Issued();

            issued.ExpiresAt.Should().Be(_testBed.IssuedAt.AddDays(7));
        }
    }

    public class TheAcceptMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAnUnusedInvitation_WhenAcceptedBeforeExpiry_ThenTheUserJoinsItsTrust()
        {
            var sut = _testBed.IssuedInvitation();

            var accepted = sut.Accept(_testBed.Colleague, _testBed.IssuedAt.AddDays(7).AddTicks(-1));

            accepted.Should().BeEquivalentTo(new { TrustId = _testBed.TrustId.Value, UserId = _testBed.Colleague.Value });
        }

        [Fact]
        public void GivenAnExpiredInvitation_WhenAccepted_ThenThrowsNoLongerValid()
        {
            var sut = _testBed.IssuedInvitation();

            var act = () => sut.Accept(_testBed.Colleague, _testBed.IssuedAt.AddDays(7));

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("trust_management.trust_invitation.accept.no_longer_valid");
        }

        [Fact]
        public void GivenAnAcceptedInvitation_WhenAcceptedAgain_ThenThrowsNoLongerValid()
        {
            var sut = _testBed.AcceptedInvitation();

            var act = () => sut.Accept(UserId.Generate(), _testBed.IssuedAt.AddHours(2));

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("trust_management.trust_invitation.accept.no_longer_valid");
        }

        [Fact]
        public void GivenACancelledInvitation_WhenAccepted_ThenThrowsNoLongerValid()
        {
            var sut = _testBed.CancelledInvitation();

            var act = () => sut.Accept(_testBed.Colleague, _testBed.IssuedAt.AddHours(2));

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("trust_management.trust_invitation.accept.no_longer_valid");
        }
    }

    public class TheCancelMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAnotherTrust_WhenCancelled_ThenThrowsNotFound()
        {
            var sut = _testBed.IssuedInvitation();

            var act = () => sut.Cancel(TrustId.Generate(), _testBed.Member);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("trust_management.trust_invitation.not_found");
        }

        [Fact]
        public void GivenAnAcceptedInvitation_WhenCancelled_ThenThrowsAlreadyAccepted()
        {
            var sut = _testBed.AcceptedInvitation();

            var act = () => sut.Cancel(_testBed.TrustId, _testBed.Member);

            act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("trust_management.trust_invitation.cancel.already_accepted");
        }

        [Fact]
        public void GivenACancelledInvitation_WhenCancelledAgain_ThenDecidesNothing()
        {
            var sut = _testBed.CancelledInvitation();

            sut.Cancel(_testBed.TrustId, _testBed.Member).Should().BeNull();
        }
    }
}
