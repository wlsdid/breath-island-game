using UnityEditor;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class VisualRepairAuthoring
    {
        public static void Build()
        {
            CharacterMotionAuthoring.Build();TerrainRepairAuthoring.Build();
            VillagePolishAuthoring.Build();HouseShellAuthoring.Build();CoastalVisualAuthoring.Build();
            var ko=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            ko.AddEntry("ui_s_ease_line","Q · 줄 느슨하게");en.AddEntry("ui_s_ease_line","Q · Ease line");
            ko.AddEntry("fishing_move_hint","WASD 부두 이동 · 우클릭 유지 시점 이동 · Q 줄 풀기");
            en.AddEntry("fishing_move_hint","WASD move on pier · Hold RMB to look · Q ease line");
            EditorUtility.SetDirty(ko);EditorUtility.SetDirty(en);EditorUtility.SetDirty(ko.SharedData);AssetDatabase.SaveAssets();
        }
    }
}
