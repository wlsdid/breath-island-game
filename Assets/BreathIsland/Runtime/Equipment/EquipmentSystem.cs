using UnityEngine;
namespace BreathIsland
{
    public sealed class EquipmentSystem
    {
        private readonly GameContext context;
        private readonly IslandLifeCatalog catalog;
        private readonly GamePhaseController phases;
        private readonly AchievementSystem achievements;
        public event System.Action Changed;
        public EquipmentSystem(GameContext context, GamePhaseController phases, IslandLifeCatalog catalog, AchievementSystem achievements)
        { this.context = context; this.phases = phases; this.catalog = catalog; this.achievements = achievements; }
        public WeaponDefinition Weapon { get { var weapon = catalog.Weapon(context.Save.equipment.weaponId); return weapon != null && Unlocked(weapon) ? weapon : null; } }
        public bool Unlocked(WeaponDefinition weapon) => achievements.Unlocked(weapon);
        public JobDefinition Job => Weapon?.job;
        public LifeTool Tool => context.Save.equipment.tool;
        public WeaponDefinition[] Weapons => catalog.weapons;
        private bool CanChange => phases.Current == GamePhase.Explore || phases.Current == GamePhase.Equipment;
        public bool Equip(string id)
        {
            if (!CanChange || (!string.IsNullOrEmpty(id) && catalog.Weapon(id) == null)) return false;
            if (!string.IsNullOrEmpty(id) && !Unlocked(catalog.Weapon(id))) return false;
            context.Save.equipment.weaponId = id ?? "";
            context.Save.equipment.tool = LifeTool.None;
            Changed?.Invoke();
            context.SaveTo();
            return true;
        }
        public bool SelectTool(LifeTool tool)
        {
            if ((!CanChange && phases.Current != GamePhase.Home) || !System.Enum.IsDefined(typeof(LifeTool), tool)) return false;
            // A tool selection leaves the weapon/job intact; only the visible held item changes.
            context.Save.equipment.tool = tool;
            Changed?.Invoke();
            context.SaveTo();
            return true;
        }
        public void Restore() => Changed?.Invoke();
        public void ShowWeapon()
        {
            if (Tool == LifeTool.None) return;
            context.Save.equipment.tool = LifeTool.None;
            Changed?.Invoke();
        }
    }
}
