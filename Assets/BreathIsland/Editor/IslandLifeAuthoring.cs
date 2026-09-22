using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace BreathIsland.Editor
{
    // Explicit authoring command only. Existing assets are never regenerated on load or build.
    public static class IslandLifeAuthoring
    {
        private const string Art = "Assets/BreathIsland/Art/ThirdParty/KayKit/";
        private const string Data = "Assets/BreathIsland/Resources/Content/Life/";
        [MenuItem("숨결섬/확장 초기 콘텐츠 작성")]
        public static void CreateInitialContent()
        {
            Directory.CreateDirectory(Data); AssetDatabase.Refresh();
            ConfigureAnimationImport("Character/Ranger.fbx");
            ConfigureAnimationImport("Character/Rig_Medium_General.fbx");
            ConfigureAnimationImport("Character/Rig_Medium_MovementBasic.fbx");
            var controller = AnimationController();
            var jobs = new[] { "warrior", "ranger", "mage" }.Select(id => Create<JobDefinition>(id, asset => { asset.id = id; asset.nameKey = "life_job_" + id; })).ToArray();
            var achievements = new[]
            {
                Challenge("craft", AchievementCondition.CraftFurniture, 3),
                Challenge("training", AchievementCondition.TrainingHits, 30),
                Challenge("gather", AchievementCondition.AllResources, 3)
            };
            string[] ids = { "sword", "bow", "staff" }, models = { "sword_A", "bow_A_withString", "staff_A" };
            float[] ranges = { 3, 11, 8 };
            Color[] colors = { new Color(1, .65f, .2f), new Color(.4f, 1, .5f), new Color(.55f, .35f, 1) };
            var weapons = new WeaponDefinition[6];
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var basic = Create<SkillDefinition>(ids[i] + "_basic", a => { a.id = ids[index] + "_basic"; a.nameKey = "life_" + a.id; a.damage = 12; a.range = ranges[index]; a.cooldown = .5f; a.effectColor = colors[index]; });
                var skill = Create<SkillDefinition>(ids[i] + "_skill", a => { a.id = ids[index] + "_skill"; a.nameKey = "life_" + a.id; a.damage = 28; a.range = ranges[index] + 1; a.cooldown = 3; a.effectColor = colors[index]; });
                for (int special = 0; special < 2; special++)
                {
                    bool isSpecial = special == 1; string id = ids[i] + (isSpecial ? "_special" : "");
                    weapons[i + special * 3] = Create<WeaponDefinition>(id, a => { a.id = id; a.nameKey = "life_weapon_" + id; a.job = jobs[index]; a.basicAttack = basic; a.skill = isSpecial ? skill : null; a.model = Model("Weapons/" + models[index] + ".fbx"); a.achievementId = isSpecial ? achievements[index].id : ""; });
                }
            }
            var furniture = new[]
            {
                Furniture("chair", "chair_A_wood", new Vector2Int(1, 1), 1.2f, 3, 0),
                Furniture("table", "table_medium", new Vector2Int(2, 2), 1, 5, 0),
                Furniture("bed", "bed_single_A", new Vector2Int(2, 3), 1.1f, 8, 0),
                Furniture("lamp", "lamp_standing", new Vector2Int(1, 1), 1.6f, 2, 2)
            };
            var nodes = new[]
            {
                Node("tree_a", LifeTool.Axe, LifeResource.Wood, "Tree_1_A_Color1", new Vector3(-5, .6f, -2), 2.7f),
                Node("tree_b", LifeTool.Axe, LifeResource.Wood, "Tree_1_A_Color1", new Vector3(-3, .6f, -5), 2.7f),
                Node("tree_c", LifeTool.Axe, LifeResource.Wood, "Tree_1_A_Color1", new Vector3(-6, .6f, 2), 2.7f),
                Node("rock_a", LifeTool.Pickaxe, LifeResource.Stone, "Rock_1_A_Color1", new Vector3(1, .6f, -4), .85f),
                Node("rock_b", LifeTool.Pickaxe, LifeResource.Stone, "Rock_1_A_Color1", new Vector3(3, .6f, 0), .85f),
                Node("herb_a", LifeTool.None, LifeResource.Herb, "Bush_1_A_Color1", new Vector3(-1, .6f, -4), .6f),
                Node("herb_b", LifeTool.None, LifeResource.Herb, "Bush_1_A_Color1", new Vector3(2, .6f, 2), .6f)
            };
            const string path = "Assets/BreathIsland/Resources/Content/IslandLife.asset";
            if (AssetDatabase.LoadAssetAtPath<IslandLifeCatalog>(path) == null)
            {
                var catalog = ScriptableObject.CreateInstance<IslandLifeCatalog>();
                catalog.weapons = weapons; catalog.furniture = furniture; catalog.nodes = nodes; catalog.achievements = achievements;
                catalog.axeModel = Model("Tools/axe.fbx"); catalog.pickaxeModel = Model("Tools/pickaxe.fbx"); catalog.hammerModel = Model("Tools/hammer.fbx");
                catalog.characterModel = Model("Character/Ranger.fbx"); catalog.animator = controller;
                catalog.furnitureMaterial = Material("Furniture/furniturebits_texture.png", "Furniture");
                catalog.weaponMaterial = Material("Weapons/weapons_bits_texture.png", "Weapons");
                catalog.toolMaterial = Material("Tools/tools_bits_texture.png", "Tools");
                catalog.natureMaterial = Material("Nature/forest_texture.png", "Nature");
                catalog.characterMaterial = Material("Character/ranger_texture.png", "Character");
                AssetDatabase.CreateAsset(catalog, path);
            }
            IslandLifeLocalization.AddMissingEntries();
            var authored = AssetDatabase.LoadAssetAtPath<IslandLifeCatalog>(path);
            if (authored.characterAvatar == null)
            {
                var instance = UnityEngine.Object.Instantiate(authored.characterModel);
                var avatar = AvatarBuilder.BuildGenericAvatar(instance, ""); avatar.name = "IslandCharacter";
                AssetDatabase.CreateAsset(avatar, Data + "IslandCharacterAvatar.asset");
                UnityEngine.Object.DestroyImmediate(instance);
                authored.characterAvatar = avatar; EditorUtility.SetDirty(authored);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Island life authored: 6 weapons, 3 jobs, 6 actions, 3 challenges, 4 furniture, 7 resource nodes.");
            Debug.Log("Character hierarchy: " + string.Join(", ", authored.characterModel.GetComponentsInChildren<Transform>().Select(t => t.name)));
        }
        private static T Create<T>(string id, Action<T> configure) where T : ScriptableObject
        {
            string path = Data + typeof(T).Name + "_" + id + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); configure(asset); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        private static GameObject Model(string relative) => AssetDatabase.LoadAssetAtPath<GameObject>(Art + relative) ?? throw new InvalidOperationException("Missing model: " + relative);
        private static Material Material(string texture, string id)
        {
            string path = Data + id + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + texture);
            material.SetFloat("_Smoothness", .15f); AssetDatabase.CreateAsset(material, path); return material;
        }
        private static AchievementDefinition Challenge(string id, AchievementCondition condition, int count) => Create<AchievementDefinition>(id, a => { a.id = id; a.nameKey = "life_achievement_" + id; a.condition = condition; a.threshold = count; });
        private static FurnitureDefinition Furniture(string id, string model, Vector2Int footprint, float height, int wood, int stone) => Create<FurnitureDefinition>(id, a => { a.id = id; a.nameKey = "life_furniture_" + id; a.model = Model("Furniture/" + model + ".fbx"); a.footprint = footprint; a.height = height; a.woodCost = wood; a.stoneCost = stone; });
        private static ResourceNodeDefinition Node(string id, LifeTool tool, LifeResource resource, string model, Vector3 position, float height) => Create<ResourceNodeDefinition>(id, a => { a.id = id; a.nameKey = "life_resource_" + resource; a.tool = tool; a.resource = resource; a.yield = resource == LifeResource.Herb ? 2 : 3; a.model = Model("Nature/" + model + ".fbx"); a.position = position; a.height = height; });
        private static void ConfigureAnimationImport(string path)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(Art + path);
            if (importer.animationType == ModelImporterAnimationType.Generic) return;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips) clip.loopTime = true;
            importer.clipAnimations = clips; importer.SaveAndReimport();
        }
        private static RuntimeAnimatorController AnimationController()
        {
            string path = Data + "IslandCharacter.controller";
            var existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path); if (existing != null) return existing;
            var clips = new[] { "Rig_Medium_General", "Rig_Medium_MovementBasic" }.SelectMany(id => AssetDatabase.LoadAllAssetsAtPath(Art + "Character/" + id + ".fbx").OfType<AnimationClip>()).Where(c => !c.name.StartsWith("__preview__")).ToArray();
            Debug.Log("Imported animation clips: " + string.Join(", ", clips.Select(c => c.name)));
            var idle = clips.First(c => c.name.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) >= 0);
            var walk = clips.First(c => c.name.IndexOf("Walk", StringComparison.OrdinalIgnoreCase) >= 0);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path); controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine; var idleState = machine.AddState("Idle"); idleState.motion = idle;
            var walkState = machine.AddState("Walk"); walkState.motion = walk;
            var start = idleState.AddTransition(walkState); start.hasExitTime = false; start.duration = .15f; start.AddCondition(AnimatorConditionMode.Greater, .1f, "Speed");
            var stop = walkState.AddTransition(idleState); stop.hasExitTime = false; stop.duration = .15f; stop.AddCondition(AnimatorConditionMode.Less, .1f, "Speed");
            return controller;
        }
    }
}
