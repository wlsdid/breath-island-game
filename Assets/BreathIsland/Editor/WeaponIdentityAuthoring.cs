using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class WeaponIdentityAuthoring
    {
        private const string Folder = "Assets/BreathIsland/Resources/World/Weapons/";
        private const string Art = "Assets/BreathIsland/Art/ThirdParty/KayKit/Weapons/";
        public static void Build()
        {
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var catalog = IslandLifeCatalog.Load();
            Assign(catalog, "bow", "bow_A", false);
            Assign(catalog, "sword_special", "sword_E", true);
            Assign(catalog, "bow_special", "bow_B", true);
            Assign(catalog, "staff_special", "staff_B", true);
            Model("Arrow", "arrow_A", catalog.weaponMaterial, .45f);
            var ko = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            ko.AddEntry("fishing_move_hint", "WASD 부두 이동 · 마우스 시점 · Alt 유지 버튼 조작 · Q 줄 풀기");
            en.AddEntry("fishing_move_hint", "WASD move · Mouse look · Hold Alt for buttons · Q ease line");
            EditorUtility.SetDirty(ko); EditorUtility.SetDirty(en); EditorUtility.SetDirty(ko.SharedData);
            AssetDatabase.SaveAssets();
        }
        private static void Assign(IslandLifeCatalog catalog, string id, string source, bool special)
        {
            var definition = catalog.Weapon(id); var material = catalog.weaponMaterial;
            if (special)
            {
                string path = Folder + id + ".mat";
                material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(catalog.weaponMaterial); AssetDatabase.CreateAsset(material,path); }
                material.color = Color.Lerp(Color.white, definition.skill.effectColor, .4f);
                material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor",definition.skill.effectColor*.22f);
                EditorUtility.SetDirty(material);
            }
            definition.model = Model(id,source,material,1);
            EditorUtility.SetDirty(definition);
        }
        private static GameObject Model(string id, string source, Material material, float length)
        {
            var root = new GameObject(id);
            try
            {
                var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art+source+".fbx"),root.transform);
                var renderers = model.GetComponentsInChildren<Renderer>();
                var bounds = renderers[0].bounds;
                foreach(var renderer in renderers) { bounds.Encapsulate(renderer.bounds); var materials=renderer.sharedMaterials; for(int i=0;i<materials.Length;i++)materials[i]=material;renderer.sharedMaterials=materials; }
                Vector3 axis = bounds.size.x > bounds.size.y && bounds.size.x > bounds.size.z ? Vector3.right : bounds.size.z > bounds.size.y ? Vector3.forward : Vector3.up;
                model.transform.rotation = Quaternion.FromToRotation(axis,Vector3.up)*model.transform.rotation;
                model.transform.localScale *= length/Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
                return PrefabUtility.SaveAsPrefabAsset(root,Folder+id+".prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
