using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class VillagePolishTests
    {
        [TestCase("Nari")]
        [TestCase("Maru")]
        public void VillagersHaveAuthoredSkinsIdleRigsAndGroundedVisuals(string name)
        {
            var prefab=Resources.Load<GameObject>("World/Npcs/"+name);
            Assert.That(prefab,Is.Not.Null);
            var presentation=prefab.GetComponent<NpcPresentation>();
            Assert.That(presentation.visual,Is.Not.Null);
            Assert.That(presentation.visual.localPosition.y,Is.EqualTo(-.6f).Within(.001f));
            var animator=prefab.GetComponentInChildren<Animator>();
            Assert.That(animator.avatar,Is.Not.Null);
            Assert.That(animator.avatar.isValid,Is.True);
            Assert.That(animator.runtimeAnimatorController,Is.Not.Null);
            Assert.That(animator.applyRootMotion,Is.False);
            Assert.That(prefab.GetComponentsInChildren<Collider>().Length,Is.EqualTo(1));
            foreach(var renderer in prefab.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Assert.That(renderer.bones,Is.Not.Empty);
                foreach(var bone in renderer.bones)Assert.That(bone,Is.Not.Null);
                Assert.That(renderer.sharedMaterial.GetTexture("_BaseMap"),Is.Not.Null);
            }
        }

        [Test] public void DressingHasNoGameplayCollisionAndSafeAnchorsRemain()
        {
            var prefab=Resources.Load<GameObject>("World/ForestVillage");
            var details=prefab.transform.Find("Village Detail Pass");
            Assert.That(details,Is.Not.Null);
            Assert.That(details.GetComponentsInChildren<Collider>(),Is.Empty);
            Assert.That(details.GetComponentsInChildren<Renderer>().Length,Is.GreaterThan(35));
            var layout=prefab.GetComponent<VillageLayout>();
            CollectionAssert.Contains(layout.safePoints,new Vector3(-7,1.2f,9));
            CollectionAssert.Contains(layout.safePoints,new Vector3(8,1.2f,6));
            CollectionAssert.Contains(layout.safePoints,new Vector3(4,1.2f,5.5f));
        }
    }
}
