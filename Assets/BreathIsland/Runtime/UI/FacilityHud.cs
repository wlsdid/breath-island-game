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
            Rect panel = new(width - 570, 0, 570, height); ui.Panel(panel, string.Empty, panelStyle);
            ui.Label(new Rect(panel.x + 35, 45, 500, 46), context.Text("ui_village_growth"), titleStyle);
            ui.Label(new Rect(panel.x + 35, 100, 500, 52), context.Text("ui_restore_either_facility_first_every_choice_can_be_completed_later"), bodyStyle);
            DrawFacilityCard(new Rect(panel.x + 35, 185, 500, 190), FacilityId.Boatyard, facilities.SelectedIndex == 0);
            DrawFacilityCard(new Rect(panel.x + 35, 395, 500, 190), FacilityId.Aquarium, facilities.SelectedIndex == 1);
            ui.Button(new Rect(panel.x + 35, height - 82, 500, 52), context.Text("ui_close_esc"), () => phases.Set(GamePhase.Explore), buttonStyle);
        }
        private void DrawFacilityCard(Rect rect, FacilityId id, bool selected)
        {
            bool ko = context.Korean;
            ui.Panel(rect, string.Empty, panelStyle);
            FacilityDefinition definition = context.Content.Facilities[id];
            int level = context.Save.facilities.Get(id);
            ui.Label(new Rect(rect.x + 20, rect.y + 15, 450, 36), string.Format(context.Text("facility_level"), definition.LocalizedName(ko), level), titleStyle);
            ui.Label(new Rect(rect.x + 20, rect.y + 61, 450, 45), id == FacilityId.Boatyard ? (context.Text("ui_unlocks_a_small_boat_and_the_reef_route")) : (context.Text("ui_displays_creatures_and_brings_villagers_back")), smallStyle);
            if (level < 2)
            {
                string cost = string.Format(context.Text("bag_facility_cost"), definition.coinCosts[level], definition.materialCosts[level]);
                ui.Button(new Rect(rect.x + 20, rect.y + 122, 460, 48), (context.Text("ui_restore")) + cost, () => facilities.Upgrade(id), primaryButtonStyle);
            }
            else ui.Label(new Rect(rect.x + 20, rect.y + 130, 460, 38), context.Text("ui_restoration_complete"), centeredStyle);
            if (selected) ui.Icon(new Rect(rect.x + 446, rect.y + 18, 28, 28), "sun");
        }
    }
}
