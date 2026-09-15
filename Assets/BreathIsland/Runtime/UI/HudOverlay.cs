using UnityEngine;
namespace BreathIsland
{
    public sealed class HudOverlay : HudScreen
    {
        private readonly GameSession session;
        public HudOverlay(GameContext context, GamePhaseController phases, GameSession session) : base(context, phases) { this.session = session; }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            DrawHud(width);
            if (Time.unscaledTime < session.MessageUntil) GUI.Box(new Rect(width / 2f - 260f, 22f, 520f, 48f), session.EventMessage, DarkBox());
        }
        private void DrawHud(float width)
        {
            bool ko = context.Korean;
            GUI.Box(new Rect(24, 22, 235, 68), context.Text("ui_breath_island_nquiet_coastal_village"), DarkBox());
            GUI.Box(new Rect(width - 330, 22, 306, 68), $"◉ {context.Save.inventory.coins}     ◆ {context.Save.inventory.materials}     🎣 {context.Save.inventory.bait}", DarkBox());
            GUI.Box(new Rect(width / 2f - 92f, 22, 184, 42), $"{(context.Text("ui_time"))} {context.Save.player.hour:00}:00", DarkBox());
        }
    }
}
