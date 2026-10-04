using System.IO;
using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class FinishMeshAuthoring
    {
        public static void Build()
        {
            const string directory = "Assets/BreathIsland/Resources/World/FinishMeshes/";
            const string prefab = "Assets/BreathIsland/Resources/World/HomeInterior.prefab";
            Directory.CreateDirectory(directory); AssetDatabase.Refresh();
            var root = PrefabUtility.LoadPrefabContents(prefab); int index = 0;
            foreach (Transform part in root.transform)
            {
                bool wall = part.name.StartsWith("wall-");
                if (!wall && part.name != "planks" && part.name != "Subfloor") continue;
                foreach (var filter in part.GetComponentsInChildren<MeshFilter>())
                {
                    string path = directory + "surface_" + index++ + ".asset";
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    var authored = Object.Instantiate(filter.sharedMesh); authored.name = part.name + " Finish UV";
                    {
                        var vertices = authored.vertices; var uv = new Vector2[vertices.Length];
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            var p = root.transform.InverseTransformPoint(filter.transform.TransformPoint(vertices[i]));
                            bool side = Mathf.Abs(Vector3.Dot(part.forward, Vector3.right)) > .5f;
                            uv[i] = wall ? new Vector2(side ? p.z : p.x, p.y) * .5f : new Vector2(p.x,p.z) * .5f;
                        }
                        authored.uv = uv;
                    }
                    if (mesh == null) { mesh = authored; AssetDatabase.CreateAsset(mesh,path); }
                    else { EditorUtility.CopySerialized(authored,mesh); Object.DestroyImmediate(authored); EditorUtility.SetDirty(mesh); }
                    filter.sharedMesh = mesh;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(root,prefab); PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
