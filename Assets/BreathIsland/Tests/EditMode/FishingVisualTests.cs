using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class FishingVisualTests
    {
        [Test] public void RodBendsAtTipWithoutMovingGripAndClampsTension()
        {
            Assert.That(FishingPose.RodPoint(0,100),Is.EqualTo(Vector3.zero));
            Assert.That(FishingPose.RodPoint(1,90).y,Is.LessThan(FishingPose.RodPoint(1,20).y));
            Assert.That(FishingPose.RodPoint(1,500),Is.EqualTo(FishingPose.RodPoint(1,100)));
        }
        [Test] public void CastAndLineKeepEndpointsAndEaseCreatesSlack()
        {
            var start=new Vector3(1,2,3);var end=new Vector3(0,-.6f,28);
            Assert.That(FishingPose.CastPoint(start,end,0),Is.EqualTo(start));
            Assert.That(FishingPose.CastPoint(start,end,1),Is.EqualTo(end));
            Assert.That(FishingPose.CastPoint(start,end,.5f).y,Is.GreaterThan(Vector3.Lerp(start,end,.5f).y));
            Assert.That(FishingPose.LinePoint(start,end,0,30),Is.EqualTo(start));
            Assert.That(FishingPose.LinePoint(start,end,1,30),Is.EqualTo(end));
            Assert.That(FishingPose.LinePoint(start,end,.5f,10).y,Is.LessThan(FishingPose.LinePoint(start,end,.5f,90).y));
        }
        [Test] public void AuthoredRigContainsNoGameplayCollidersAndAllRequiredParts()
        {
            var prefab=Resources.Load<GameObject>("World/FishingRig");Assert.That(prefab,Is.Not.Null);
            var rod=prefab.GetComponentInChildren<FishingRodView>();Assert.That(rod.shaft,Is.Not.Null);
            Assert.That(rod.guides.Length,Is.EqualTo(6));Assert.That(rod.reelHandle,Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<Collider>(),Is.Empty);
        }
    }
}
