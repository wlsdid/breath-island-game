using UnityEngine;

namespace BreathIsland
{
    public sealed class GamePhaseController
    {
        private readonly WorldHandles world;

        public GamePhaseController(WorldHandles world) => this.world = world;

        public GamePhase Current { get; private set; } = GamePhase.Explore;
        public GamePhase BeforePause { get; private set; } = GamePhase.Explore;

        public void Set(GamePhase next)
        {
            if (next == GamePhase.Pause && Current != GamePhase.Pause) BeforePause = Current;
            Current = next;
            world.motor.CanMove = next == GamePhase.Explore;
            world.cameraRig.firstPerson = next == GamePhase.Fishing || next == GamePhase.PredatorCombat;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Cancel()
        {
            if (Current == GamePhase.Explore)
            {
                BeforePause = Current;
                Set(GamePhase.Pause);
            }
            else if (Current == GamePhase.Pause) Set(BeforePause);
            else Set(GamePhase.Explore);
        }
    }
}
