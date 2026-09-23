using System.IO;
using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    // Explicit prefab authoring; the game only instantiates the saved result.
    public static class HomeInteriorAuthoring
    {
        private const string Art = "Assets/BreathIsland/Art/ThirdParty/Kenney/Town/";
        private const string Output = "Assets/BreathIsland/Resources/World/";
        [MenuItem("숨결섬/집 실내 에셋 작성")]
        public static void Build()
        {
            Directory.CreateDirectory(Output); AssetDatabase.Refresh();
            var material = AssetDatabase.LoadAssetAtPath<Material>(Output + "Town.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "colormap.png"));
                AssetDatabase.CreateAsset(material, Output + "Town.mat");
            }
            var root = new GameObject("Home Interior");
            for (int x = 0; x < 4; x++) for (int z = 0; z < 4; z++)
                Part(root.transform, "planks", new Vector3(1 + x * 2, -.08f, 1 + z * 2), new Vector3(2, .08f, 2), 0, material);
            for (int i = 0; i < 4; i++)
            {
                Part(root.transform, i == 1 || i == 2 ? "wall-wood-window-small" : "wall-wood", new Vector3(1 + i * 2, 0, 8.1f), new Vector3(2, 1.8f, .2f), 0, material);
                Part(root.transform, "wall-wood", new Vector3(-.1f, 0, 1 + i * 2), new Vector3(2, .85f, .2f), 90, material);
                Part(root.transform, "wall-wood", new Vector3(8.1f, 0, 1 + i * 2), new Vector3(2, .85f, .2f), 90, material);
            }
            var floor = new GameObject("Floor Collision"); floor.transform.SetParent(root.transform, false);
            var box = floor.AddComponent<BoxCollider>(); box.center = new Vector3(4, -.15f, 4); box.size = new Vector3(8, .3f, 8);
            var backingMaterial = AssetDatabase.LoadAssetAtPath<Material>(Output + "FloorBacking.mat");
            if (backingMaterial == null)
            {
                backingMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                backingMaterial.SetColor("_BaseColor", new Color(.25f, .13f, .07f));
                AssetDatabase.CreateAsset(backingMaterial, Output + "FloorBacking.mat");
            }
            var backing = GameObject.CreatePrimitive(PrimitiveType.Cube); backing.name = "Subfloor";
            backing.transform.SetParent(root.transform, false); backing.transform.localPosition = new Vector3(4, -.17f, 4);
            backing.transform.localScale = new Vector3(8, .28f, 8);
            Object.DestroyImmediate(backing.GetComponent<Collider>());
            backing.GetComponent<Renderer>().sharedMaterial = backingMaterial;
            PrefabUtility.SaveAsPrefabAsset(root, Output + "HomeInterior.prefab"); Object.DestroyImmediate(root);
            root = new GameObject("Home Doorway");
            Part(root.transform, "wall-wood-doorway-square", Vector3.zero, new Vector3(1.6f, 2, .25f), 0, material);
            Part(root.transform, "wall-wood-door", new Vector3(0, .02f, .02f), new Vector3(.95f, 1.7f, .12f), 0, material);
            PrefabUtility.SaveAsPrefabAsset(root, Output + "HomeDoorway.prefab"); Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
        }
        private static void Part(Transform parent, string name, Vector3 position, Vector3 size, float yaw, Material material)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Art + name + ".fbx");
            if (source == null) throw new System.InvalidOperationException("Missing house model: " + name);
            var pivot = new GameObject(name).transform; pivot.SetParent(parent, false);
            var model = Object.Instantiate(source, pivot); model.transform.localPosition = Vector3.zero;
            var bounds = new Bounds(); bool first = true;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            {
                if (first) { bounds = renderer.bounds; first = false; } else bounds.Encapsulate(renderer.bounds);
                var slots = renderer.sharedMaterials; for (int i = 0; i < slots.Length; i++) slots[i] = material; renderer.sharedMaterials = slots;
            }
            Vector3 scale = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, size.z / bounds.size.z);
            pivot.localScale = scale;
            model.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            pivot.localPosition = position; pivot.localRotation = Quaternion.Euler(0, yaw, 0);
        }
    }
}
