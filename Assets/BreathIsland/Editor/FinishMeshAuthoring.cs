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
                    if (mesh == null)
                    {
                        mesh = Object.Instantiate(filter.sharedMesh); mesh.name = part.name + " Finish UV";
                        var vertices = mesh.vertices; var uv = new Vector2[vertices.Length];
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            var p = root.transform.InverseTransformPoint(filter.transform.TransformPoint(vertices[i]));
                            uv[i] = wall ? new Vector2(part.localPosition.z > 8 ? p.x : p.z, p.y) * .5f : new Vector2(p.x,p.z) * .5f;
                        }
                        mesh.uv = uv; AssetDatabase.CreateAsset(mesh,path);
                    }
                    filter.sharedMesh = mesh;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(root,prefab); PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
