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
            ui.Panel(panel, string.Empty, panelStyle);
            ui.Label(new Rect(panel.x + 30, panel.y + 25, 540, 38), context.Text("ui_new_discovery"), centeredStyle);
            ui.Label(new Rect(panel.x + 30, panel.y + 70, 540, 52), fish.LocalizedName(ko), titleStyle);
            ui.Label(new Rect(panel.x + 60, panel.y + 130, 480, 85), fish.LocalizedDescription(ko), centeredStyle);
            string rarity = context.Text("rarity_" + fish.rarity);
            ui.Label(new Rect(panel.x + 60, panel.y + 218, 480, 36), rarity, centeredStyle);
            const int columnWidth = 255;
            if (!fish.IsPredator) ui.Button(new Rect(panel.x + 35, panel.y + 290, columnWidth, 68), string.Format(context.Text("ui_sell_0"), fish.coinValue), () => fishing.Resolve(CatchDisposition.Sell), buttonStyle);
            ui.Button(new Rect(panel.x + 310, panel.y + 290, columnWidth, 68), context.Text("ui_record"), () => fishing.Resolve(CatchDisposition.Record), buttonStyle);
            ui.Button(new Rect(panel.x + 35, panel.y + 378, columnWidth, 68), context.Text("ui_display"), () => fishing.Resolve(CatchDisposition.Display), buttonStyle);
            ui.Button(new Rect(panel.x + 310, panel.y + 378, columnWidth, 68), string.Format(context.Text("ui_material_0"), fish.materialValue), () => fishing.Resolve(CatchDisposition.Material), primaryButtonStyle);
        }
    }
}
