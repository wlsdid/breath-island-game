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
        private IslandLifeSession life;

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
            gameObject.AddComponent<FishingPresentation>().Initialize(world, fishing, phases);
            relationships = new RelationshipSystem(context, phases);
            facilities = new FacilitySystem(context);
            session = new GameSession(context, phases);
            life = new IslandLifeSession(context, phases, transform);
            explore = new ExploreSystem(context, phases, fishing, relationships);
            hud = new GameHud(phases, new Dictionary<GamePhase, HudScreen>
            {
                [GamePhase.Explore] = new ExploreHud(context, phases, relationships, life),
                [GamePhase.Fishing] = new FishingHud(context, phases, fishing),
                [GamePhase.CatchResolution] = new CatchResolutionHud(context, phases, fishing),
                [GamePhase.PredatorCombat] = new PredatorCombatHud(context, phases, combat),
                [GamePhase.Dialogue] = new DialogueHud(context, phases, relationships),
                [GamePhase.Facility] = new FacilityHud(context, phases, facilities),
                [GamePhase.Collection] = new CollectionHud(context, phases),
                [GamePhase.Pause] = new PauseHud(context, phases, session),
                [GamePhase.Equipment] = new EquipmentHud(context, phases, life),
                [GamePhase.Home] = new HousingHud(context, phases, life),
                [GamePhase.HomeDecorate] = new HomeDecorateHud(context, phases, life),
                [GamePhase.HomeStorage] = new HomeStorageHud(context, phases, life.Storage),
                [GamePhase.HomeRest] = new HomeRestHud(context, phases, life.Home)
            }, new HudOverlay(context, phases, session));

            phases.Set(context.Save.housing.inside ? GamePhase.Home : GamePhase.Explore);
            session.ShowMessage(context.Text("welcome"));
        }

        private void Update()
        {
            context.Positions.Tick();
            bool handled = session.TickGlobalInput();
            life.Tick();
            if (handled) return;
            switch (phases.Current)
            {
                case GamePhase.Explore: if (!life.TickExplore()) explore.Tick(); break;
                case GamePhase.Home: life.TickHome(); break;
                case GamePhase.Fishing: fishing.Tick(); break;
                case GamePhase.PredatorCombat: combat.Tick(); break;
                case GamePhase.CatchResolution: fishing.TickResolutionInput(); break;
                case GamePhase.Dialogue: relationships.TickInput(); break;
                case GamePhase.Facility: facilities.TickInput(); break;
            }
        }

        private void LateUpdate() => hud?.Draw();
        private void OnDestroy() { hud?.Dispose(); life?.Dispose(); }
        private void OnApplicationQuit() { if (context != null) context.SaveTo(); }
    }
}
