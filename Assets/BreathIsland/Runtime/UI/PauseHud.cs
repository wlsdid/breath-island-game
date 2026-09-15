using UnityEngine;
namespace BreathIsland
{
    public sealed class PauseHud : HudScreen
    {
        private readonly GameSession session;
        public PauseHud(GameContext context, GamePhaseController phases, GameSession session) : base(context, phases) { this.session = session; }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            Rect panel = new(width / 2f - 310, height / 2f - 280, 620, 560); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 28, 440, 45), context.Text("ui_take_a_breath"), titleStyle);
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 92, 490, 50), context.Text("ui_continue"), primaryButtonStyle)) phases.Set(phases.BeforePause);
            for (int slot = 0; slot < 3; slot++)
            {
                int capturedSlot = slot;
                float y = panel.y + 164 + slot * 70;
                if (GUI.Button(new Rect(panel.x + 65, y, 235, 50), $"{(context.Text("ui_save"))} {slot + 1}", buttonStyle)) { session.Save(capturedSlot); }
                if (GUI.Button(new Rect(panel.x + 320, y, 235, 50), $"{(context.Text("ui_load"))} {slot + 1}", buttonStyle)) { session.Load(capturedSlot); }
            }
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 390, 490, 50), context.Text("ui_language"), buttonStyle)) context.Korean = !context.Korean;
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 462, 490, 50), context.Text("ui_quit_game"), buttonStyle)) Application.Quit();
        }
    }
}
