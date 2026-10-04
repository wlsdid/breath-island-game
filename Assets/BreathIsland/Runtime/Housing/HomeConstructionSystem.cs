using System;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class HomeConstructionSystem : IDisposable
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly HousingSystem housing;
        private readonly IslandLifeCatalog catalog;
        private HomeStructureState original;
        public HomeStructureState Draft {get;private set;}
        public HomeStructureState Display=>Draft??context.Save.housing.structure;
        public bool Editing=>Draft!=null;
        public event Action Changed;
        public HomeConstructionSystem(GameContext context,GamePhaseController phases,HousingSystem housing,IslandLifeCatalog catalog)
        {
            this.context=context;this.phases=phases;this.housing=housing;this.catalog=catalog;
            context.Loaded+=Cancel;phases.Changed+=OnPhase;
        }
        private bool Allowed=>context.Save.housing.inside&&phases.Current==GamePhase.HomeDecorate;
        public bool Begin()
        {
            if(!Allowed)return false;housing.Cancel();original=HomeFootprint.Copy(context.Save.housing.structure);
            Draft=HomeFootprint.Copy(original);WoodCost=0;Changed?.Invoke();return true;
        }
        public int WoodCost {get;private set;}
        private int ComputeCost()
        {
            return Draft.cells.Count(c=>!original.cells.Any(o=>o.x==c.x&&o.z==c.z))*2
                -original.cells.Count(c=>c.paid&&!Draft.cells.Any(o=>o.x==c.x&&o.z==c.z))
                +Draft.walls.Count(w=>!original.walls.Any(o=>Same(o,w)))
                -original.walls.Count(w=>!Draft.walls.Any(o=>Same(o,w)));
        }
        private static bool Same(WallEdge a,WallEdge b)=>a.x==b.x&&a.z==b.z&&a.vertical==b.vertical;
        public bool ToggleFloor(int x,int z)
        {
            if(!Allowed||!Editing||x<0||z<0||x>=HomeFootprint.Limit||z>=HomeFootprint.Limit)return false;
            var next=HomeFootprint.Copy(Draft);var cell=next.cells.FirstOrDefault(c=>c.x==x&&c.z==z);
            if(cell==null)next.cells.Add(new FloorCell{x=x,z=z,paid=original.cells.FirstOrDefault(c=>c.x==x&&c.z==z)?.paid??true});
            else
            {
                next.cells.Remove(cell);
                next.walls.RemoveAll(w=>!Adjacent(next,w));
            }
            return Accept(next);
        }
        public bool ToggleWall(int x,int z,bool vertical)
        {
            if(!Allowed||!Editing)return false;
            var next=HomeFootprint.Copy(Draft);var wall=new WallEdge{x=x,z=z,vertical=vertical};
            if(!Adjacent(next,wall))return false;
            var existing=next.walls.FirstOrDefault(w=>Same(w,wall));
            if(existing!=null)next.walls.Remove(existing);else next.walls.Add(wall);
            return Accept(next);
        }
        private static bool Adjacent(HomeStructureState s,WallEdge w)=>HomeFootprint.Cell(s,w.x,w.z)
            &&HomeFootprint.Cell(s,w.x-(w.vertical?1:0),w.z-(w.vertical?0:1));
        private bool Valid(HomeStructureState s)
        {
            if(!HomeFootprint.Connected(s)||!HomeFootprint.Contains(s,RoomLayoutDefinition.Default.entranceClearance))return false;
            var p=context.World.player.transform.position-HousingSystem.InteriorOrigin;
            if(!HomeFootprint.Contains(s,new Rect(p.x-.4f,p.z-.4f,.8f,.8f)))return false;
            if(!housing.State.placed.All(item=>{var d=catalog.Furniture(item.furnitureId);return d==null||HomeFootprint.Contains(s,FurniturePlacement.Area(d,item.x,item.z,item.turns));}))return false;
            // Rest regenerates resources. Never let demolition remove the last place for a bed.
            if(housing.State.placed.Any(item=>item.furnitureId=="bed"))return true;
            var bed=catalog.Furniture("bed");
            return s.cells.Any(c=>FurniturePlacement.Valid(catalog,bed,c.x,c.z,0,housing.State.placed,s)
                ||FurniturePlacement.Valid(catalog,bed,c.x,c.z,1,housing.State.placed,s));
        }
        private bool Accept(HomeStructureState next)
        {
            if(!Valid(next)){context.Message(context.Text("build_blocked"));return false;}
            Draft=next;WoodCost=ComputeCost();Changed?.Invoke();return true;
        }
        public bool Apply()
        {
            if(!Allowed||!Editing||!Valid(Draft))return false;
            int cost=WoodCost;if(context.Save.resources.wood<cost){context.Message(context.Text("build_need_wood"));return false;}
            context.Save.resources.wood-=cost;context.Save.housing.structure=Draft;Draft=null;original=null;WoodCost=0;
            housing.Restore();Changed?.Invoke();context.SaveTo();return true;
        }
        public void Cancel(){Draft=null;original=null;WoodCost=0;Changed?.Invoke();}
        private void OnPhase(){if(!Allowed)Cancel();}
        public void Dispose(){context.Loaded-=Cancel;phases.Changed-=OnPhase;}
    }
}
