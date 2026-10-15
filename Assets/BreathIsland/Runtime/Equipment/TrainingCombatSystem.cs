using UnityEngine;
namespace BreathIsland
{
    public sealed class TrainingCombatSystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly EquipmentSystem equipment;
        private readonly Transform target;
        private readonly JobMasterySystem mastery;
        private readonly AchievementSystem achievements;
        private float basicCooldown, skillCooldown;
        public int Health { get; private set; } = 100;
        public float SkillCooldown => skillCooldown;
        public Vector3 TargetPosition => target.position;
        public event System.Action<Vector3, Vector3, Color> Hit;
        public event System.Action<WeaponDefinition, bool, Vector3> Presented;
        public TrainingCombatSystem(GameContext context, GamePhaseController phases, EquipmentSystem equipment, Transform target, JobMasterySystem mastery, AchievementSystem achievements)
        { this.context = context; this.phases = phases; this.equipment = equipment; this.target = target; this.mastery = mastery; this.achievements = achievements; }
        public void Tick(float dt)
        {
            basicCooldown = Mathf.Max(0, basicCooldown - dt);
            skillCooldown = Mathf.Max(0, skillCooldown - dt);
        }
        public bool Attack(bool skill, string expectedWeaponId = null)
        {
            var weapon = equipment.Weapon;
            if (phases.Current != GamePhase.Explore || equipment.Tool != LifeTool.None || weapon == null || Health <= 0) return false;
            if (expectedWeaponId != null && expectedWeaponId != weapon.id) return false;
            var action = skill ? weapon.skill : weapon.basicAttack;
            if (action == null) return false;
            if ((skill ? skillCooldown : basicCooldown) > 0) return false;
            Vector3 origin = context.World.player.transform.position;
            if (Vector3.Distance(origin, target.position) > action.range)
            { context.Message(context.Text("life_out_of_range")); return false; }
            if (skill) skillCooldown = mastery.SkillCooldown(weapon.job.id, action); else basicCooldown = action.cooldown;
            equipment.ShowWeapon();
            var direction = Vector3.ProjectOnPlane(target.position - origin, Vector3.up);
            if (direction.sqrMagnitude > .01f) context.World.player.transform.rotation = Quaternion.LookRotation(direction);
            int damage = skill ? mastery.SkillDamage(weapon.job.id, action) : action.damage;
            Health = Mathf.Max(0, Health - damage);
            mastery.RecordHit(weapon.job.id);
            Hit?.Invoke(origin + Vector3.up, target.position + Vector3.up, action.effectColor);
            Presented?.Invoke(weapon, skill, target.position + Vector3.up);
            context.Message(string.Format(context.Text("life_hit"), context.Text(action.nameKey), damage, Health));
            achievements.Evaluate(); context.SaveTo();
            return true;
        }
        public void ResetTarget()
        {
            if (phases.Current == GamePhase.Explore && Vector3.Distance(context.World.player.transform.position, target.position) < 12)
                Health = 100;
        }
    }
}
