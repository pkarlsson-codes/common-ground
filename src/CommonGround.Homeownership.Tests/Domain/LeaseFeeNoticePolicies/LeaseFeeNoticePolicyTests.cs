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
        public void GivenPolicySetTwice_WhenReplayed_ThenTheLatestScheduleCurePeriodAndTrimmedSignOffWin()
        {
            var id = LeaseFeeNoticePolicyId.ForTrust("trust-1");
            var user = UserId.Generate();

            var sut = EventReplay.Replay<LeaseFeeNoticePolicy>(
                LeaseFeeNoticePolicy.Set(id, LeaseFeeNoticeSchedule.Default, DefaultCurePeriod.Default, null, user),
                LeaseFeeNoticePolicy.Set(id, LeaseFeeNoticeSchedule.Create(14, 45, 75), DefaultCurePeriod.Create(20, 10), "  Riverbend CLT\n(503) 555-0100  ", user));

            sut.Should().BeEquivalentTo(new
            {
                Schedule = LeaseFeeNoticeSchedule.Create(14, 45, 75),
                CurePeriod = DefaultCurePeriod.Create(20, 10),
                LetterSignOff = "Riverbend CLT\n(503) 555-0100",
            });
        }
    }
}
