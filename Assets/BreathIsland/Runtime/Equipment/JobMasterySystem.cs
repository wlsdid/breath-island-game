using System.Linq;
namespace BreathIsland
{
    public sealed class JobMasterySystem
    {
        private readonly GameContext context;
        public JobMasterySystem(GameContext context) => this.context = context;
        public int Hits(string job) => context.Save.equipment.mastery.FirstOrDefault(m => m.jobId == job)?.hits ?? 0;
        public int Rank(string job) => Hits(job) >= 30 ? 2 : Hits(job) >= 10 ? 1 : 0;
        public int SkillDamage(string job, SkillDefinition skill) => UnityEngine.Mathf.RoundToInt(skill.damage * (Rank(job) >= 1 ? 1.25f : 1));
        public float SkillCooldown(string job, SkillDefinition skill) => skill.cooldown * (Rank(job) >= 2 ? .8f : 1);
        public void RecordHit(string job)
        {
            var entry = context.Save.equipment.mastery.FirstOrDefault(m => m.jobId == job);
            if (entry == null) { entry = new JobMastery { jobId = job }; context.Save.equipment.mastery.Add(entry); }
            entry.hits++;
        }
    }
}
