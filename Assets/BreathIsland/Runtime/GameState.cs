using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreathIsland
{
    [Serializable]
    public sealed class PlayerProgress
    {
        public float x;
        public float y = 1f;
        public float z = 4f;
        public int hour = 9;
        public bool boatUnlocked;
        public bool endingSeen;
    }

    [Serializable]
    public sealed class InventoryState
    {
        public int coins = 72;
        public int materials = 4;
        public int bait = 8;
        public List<string> discoveredFish = new();
        public List<string> displayedFish = new();
        public int totalCatchCount;
    }

    [Serializable]
    public sealed class RelationshipState
    {
        [Range(0f, 3f)] public float nari;
        [Range(0f, 3f)] public float maru;
        public float Get(NpcId id) => id == NpcId.Nari ? nari : maru;
        public void Add(NpcId id, float amount)
        {
            if (id == NpcId.Nari) nari = Mathf.Clamp(nari + amount, 0f, 3f);
            else maru = Mathf.Clamp(maru + amount, 0f, 3f);
        }
    }

    [Serializable]
    public sealed class FacilityState
    {
        [Range(0, 2)] public int boatyard;
        [Range(0, 2)] public int aquarium;
        public int Get(FacilityId id) => id == FacilityId.Boatyard ? boatyard : aquarium;
        public void Set(FacilityId id, int value)
        {
            if (id == FacilityId.Boatyard) boatyard = Mathf.Clamp(value, 0, 2);
            else aquarium = Mathf.Clamp(value, 0, 2);
        }
    }

    [Serializable]
    public sealed class WorldState
    {
        public string weather = "Clear";
        public int day = 1;
        public bool coralRouteUnlocked;
    }

    [Serializable]
    public sealed class GameSaveData
    {
        public int version = CurrentVersion;
        public long savedAtUtcTicks;
        public PlayerProgress player = new();
        public InventoryState inventory = new();
        public RelationshipState relationships = new();
        public FacilityState facilities = new();
        public WorldState world = new();
        public EquipmentState equipment = new();
        public LifeResources resources = new();
        public HousingState housing = new();
        public const int CurrentVersion = 3;

        public static GameSaveData CreateDefault() => new() { savedAtUtcTicks = DateTime.UtcNow.Ticks };
    }

    public static class GameEconomy
    {
        public static void ApplyCatch(GameSaveData save, FishDefinition fish, CatchDisposition disposition)
        {
            if (!save.inventory.discoveredFish.Contains(fish.id)) save.inventory.discoveredFish.Add(fish.id);
            if (disposition == CatchDisposition.Sell) save.inventory.coins += fish.coinValue;
            if (disposition == CatchDisposition.Material) save.inventory.materials += fish.materialValue;
            if (disposition == CatchDisposition.Display && !save.inventory.displayedFish.Contains(fish.id)) save.inventory.displayedFish.Add(fish.id);
            save.inventory.totalCatchCount++;
        }

        public static bool TryUpgrade(GameSaveData save, FacilityDefinition definition)
        {
            int level = save.facilities.Get(definition.id);
            if (level >= 2) return false;
            int coinCost = definition.coinCosts[level];
            int materialCost = definition.materialCosts[level];
            if (save.inventory.coins < coinCost || save.inventory.materials < materialCost) return false;
            save.inventory.coins -= coinCost;
            save.inventory.materials -= materialCost;
            save.facilities.Set(definition.id, level + 1);
            if (definition.id == FacilityId.Boatyard && level == 0)
            {
                save.player.boatUnlocked = true;
                save.world.coralRouteUnlocked = true;
            }
            return true;
        }
    }

    public static class FishingMath
    {
        public static float PassiveTension(float current, float pullStrength, float deltaTime) => Mathf.Clamp(current + pullStrength * 1.08f * deltaTime, 0f, 100f);
        public static float PassiveProgress(float current, float deltaTime) => Mathf.Max(0f, current - 2.2f * deltaTime);
        public static void Reel(ref float progress, ref float tension)
        {
            progress = Mathf.Clamp(progress + 12.5f, 0f, 100f);
            tension = Mathf.Clamp(tension + 10f, 0f, 100f);
        }
        public static void Ease(ref float tension) => tension = Mathf.Max(12f, tension - 19f);
    }
}

