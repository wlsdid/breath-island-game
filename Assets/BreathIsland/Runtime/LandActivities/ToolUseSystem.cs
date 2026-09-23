using System;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    // A swing may miss. Only the impact, never the animation/UI, awards resources.
    public sealed class ToolUseSystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly EquipmentSystem equipment;
        private readonly LandActivitySystem land;
        private readonly IslandLifeCatalog catalog;
        private float remaining;
        private bool impacted;
        private LifeTool startedTool;
        public event Action Swing;
        public bool Busy => remaining > 0;
        public ToolUseSystem(GameContext context, GamePhaseController phases, EquipmentSystem equipment, LandActivitySystem land, IslandLifeCatalog catalog)
        { this.context = context; this.phases = phases; this.equipment = equipment; this.land = land; this.catalog = catalog; }
        public bool Use()
        {
            if (phases.Current != GamePhase.Explore || equipment.Tool == LifeTool.None || Busy) return false;
            startedTool = equipment.Tool; remaining = .5f; impacted = false; Swing?.Invoke(); return true;
        }
        public void Cancel() { remaining = 0; impacted = true; }
        public void Tick(float delta)
        {
            if (!Busy) return;
            if (phases.Current != GamePhase.Explore || equipment.Tool != startedTool) { Cancel(); return; }
            remaining = Mathf.Max(0, remaining - delta);
            if (!impacted && remaining <= .25f)
            {
                impacted = true;
                var player = context.World.player.transform;
                var node = catalog.nodes.Where(n => n.tool != LifeTool.None && !land.Depleted(n.id))
                    .Where(n => InReach(player.position, player.forward, n.position))
                    .OrderBy(n => Vector3.Distance(player.position, n.position)).FirstOrDefault();
                if (node != null) land.Harvest(node.id);
            }
        }
        public static bool InReach(Vector3 origin, Vector3 forward, Vector3 target)
        {
            Vector3 difference = target - origin;
            if (difference.magnitude >= 2.3f) return false;
            Vector3 flat = Vector3.ProjectOnPlane(difference, Vector3.up);
            return flat.sqrMagnitude < .01f || Vector3.Dot(forward.normalized, flat.normalized) >= .65f;
        }
    }
}
