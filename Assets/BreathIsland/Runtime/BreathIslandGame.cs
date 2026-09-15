using UnityEngine;
using System.Collections.Generic;

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
        private GameSession session;
        private ExploreSystem explore;

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

            context = new GameContext(save, saveService, world, content, text => session.ShowMessage(text));
            context.RestorePosition();
            phases = new GamePhaseController(world);
            combat = new PredatorCombatSystem(context, phases);
            fishing = new FishingSystem(context, phases, combat.Begin);
            relationships = new RelationshipSystem(context, phases);
            facilities = new FacilitySystem(context);
            session = new GameSession(context, phases);
            explore = new ExploreSystem(context, phases, fishing, relationships);
            hud = new GameHud(phases, new Dictionary<GamePhase, HudScreen>
            {
                [GamePhase.Explore] = new ExploreHud(context, phases, relationships),
                [GamePhase.Fishing] = new FishingHud(context, phases, fishing),
                [GamePhase.CatchResolution] = new CatchResolutionHud(context, phases, fishing),
                [GamePhase.PredatorCombat] = new PredatorCombatHud(context, phases, combat),
                [GamePhase.Dialogue] = new DialogueHud(context, phases, relationships),
                [GamePhase.Facility] = new FacilityHud(context, phases, facilities),
                [GamePhase.Collection] = new CollectionHud(context, phases),
                [GamePhase.Pause] = new PauseHud(context, phases, session)
            }, new HudOverlay(context, phases, session));

            phases.Set(GamePhase.Explore);
            session.ShowMessage(context.Text("welcome"));
        }

        private void Update()
        {
            context.Positions.Tick();
            if (session.TickGlobalInput()) return;
            switch (phases.Current)
            {
                case GamePhase.Explore: explore.Tick(); break;
                case GamePhase.Fishing: fishing.Tick(); break;
                case GamePhase.PredatorCombat: combat.Tick(); break;
                case GamePhase.CatchResolution: fishing.TickResolutionInput(); break;
                case GamePhase.Dialogue: relationships.TickInput(); break;
                case GamePhase.Facility: facilities.TickInput(); break;
            }
        }

        private void OnGUI() => hud?.Draw();
        private void OnDestroy() => hud?.Dispose();
        private void OnApplicationQuit() { if (context != null) context.SaveTo(); }
    }
}
