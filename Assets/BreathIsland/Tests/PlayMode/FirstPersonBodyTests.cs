using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class FirstPersonBodyTests
    {
        [UnityTest] public IEnumerator PhaseChangesHideOnlyPlayerAndRestorePreexistingVisibility()
        {
            var root=new GameObject("Camera visibility regression");var world=ProceduralWorld.Build(root.transform);
            try
            {
                var phases=new GamePhaseController(world);
                var body=world.player.GetComponentsInChildren<Renderer>();Assert.That(body.Length,Is.GreaterThan(0));
                body[0].forceRenderingOff=true;
                var npc=world.npcs[NpcId.Nari].GetComponentsInChildren<Renderer>();
                foreach(var phase in new[]{GamePhase.Fishing,GamePhase.PredatorCombat})
                {
                    phases.Set(phase);yield return null;
                    foreach(var renderer in body)Assert.That(renderer.forceRenderingOff,Is.True);
                    foreach(var renderer in npc)Assert.That(renderer.forceRenderingOff,Is.False);
                    phases.Set(GamePhase.Explore);yield return null;
                    Assert.That(body[0].forceRenderingOff,Is.True);
                    for(int i=1;i<body.Length;i++)Assert.That(body[i].forceRenderingOff,Is.False);
                }
                phases.Set(GamePhase.Fishing);world.player.GetComponent<FirstPersonBodyView>().enabled=false;
                for(int i=1;i<body.Length;i++)Assert.That(body[i].forceRenderingOff,Is.False);
            }
            finally{UnityEngine.Object.Destroy(root);Cursor.lockState=CursorLockMode.None;}
            yield return null;
        }
    }
}
