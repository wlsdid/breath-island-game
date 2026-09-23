using UnityEngine;
namespace BreathIsland
{
    public sealed class PredatorCombatHud : HudScreen
    {
        private readonly PredatorCombatSystem combat;
        public PredatorCombatHud(GameContext context, GamePhaseController phases, PredatorCombatSystem combat) : base(context, phases) { this.combat = combat; }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            ui.Panel(new Rect(width / 2f - 330, 100, 660, 136), string.Empty, DarkBox());
            ui.Label(new Rect(width / 2f - 280, 113, 560, 34), context.Text("ui_predator_breaker_shark"), CenteredLight());
            ui.Meter(new Rect(width / 2f - 275, 188, 550, 24), context.Text("ui_health"), combat.Health, new Color(.95f, .42f, .28f), NotebookStyle.Light);
            ui.Panel(new Rect(width / 2f - 250, height - 240, 500, 58), string.Format(context.Text("ui_next_attack_0_0"), combat.Charge), DarkBox());
            Rect bar = new(width / 2f - 360, height - 160, 720, 92); ui.Panel(bar, string.Empty, DarkBox());
            ui.Icon(new Rect(bar.x + 18, bar.y + 28, 36, 36), "heart", NotebookSurface.Paper);
            ui.Label(new Rect(bar.x + 60, bar.y + 28, 60, 35), combat.PlayerHearts.ToString(), CenteredLight());
            ui.Button(new Rect(bar.x + 150, bar.y + 17, 165, 58), context.Text("ui_1_dodge"), () => combat.Act(0), buttonStyle);
            ui.Button(new Rect(bar.x + 325, bar.y + 17, 165, 58), context.Text("ui_2_net"), () => combat.Act(1), buttonStyle);
            ui.Button(new Rect(bar.x + 500, bar.y + 17, 195, 58), context.Text("ui_3_calm_pulse"), () => combat.Act(2), primaryButtonStyle);
        }
    }
}
