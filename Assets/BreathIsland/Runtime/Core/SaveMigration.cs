using System;
using System.Collections.Generic;

namespace BreathIsland
{
    public static class SaveMigration
    {
        // Additive migrations preserve legacy inventory, world, equipment and furniture fields.
        public static GameSaveData Apply(GameSaveData data)
        {
            if (data == null) throw new ArgumentException("Empty save data.");
            if (data.version > GameSaveData.CurrentVersion || data.version < 0)
                throw new NotSupportedException($"Unsupported save version: {data.version}");
            data.player ??= new PlayerProgress();
            data.inventory ??= new InventoryState();
            data.relationships ??= new RelationshipState();
            data.facilities ??= new FacilityState();
            data.world ??= new WorldState();
            data.inventory.discoveredFish ??= new List<string>();
            data.inventory.displayedFish ??= new List<string>();
            data.inventory.slotOrder ??= new List<string>();
            data.equipment ??= new EquipmentState();
            data.equipment.weaponId ??= "";
            data.equipment.mastery ??= new List<JobMastery>();
            data.equipment.achievements ??= new List<string>();
            data.resources ??= new LifeResources();
            data.resources.harvested ??= new List<NodeHarvest>();
            data.housing ??= new HousingState();
            data.housing.structure ??= new HomeStructureState();
            data.housing.structure.cells ??= new List<FloorCell>();
            data.housing.structure.walls ??= new List<WallEdge>();
            data.housing.placed ??= new List<PlacedFurniture>();
            data.housing.stock ??= new List<FurnitureStock>();
            data.housing.chests ??= new List<ChestState>();
            foreach (var chest in data.housing.chests)
            {
                chest.slots ??= new List<StorageSlot>();
                while (chest.slots.Count < 16) chest.slots.Add(new StorageSlot());
                for (int i = 0; i < chest.slots.Count; i++) chest.slots[i] ??= new StorageSlot();
            }
            if (data.version < 3 || string.IsNullOrEmpty(data.housing.wallFinishId)) data.housing.wallFinishId = "wood_dark";
            if (data.version < 3 || string.IsNullOrEmpty(data.housing.floorFinishId)) data.housing.floorFinishId = "planks_light";
            data.version = GameSaveData.CurrentVersion;
            return data;
        }
    }
}
