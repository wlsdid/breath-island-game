using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class HudTestHost : MonoBehaviour
    {
        [System.NonSerialized] public GameHud Hud;
        public int DrawCount { get; private set; }
        private void OnGUI() { if (Hud != null) { Hud.Draw(); DrawCount++; } }
        private void OnDestroy() => Hud?.Dispose();
    }
}
