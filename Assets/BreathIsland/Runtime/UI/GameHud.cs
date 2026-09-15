using System.Collections.Generic;
namespace BreathIsland
{
    // Routes the current phase; layout and interaction belong to the screens.
    public sealed class GameHud : System.IDisposable
    {
        private readonly GamePhaseController phases;
        private readonly IReadOnlyDictionary<GamePhase, HudScreen> screens;
        private readonly HudOverlay overlay;
        public GameHud(GamePhaseController phases, IReadOnlyDictionary<GamePhase, HudScreen> screens, HudOverlay overlay)
        { this.phases = phases; this.screens = screens; this.overlay = overlay; }
        public void Draw()
        {
            using var canvas = new HudCanvas(true);
            screens[phases.Current].Draw(canvas.Width, canvas.Height);
            overlay.Draw(canvas.Width, canvas.Height);
        }
        public void Dispose() { foreach (var screen in screens.Values) screen.Dispose(); overlay.Dispose(); }
    }
}
