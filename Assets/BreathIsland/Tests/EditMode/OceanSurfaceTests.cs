using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class OceanSurfaceTests
    {
        [Test] public void WavesStayBoundedAndNormalsFollowTheirSlope()
        {
            for(int i=0;i<40;i++)
            {
                var p=new Vector3(i*.71f,0,24+i*.31f);float time=i*.17f;
                Assert.That(OceanSurface.Height(p,time),Is.InRange(OceanSurface.Level-.191f,OceanSurface.Level+.191f));
                float dx=(OceanSurface.Height(p+Vector3.right*.01f,time)-OceanSurface.Height(p-Vector3.right*.01f,time))/.02f;
                float dz=(OceanSurface.Height(p+Vector3.forward*.01f,time)-OceanSurface.Height(p-Vector3.forward*.01f,time))/.02f;
                Assert.That(Vector3.Distance(OceanSurface.Normal(p,time),new Vector3(-dx,1,-dz).normalized),Is.LessThan(.001f));
            }
        }
        [Test] public void BobberFloatsAboveWavesAndBitePullsItBelowSurface()
        {
            var p=new Vector3(0,0,33);
            for(int i=0;i<30;i++)
            {
                float time=i*.1f,surface=OceanSurface.Height(p,time);
                Assert.That(OceanSurface.FloatHeight(p,time,false,time)-surface,Is.EqualTo(.035f).Within(.0001f));
                Assert.That(OceanSurface.FloatHeight(p,time,true,time)-surface,Is.InRange(-.226f,-.124f));
            }
            Assert.That(Resources.Load<Material>("World/Coastal/Ocean").shader.name,Is.EqualTo("BreathIsland/OceanWater"));
        }
    }
}
