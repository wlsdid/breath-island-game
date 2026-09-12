using UnityEngine;

namespace BreathIsland
{
    // Scene composition root. Gameplay rules live in feature modules under Runtime/*.
    public sealed class BreathIslandGame : MonoBehaviour
    {
        private GameContext context;
        private GamePhaseController phases;
        private FishingSystem fishing;
        private PredatorCombatSystem combat;
        private RelationshipSystem relationships;
        private FacilitySystem facilities;
        private GameHud hud;
        private string eventMessage = string.Empty;
        private float eventMessageUntil;

        public GamePhase Phase => phases?.Current ?? GamePhase.Explore;
        public GameSaveData SaveData => context?.Save;

        private void Awake()
        {
            name = "Breath Island Game";
            var saveService = new SaveService();
            var save = saveService.Load();
            var content = DefinitionCatalog.CreateVerticalSlice();
            WorldHandles world = ProceduralWorld.Build(transform);
            gameObject.AddComponent<ProceduralSeaAudio>();
            world.player.transform.position = new Vector3(save.player.x, Mathf.Max(1.2f, save.player.y), save.player.z == 0f ? 4f : save.player.z);

            context = new GameContext(save, saveService, world, content, ShowMessage);
            phases = new GamePhaseController(world);
            combat = new PredatorCombatSystem(context, phases);
            fishing = new FishingSystem(context, phases, combat.Begin);
            relationships = new RelationshipSystem(context, phases);
            facilities = new FacilitySystem(context);
            hud = new GameHud(context, phases, fishing, combat, relationships, facilities, () => eventMessage, () => eventMessageUntil);

            phases.Set(GamePhase.Explore);
            ShowMessage(context.Text("welcome"));
        }

        private void Update()
        {
            HandleGlobalInput();
            switch (phases.Current)
            {
                case GamePhase.Explore: UpdateExplore(); break;
                case GamePhase.Fishing: fishing.Tick(); break;
                case GamePhase.PredatorCombat: combat.Tick(); break;
                case GamePhase.CatchResolution: fishing.TickResolutionInput(); break;
                case GamePhase.Dialogue: relationships.TickInput(); break;
                case GamePhase.Facility: facilities.TickInput(); break;
            }
        }

        private void HandleGlobalInput()
        {
            if (GameInput.LanguagePressed()) { context.Korean = !context.Korean; ShowMessage(context.Text("language")); }
            if (GameInput.SavePressed()) { context.SaveTo(0); ShowMessage(context.Text("saved")); }
            if (GameInput.LoadPressed()) { context.LoadFrom(0); ShowMessage(context.Text("loaded")); }
            if (GameInput.CancelPressed()) phases.Cancel();
        }

        private void UpdateExplore()
        {
            if (GameInput.FacilityPressed()) phases.Set(GamePhase.Facility);
            if (GameInput.CollectionPressed()) phases.Set(GamePhase.Collection);
            if (GameInput.InteractPressed())
            {
                NpcId? nearby = relationships.NearbyNpc();
                if (nearby.HasValue) relationships.Open(nearby.Value);
            }
            if (GameInput.PrimaryPressed() && Vector3.Distance(context.World.player.transform.position, context.World.fishingPoint) < 7.5f) fishing.Start();
        }

        private void OnGUI() => hud?.Draw();

        private void OnApplicationQuit()
        {
            if (context != null) context.SaveTo();
        }

        private void ShowMessage(string text)
        {
            eventMessage = text;
            eventMessageUntil = Time.unscaledTime + 2.6f;
        }
    }
}
