using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class HousingExpansionAuthoring
    {
        private const string Finishes = "Assets/BreathIsland/Resources/Content/Finishes/";
        public static void Build()
        {
            Directory.CreateDirectory(Finishes); AssetDatabase.Refresh();
            var catalog = IslandLifeCatalog.Load();
            AddFurniture(catalog, "couch", "couch", 3, 1, 1.2f, 8);
            AddFurniture(catalog, "cabinet", "cabinet_medium", 2, 1, 1.5f, 6);
            AddFurniture(catalog, "shelf", "shelf_A_small", 2, 1, 1.25f, 4);
            AddFurniture(catalog, "table_low", "table_low", 2, 1, .6f, 3);
            Finish("plaster", FinishSurface.Wall, new Color(.86f,.8f,.65f), false);
            Finish("wood_dark", FinishSurface.Wall, new Color(.31f,.17f,.1f), false);
            Finish("stone", FinishSurface.Wall, new Color(.46f,.49f,.46f), true);
            Finish("planks_light", FinishSurface.Floor, new Color(.68f,.44f,.24f), false);
            Finish("planks_dark", FinishSurface.Floor, new Color(.3f,.18f,.11f), false);
            Finish("tiles", FinishSurface.Floor, new Color(.51f,.54f,.5f), true);
            var ko = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            foreach (var row in Text)
            { if (ko.GetEntry(row.Item1) == null) ko.AddEntry(row.Item1, row.Item2); if (en.GetEntry(row.Item1) == null) en.AddEntry(row.Item1, row.Item3); }
            EditorUtility.SetDirty(ko); EditorUtility.SetDirty(en); EditorUtility.SetDirty(ko.SharedData);
            FinishMeshAuthoring.Build(); VillageAuthoring.Build(); AssetDatabase.SaveAssets();
        }
        private static void AddFurniture(IslandLifeCatalog catalog, string id, string model, int x, int z, float height, int wood)
        {
            if (catalog.Furniture(id) != null) return;
            var item = ScriptableObject.CreateInstance<FurnitureDefinition>(); item.id = id; item.nameKey = "life_furniture_" + id;
            item.model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BreathIsland/Art/ThirdParty/KayKit/Furniture/" + model + ".fbx");
            item.footprint = new Vector2Int(x,z); item.height = height; item.woodCost = wood;
            AssetDatabase.CreateAsset(item, "Assets/BreathIsland/Resources/Content/Life/FurnitureDefinition_" + id + ".asset");
            catalog.furniture = catalog.furniture.Concat(new[] {item}).ToArray(); EditorUtility.SetDirty(catalog);
        }
        private static void Finish(string id, FinishSurface surface, Color color, bool tiles)
        {
            if (AssetDatabase.LoadAssetAtPath<FinishDefinition>(Finishes + id + ".asset") != null) return;
            var texture = new Texture2D(64,64) { name = id + " Pattern", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                bool seam = tiles ? x % 32 < 2 || y % 32 < 2 : id != "plaster" && (x % 16 == 0 || (y + (x / 16 % 2) * 32) % 64 == 0);
                float grain = id == "plaster" ? 1 : 1 + Mathf.Sin(x * 1.7f + y * .2f) * .025f;
                texture.SetPixel(x,y,seam ? color * .65f : color * grain);
            }
            texture.Apply(); AssetDatabase.CreateAsset(texture, Finishes + id + "_texture.asset");
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Smoothness", .08f); AssetDatabase.CreateAsset(material, Finishes + id + ".mat");
            var definition = ScriptableObject.CreateInstance<FinishDefinition>(); definition.id=id; definition.nameKey="finish_"+id;
            definition.surface=surface; definition.material=material; AssetDatabase.CreateAsset(definition, Finishes+id+".asset");
        }
        private static readonly (string,string,string)[] Text = {
            ("life_furniture_couch","소파","Sofa"),("life_furniture_cabinet","수납장","Cabinet"),
            ("life_furniture_shelf","작은 선반","Small shelf"),("life_furniture_table_low","낮은 탁자","Low table"),
            ("finish_title","실내 마감","Room finishes"),("finish_preview_help","방을 보며 골라요. 적용 전에는 저장되지 않아요.","Preview your room. Changes are saved only when applied."),
            ("finish_wall","벽","Walls"),("finish_floor","바닥","Floor"),("finish_apply","마감 적용","Apply finishes"),("finish_cancel","마감 취소","Cancel preview"),
            ("finish_plaster","밝은 회벽","Light plaster"),("finish_wood_dark","짙은 목재","Dark timber"),
            ("finish_stone","회색 석조","Grey stone"),("finish_planks_light","밝은 판재","Light planks"),
            ("finish_planks_dark","짙은 판재","Dark planks"),("finish_tiles","석재 타일","Stone tiles")};
    }
}
