using UnityEngine;

namespace BreathIsland
{
    public sealed class PredatorCombatSystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;

        public PredatorCombatSystem(GameContext context, GamePhaseController phases)
        {
            this.context = context;
            this.phases = phases;
        }

        public int Health { get; private set; }
        public int PlayerHearts { get; private set; }
        public float Charge { get; private set; }

        public void Begin()
        {
            Health = 100;
            PlayerHearts = 3;
            Charge = 12f;
            phases.Set(GamePhase.PredatorCombat);
            context.Message(context.Text("predator"));
        }

        public void Tick()
        {
            if (GameInput.PadSouthPressed()) Act(0);
            if (GameInput.PadWestPressed()) Act(1);
            if (GameInput.PadNorthPressed()) Act(2);
            Charge += Time.deltaTime / 3.4f * 100f;
            if (Charge < 100f) return;
            Charge = 0f;
            PlayerHearts--;
            context.Message(context.Text("hit"));
            if (PlayerHearts > 0) return;
            context.Save.inventory.bait = Mathf.Max(0, context.Save.inventory.bait - 1);
            context.World.player.transform.position = new Vector3(0f, 1.2f, 8f);
            context.SaveTo();
            phases.Set(GamePhase.Explore);
            context.Message(context.Text("washed_ashore"));
        }

        public void Act(int action)
        {
            if (action == 0)
            {
                Charge = Mathf.Max(0f, Charge - 68f);
                context.Message(context.Text("dodged"));
                return;
            }
            int damage = action == 1 ? 18 : 28;
            Health = Mathf.Max(0, Health - damage);
            context.Message(action == 1 ? context.Text("net") : context.Text("shock"));
            if (Health == 0) phases.Set(GamePhase.CatchResolution);
        }
    }
}
