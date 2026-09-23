using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class HomeInteriorAssetTests
    {
        [Test] public void AuthoredRoomKeepsTheSavedEightByEightFloorAndClearEntrance()
        {
            var room = Resources.Load<GameObject>("World/HomeInterior");
            var doorway = Resources.Load<GameObject>("World/HomeDoorway");
            Assert.That(room, Is.Not.Null); Assert.That(doorway, Is.Not.Null);
            var floor = room.GetComponentInChildren<BoxCollider>();
            Assert.That(floor.center, Is.EqualTo(new Vector3(4, -.15f, 4)));
            Assert.That(floor.size, Is.EqualTo(new Vector3(8, .3f, 8)));
            Assert.That(doorway.GetComponentsInChildren<Collider>(), Is.Empty, "The existing E interaction zone must remain reachable.");
            foreach (var renderer in room.GetComponentsInChildren<Renderer>())
                foreach (var material in renderer.sharedMaterials)
                {
                    Assert.That(material, Is.Not.Null);
                    Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
                    if (renderer.name != "Subfloor") Assert.That(material.GetTexture("_BaseMap"), Is.Not.Null);
                    else Assert.That(material.GetColor("_BaseColor").a, Is.EqualTo(1), "The solid backing must cover plank gaps.");
                }
            Assert.That(room.GetComponentsInChildren<Renderer>().Length, Is.GreaterThanOrEqualTo(28));
        }
    }
}
