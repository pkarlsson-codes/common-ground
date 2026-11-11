using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies;
using CommonGround.SharedKernel.Domain;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.LeaseFeeNoticePolicies;

public class LeaseFeeNoticePolicyTests
{
    public class TheSetMethod
    {
        [Fact]
        public void GivenPolicySetTwice_WhenReplayed_ThenTheLatestScheduleAndTrimmedSignOffWin()
        {
            var id = LeaseFeeNoticePolicyId.ForTrust("trust-1");
            var user = UserId.Generate();

            var sut = EventReplay.Replay<LeaseFeeNoticePolicy>(
                LeaseFeeNoticePolicy.Set(id, LeaseFeeNoticeSchedule.Default, null, user),
                LeaseFeeNoticePolicy.Set(id, LeaseFeeNoticeSchedule.Create(14, 45), "  Riverbend CLT\n(503) 555-0100  ", user));

            sut.Should().BeEquivalentTo(new
            {
                Schedule = LeaseFeeNoticeSchedule.Create(14, 45),
                LetterSignOff = "Riverbend CLT\n(503) 555-0100",
            });
        }
    }
}
