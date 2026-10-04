using UnityEngine;

namespace BreathIsland
{
    public sealed class ExploreSystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly FishingSystem fishing;
        private readonly RelationshipSystem relationships;
        public ExploreSystem(GameContext context, GamePhaseController phases, FishingSystem fishing, RelationshipSystem relationships)
        { this.context = context; this.phases = phases; this.fishing = fishing; this.relationships = relationships; }
        public void Tick()
        {
            if (GameInput.FacilityPressed()) { phases.Set(GamePhase.Facility); return; }
            if (GameInput.CollectionPressed()) { phases.Set(GamePhase.Collection); return; }
            if (GameInput.InteractPressed())
            {
                var nearby = relationships.NearbyNpc();
                if (nearby.HasValue) { relationships.Open(nearby.Value); return; }
            }
            if (GameInput.FishingPressed() && Vector3.Distance(context.World.player.transform.position, context.World.fishingPoint) < 7.5f) fishing.Start();
        }
    }
}
