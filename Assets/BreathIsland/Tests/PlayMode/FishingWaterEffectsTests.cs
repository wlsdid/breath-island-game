using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class FishingWaterEffectsTests
    {
        [UnityTest] public IEnumerator EffectsFireOncePerContactAndClearWithoutTouchingRandomState()
        {
            var root=new GameObject("Effects test");var effects=root.AddComponent<FishingWaterEffects>();
            var state=Random.state;
            try
            {
                var p=new Vector3(0,OceanSurface.Level,33);effects.ResetCast();
                effects.Sample(FishingStage.Waiting,.2f,p,p,.1f);Assert.That(effects.BurstCount,Is.Zero);
                effects.Sample(FishingStage.Waiting,1,p,p,.1f);effects.Sample(FishingStage.Waiting,2,p,p,.1f);
                Assert.That(effects.BurstCount,Is.EqualTo(1));
                effects.Sample(FishingStage.Bite,2,p,p,.1f);effects.Sample(FishingStage.Bite,2,p,p,.1f);
                Assert.That(effects.BurstCount,Is.EqualTo(2));
                effects.Sample(FishingStage.Hooked,2,p,p+Vector3.up*2,.1f);
                effects.Sample(FishingStage.Hooked,2,p,p+Vector3.up*2,.1f);
                effects.Sample(FishingStage.Hooked,2,p,p-Vector3.up*2,.1f);
                Assert.That(effects.BurstCount,Is.EqualTo(4));
                effects.Sample(FishingStage.Hooked,2,p,p-Vector3.up*2,2);
                Assert.That(effects.ActiveCount,Is.Zero);Assert.That(Random.state,Is.EqualTo(state));
                yield return null;
                Assert.That(root.GetComponentsInChildren<Collider>(true),Is.Empty);
            }
            finally{Object.Destroy(root);}
        }
        [UnityTest] public IEnumerator TailMovesWithoutMovingFishRoot()
        {
            var fish=Object.Instantiate(Resources.Load<GameObject>("World/Fish/BI_Fish_Glimmer"));
            try
            {
                var swim=fish.GetComponent<FishSwimView>();var initial=swim.tail.localRotation;
                var position=fish.transform.position;var rotation=fish.transform.rotation;
                yield return new WaitForSeconds(.15f);
                Assert.That(Quaternion.Angle(initial,swim.tail.localRotation),Is.GreaterThan(1));
                Assert.That(fish.transform.position,Is.EqualTo(position));Assert.That(fish.transform.rotation,Is.EqualTo(rotation));
            }
            finally{Object.Destroy(fish);}
        }
    }
}
