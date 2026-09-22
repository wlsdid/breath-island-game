using System;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class HousingSystem
    {
        public static readonly Vector3 InteriorOrigin = new(40, 1, 0);
        public static readonly Vector3 Entrance = new(4, .65f, 4);
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly IslandLifeCatalog catalog;
        public event Action Changed;
        public HousingSystem(GameContext context, GamePhaseController phases, IslandLifeCatalog catalog)
        { this.context = context; this.phases = phases; this.catalog = catalog; }
        public FurnitureDefinition[] Furniture => catalog.furniture;
        public HousingState State => context.Save.housing;
        public FurnitureDefinition Selection { get; private set; }
        public int X { get; private set; }
        public int Z { get; private set; }
        public int Turns { get; private set; }
        public bool CanEnter => phases.Current == GamePhase.Explore && Vector3.Distance(context.World.player.transform.position, Entrance) < 2.5f;
        public bool IsValid => Selection != null && FurniturePlacement.Valid(catalog, Selection, X, Z, Turns, State.placed);
        public int Available(string id) => State.stock.FirstOrDefault(s => s.id == id)?.count ?? 0;
        public bool Enter()
        {
            if (!CanEnter) return false;
            State.outdoorPosition = context.World.player.transform.position; State.inside = true;
            context.World.motor.Teleport(InteriorOrigin + new Vector3(4, .1f, 1));
            phases.Set(GamePhase.Home); Cancel(); context.SaveTo(); return true;
        }
        public void Exit()
        {
            if (phases.Current != GamePhase.Home) return;
            Cancel(); State.inside = false;
            context.World.motor.Teleport(State.outdoorPosition);
            phases.Set(GamePhase.Explore); context.SaveTo();
        }
        public void Restore()
        {
            Selection = null;
            if (State.inside) context.World.motor.Teleport(InteriorOrigin + new Vector3(4, .1f, 1));
            Changed?.Invoke();
        }
        public bool Select(string id)
        {
            if (phases.Current != GamePhase.Home || Available(id) < 1) return false;
            Selection = catalog.Furniture(id); X = 0; Z = 0; Turns = 0; Changed?.Invoke(); return Selection != null;
        }
        public void Move(int x, int z)
        {
            if (phases.Current != GamePhase.Home || Selection == null) return;
            X = x; Z = z; Changed?.Invoke();
        }
        public void Rotate()
        {
            if (phases.Current != GamePhase.Home || Selection == null) return;
            Turns = (Turns + 1) % 4; Changed?.Invoke();
        }
        public void Cancel() { Selection = null; Changed?.Invoke(); }
        public bool Place()
        {
            if (phases.Current != GamePhase.Home || !IsValid || Available(Selection.id) < 1) return false;
            State.stock.First(s => s.id == Selection.id).count--;
            State.placed.Add(new PlacedFurniture { instanceId = Guid.NewGuid().ToString("N"), furnitureId = Selection.id, x = X, z = Z, turns = Turns });
            Cancel(); context.SaveTo(); return true;
        }
        public bool Recall(string instanceId)
        {
            if (phases.Current != GamePhase.Home) return false;
            var item = State.placed.FirstOrDefault(p => p.instanceId == instanceId);
            if (item == null) return false;
            Stock(item.furnitureId).count++; State.placed.Remove(item); Changed?.Invoke(); context.SaveTo(); return true;
        }
        public FurnitureStock Stock(string id)
        {
            var item = State.stock.FirstOrDefault(s => s.id == id);
            if (item == null) { item = new FurnitureStock { id = id }; State.stock.Add(item); }
            return item;
        }
    }
}
