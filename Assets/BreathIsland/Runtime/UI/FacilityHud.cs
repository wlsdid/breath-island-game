using UnityEngine;
namespace BreathIsland
{
    public sealed class FacilityHud : HudScreen
    {
        private readonly FacilitySystem facilities;
        public FacilityHud(GameContext context, GamePhaseController phases, FacilitySystem facilities) : base(context, phases) { this.facilities = facilities; }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            Rect panel = new(width - 570, 0, 570, height); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 35, 45, 500, 46), context.Text("ui_village_growth"), titleStyle);
            GUI.Label(new Rect(panel.x + 35, 100, 500, 52), context.Text("ui_restore_either_facility_first_every_choice_can_be_completed_later"), bodyStyle);
            DrawFacilityCard(new Rect(panel.x + 35, 185, 500, 190), FacilityId.Boatyard, facilities.SelectedIndex == 0);
            DrawFacilityCard(new Rect(panel.x + 35, 395, 500, 190), FacilityId.Aquarium, facilities.SelectedIndex == 1);
            if (GUI.Button(new Rect(panel.x + 35, height - 82, 500, 52), context.Text("ui_close_esc"), buttonStyle)) phases.Set(GamePhase.Explore);
        }
        private void DrawFacilityCard(Rect rect, FacilityId id, bool selected)
        {
            bool ko = context.Korean;
            GUI.Box(rect, string.Empty, new GUIStyle(panelStyle) { normal = { background = paleTexture, textColor = Color.black } });
            FacilityDefinition definition = context.Content.Facilities[id];
            int level = context.Save.facilities.Get(id);
            GUI.Label(new Rect(rect.x + 20, rect.y + 15, 450, 36), string.Format(context.Text("facility_level"), definition.LocalizedName(ko), level), titleStyle);
            GUI.Label(new Rect(rect.x + 20, rect.y + 61, 450, 45), id == FacilityId.Boatyard ? (context.Text("ui_unlocks_a_small_boat_and_the_reef_route")) : (context.Text("ui_displays_creatures_and_brings_villagers_back")), smallStyle);
            if (level < 2)
            {
                string cost = $"{definition.coinCosts[level]} ◉  +  {definition.materialCosts[level]} ◆";
                if (GUI.Button(new Rect(rect.x + 20, rect.y + 122, 460, 48), (context.Text("ui_restore")) + cost, primaryButtonStyle)) facilities.Upgrade(id);
            }
            else GUI.Label(new Rect(rect.x + 20, rect.y + 130, 460, 38), context.Text("ui_restoration_complete"), centeredStyle);
            if (selected) GUI.Label(new Rect(rect.x + 440, rect.y + 12, 40, 28), "●", bodyStyle);
        }
    }
}
