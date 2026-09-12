using UnityEngine;

namespace BreathIsland
{
    public sealed class FacilitySystem
    {
        private readonly GameContext context;

        public FacilitySystem(GameContext context) => this.context = context;

        public int SelectedIndex { get; private set; }

        public void TickInput()
        {
            int direction = GameInput.PadVerticalPressed();
            if (direction != 0) SelectedIndex = Mathf.Clamp(SelectedIndex + direction, 0, 1);
            if (GameInput.PadSouthPressed()) Upgrade((FacilityId)SelectedIndex);
        }

        public void Upgrade(FacilityId id)
        {
            if (!GameEconomy.TryUpgrade(context.Save, context.Content.Facilities[id]))
            {
                context.Message(context.Text("upgrade_fail"));
                return;
            }
            context.SaveTo();
            bool boatUnlocked = id == FacilityId.Boatyard && context.Save.facilities.boatyard == 1;
            context.Message(context.Text(boatUnlocked ? "boat_unlocked" : "upgrade_ok"));
        }
    }
}
