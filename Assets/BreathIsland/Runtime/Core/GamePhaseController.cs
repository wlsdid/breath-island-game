using UnityEngine;

namespace BreathIsland
{
    public sealed class GamePhaseController
    {
        private readonly WorldHandles world;

        public GamePhaseController(WorldHandles world) => this.world = world;

        public GamePhase Current { get; private set; } = GamePhase.Explore;
        public GamePhase BeforePause { get; private set; } = GamePhase.Explore;
        public GamePhase BeforeInventory { get; private set; } = GamePhase.Explore;
        private int enteredFrame;
        public event System.Action Changed;
        public bool FieldInputAllowed => Current == GamePhase.Explore && Time.frameCount > enteredFrame + 1;
        public bool HomeInputAllowed => Current == GamePhase.Home && Time.frameCount > enteredFrame + 1;
        public static bool IsHome(GamePhase phase) => phase == GamePhase.Home || phase == GamePhase.HomeDecorate || phase == GamePhase.HomeStorage || phase == GamePhase.HomeRest;
        public void OpenInventory()
        {
            if (Current == GamePhase.HomeDecorate) Set(GamePhase.Home);
            if (Current != GamePhase.Explore && Current != GamePhase.Home) return;
            BeforeInventory = Current; Set(GamePhase.Equipment);
        }
        public void CloseInventory() { if (Current == GamePhase.Equipment) Set(BeforeInventory); }

        public void Set(GamePhase next)
        {
            if (next == GamePhase.Pause && Current != GamePhase.Pause) BeforePause = Current;
            Current = next;
            enteredFrame = Time.frameCount;
            world.motor.BlockActions();
            world.motor.CanMove = next == GamePhase.Explore || next == GamePhase.Home;
            world.cameraRig.firstPerson = next == GamePhase.Fishing || next == GamePhase.PredatorCombat;
            world.cameraRig.fishingView = next == GamePhase.Fishing;
            world.player.GetComponent<FirstPersonBodyView>()?.SetFirstPerson(world.cameraRig.firstPerson);
            world.cameraRig.homeView = IsHome(next) || (next == GamePhase.Equipment && IsHome(BeforeInventory)) || (next == GamePhase.Pause && IsHome(BeforePause));
            world.cameraRig.homeOverview = next == GamePhase.HomeDecorate || next == GamePhase.Equipment;
            world.cameraRig.allowLook = next == GamePhase.Explore;
            Cursor.lockState = next == GamePhase.Explore ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = next != GamePhase.Explore;
            Changed?.Invoke();
        }

        public void Cancel()
        {
            if (Current == GamePhase.HomeDecorate || Current == GamePhase.HomeStorage || Current == GamePhase.HomeRest) { Set(GamePhase.Home); return; }
            if (Current == GamePhase.Equipment) { CloseInventory(); return; }
            if (Current == GamePhase.Explore || Current == GamePhase.Home)
            {
                BeforePause = Current;
                Set(GamePhase.Pause);
            }
            else if (Current == GamePhase.Pause) Set(BeforePause);
            else Set(GamePhase.Explore);
        }
    }
}
