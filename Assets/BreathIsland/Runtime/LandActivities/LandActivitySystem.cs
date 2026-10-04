using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class LandActivitySystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly IslandLifeCatalog catalog;
        private readonly EquipmentSystem equipment;
        private readonly AchievementSystem achievements;
        private readonly GatheringWork work=new();
        public float Progress(ResourceNodeDefinition node)=>node.tool==LifeTool.None
            ? (node.gatherSeconds>0?Mathf.Clamp01(work.Seconds(node.id)/node.gatherSeconds):0)
            : Mathf.Clamp01(work.Hits(node.id)/(float)Mathf.Max(1,node.requiredHits));
        public event System.Action Changed;
        public LandActivitySystem(GameContext context, GamePhaseController phases, IslandLifeCatalog catalog, EquipmentSystem equipment, AchievementSystem achievements)
        { this.context = context; this.phases = phases; this.catalog = catalog; this.equipment = equipment; this.achievements = achievements; }
        public ResourceNodeDefinition Nearby => catalog.nodes.OrderBy(n => Vector3.Distance(context.World.player.transform.position, n.position))
            .FirstOrDefault(n => Vector3.Distance(context.World.player.transform.position, n.position) < 2.3f);
        public bool Depleted(string id) => context.Save.resources.harvested.Any(n => n.id == id && n.day >= context.Save.world.day);
        public bool Harvest(string id)
        {
            var node = catalog.nodes.FirstOrDefault(n => n.id == id);
            if (phases.Current != GamePhase.Explore || node == null || Depleted(id) || Vector3.Distance(context.World.player.transform.position, node.position) >= 2.3f) return false;
            if (node.tool != LifeTool.None && equipment.Tool != node.tool)
            { context.Message(string.Format(context.Text("life_need_tool"), context.Text("life_tool_" + node.tool))); return false; }
            if(node.tool==LifeTool.None && node.gatherSeconds>0)return false;
            if(node.tool!=LifeTool.None && !work.Hit(id,node.requiredHits))return false;
            return Complete(node);
        }
        public bool TickHeldGather(float delta,bool held)
        {
            var node=Nearby;
            if(!held || !phases.FieldInputAllowed || node==null || node.tool!=LifeTool.None || node.gatherSeconds<=0 || Depleted(node.id)
                || !ToolUseSystem.InReach(context.World.player.transform.position,context.World.player.transform.forward,node.position))
            {work.CancelHold();return false;}
            return work.Hold(node.id,delta,node.gatherSeconds) && Complete(node);
        }
        private bool Complete(ResourceNodeDefinition node)
        {
            if(Depleted(node.id))return false;
            string id=node.id;work.Complete(id);
            context.Save.resources.Add(node.resource, node.yield);
            context.Save.resources.harvested.RemoveAll(n => n.id == id);
            context.Save.resources.harvested.Add(new NodeHarvest { id = id, day = context.Save.world.day });
            Changed?.Invoke();
            context.Message(string.Format(context.Text("life_harvested"), context.Text(node.nameKey), node.yield));
            achievements.Evaluate(); context.SaveTo(); return true;
        }
        public void Restore() { work.Reset(); Changed?.Invoke(); }
        public bool Rest()
        {
            if (phases.Current != GamePhase.Home) return false;
            context.Save.world.day++; context.Save.player.hour = 9;
            work.Reset();
            Changed?.Invoke(); context.SaveTo(); context.Message(context.Text("life_new_day")); return true;
        }
    }
}
