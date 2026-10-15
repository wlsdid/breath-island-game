using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class VisualRepairAudit
    {
        public static void Run()
        {
            var log=new StringBuilder();var catalog=IslandLifeCatalog.Load();
            var controller=(AnimatorController)catalog.animator;
            foreach(var state in controller.layers[0].stateMachine.states)log.AppendLine("STATE "+state.state.name+" = "+state.state.motion?.name);
            var instance=Object.Instantiate(catalog.characterModel);
            try
            {
                foreach(var skin in instance.GetComponentsInChildren<SkinnedMeshRenderer>())log.AppendLine("SKIN "+skin.name+" bones "+string.Join(",",skin.bones.Select(b=>b.name)));
                foreach(var t in instance.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("hand")||t.name.Contains("wrist")))log.AppendLine("HAND "+t.name+" "+t.position);
            }
            finally{Object.DestroyImmediate(instance);}
            Directory.CreateDirectory("Builds/VisualRepair");File.WriteAllText("Builds/VisualRepair/audit.txt",log.ToString());
        }
    }
}
