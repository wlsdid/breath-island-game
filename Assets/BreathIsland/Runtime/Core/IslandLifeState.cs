using System;
using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    [Serializable]
    public sealed class EquipmentState
    {
        public string weaponId = "";
        public LifeTool tool;
        public List<JobMastery> mastery = new();
        public List<string> achievements = new();
    }
    [Serializable]
    public sealed class JobMastery { public string jobId; public int hits; }
    [Serializable]
    public sealed class FurnitureStock { public string id; public int count; }
    [Serializable]
    public sealed class LifeResources
    {
        public int wood, stone, herbs;
        public int totalWood, totalStone, totalHerbs, craftedFurniture;
        public int woodHarvests, stoneHarvests, herbHarvests;
        public List<NodeHarvest> harvested = new();
        public void Add(LifeResource type, int count)
        {
            if (type == LifeResource.Wood) { wood += count; totalWood += count; woodHarvests++; }
            else if (type == LifeResource.Stone) { stone += count; totalStone += count; stoneHarvests++; }
            else { herbs += count; totalHerbs += count; herbHarvests++; }
        }
    }
    [Serializable]
    public sealed class NodeHarvest { public string id; public int day; }
    [Serializable]
    public sealed class PlacedFurniture
    {
        public string instanceId;
        public string furnitureId;
        public int x, z, turns;
    }
    [Serializable]
    public sealed class HousingState
    {
        public bool inside;
        public Vector3 outdoorPosition;
        public List<PlacedFurniture> placed = new();
        public List<FurnitureStock> stock = new();
    }
}
