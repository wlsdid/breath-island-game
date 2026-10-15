using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class VisualRepairTests
    {
        [Test] public void PierLimitsKeepWholeCapsuleOverPlanksAndPreserveHeight()
        {
            foreach(var p in new[]{new Vector3(-20,2,0),new Vector3(20,3,50),new Vector3(0,4,18)})
            {var q=DockMovement.Clamp(p);Assert.That(q.x,Is.InRange(-1.78f,1.78f));Assert.That(q.z,Is.InRange(11.55f,24.45f));Assert.That(q.y,Is.EqualTo(p.y));}
        }
        [Test] public void CastHasBackswingReleaseAndSettles()
        {Assert.That(FishingPose.CastSwing(.2f),Is.LessThan(-1));Assert.That(FishingPose.CastSwing(.34f),Is.GreaterThan(1));Assert.That(FishingPose.CastSwing(1),Is.Zero);}
        [Test] public void LegacyPositionsOutsidePierAreNotTeleported()
        {var p=new Vector3(4,1,18);Assert.That(DockMovement.ConstrainStep(p,Vector3.zero),Is.EqualTo(Vector3.zero));Assert.That(DockMovement.ConstrainStep(p,Vector3.left*.1f).x,Is.EqualTo(-.1f));}
        [Test] public void GroundAndHandsUseAuthoredAssets()
        {
            Assert.That(Resources.Load<Material>("World/Coastal/GrassGround").shader.name,Is.EqualTo("BreathIsland/Ground"));
            Assert.That(Resources.Load<GameObject>("World/GroundCover").GetComponentsInChildren<Renderer>().Length,Is.GreaterThan(20));
            foreach(var side in new[]{"Left","Right"})Assert.That(Resources.Load<Mesh>("World/Coastal/"+side+"FishingHand").triangles.Length,Is.GreaterThan(30));
        }
    }
}
