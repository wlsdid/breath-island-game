using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BreathIsland.Editor
{
    // Measures the visible mesh rather than the conservative animation culling bounds.
    public static class CharacterPresentationAudit
    {
        public static void Run()
        {
            var report = new StringBuilder();
            var catalog = Resources.Load<IslandLifeCatalog>("Content/IslandLife");
            var parent = new GameObject("Presentation audit");
            var visual = ModelPresentation.Create(catalog.characterModel, parent.transform, catalog.characterMaterial, 1.7f);
            var clips = AssetDatabase.FindAssets("t:Model", new[] { "Assets/BreathIsland/Art/ThirdParty/KayKit/Character" })
                .Select(AssetDatabase.GUIDToAssetPath).SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
            report.AppendLine("Clips: " + string.Join(", ", clips.Select(c => c.name)));
            foreach (var clip in clips.Where(c => c.name.Contains("Idle") || c.name.Contains("Walk") || c.name.Contains("Jump")))
            {
                clip.SampleAnimation(visual.transform.GetChild(0).gameObject, 0);
                foreach (var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var mesh = new Mesh();
                    renderer.BakeMesh(mesh);
                    var vertices = mesh.vertices.Select(renderer.transform.TransformPoint).ToArray();
                    report.AppendLine($"{clip.name}: mesh minY={vertices.Min(v=>v.y):F4} maxY={vertices.Max(v=>v.y):F4}; culling minY={renderer.bounds.min.y:F4}");
                    Object.DestroyImmediate(mesh);
                }
            }
            Object.DestroyImmediate(parent);
            Directory.CreateDirectory("Builds/PresentationUpgrade");
            File.WriteAllText("Builds/PresentationUpgrade/character-audit.txt", report.ToString());
            Debug.Log(report.ToString());
        }
    }
}
