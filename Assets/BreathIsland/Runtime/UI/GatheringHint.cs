using UnityEngine;
namespace BreathIsland
{
    // Read-only presentation: the impact/hold systems remain the reward authority.
    public static class GatheringHint
    {
        public static string Text(GameContext context, IslandLifeSession life, ResourceNodeDefinition node)
        {
            if (life.Land.Depleted(node.id)) return context.Text("life_depleted");
            var player = context.World.player.transform;
            if (!ToolUseSystem.InReach(player.position, player.forward, node.position))
                return context.Text("gather_face_target");
            if (node.tool != LifeTool.None && life.Equipment.Tool != node.tool)
                return string.Format(context.Text("life_need_tool"), context.Text("life_tool_" + node.tool));
            return string.Format(context.Text(node.tool == LifeTool.None ? "bag_gather_hold" : "bag_tool_hint"),
                context.Text(node.nameKey), context.Text("life_tool_" + node.tool));
        }
        public static string Progress(GameContext context, LandActivitySystem land, ResourceNodeDefinition node)
        {
            float progress = land.Progress(node);
            return node.tool == LifeTool.None
                ? string.Format(context.Text("gather_seconds"), Mathf.Max(0, node.gatherSeconds * (1 - progress)))
                : string.Format(context.Text("gather_hits"), Mathf.RoundToInt(progress * node.requiredHits), node.requiredHits);
        }
    }
}
