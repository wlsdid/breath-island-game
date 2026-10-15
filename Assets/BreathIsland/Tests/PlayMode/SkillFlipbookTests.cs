using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class SkillFlipbookTests
    {
        [UnityTest] public IEnumerator AuthoredEffectsReplayExpireAndStopWithOwner()
        {
            foreach (var kind in new[] { "warrior", "ranger", "mage" })
            {
                var prefab = Resources.Load<SkillFlipbook>("World/SkillVfx/" + kind);
                Assert.That(prefab, Is.Not.Null, kind);
                Assert.That(prefab.firstFrame + prefab.frameCount, Is.LessThanOrEqualTo(prefab.columns * prefab.rows));
                Assert.That(prefab.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.Not.Null);
                var owner = new GameObject("VFX lifecycle test");
                try
                {
                    var view = owner.AddComponent<WeaponSkillView>(); view.Play(kind, Vector3.up, Color.white);
                    Assert.That(view.Active, Is.True);
                    yield return new WaitForSeconds(prefab.duration + .1f);
                    Assert.That(view.Active, Is.False);
                    view.Play(kind, Vector3.zero, Color.white); Assert.That(view.Active, Is.True);
                    owner.SetActive(false); Assert.That(view.Active, Is.False);
                }
                finally { Object.Destroy(owner); }
                yield return null;
            }
        }
        [Test] public void AtlasFramesAdvanceLeftToRightThenDown()
        {
            Assert.That(SkillFlipbook.FrameUv(0, 7, 6).z, Is.Zero);
            Assert.That(SkillFlipbook.FrameUv(7, 7, 6).w, Is.EqualTo(4f / 6).Within(.0001));
            Assert.That(SkillFlipbook.FrameUv(29, 6, 5).z, Is.EqualTo(5f / 6).Within(.0001));
            Assert.That(SkillFlipbook.FrameUv(29, 6, 5).w, Is.Zero.Within(.0001));
        }
    }
}
