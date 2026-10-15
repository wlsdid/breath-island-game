using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class FishVisualTests
    {
        [Test] public void AuthoredFishHaveDistinctBodiesDarkEyesAndForwardFacingNoses()
        {
            string[] names={"BI_Fish_Glimmer","BI_Fish_Cloud","BI_Fish_Coral","BI_Fish_MoonRay","BI_Fish_Pearl","BI_Predator_Breaker"};
            foreach(var name in names)
            {
                var prefab=Resources.Load<GameObject>("World/Fish/"+name);Assert.That(prefab,Is.Not.Null);
                var swim=prefab.GetComponent<FishSwimView>();Assert.That(swim.tail,Is.Not.Null);Assert.That(swim.fin,Is.Not.Null);
                Assert.That(prefab.GetComponentsInChildren<Collider>(),Is.Empty);
                foreach(var renderer in prefab.GetComponentsInChildren<Renderer>())
                {
                    Assert.That(renderer.sharedMaterial.shader.name,Is.EqualTo("Universal Render Pipeline/Lit"));
                    var color=renderer.sharedMaterial.GetColor("_BaseColor");
                    if(renderer.name.StartsWith("Eye"))
                    {
                        Assert.That(color.maxColorComponent,Is.LessThan(.1f));
                        Assert.That(prefab.transform.InverseTransformPoint(renderer.transform.position).z,Is.GreaterThan(0),name+" nose must face +Z");
                    }
                    if(renderer.name=="Body")Assert.That(color.maxColorComponent-Mathf.Min(color.r,Mathf.Min(color.g,color.b)),Is.GreaterThan(.2f));
                }
            }
        }
    }
}
