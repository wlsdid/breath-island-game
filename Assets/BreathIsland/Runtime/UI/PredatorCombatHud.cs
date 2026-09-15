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
            GUI.Box(new Rect(width / 2f - 330, 100, 660, 100), string.Empty, DarkBox());
            GUI.Label(new Rect(width / 2f - 280, 113, 560, 34), context.Text("ui_predator_breaker_shark"), CenteredLight());
            DrawMeter(new Rect(width / 2f - 275, 154, 550, 24), context.Text("ui_health"), combat.Health, new Color(.95f, .42f, .28f));
            GUI.Box(new Rect(width / 2f - 250, height - 240, 500, 58), string.Format(context.Text("ui_next_attack_0_0"), combat.Charge), DarkBox());
            Rect bar = new(width / 2f - 360, height - 160, 720, 92); GUI.Box(bar, string.Empty, DarkBox());
            GUI.Label(new Rect(bar.x + 18, bar.y + 28, 120, 35), new string('♥', combat.PlayerHearts), CenteredLight());
            if (GUI.Button(new Rect(bar.x + 150, bar.y + 17, 165, 58), context.Text("ui_1_dodge"), buttonStyle)) combat.Act(0);
            if (GUI.Button(new Rect(bar.x + 325, bar.y + 17, 165, 58), context.Text("ui_2_net"), buttonStyle)) combat.Act(1);
            if (GUI.Button(new Rect(bar.x + 500, bar.y + 17, 195, 58), context.Text("ui_3_calm_pulse"), primaryButtonStyle)) combat.Act(2);
        }
    }
}
