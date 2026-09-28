using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Tests
{
    public sealed class IslandLifeRulesTests
    {
        private GameObject root;
        private string directory;
        private GameContext context;
        private GamePhaseController phases;
        private IslandLifeCatalog catalog;
        private SaveService saves;
        private EquipmentSystem equipment;
        private AchievementSystem achievements;
        private JobMasterySystem mastery;
        private TrainingCombatSystem combat;
        private LandActivitySystem land;
        private HousingSystem housing;
        private FurnitureCraftingSystem crafting;
        [SetUp] public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "breath-life-" + Guid.NewGuid().ToString("N")); saves = new SaveService(directory);
            root = new GameObject("Test player", typeof(CharacterController), typeof(ThirdPersonMotor), typeof(IslandCameraRig));
            var world = new WorldHandles { player = root, motor = root.GetComponent<ThirdPersonMotor>(), cameraRig = root.GetComponent<IslandCameraRig>() };
            context = new GameContext(GameSaveData.CreateDefault(), saves, world, DefinitionCatalog.CreateVerticalSlice(), _ => { });
            phases = new GamePhaseController(world); catalog = IslandLifeCatalog.Load();
            achievements = new AchievementSystem(context, catalog); equipment = new EquipmentSystem(context, phases, catalog, achievements);
            mastery = new JobMasterySystem(context); combat = new TrainingCombatSystem(context, phases, equipment, root.transform, mastery, achievements);
            land = new LandActivitySystem(context, phases, catalog, equipment, achievements); housing = new HousingSystem(context, phases, catalog);
            crafting = new FurnitureCraftingSystem(context, phases, catalog, housing, equipment, achievements);
        }
        [TearDown] public void TearDown() { UnityEngine.Object.DestroyImmediate(root); Directory.Delete(directory, true); }
        [Test] public void AuthoredAssetsAndBothLanguagesContainAllFeatureDefinitions()
        {
            Assert.That(catalog.weapons.Length, Is.EqualTo(6)); Assert.That(catalog.furniture.Length, Is.EqualTo(8));
            foreach (var w in catalog.weapons)
            {
                Assert.That(AssetDatabase.Contains(w), Is.True); Assert.That(AssetDatabase.Contains(w.job), Is.True);
                Assert.That(AssetDatabase.Contains(w.basicAttack), Is.True); Assert.That(w.model, Is.Not.Null);
                Assert.That(w.skill != null, Is.EqualTo(!string.IsNullOrEmpty(w.achievementId)));
                if (w.skill != null) Assert.That(AssetDatabase.Contains(w.skill), Is.True);
            }
            var keys = catalog.weapons.SelectMany(w => new[] { w.nameKey, w.job.nameKey, w.basicAttack.nameKey }.Concat(w.skill == null ? Array.Empty<string>() : new[] { w.skill.nameKey }))
                .Concat(catalog.nodes.Select(n => n.nameKey)).Concat(catalog.furniture.Select(f => f.nameKey)).Concat(catalog.achievements.Select(a => a.nameKey));
            foreach (string locale in new[] { "ko", "en" })
            {
                var table = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_" + locale + ".asset");
                foreach (var key in keys) Assert.That(table.GetEntry(key)?.Value, Is.Not.Null.And.Not.Empty, key);
            }
        }
        [Test] public void ToolsKeepJobAndNeverGrantWeaponSkills()
        {
            equipment.SelectTool(LifeTool.Axe); Assert.That(equipment.Job, Is.Null);
            equipment.Equip("sword"); var job = equipment.Job;
            equipment.SelectTool(LifeTool.Pickaxe); Assert.That(equipment.Job, Is.SameAs(job));
            Assert.That(equipment.Weapon.skill, Is.Null); Assert.That(combat.Attack(true), Is.False);
            equipment.Equip(""); Assert.That(equipment.Job, Is.Null); Assert.That(combat.Attack(false), Is.False);
        }
        [Test] public void AnimationBindingsMatchImportedCharacterSkeleton()
        {
            var animator = catalog.characterModel.GetComponentInChildren<Animator>();
            Assert.That(catalog.characterAvatar, Is.Not.Null); Assert.That(catalog.characterAvatar.isValid, Is.True);
            Transform bindingRoot = animator != null ? animator.transform : catalog.characterModel.transform;
            foreach (var clip in catalog.animator.animationClips)
            {
                var missing = AnimationUtility.GetCurveBindings(clip).Where(b => b.type == typeof(Transform) && b.path.Length > 0 && bindingRoot.Find(b.path) == null).Select(b => b.path).Distinct().ToArray();
                Assert.That(missing, Is.Empty, clip.name + ": " + string.Join(", ", missing));
            }
        }
        [Test] public void ChallengesUnlockAtThresholdAndRetainEquippedWeapon()
        {
            equipment.Equip("sword"); Assert.That(equipment.Equip("sword_special"), Is.False);
            context.Save.resources.craftedFurniture = 2; achievements.Evaluate(); Assert.That(equipment.Unlocked(catalog.Weapon("sword_special")), Is.False);
            context.Save.resources.craftedFurniture = 3; achievements.Evaluate(); achievements.Evaluate();
            Assert.That(context.Save.equipment.achievements.Count, Is.EqualTo(1)); Assert.That(equipment.Weapon.id, Is.EqualTo("sword"));
            Assert.That(equipment.Equip("sword_special"), Is.True);
            equipment.Equip("bow"); Assert.That(combat.Attack(true, "sword_special"), Is.False);
        }
        [Test] public void MasteryChangesOnlySkillAndSurvivesWeaponSwitch()
        {
            var skill = catalog.Weapon("sword_special").skill;
            for (int i = 0; i < 9; i++) mastery.RecordHit("warrior");
            Assert.That(mastery.SkillDamage("warrior", skill), Is.EqualTo(28));
            mastery.RecordHit("warrior"); Assert.That(mastery.SkillDamage("warrior", skill), Is.EqualTo(35));
            for (int i = 0; i < 20; i++) mastery.RecordHit("warrior");
            Assert.That(mastery.SkillCooldown("warrior", skill), Is.EqualTo(2.4f).Within(.01));
            equipment.Equip("sword"); equipment.Equip("staff"); equipment.Equip("sword");
            Assert.That(mastery.Hits("warrior"), Is.EqualTo(30)); Assert.That(mastery.Hits("mage"), Is.Zero);
            Assert.That(catalog.Weapon("sword").basicAttack.damage, Is.EqualTo(12));
        }
        [Test] public void GatheringChallengeCountsActionsRatherThanResourceYield()
        {
            for (int i = 0; i < 2; i++) foreach (LifeResource resource in Enum.GetValues(typeof(LifeResource))) context.Save.resources.Add(resource, 99);
            achievements.Evaluate(); Assert.That(equipment.Unlocked(catalog.Weapon("staff_special")), Is.False);
            foreach (LifeResource resource in Enum.GetValues(typeof(LifeResource))) context.Save.resources.Add(resource, 1);
            achievements.Evaluate(); Assert.That(equipment.Unlocked(catalog.Weapon("staff_special")), Is.True);
        }
        [Test] public void CombatRejectsCooldownMenuAndDefeatedTargetWithoutMastery()
        {
            equipment.Equip("bow"); Assert.That(combat.Attack(false), Is.True); Assert.That(combat.Attack(false), Is.False);
            phases.Set(GamePhase.Pause); combat.Tick(1); Assert.That(combat.Attack(false), Is.False); phases.Set(GamePhase.Explore);
            for (int i = 0; i < 8; i++) { combat.Tick(1); combat.Attack(false); }
            Assert.That(combat.Health, Is.Zero); int hits = mastery.Hits("ranger");
            combat.Tick(1); Assert.That(combat.Attack(false), Is.False); Assert.That(mastery.Hits("ranger"), Is.EqualTo(hits));
        }
        [Test] public void HarvestRequiresRangeToolAndOnlyRewardsOncePerDay()
        {
            var node = catalog.nodes.First(n => n.tool == LifeTool.Axe);
            Assert.That(land.Harvest(node.id), Is.False);
            root.transform.position = node.position; Assert.That(land.Harvest(node.id), Is.False);
            equipment.SelectTool(LifeTool.Axe); Assert.That(land.Harvest(node.id), Is.True); Assert.That(land.Harvest(node.id), Is.False);
            context.LoadFrom(); Assert.That(land.Depleted(node.id), Is.True); Assert.That(context.Save.resources.wood, Is.EqualTo(3));
            phases.Set(GamePhase.Home); Assert.That(land.Rest(), Is.True); phases.Set(GamePhase.Explore);
            root.transform.position = node.position; Assert.That(land.Harvest(node.id), Is.True); Assert.That(context.Save.resources.wood, Is.EqualTo(6));
            Assert.That(context.Save.inventory.materials, Is.EqualTo(4));
        }
        [Test] public void CraftingIsAtomicRequiresHammerAndDoesNotSpendLegacyCurrency()
        {
            phases.Set(GamePhase.Home); context.Save.resources.wood = 5;
            Assert.That(crafting.Craft("table"), Is.False); equipment.SelectTool(LifeTool.Hammer);
            Assert.That(crafting.Craft("table"), Is.True); Assert.That(context.Save.resources.wood, Is.Zero); Assert.That(housing.Available("table"), Is.EqualTo(1));
            Assert.That(crafting.Craft("table"), Is.False); Assert.That(context.Save.resources.craftedFurniture, Is.EqualTo(1));
            Assert.That(context.Save.inventory.coins, Is.EqualTo(72)); Assert.That(context.Save.inventory.materials, Is.EqualTo(4));
        }
        [Test] public void PlacementRejectsBoundsOverlapDoorAndCannotDuplicateFurniture()
        {
            phases.Set(GamePhase.Home); housing.Stock("bed").count = 2; housing.Select("bed");
            housing.Move(7, 7); Assert.That(housing.Place(), Is.False);
            housing.Move(3, 0); Assert.That(housing.Place(), Is.False);
            housing.Move(0, 2); housing.Rotate(); Assert.That(housing.Place(), Is.True); Assert.That(housing.Place(), Is.False);
            Assert.That(housing.State.placed[0].turns, Is.EqualTo(1));
            housing.Select("bed"); housing.Move(1, 2); Assert.That(housing.Place(), Is.False);
            string id = housing.State.placed[0].instanceId; housing.Cancel(); Assert.That(housing.Recall(id), Is.True); Assert.That(housing.Recall(id), Is.False);
            Assert.That(housing.Available("bed"), Is.EqualTo(2));
        }
        [TestCase(-1)] [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void ExpandedSaveRoundTripsAllSlotsWithoutLosingOldProgress(int slot)
        {
            equipment.Equip("staff"); equipment.SelectTool(LifeTool.Hammer); mastery.RecordHit("mage");
            context.Save.inventory.coins = 321; context.Save.facilities.aquarium = 2; context.Save.resources.wood = 8;
            phases.Set(GamePhase.Home); Assert.That(crafting.Craft("bed"), Is.True); housing.Select("bed"); housing.Move(0, 2); housing.Place();
            context.Save.housing.inside = true; context.Save.housing.outdoorPosition = new Vector3(4, 1, 4); context.SaveTo(slot);
            context.LoadFrom(slot);
            Assert.That(context.Save.version, Is.EqualTo(GameSaveData.CurrentVersion)); Assert.That(context.Save.inventory.coins, Is.EqualTo(321)); Assert.That(context.Save.facilities.aquarium, Is.EqualTo(2));
            Assert.That(equipment.Job.id, Is.EqualTo("mage")); Assert.That(mastery.Hits("mage"), Is.EqualTo(1));
            Assert.That(housing.State.inside, Is.True); Assert.That(housing.State.placed.Count, Is.EqualTo(1)); Assert.That(housing.Available("bed"), Is.Zero);
        }
    }
}
