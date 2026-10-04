using NUnit.Framework;
namespace BreathIsland.Tests
{
    public sealed class GatheringWorkTests
    {
        [Test] public void HitsArePerTargetAndResetDoesNotCompleteAnything()
        {
            var work=new GatheringWork();Assert.That(work.Hit("a",3),Is.False);Assert.That(work.Hit("b",2),Is.False);
            Assert.That(work.Hits("a"),Is.EqualTo(1));Assert.That(work.Hit("a",3),Is.False);Assert.That(work.Hit("a",3),Is.True);
            work.Complete("a");Assert.That(work.Hits("a"),Is.Zero);Assert.That(work.Hits("b"),Is.EqualTo(1));
            work.Reset();Assert.That(work.Hits("b"),Is.Zero);
        }
        [Test] public void ReleasingOrSwitchingTargetRestartsHold()
        {
            var work=new GatheringWork();Assert.That(work.Hold("a",.6f,1),Is.False);work.CancelHold();
            Assert.That(work.Hold("a",.6f,1),Is.False);Assert.That(work.Hold("b",.5f,1),Is.False);
            Assert.That(work.Seconds("a"),Is.Zero);Assert.That(work.Hold("b",.5f,1),Is.True);
            work.Complete("b");Assert.That(work.Seconds("b"),Is.Zero);
        }
        [Test] public void InvalidTimeCannotCompleteWork()
        {
            var work=new GatheringWork();Assert.That(work.Hold("a",float.NaN,1),Is.False);
            Assert.That(work.Hold("a",float.PositiveInfinity,1),Is.False);Assert.That(work.Hold("a",-1,1),Is.False);
            Assert.That(work.Seconds("a"),Is.Zero);
        }
    }
}
