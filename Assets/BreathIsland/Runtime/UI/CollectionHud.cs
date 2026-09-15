using UnityEngine;
namespace BreathIsland
{
    public sealed class CollectionHud : HudScreen
    {
        public CollectionHud(GameContext context, GamePhaseController phases) : base(context, phases) {  }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            Rect panel = new(width - 650, 0, 650, height); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 35, 40, 570, 46), context.Text("ui_field_guide"), titleStyle);
            GUI.Label(new Rect(panel.x + 35, 91, 570, 32), $"{context.Save.inventory.discoveredFish.Count} / {context.Content.Fish.Count} {(context.Text("ui_discovered"))}", bodyStyle);
            float y = 145;
            foreach (FishDefinition definition in context.Content.Fish)
            {
                bool found = context.Save.inventory.discoveredFish.Contains(definition.id);
                GUI.Box(new Rect(panel.x + 35, y, 570, 72), string.Empty, new GUIStyle(panelStyle) { normal = { background = paleTexture } });
                GUI.Label(new Rect(panel.x + 55, y + 12, 360, 28), found ? definition.LocalizedName(ko) : (context.Text("ui_undiscovered_creature")), bodyStyle);
                GUI.Label(new Rect(panel.x + 55, y + 40, 470, 24), found ? definition.LocalizedDescription(ko) : (context.Text("ui_watch_the_ripples_and_time_of_day")), smallStyle);
                if (context.Save.inventory.displayedFish.Contains(definition.id)) GUI.Label(new Rect(panel.x + 490, y + 12, 90, 25), context.Text("ui_on_show"), smallStyle);
                y += 82;
            }
            if (GUI.Button(new Rect(panel.x + 35, height - 70, 570, 48), context.Text("ui_close_esc"), buttonStyle)) phases.Set(GamePhase.Explore);
        }
    }
}
