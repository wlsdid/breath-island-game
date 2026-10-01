using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class HomeStorageSystem : IDisposable
    {
        public const int Capacity=16, ResourceStack=99;
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly HousingSystem housing;
        private readonly IslandLifeCatalog catalog;
        public string OpenId { get; private set; }
        public ChestState Current => context.Save.housing.chests.FirstOrDefault(c=>c.instanceId==OpenId);
        public HomeStorageSystem(GameContext context,GamePhaseController phases,HousingSystem housing,IslandLifeCatalog catalog)
        {this.context=context;this.phases=phases;this.housing=housing;this.catalog=catalog;phases.Changed+=PhaseChanged;context.Loaded+=Reset;}
        public bool Open(string id)
        {
            if(phases.Current!=GamePhase.Home || !housing.State.inside)return false;
            var item=housing.State.placed.FirstOrDefault(p=>p.instanceId==id);
            if(item==null || catalog.Furniture(item.furnitureId)?.interaction!=FurnitureInteraction.Storage)return false;
            var player=context.World.player.transform;
            var first=Physics.RaycastAll(player.position+Vector3.up*.65f,player.forward,2.3f,~0,QueryTriggerInteraction.Ignore)
                .OrderBy(h=>h.distance).FirstOrDefault(h=>!h.transform.IsChildOf(player));
            if(first.collider==null || first.collider.GetComponent<HomeFurnitureTarget>()?.InstanceId!=id)return false;
            var chest=housing.State.chests.FirstOrDefault(c=>c.instanceId==id);
            if(chest==null)
            {
                chest=new ChestState{instanceId=id};
                for(int i=0;i<Capacity;i++)chest.slots.Add(new StorageSlot());
                housing.State.chests.Add(chest);
            }
            OpenId=id;phases.Set(GamePhase.HomeStorage);return true;
        }
        public List<InventoryItem> BagItems()
        {
            var items=new List<InventoryItem>();
            foreach(var id in new[]{"wood","stone","herbs","coins","materials","bait"})items.Add(new InventoryItem{Id=id,NameKey="bag_"+id,Count=Count(id,false)});
            foreach(var f in catalog.furniture)items.Add(new InventoryItem{Id=f.id,NameKey=f.nameKey,Count=Count(f.id,true)});
            return items;
        }
        public int Count(string id,bool furniture) => furniture ? housing.Available(id) : id switch
        {"wood"=>context.Save.resources.wood,"stone"=>context.Save.resources.stone,"herbs"=>context.Save.resources.herbs,"coins"=>context.Save.inventory.coins,"materials"=>context.Save.inventory.materials,"bait"=>context.Save.inventory.bait,_=>0};
        public string NameKey(StorageSlot item)=>item.furniture ? catalog.Furniture(item.id)?.nameKey??"life_unknown" : "bag_"+item.id;
        private bool Valid(string id,bool furniture)=>furniture?catalog.Furniture(id)!=null:id=="wood"||id=="stone"||id=="herbs"||id=="coins"||id=="materials"||id=="bait";
        private bool Allowed => phases.Current==GamePhase.HomeStorage && housing.State.inside && Current!=null
            && housing.State.placed.Any(p=>p.instanceId==OpenId && catalog.Furniture(p.furnitureId)?.interaction==FurnitureInteraction.Storage);
        public int Deposit(string id,bool furniture,int requested)
        {
            if(!Allowed || !Valid(id,furniture) || requested<=0)return 0;
            int remaining=Math.Min(requested,Count(id,furniture)), moved=0,limit=furniture?1:ResourceStack;
            // Fill existing stacks before consuming empty slots, then debit exactly the moved quantity.
            var slots=Current.slots.Take(Capacity).Where(s=>s.count>0&&s.id==id&&s.furniture==furniture)
                .Concat(Current.slots.Take(Capacity).Where(s=>s.count==0)).ToArray();
            foreach(var slot in slots)
            {
                int take=Math.Min(remaining,Math.Max(0,limit-slot.count));
                if(take==0)continue;
                slot.id=id;slot.furniture=furniture;slot.count+=take;remaining-=take;moved+=take;
                if(remaining==0)break;
            }
            if(moved>0){ChangeBag(id,furniture,-moved);context.SaveTo();}
            return moved;
        }
        public int Withdraw(int index,int requested)
        {
            if(!Allowed || index<0 || index>=Capacity || index>=Current.slots.Count || requested<=0)return 0;
            var slot=Current.slots[index];
            if(!Valid(slot.id,slot.furniture)||slot.count<=0)return 0;
            int moved=Math.Min(Math.Min(requested,slot.count),int.MaxValue-Count(slot.id,slot.furniture));
            if(moved<=0)return 0;
            ChangeBag(slot.id,slot.furniture,moved);slot.count-=moved;
            if(slot.count==0){slot.id="";slot.furniture=false;}
            context.SaveTo();return moved;
        }
        private void ChangeBag(string id,bool furniture,int delta)
        {
            if(furniture){housing.Stock(id).count+=delta;return;}
            var r=context.Save.resources;
            if(id=="wood")r.wood+=delta;else if(id=="stone")r.stone+=delta;else if(id=="herbs")r.herbs+=delta; else if(id=="coins")context.Save.inventory.coins+=delta; else if(id=="materials")context.Save.inventory.materials+=delta; else if(id=="bait")context.Save.inventory.bait+=delta;
        }
        private void PhaseChanged(){if(phases.Current!=GamePhase.HomeStorage)Reset();}
        private void Reset()=>OpenId=null;
        public void Dispose(){phases.Changed-=PhaseChanged;context.Loaded-=Reset;}
    }
}
