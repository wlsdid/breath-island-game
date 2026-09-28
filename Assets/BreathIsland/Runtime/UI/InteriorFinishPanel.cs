using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    internal static class InteriorFinishPanel
    {
        public static void Draw(NotebookSurface ui, GameContext context, InteriorFinishSystem finishes)
        {
            ui.Panel(new Rect(30, 105, 360, 690));
            ui.Label(new Rect(50, 120, 320, 60), context.Text("finish_title"), NotebookStyle.Title);
            ui.Label(new Rect(50, 185, 320, 56), context.Text("finish_preview_help"), NotebookStyle.Small);
            int row = 0;
            foreach (var surface in new[] { FinishSurface.Wall, FinishSurface.Floor })
            {
                ui.Label(new Rect(50, 250 + row * 47, 320, 38), context.Text(surface == FinishSurface.Wall ? "finish_wall" : "finish_floor"), NotebookStyle.Body); row++;
                foreach (var definition in finishes.Definitions.Where(d => d.surface == surface).OrderBy(d => d.id))
                {
                    bool selected = definition.id == (surface == FinishSurface.Wall ? finishes.WallId : finishes.FloorId);
                    ui.Button(new Rect(50, 250 + row * 47, 320, 40), context.Text(definition.nameKey), () => finishes.Preview(definition.id), selected ? NotebookStyle.Selected : NotebookStyle.Button);
                    row++;
                }
            }
            ui.Button(new Rect(50, 652, 320, 50), context.Text("finish_apply"), () => finishes.Apply(), NotebookStyle.Primary);
            ui.Button(new Rect(50, 719, 320, 45), context.Text("finish_cancel"), () => finishes.Cancel());
        }
    }
}
