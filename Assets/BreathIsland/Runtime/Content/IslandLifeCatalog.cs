using System;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Island Life Catalog")]
    public sealed class IslandLifeCatalog : ScriptableObject
    {
        public WeaponDefinition[] weapons;
        public FurnitureDefinition[] furniture;
        public ResourceNodeDefinition[] nodes;
        public AchievementDefinition[] achievements;
        public GameObject axeModel, pickaxeModel, hammerModel, characterModel;
        public RuntimeAnimatorController animator;
        public Avatar characterAvatar;
        public Material furnitureMaterial, weaponMaterial, toolMaterial, natureMaterial, characterMaterial;
        public WeaponDefinition Weapon(string id) => weapons.FirstOrDefault(w => w.id == id);
        public FurnitureDefinition Furniture(string id) => furniture.FirstOrDefault(f => f.id == id);
        public static IslandLifeCatalog Load() => Resources.Load<IslandLifeCatalog>("Content/IslandLife")
            ?? throw new InvalidOperationException("Missing IslandLife content asset.");
    }
}
