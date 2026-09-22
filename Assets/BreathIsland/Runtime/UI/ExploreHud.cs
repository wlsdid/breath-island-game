using UnityEngine;
namespace BreathIsland
{
    public sealed class ExploreHud : HudScreen
    {
        private readonly RelationshipSystem relationships;
        private bool showHelp = true;
        private readonly LifeExploreHud lifeHud;
        public ExploreHud(GameContext context, GamePhaseController phases, RelationshipSystem relationships, IslandLifeSession life = null) : base(context, phases)
        { this.relationships = relationships; if (life != null) lifeHud = new LifeExploreHud(context, phases, life); }
        public override void Draw(float width, float height)
        {
            EnsureStyles();
            bool ko = context.Korean;
            NpcId? nearby = relationships.NearbyNpc();
            if (nearby.HasValue)
            {
                NpcDefinition npc = context.Content.Npcs[nearby.Value];
                GUI.Box(new Rect(width / 2f - 190f, height - 170f, 380f, 48f), string.Format(context.Text("talk_prompt"), npc.LocalizedName(ko)), DarkBox());
            }
            if (Vector3.Distance(context.World.player.transform.position, context.World.fishingPoint) < 7.5f)
                GUI.Box(new Rect(width / 2f - 190f, height - 220f, 380f, 48f), context.Text("ui_f_space_start_fishing"), DarkBox());
            GUI.Box(new Rect(24, height - 118f, 390f, 92f), context.Text("ui_wasd_move_mouse_look_e_talk_nm_village_tab_guide_l_language"), DarkBox());
            if (showHelp && GUI.Button(new Rect(width - 182f, height - 72f, 158f, 46f), context.Text("ui_hide_help"), buttonStyle)) showHelp = false;
            lifeHud?.Draw(width, height);
        }
        public override void Dispose() { lifeHud?.Dispose(); base.Dispose(); }
    }
}
