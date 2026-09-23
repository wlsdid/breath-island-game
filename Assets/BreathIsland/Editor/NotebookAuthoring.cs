using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class NotebookAuthoring
    {
        public static void Build()
        {
            BuildElement();
            const string directory="Assets/BreathIsland/Resources/UI/Screens";
            System.IO.Directory.CreateDirectory(directory);
            foreach(string name in new[]{"ExploreHud","EquipmentHud","HousingHud","FishingHud","PredatorCombatHud","CatchResolutionHud","DialogueHud","FacilityHud","CollectionHud","PauseHud","HudOverlay"})
            {
                string path=directory+"/"+name+".prefab";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)continue;
                var root=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(CanvasGroup));
                root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
                var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
                PrefabUtility.SaveAsPrefabAsset(root,path);Object.DestroyImmediate(root);
            }
            var ko=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            foreach(var row in Entries)
            { if(ko.GetEntry(row.Item1)==null)ko.AddEntry(row.Item1,row.Item2);if(en.GetEntry(row.Item1)==null)en.AddEntry(row.Item1,row.Item3); }
            EditorUtility.SetDirty(ko);EditorUtility.SetDirty(en);EditorUtility.SetDirty(ko.SharedData);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        private static void BuildElement()
        {
            const string path="Assets/BreathIsland/Resources/UI/Elements/NotebookElement.prefab";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)return;
            var node=new GameObject("Notebook Element",typeof(RectTransform),typeof(NotebookGraphic),typeof(Button),typeof(NotebookButtonMotion));
            var label=new GameObject("Label",typeof(RectTransform),typeof(Text));label.transform.SetParent(node.transform,false);
            var rt=label.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(14,3);rt.offsetMax=new Vector2(-14,-3);
            label.GetComponent<Text>().raycastTarget=false;
            var icon=new GameObject("Icon",typeof(RectTransform),typeof(IslandIcon));icon.transform.SetParent(node.transform,false);
            var ir=icon.GetComponent<RectTransform>();ir.anchorMin=Vector2.zero;ir.anchorMax=Vector2.one;ir.offsetMin=Vector2.zero;ir.offsetMax=Vector2.zero;
            icon.GetComponent<IslandIcon>().raycastTarget=false;
            PrefabUtility.SaveAsPrefabAsset(node,path);Object.DestroyImmediate(node);
        }
        private static readonly (string,string,string)[] Entries={
            ("bag_title","섬살이 가방","My island satchel"),
            ("bag_subtitle","오늘의 작은 모험을 챙겨요","A little room for today's adventures"),
            ("bag_tab_Tools","생활 도구","Tools"),("bag_tab_Weapons","무기","Weapons"),("bag_tab_Resources","자원","Resources"),("bag_tab_Furniture","가구","Furniture"),
            ("bag_open","I  가방 열기","I  Open satchel"),("bag_close","닫기  I","Close  I"),
            ("bag_hold","손에 들기","Take in hand"),("bag_held","손에 든 물건","In hand"),("bag_put_away","내려놓기","Put away"),
            ("bag_tool_detail","가방에서 꺼내 자유롭게 사용해요. 알맞은 대상에 닿으면 자원을 얻어요.","Take it out and use it freely. Gather resources when it strikes a matching target."),
            ("bag_regular_detail","이 무기의 직업으로 전환해요. 기본 공격을 사용할 수 있어요.","Switch to this weapon's job. This regular weapon has a basic attack."),
            ("bag_special_detail","도전과제로 얻은 특별한 무기. 직업 숙련도에 따라 스킬이 자라요.","A special weapon earned through a challenge. Its skill grows with job mastery."),
            ("bag_resource_detail","섬에서 모은 소중한 재료. 제작과 기존 섬 활동에 사용해요.","Supplies collected around the island. Used for crafting and island activities."),
            ("bag_furniture_detail","방 안에서 배치할 수 있어요. 회수하면 가방으로 돌아와요.","Place it inside your room. Reclaim it to return it to your satchel."),
            ("bag_wood","나무","Wood"),("bag_stone","돌","Stone"),("bag_herbs","약초","Herbs"),("bag_coins","코인","Coins"),("bag_materials","시설 재료","Building materials"),("bag_bait","미끼","Bait"),
            ("bag_craft","망치로 만들기","Craft with hammer"),("bag_cost","나무 {0} · 돌 {1}","Wood {0} · Stone {1}"),
            ("bag_place","방에 배치하기","Place in room"),("bag_indoor_only","실내에서 사용할 수 있어요","Available inside your room"),
            ("bag_back","돌아가기","Back"),("bag_day","섬살이 {0}일째 · {1:00}:00","Island day {0} · {1:00}:00"),("bag_day_short","섬살이 {0}일째","Island day {0}"),
            ("bag_hint","물건을 선택하고, 오늘 하고 싶은 일을 시작해요.","Choose something and make a little plan for today."),
            ("bag_gather_hint","E  {0} 채집","E  Gather {0}"),("bag_tool_hint","좌클릭  {0} · 필요한 도구: {1}","Left click  {0} · Tool: {1}"),
            ("bag_talk_hint","E  이야기 나누기","E  Talk"),("bag_fish_hint","F  낚시 시작","F  Start fishing"),("bag_reset_hint","T  훈련 대상 회복","T  Reset training target"),
            ("bag_room_help","가방에서 망치로 가구를 만들고, 보유 가구를 선택해 배치해요.","Open your satchel to craft with the hammer. Select furniture you own to place it."),
            ("bag_placed","방 안의 물건","In your room"),("bag_previous","이전","Previous"),("bag_next","다음","Next"),
            ("bag_facility_cost","코인 {0} + 재료 {1}","Coins {0} + materials {1}")
        };
    }
}
