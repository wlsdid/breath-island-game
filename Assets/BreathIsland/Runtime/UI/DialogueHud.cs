using UnityEngine;
namespace BreathIsland
{
    public sealed class DialogueHud : HudScreen
    {
        private readonly RelationshipSystem relationships;
        public DialogueHud(GameContext context, GamePhaseController phases, RelationshipSystem relationships) : base(context, phases) { this.relationships = relationships; }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            Rect panel = new(120, height - 300, width - 240, 245); ui.Panel(panel, string.Empty, panelStyle);
            NpcDefinition npc = context.Content.Npcs[relationships.ActiveNpc];
            ui.Label(new Rect(panel.x + 30, panel.y + 24, 350, 42), npc.LocalizedName(ko), titleStyle);
            ui.Label(new Rect(panel.x + 30, panel.y + 64, 350, 28), npc.LocalizedRole(ko), smallStyle);
            ui.Label(new Rect(panel.x + 30, panel.y + 106, panel.width - 60, 55), $"“{npc.LocalizedLine(ko, relationships.DialogueIndex)}”", bodyStyle);
            ui.Icon(new Rect(panel.x + panel.width - 230, panel.y + 30, 32, 32), "heart");
            ui.Label(new Rect(panel.x + panel.width - 187, panel.y + 30, 147, 35), $"{context.Save.relationships.Get(relationships.ActiveNpc):0.0} / 3", bodyStyle);
            ui.Button(new Rect(panel.x + panel.width - 430, panel.y + 177, 180, 48), context.Text("ui_gift_12_coins"), () => relationships.GiveGift(), buttonStyle);
            ui.Button(new Rect(panel.x + panel.width - 230, panel.y + 177, 200, 48), context.Text("ui_keep_talking"), () => relationships.Talk(), primaryButtonStyle);
        }
    }
}
