namespace BreathIsland
{
    public sealed class FurnitureCraftingSystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly IslandLifeCatalog catalog;
        private readonly HousingSystem housing;
        private readonly EquipmentSystem equipment;
        private readonly AchievementSystem achievements;
        public FurnitureCraftingSystem(GameContext context, GamePhaseController phases, IslandLifeCatalog catalog, HousingSystem housing, EquipmentSystem equipment, AchievementSystem achievements)
        { this.context = context; this.phases = phases; this.catalog = catalog; this.housing = housing; this.equipment = equipment; this.achievements = achievements; }
        public bool Craft(string id)
        {
            if (phases.Current != GamePhase.Home && phases.Current != GamePhase.HomeDecorate && !(phases.Current == GamePhase.Equipment && phases.BeforeInventory == GamePhase.Home && context.Save.housing.inside)) return false;
            var furniture = catalog.Furniture(id);
            if (furniture == null) return false;
            if (equipment.Tool != LifeTool.Hammer) { context.Message(context.Text("life_craft_hammer")); return false; }
            var resources = context.Save.resources;
            if (resources.wood < furniture.woodCost || resources.stone < furniture.stoneCost)
            { context.Message(context.Text("life_craft_materials")); return false; }
            resources.wood -= furniture.woodCost; resources.stone -= furniture.stoneCost;
            resources.craftedFurniture++; housing.Stock(id).count++;
            achievements.Evaluate(); context.SaveTo(); return true;
        }
    }
}
