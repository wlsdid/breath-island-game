using System;
using System.Collections.Generic;

namespace BreathIsland
{
    public static class SaveMigration
    {
        // The asset/UI refactor does not change the v1 JSON schema or content IDs.
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
            data.version = GameSaveData.CurrentVersion;
            return data;
        }
    }
}
