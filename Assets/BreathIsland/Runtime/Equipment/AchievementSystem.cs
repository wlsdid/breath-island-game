using System.Linq;
namespace BreathIsland
{
    public sealed class AchievementSystem
    {
        private readonly GameContext context;
        private readonly IslandLifeCatalog catalog;
        public AchievementSystem(GameContext context, IslandLifeCatalog catalog) { this.context = context; this.catalog = catalog; }
        public bool Unlocked(WeaponDefinition weapon) => string.IsNullOrEmpty(weapon.achievementId) || context.Save.equipment.achievements.Contains(weapon.achievementId);
        public void Evaluate()
        {
            var resources = context.Save.resources;
            foreach (var achievement in catalog.achievements)
            {
                bool complete = achievement.condition switch
                {
                    AchievementCondition.CraftFurniture => resources.craftedFurniture >= achievement.threshold,
                    AchievementCondition.TrainingHits => context.Save.equipment.mastery.Sum(m => m.hits) >= achievement.threshold,
                    _ => resources.woodHarvests >= achievement.threshold && resources.stoneHarvests >= achievement.threshold && resources.herbHarvests >= achievement.threshold
                };
                if (complete && !context.Save.equipment.achievements.Contains(achievement.id))
                {
                    context.Save.equipment.achievements.Add(achievement.id);
                    context.Message(string.Format(context.Text("life_achievement_unlocked"), context.Text(achievement.nameKey)));
                }
            }
        }
    }
}
