using UnityEngine;
namespace BreathIsland
{
    public sealed class EquipmentHud : HudScreen
    {
        private readonly IslandLifeSession life;
        public EquipmentHud(GameContext context, GamePhaseController phases, IslandLifeSession life) : base(context, phases) => this.life = life;
        public override void Draw(float width, float height)
        {
            EnsureStyles(); float x = width / 2 - 400;
            GUI.Box(new Rect(x, 110, 800, 680), "", panelStyle);
            GUI.Label(new Rect(x + 25, 130, 740, 40), context.Text("life_equipment"), titleStyle);
            for (int i = 0; i < life.Equipment.Weapons.Length; i++)
            {
                var weapon = life.Equipment.Weapons[i]; float y = 180 + i * 65;
                bool unlocked = life.Equipment.Unlocked(weapon);
                GUI.enabled = unlocked;
                if (GUI.Button(new Rect(x + 25, y, 245, 54), context.Text(weapon.nameKey), life.Equipment.Weapon == weapon ? primaryButtonStyle : buttonStyle)) life.Equipment.Equip(weapon.id);
                GUI.enabled = true;
                string detail = unlocked ? string.Format(context.Text("life_mastery"), context.Text(weapon.job.nameKey), life.Mastery.Hits(weapon.job.id), life.Mastery.Rank(weapon.job.id))
                    : context.Text("life_achievement_" + weapon.achievementId);
                GUI.Label(new Rect(x + 290, y, 480, 56), detail, bodyStyle);
            }
            if (GUI.Button(new Rect(x + 25, 580, 245, 42), context.Text("life_unequip"), buttonStyle)) life.Equipment.Equip("");
            int index = 0;
            foreach (LifeTool tool in System.Enum.GetValues(typeof(LifeTool)))
            {
                var toolStyle = new GUIStyle(life.Equipment.Tool == tool ? primaryButtonStyle : buttonStyle) { fontSize = 14, wordWrap = true };
                if (GUI.Button(new Rect(x + 25 + index * 188, 634, 178, 42), context.Text("life_tool_" + tool), toolStyle)) life.Equipment.SelectTool(tool);
                index++;
            }
            GUI.Label(new Rect(x + 25, 685, 740, 44), context.Text("life_mastery_help"), smallStyle);
            if (GUI.Button(new Rect(x + 25, 736, 740, 36), context.Text("ui_close_esc"), buttonStyle)) phases.Set(GamePhase.Explore);
        }
    }
}
