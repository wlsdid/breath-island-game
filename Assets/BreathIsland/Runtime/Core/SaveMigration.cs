using System;
using System.Collections.Generic;

namespace BreathIsland
{
    public static class SaveMigration
    {
        // v2 only adds equipment, land resources and housing; legacy values are retained.
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
            data.equipment ??= new EquipmentState();
            data.equipment.weaponId ??= "";
            data.equipment.mastery ??= new List<JobMastery>();
            data.equipment.achievements ??= new List<string>();
            data.resources ??= new LifeResources();
            data.resources.harvested ??= new List<NodeHarvest>();
            data.housing ??= new HousingState();
            data.housing.placed ??= new List<PlacedFurniture>();
            data.housing.stock ??= new List<FurnitureStock>();
            data.version = GameSaveData.CurrentVersion;
            return data;
        }
    }
}
