using UnityEngine;
namespace BreathIsland
{
    public sealed class CatchResolutionHud : HudScreen
    {
        private readonly FishingSystem fishing;
        public CatchResolutionHud(GameContext context, GamePhaseController phases, FishingSystem fishing) : base(context, phases) { this.fishing = fishing; }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            FishDefinition fish = fishing.ActiveFish;
            Rect panel = new(width / 2f - 300f, height / 2f - 285f, 600f, 570f);
            GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 25, 540, 38), context.Text("ui_new_discovery"), centeredStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 70, 540, 52), fish.LocalizedName(ko), titleStyle);
            GUI.Label(new Rect(panel.x + 60, panel.y + 130, 480, 85), fish.LocalizedDescription(ko), centeredStyle);
            string rarity = context.Text("rarity_" + fish.rarity);
            GUI.Label(new Rect(panel.x + 60, panel.y + 218, 480, 36), rarity, centeredStyle);
            const int columnWidth = 255;
            if (!fish.IsPredator && GUI.Button(new Rect(panel.x + 35, panel.y + 290, columnWidth, 68), string.Format(context.Text("ui_sell_0"), fish.coinValue), buttonStyle)) fishing.Resolve(CatchDisposition.Sell);
            if (GUI.Button(new Rect(panel.x + 310, panel.y + 290, columnWidth, 68), context.Text("ui_record"), buttonStyle)) fishing.Resolve(CatchDisposition.Record);
            if (GUI.Button(new Rect(panel.x + 35, panel.y + 378, columnWidth, 68), context.Text("ui_display"), buttonStyle)) fishing.Resolve(CatchDisposition.Display);
            if (GUI.Button(new Rect(panel.x + 310, panel.y + 378, columnWidth, 68), string.Format(context.Text("ui_material_0"), fish.materialValue), primaryButtonStyle)) fishing.Resolve(CatchDisposition.Material);
        }
    }
}
