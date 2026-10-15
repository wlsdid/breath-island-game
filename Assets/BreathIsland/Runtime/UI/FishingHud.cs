using UnityEngine;
namespace BreathIsland
{
    public sealed class FishingHud : HudScreen
    {
        private readonly FishingSystem fishing;
        public FishingHud(GameContext context, GamePhaseController phases, FishingSystem fishing) : base(context, phases) { this.fishing = fishing; }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            ui.Panel(new Rect(width/2-350,height-40,700,34),context.Text("fishing_move_hint"));
            ui.Panel(new Rect(24, 105, 210, 48), context.Text("ui_first_person_sunny_shore"), DarkBox());
            Rect panel = new(width / 2f - 275f, height - 290f, 550f, 245f);
            ui.Panel(panel, string.Empty, panelStyle);
            if (fishing.Stage == FishingStage.Ready)
            {
                ui.Label(new Rect(panel.x + 25, panel.y + 25, 500, 42), context.Text("ui_gentle_ripples"), titleStyle);
                ui.Label(new Rect(panel.x + 25, panel.y + 72, 500, 56), context.Text("ui_cast_when_a_fish_shadow_passes_through_the_water"), bodyStyle);
                ui.Button(new Rect(panel.x + 135, panel.y + 150, 280, 58), context.Text("ui_cast_line"), () => fishing.Cast(), primaryButtonStyle);
            }
            else if (fishing.Stage == FishingStage.Waiting)
            {
                ui.Label(new Rect(panel.x + 25, panel.y + 52, 500, 42), context.Text("ui_waiting_for_a_bite"), titleStyle);
                ui.Label(new Rect(panel.x + 25, panel.y + 105, 500, 50), context.Text("ui_watch_the_movement_of_the_water"), centeredStyle);
            }
            else if (fishing.Stage == FishingStage.Bite)
            {
                ui.Label(new Rect(panel.x + 25, panel.y + 18, 500, 42), context.Text("fishing_bite_now"), titleStyle);
                ui.Label(new Rect(panel.x + 25, panel.y + 58, 500, 40), context.Text("fishing_hook_hint"), centeredStyle);
                DrawMeter(new Rect(panel.x + 28, panel.y + 122, 494, 25),
                    string.Format(context.Text("fishing_bite_time"), fishing.ReactionRemaining),
                    100 * fishing.ReactionRemaining / FishingSystem.ReactionWindow, new Color(.95f, .5f, .3f));
                ui.Button(new Rect(panel.x + 135, panel.y + 166, 280, 56), context.Text("fishing_hook_action"), () => fishing.Hook(), primaryButtonStyle);
            }
            else if (fishing.Stage == FishingStage.Hooked)
            {
                ui.Label(new Rect(panel.x + 25, panel.y + 18, 500, 34), fishing.ActiveFish.LocalizedName(ko), titleStyle);
                DrawMeter(new Rect(panel.x + 28, panel.y + 67, 494, 25), context.Text("ui_reel"), fishing.Progress, new Color(.2f, .75f, .66f));
                DrawMeter(new Rect(panel.x + 28, panel.y + 111, 494, 25), context.Text("ui_tension"), fishing.Tension, fishing.Tension > 74f ? new Color(.95f, .38f, .28f) : new Color(.95f, .67f, .22f));
                ui.Button(new Rect(panel.x + 28, panel.y + 164, 235, 52), context.Text("ui_s_ease_line"), () => fishing.Ease(), buttonStyle);
                ui.Button(new Rect(panel.x + 278, panel.y + 164, 244, 52), context.Text("ui_space_reel"), () => fishing.Reel(), primaryButtonStyle);
            }
            else
            {
                ui.Label(new Rect(panel.x + 25, panel.y + 35, 500, 42), context.Text(fishing.MissedBite ? "fishing_bite_missed" : "ui_the_line_snapped"), titleStyle);
                ui.Label(new Rect(panel.x + 25, panel.y + 91, 500, 48), context.Text("ui_it_is_okay_you_only_lost_one_bait"), centeredStyle);
                ui.Button(new Rect(panel.x + 135, panel.y + 157, 280, 52), context.Text("ui_try_again"), () => fishing.Retry(), primaryButtonStyle);
            }
        }
    }
}
