using UnityEngine;
using UnityEngine.InputSystem;
namespace BreathIsland
{
    // Connects independent features and routes exploration inputs; each system owns its rules.
    public sealed class IslandLifeSession : System.IDisposable
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly IslandLifeWorld world;
        private readonly EquipmentView equipmentView;
        private readonly HousingView housingView;
        private readonly HomeSeatedView seatedView;
        private readonly InteriorFinishView finishView;
        public InteriorFinishSystem Finishes { get; }
        public IslandLifeCatalog Catalog { get; }
        public EquipmentSystem Equipment { get; }
        public JobMasterySystem Mastery { get; }
        public TrainingCombatSystem Training { get; }
        public LandActivitySystem Land { get; }
        public HousingSystem Housing { get; }
        public FurnitureCraftingSystem Crafting { get; }
        public ToolUseSystem Tools { get; }
        public HomeInteractionSystem Home { get; }
        public HomeStorageSystem Storage { get; }
        public IslandLifeSession(GameContext context, GamePhaseController phases, Transform parent)
        {
            this.context = context; this.phases = phases; Catalog = IslandLifeCatalog.Load();
            var achievements = new AchievementSystem(context, Catalog);
            Equipment = new EquipmentSystem(context, phases, Catalog, achievements);
            Mastery = new JobMasterySystem(context);
            world = new IslandLifeWorld(parent, Catalog);
            (context.World.player.GetComponent<CharacterPresentation>() ?? context.World.player.AddComponent<CharacterPresentation>()).Initialize(Catalog);
            Training = new TrainingCombatSystem(context, phases, Equipment, world.TrainingTarget, Mastery, achievements);
            Land = new LandActivitySystem(context, phases, Catalog, Equipment, achievements);
            Housing = new HousingSystem(context, phases, Catalog);
            Finishes = new InteriorFinishSystem(context, phases);
            finishView = new InteriorFinishView(Finishes, world.Interior);
            Crafting = new FurnitureCraftingSystem(context, phases, Catalog, Housing, Equipment, achievements);
            equipmentView = new EquipmentView(Equipment, Catalog, context.World.player.transform);
            Tools = new ToolUseSystem(context, phases, Equipment, Land, Catalog);
            Tools.Swing += equipmentView.Swing;
            housingView = new HousingView(Housing, Catalog, parent);
            Storage = new HomeStorageSystem(context, phases, Housing, Catalog);
            Home = new HomeInteractionSystem(context, phases, Housing, Catalog, Land, Storage);
            seatedView=context.World.player.AddComponent<HomeSeatedView>();seatedView.Initialize(Home, equipmentView);
            var feedback = new GameObject("Weapon Feedback"); feedback.transform.SetParent(parent, false);
            var feedbackView = feedback.AddComponent<CombatFeedback>(); feedbackView.Initialize(Catalog.weaponMaterial);
            Training.Hit += feedbackView.Show;
            Land.Changed += RefreshNodes;
            context.Loaded += Restore;
            Restore();
        }
        private void RefreshNodes() => world.RefreshNodes(Land);
        private void Restore() { StarterBedSystem.Ensure(context.Save.housing); Tools.Cancel(); Equipment.Restore(); Land.Restore(); Housing.Restore(); }
        public void Tick() { Tools.Tick(Time.deltaTime); }
        public void TickHome() => Home.Tick();
        public bool TickExplore()
        {
            Training.Tick(Time.deltaTime);
            var keys = Keyboard.current;
            if (!phases.FieldInputAllowed) return true;
            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                if (Equipment.Tool != LifeTool.None) Tools.Use(); else Training.Attack(false);
                return true;
            }
            if (keys?.rKey.wasPressedThisFrame == true) { Training.Attack(true); return true; }
            if (keys?.tKey.wasPressedThisFrame == true) { Training.ResetTarget(); return true; }
            if (GameInput.InteractPressed())
            {
                if (Housing.CanEnter) { Housing.Enter(); return true; }
                if (Land.Nearby != null && Land.Nearby.tool == LifeTool.None) { Land.Harvest(Land.Nearby.id); return true; }
            }
            return false;
        }
        public void OpenEquipment() => phases.OpenInventory();
        public void Dispose()
        {
            context.Loaded -= Restore; Land.Changed -= RefreshNodes;
            seatedView.Dispose(); Home.Dispose(); Storage.Dispose(); equipmentView.Dispose(); housingView.Dispose(); finishView.Dispose(); Finishes.Dispose(); world.Dispose();
        }
    }
}
