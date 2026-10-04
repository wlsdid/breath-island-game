using UnityEditor;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class InventoryLayoutAuthoring
    {
        public static void Build()
        {
            var ko=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            foreach(var row in new[]{
                ("bag_tab_All","가방","Satchel"),
                ("bag_sort","종류별로 정리","Sort by kind"),
                ("bag_empty_slot","빈 자리\n물건을 끌어다 놓아보세요.","An empty pocket\nDrag an item here."),
                ("bag_drag_help","끌어서 이동 · 다른 물건 위에 놓으면 자리 교환","Drag to move · Drop onto another item to swap")})
            {if(ko.GetEntry(row.Item1)==null)ko.AddEntry(row.Item1,row.Item2);if(en.GetEntry(row.Item1)==null)en.AddEntry(row.Item1,row.Item3);}
            ko.GetEntry("home_walk_hint").Value="WASD 이동 · Space 점프 · 가구를 바라보고 E로 사용";
            en.GetEntry("home_walk_hint").Value="WASD to walk · Space to jump · Face furniture and press E";
            EditorUtility.SetDirty(ko);EditorUtility.SetDirty(en);EditorUtility.SetDirty(ko.SharedData);AssetDatabase.SaveAssets();
        }
    }
}
