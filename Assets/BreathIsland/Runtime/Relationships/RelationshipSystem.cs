using UnityEngine;

namespace BreathIsland
{
    public sealed class RelationshipSystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;

        public RelationshipSystem(GameContext context, GamePhaseController phases)
        {
            this.context = context;
            this.phases = phases;
        }

        public NpcId ActiveNpc { get; private set; }
        public int DialogueIndex { get; private set; }

        public NpcId? NearbyNpc()
        {
            foreach (var pair in context.World.npcs)
                if (Vector3.Distance(context.World.player.transform.position, pair.Value.position) < 3.3f) return pair.Key;
            return null;
        }

        public void Open(NpcId id)
        {
            ActiveNpc = id;
            DialogueIndex = 0;
            phases.Set(GamePhase.Dialogue);
        }

        public void TickInput()
        {
            if (GameInput.PadSouthPressed()) Talk();
            else if (GameInput.PadWestPressed()) GiveGift();
        }

        public void Talk()
        {
            DialogueIndex++;
            context.Save.relationships.Add(ActiveNpc, .25f);
            context.SaveTo();
        }

        public void GiveGift()
        {
            if (context.Save.inventory.coins < 12) { context.Message(context.Text("gift_fail")); return; }
            context.Save.inventory.coins -= 12;
            context.Save.relationships.Add(ActiveNpc, 1f);
            context.SaveTo();
            context.Message(context.Text("gift_ok"));
        }
    }
}
