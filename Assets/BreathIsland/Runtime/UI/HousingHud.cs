using UnityEngine;
namespace BreathIsland
{
    public sealed class HousingHud : HudScreen
    {
        private readonly IslandLifeSession life;
        private Vector2 scroll;
        public HousingHud(GameContext context, GamePhaseController phases, IslandLifeSession life) : base(context, phases) => this.life = life;
        public override void Draw(float width, float height)
        {
            EnsureStyles(); var housing = life.Housing;
            GUI.Box(new Rect(20, 110, 345, height - 135), "", panelStyle);
            GUI.Label(new Rect(35, 126, 320, 40), context.Text("life_home"), titleStyle);
            var r = context.Save.resources;
            GUI.Label(new Rect(35, 173, 320, 54), string.Format(context.Text("life_resources"), r.wood, r.stone, r.herbs), bodyStyle);
            if (GUI.Button(new Rect(35, 230, 310, 40), context.Text("life_select_hammer"), life.Equipment.Tool == LifeTool.Hammer ? primaryButtonStyle : buttonStyle)) life.Equipment.SelectTool(LifeTool.Hammer);
            for (int i = 0; i < housing.Furniture.Length; i++)
            {
                var definition = housing.Furniture[i]; float y = 286 + i * 99;
                GUI.Label(new Rect(35, y, 315, 34), string.Format(context.Text("life_furniture_stock"), context.Text(definition.nameKey), housing.Available(definition.id)), bodyStyle);
                if (GUI.Button(new Rect(35, y + 37, 182, 46), string.Format(context.Text("life_craft"), definition.woodCost, definition.stoneCost), new GUIStyle(buttonStyle) { fontSize = 14, wordWrap = true })) life.Crafting.Craft(definition.id);
                GUI.enabled = housing.Available(definition.id) > 0;
                if (GUI.Button(new Rect(225, y + 37, 120, 46), context.Text("life_select"), primaryButtonStyle)) housing.Select(definition.id);
                GUI.enabled = true;
            }
            if (GUI.Button(new Rect(35, height - 123, 310, 40), context.Text("life_rest"), buttonStyle)) life.Land.Rest();
            if (GUI.Button(new Rect(35, height - 73, 310, 40), context.Text("life_exit"), primaryButtonStyle)) housing.Exit();
            DrawPlacement(width, height);
        }
        private void DrawPlacement(float width, float height)
        {
            var housing = life.Housing; float x = width - 365;
            GUI.Box(new Rect(x, 110, 345, height - 135), "", panelStyle);
            GUI.Label(new Rect(x + 18, 128, 312, 68), context.Text("life_grid_help"), bodyStyle);
            for (int z = 0; z < 8; z++) for (int column = 0; column < 8; column++)
            {
                Color previous = GUI.backgroundColor;
                bool marked = housing.Selection != null && FurniturePlacement.Area(housing.Selection, housing.X, housing.Z, housing.Turns).Contains(new Vector2(column + .5f, z + .5f));
                if (marked) GUI.backgroundColor = housing.IsValid ? Color.green : Color.red;
                if (GUI.Button(new Rect(x + 20 + column * 38, 205 + (7 - z) * 38, 35, 35), (column + 1) + "," + (z + 1))) housing.Move(column, z);
                GUI.backgroundColor = previous;
            }
            if (GUI.Button(new Rect(x + 20, 520, 145, 38), context.Text("life_rotate"), buttonStyle)) housing.Rotate();
            if (GUI.Button(new Rect(x + 175, 520, 145, 38), context.Text("life_cancel"), buttonStyle)) housing.Cancel();
            GUI.enabled = housing.IsValid;
            if (GUI.Button(new Rect(x + 20, 565, 300, 42), context.Text("life_place"), primaryButtonStyle)) housing.Place();
            GUI.enabled = true;
            GUI.Label(new Rect(x + 20, 615, 305, 35), context.Text("life_recall_help"), smallStyle);
            scroll = GUI.BeginScrollView(new Rect(x + 20, 655, 305, Mathf.Max(80, height - 695)), scroll, new Rect(0, 0, 280, housing.State.placed.Count * 45));
            // Stop iterating after a command that mutates the collection.
            for (int i = 0; i < housing.State.placed.Count; i++)
            {
                var item = housing.State.placed[i]; var definition = life.Catalog.Furniture(item.furnitureId);
                if (GUI.Button(new Rect(0, i * 45, 275, 38), context.Text(definition?.nameKey ?? "life_unknown"), buttonStyle)) { housing.Recall(item.instanceId); break; }
            }
            GUI.EndScrollView();
        }
    }
}
