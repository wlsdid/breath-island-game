using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    public sealed class GameHud : System.IDisposable
    {
        private readonly GamePhaseController phases;
        private readonly IReadOnlyDictionary<GamePhase,HudScreen> screens;
        private readonly HudOverlay overlay;
        public GameHud(GamePhaseController phases,IReadOnlyDictionary<GamePhase,HudScreen> screens,HudOverlay overlay)
        {this.phases=phases;this.screens=screens;this.overlay=overlay;}
        public void Draw()
            => Draw(Screen.width,Screen.height);
        public void Draw(float screenWidth,float screenHeight)
        {
            float scale=Mathf.Min(screenWidth/1600f,screenHeight/900f);
            foreach(var screen in screens) if(screen.Key!=phases.Current) screen.Value.Hide();
            screens[phases.Current].Render(screenWidth/scale,screenHeight/scale);
            overlay.Render(screenWidth/scale,screenHeight/scale);
        }
        public void Dispose(){foreach(var screen in screens.Values)screen.Dispose();overlay.Dispose();}
    }
}
