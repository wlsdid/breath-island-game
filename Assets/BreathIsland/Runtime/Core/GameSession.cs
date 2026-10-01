using UnityEngine;

namespace BreathIsland
{
    // Owns global commands and notifications, independent of the scene entry point.
    public sealed class GameSession
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        public string EventMessage { get; private set; } = string.Empty;
        public float MessageUntil { get; private set; }
        public GameSession(GameContext context, GamePhaseController phases)
        { this.context = context; this.phases = phases; }
        public void ShowMessage(string text) { EventMessage = text; MessageUntil = Time.unscaledTime + 2.6f; }
        public void Save(int slot) { context.SaveTo(slot); ShowMessage(string.Format(context.Text("saved"), slot + 1)); }
        public void Load(int slot)
        {
            context.LoadFrom(slot);
            phases.Set(context.Save.housing.inside ? GamePhase.Home : GamePhase.Explore);
            ShowMessage(string.Format(context.Text("loaded"), slot + 1));
        }
        public bool TickGlobalInput()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.iKey.wasPressedThisFrame == true)
            {
                if (phases.Current == GamePhase.HomeStorage || phases.Current == GamePhase.HomeRest) phases.Set(GamePhase.Home);
                else if (phases.Current == GamePhase.Equipment) phases.CloseInventory(); else phases.OpenInventory();
                return true;
            }
            if (GameInput.LanguagePressed()) { context.Korean = !context.Korean; ShowMessage(context.Text("language")); }
            if (GameInput.SavePressed()) Save(0);
            if (GameInput.LoadPressed()) { Load(0); return true; }
            if (GameInput.CancelPressed()) { phases.Cancel(); return true; }
            return false;
        }
    }
}
