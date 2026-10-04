using System;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    // Transient interaction state; all persisted furniture remains owned by HousingSystem.
    public sealed class HomeInteractionSystem : IDisposable
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly HousingSystem housing;
        private readonly IslandLifeCatalog catalog;
        private readonly LandActivitySystem land;
        private readonly HomeStorageSystem storage;
        private string restingBed;
        private Vector3 beforeSeat;
        public bool Seated { get; private set; }
        public event Action<bool, Vector3, Quaternion> SeatChanged;
        public HomeInteractionSystem(GameContext context, GamePhaseController phases, HousingSystem housing, IslandLifeCatalog catalog, LandActivitySystem land, HomeStorageSystem storage)
        {
            this.context=context; this.phases=phases; this.housing=housing; this.catalog=catalog; this.land=land; this.storage=storage;
            phases.Changed+=PhaseChanged; context.Loaded+=Reset;
        }
        public PlacedFurniture Nearby
        {
            get
            {
                if(phases.Current!=GamePhase.Home || !housing.State.inside || Seated) return null;
                var player=context.World.player.transform;
                var origin=player.position+Vector3.up*.65f;
                // A narrow forward ray makes walls and intervening furniture occlude the target.
                foreach(var hit in Physics.RaycastAll(origin,player.forward,2.3f,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))
                {
                    if(hit.transform.IsChildOf(player))continue;
                    var target=hit.collider.GetComponent<HomeFurnitureTarget>();
                    if(target==null)return null;
                    return housing.State.placed.FirstOrDefault(p=>p.instanceId==target.InstanceId);
                }
                return null;
            }
        }
        public bool AtDoor => phases.Current==GamePhase.Home && housing.State.inside && !Seated
            && Vector3.Distance(context.World.player.transform.position,HomeNavigation.Arrival)<1.25f
            && Vector3.Dot(context.World.player.transform.forward,Vector3.back)>.35f;
        public void Decorate() { if(phases.Current!=GamePhase.Home)return; Stand(); phases.Set(GamePhase.HomeDecorate); }
        public void Tick()
        {
            if(!phases.HomeInputAllowed)return;
            if(Seated)
            {
                if(GameInput.InteractPressed() || GameInput.Move.sqrMagnitude>.05f)Stand();
                return;
            }
            if(GameInput.InteractPressed())Use();
        }
        public bool Use()
        {
            if(phases.Current!=GamePhase.Home || !housing.State.inside)return false;
            if(Seated){Stand();return true;}
            if(AtDoor){housing.Exit();return true;}
            var item=Nearby;
            var definition=item==null?null:catalog.Furniture(item.furnitureId);
            if(definition==null)return false;
            switch(definition.interaction)
            {
                case FurnitureInteraction.Seat:
                    beforeSeat=context.World.player.transform.position;
                    Seated=true;context.World.motor.CanMove=false;
                    var size=FurniturePlacement.Size(definition,item.turns);
                    var rotation=Quaternion.Euler(0,item.turns*90,0);
                    var position=HousingSystem.InteriorOrigin+new Vector3(item.x+size.x/2f,0,item.z+size.y/2f)+rotation*definition.seatOffset;
                    SeatChanged?.Invoke(true,position,rotation*Quaternion.Euler(0,definition.seatYaw,0));return true;
                case FurnitureInteraction.Bed:
                    restingBed=item.instanceId;phases.Set(GamePhase.HomeRest);return true;
                case FurnitureInteraction.Storage:
                    return storage.Open(item.instanceId);
                default:return false;
            }
        }
        public bool ConfirmRest()
        {
            if(phases.Current!=GamePhase.HomeRest || restingBed==null)return false;
            var item=housing.State.placed.FirstOrDefault(p=>p.instanceId==restingBed);
            if(item==null || catalog.Furniture(item.furnitureId)?.interaction!=FurnitureInteraction.Bed)return false;
            restingBed=null;phases.Set(GamePhase.Home);
            context.World.motor.Teleport(HomeNavigation.Safe(context.World.player.transform.position,housing.State.placed,catalog,housing.State.structure));
            return land.Rest();
        }
        public void Stand()
        {
            if(!Seated)return;
            Seated=false;
            context.World.motor.Teleport(HomeNavigation.Safe(beforeSeat,housing.State.placed,catalog,housing.State.structure));
            context.World.motor.CanMove=phases.Current==GamePhase.Home;
            SeatChanged?.Invoke(false,Vector3.zero,Quaternion.identity);
        }
        private void PhaseChanged()
        {
            if(phases.Current!=GamePhase.Home)Stand();
            if(phases.Current!=GamePhase.HomeRest)restingBed=null;
            if(phases.Current!=GamePhase.HomeDecorate && housing.Selection!=null)housing.Cancel();
        }
        private void Reset()
        {
            Seated=false;restingBed=null;SeatChanged?.Invoke(false,Vector3.zero,Quaternion.identity);
        }
        public void Dispose(){phases.Changed-=PhaseChanged;context.Loaded-=Reset;}
    }
}
