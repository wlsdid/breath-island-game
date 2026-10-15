using System.IO;
using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class SkillVfxAuthoring
    {
        private const string Folder = "Assets/BreathIsland/Resources/World/SkillVfx/";
        public static void Build()
        {
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            Create("warrior", "big_hit_6x5", 6, 5, 11, 2.5f);
            // Supplied name says 6x5; actual atlas is 7x6 (120x109 cells).
            Create("ranger", "star_explosion_6x5", 7, 6, 29, 1.8f);
            Create("mage", "electric_ring_6x5", 6, 5, 29, 2.3f);
            AssetDatabase.SaveAssets();
        }
        private static void Create(string id, string atlas, int columns, int rows, int count, float size)
        {
            string source = "Assets/BreathIsland/Art/ThirdParty/BrackeysVfx/" + atlas + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(source);
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
            importer.wrapMode = TextureWrapMode.Clamp; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            string path = Folder + id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("BreathIsland/SkillFlipbook")); AssetDatabase.CreateAsset(material, path); }
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(source); EditorUtility.SetDirty(material);
            var root = GameObject.CreatePrimitive(PrimitiveType.Quad);
            try
            {
                root.name = id + " skill burst"; Object.DestroyImmediate(root.GetComponent<Collider>());
                root.transform.localScale = Vector3.one * size;
                var renderer = root.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
                var effect = root.AddComponent<SkillFlipbook>(); effect.columns = columns; effect.rows = rows; effect.frameCount = count;
                PrefabUtility.SaveAsPrefabAsset(root, Folder + id + ".prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
