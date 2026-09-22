using UnityEditor;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    // One-time additive authoring; subsequent runs retain edited translations.
    public static class IslandLifeLocalization
    {
        public static void AddMissingEntries()
        {
            var ko = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            foreach (var row in Entries)
            {
                if (ko.GetEntry(row.Item1) == null) ko.AddEntry(row.Item1, row.Item2);
                if (en.GetEntry(row.Item1) == null) en.AddEntry(row.Item1, row.Item3);
            }
            EditorUtility.SetDirty(ko); EditorUtility.SetDirty(en); EditorUtility.SetDirty(ko.SharedData);
        }
        private static readonly (string, string, string)[] Entries =
        {
            ("life_out_of_range", "훈련 대상에게 더 가까이 다가가세요.", "Move closer to the training target."),
            ("life_hit", "{0}: 피해 {1} · 대상 체력 {2}", "{0}: {1} damage · target health {2}"),
            ("life_achievement_unlocked", "도전과제 달성! {0}", "Challenge complete! {0}"),
            ("life_achievement_craft", "가구 3개 제작 → 특수 검", "Craft 3 furniture items → special sword"),
            ("life_achievement_training", "유효 명중 30회 → 특수 활", "Land 30 valid hits → special bow"),
            ("life_achievement_gather", "나무·돌·약초 각각 3회 채집 → 특수 지팡이", "Harvest wood, stone and herbs 3 times each → special staff"),
            ("life_need_tool", "필요한 도구: {0}", "Required tool: {0}"),
            ("life_harvested", "{0} +{1}", "{0} +{1}"),
            ("life_new_day", "푹 쉬었어요. 자원이 다시 자랐습니다.", "A new day. Resources have regrown."),
            ("life_craft_hammer", "제작용 망치를 선택하세요.", "Select the crafting hammer."),
            ("life_craft_materials", "제작에 필요한 자원이 부족합니다.", "Not enough crafting resources."),
            ("life_unarmed", "무기 없음 · 직업 없음", "Unarmed · no job"),
            ("life_tool_None", "맨손 / 무기 들기", "Hands / hold weapon"),
            ("life_tool_Axe", "도끼", "Axe"), ("life_tool_Pickaxe", "곡괭이", "Pickaxe"), ("life_tool_Hammer", "망치", "Hammer"),
            ("life_equipment_open", "I  장비와 도구", "I  Equipment & tools"),
            ("life_attack", "Q  기본 공격", "Q  Attack"), ("life_skill", "R  무기 스킬", "R  Weapon skill"),
            ("life_training", "훈련 대상 체력 {0}/100\n스킬 대기 {1:0.0}초", "Target health {0}/100\nSkill cooldown {1:0.0}s"),
            ("life_reset_target", "훈련 대상 회복", "Reset training target"),
            ("life_explore_help", "E: 채집·실내 입장\n훈련 대상은 섬 중앙에 있어요.", "E: harvest / enter home\nTraining target: island centre."),
            ("life_resources", "나무 {0} · 돌 {1} · 약초 {2}", "Wood {0} · Stone {1} · Herbs {2}"),
            ("life_enter", "E  집 꾸미기 입장", "E  Enter home"),
            ("life_depleted", "소진됨 · 하루 쉬면 다시 자라요", "Depleted · rest a day to regrow"),
            ("life_node_prompt", "E  {0} 채집 · {1}", "E  Harvest {0} · {1}"),
            ("life_equipment", "장비 · 직업 · 생활 도구", "Equipment · jobs · tools"),
            ("life_mastery", "{0} · 명중 {1}회 · 숙련 단계 {2}", "{0} · {1} hits · mastery tier {2}"),
            ("life_unequip", "무기 해제", "Unequip weapon"),
            ("life_mastery_help", "직업별 명중 10회: 스킬 피해 +25% / 30회: 대기 -20%\n일반 무기는 기본 공격만 가능합니다.", "Per job: 10 hits = +25% skill damage; 30 hits = -20% cooldown.\nRegular weapons have basic attacks only."),
            ("life_home", "작은 집 꾸미기", "Decorate your room"),
            ("life_select_hammer", "제작용 망치 들기", "Hold crafting hammer"),
            ("life_furniture_stock", "{0} · 보유 {1}", "{0} · available {1}"),
            ("life_craft", "제작: 나무 {0} / 돌 {1}", "Craft: wood {0} / stone {1}"),
            ("life_select", "배치 선택", "Select"),
            ("life_rest", "하루 쉬기", "Rest a day"), ("life_exit", "섬으로 나가기", "Return to island"),
            ("life_grid_help", "배치할 칸을 선택하세요. 초록은 가능, 빨강은 불가. 아래 중앙은 출입구입니다.", "Choose a cell. Green: valid, red: blocked. Entrance: bottom centre."),
            ("life_rotate", "90° 회전", "Rotate 90°"), ("life_cancel", "배치 취소", "Cancel"), ("life_place", "배치 확정", "Place"),
            ("life_recall_help", "배치된 가구를 눌러 회수하세요.", "Click placed furniture below to recall."),
            ("life_unknown", "알 수 없는 가구", "Unknown furniture"),
            ("life_job_warrior", "검사", "Warrior"), ("life_job_ranger", "궁수", "Ranger"), ("life_job_mage", "마법사", "Mage"),
            ("life_weapon_sword", "연습용 검", "Practice sword"), ("life_weapon_bow", "연습용 활", "Practice bow"), ("life_weapon_staff", "연습용 지팡이", "Practice staff"),
            ("life_weapon_sword_special", "숨결 검", "Breath sword"), ("life_weapon_bow_special", "바람 활", "Wind bow"), ("life_weapon_staff_special", "숲 지팡이", "Forest staff"),
            ("life_sword_basic", "베기", "Slash"), ("life_bow_basic", "화살", "Arrow"), ("life_staff_basic", "마력탄", "Magic bolt"),
            ("life_sword_skill", "숨결 가르기", "Breath cleave"), ("life_bow_skill", "집중 사격", "Focused shot"), ("life_staff_skill", "숲의 파동", "Forest pulse"),
            ("life_furniture_chair", "나무 의자", "Wooden chair"), ("life_furniture_table", "탁자", "Table"), ("life_furniture_bed", "침대", "Bed"), ("life_furniture_lamp", "조명", "Lamp"),
            ("life_resource_Wood", "나무", "Wood"), ("life_resource_Stone", "돌", "Stone"), ("life_resource_Herb", "약초", "Herbs")
        };
    }
}
