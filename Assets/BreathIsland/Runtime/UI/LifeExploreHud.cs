using UnityEngine;
namespace BreathIsland
{
    public sealed class LifeExploreHud : HudScreen
    {
        private readonly IslandLifeSession life;
        public LifeExploreHud(GameContext context, GamePhaseController phases, IslandLifeSession life) : base(context, phases) => this.life = life;
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            GUI.Box(new Rect(24, 130, 340, 390), "", panelStyle);
            var weapon = life.Equipment.Weapon;
            GUI.Label(new Rect(42, 145, 310, 55), weapon == null ? context.Text("life_unarmed") : context.Text(weapon.nameKey) + " / " + context.Text(weapon.job.nameKey), bodyStyle);
            GUI.Label(new Rect(42, 200, 310, 40), context.Text("life_tool_" + life.Equipment.Tool), bodyStyle);
            if (GUI.Button(new Rect(42, 243, 300, 40), context.Text("life_equipment_open"), buttonStyle)) life.OpenEquipment();
            if (GUI.Button(new Rect(42, 290, 145, 40), context.Text("life_attack"), buttonStyle)) life.Training.Attack(false);
            GUI.enabled = weapon?.skill != null;
            if (GUI.Button(new Rect(195, 290, 147, 40), context.Text("life_skill"), primaryButtonStyle)) life.Training.Attack(true, weapon.id);
            GUI.enabled = true;
            GUI.Label(new Rect(42, 340, 300, 52), string.Format(context.Text("life_training"), life.Training.Health, life.Training.SkillCooldown), bodyStyle);
            if (GUI.Button(new Rect(42, 398, 300, 36), context.Text("life_reset_target"), buttonStyle)) life.Training.ResetTarget();
            GUI.Label(new Rect(42, 442, 300, 70), context.Text("life_explore_help"), smallStyle);
            var resources = context.Save.resources;
            GUI.Box(new Rect(width - 420, 105, 396, 48), string.Format(context.Text("life_resources"), resources.wood, resources.stone, resources.herbs), DarkBox());
            if (life.Housing.CanEnter)
            {
                if (GUI.Button(new Rect(width / 2 - 180, height - 285, 360, 48), context.Text("life_enter"), primaryButtonStyle)) life.Housing.Enter();
            }
            else if (life.Land.Nearby != null)
            {
                var node = life.Land.Nearby;
                string label = life.Land.Depleted(node.id) ? context.Text("life_depleted") : string.Format(context.Text("life_node_prompt"), context.Text(node.nameKey), context.Text("life_tool_" + node.tool));
                if (GUI.Button(new Rect(width / 2 - 240, height - 285, 480, 48), label, primaryButtonStyle)) life.Land.Harvest(node.id);
            }
        }
    }
}
