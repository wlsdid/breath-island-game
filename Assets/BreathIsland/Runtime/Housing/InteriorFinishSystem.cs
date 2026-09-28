using System;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class InteriorFinishSystem : IDisposable
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        public FinishDefinition[] Definitions { get; }
        public string WallId { get; private set; }
        public string FloorId { get; private set; }
        public bool Previewing { get; private set; }
        public event Action Changed;
        public InteriorFinishSystem(GameContext context, GamePhaseController phases)
        {
            this.context = context; this.phases = phases;
            Definitions = Resources.LoadAll<FinishDefinition>("Content/Finishes");
            context.Loaded += Cancel; phases.Changed += PhaseChanged; Cancel();
        }
        private bool Allowed => context.Save.housing.inside && phases.Current == GamePhase.Equipment && phases.BeforeInventory == GamePhase.Home;
        public bool Begin() { if (!Allowed) return false; Cancel(); Previewing = true; return true; }
        public bool Preview(string id)
        {
            if (!Allowed || !Previewing) return false;
            var definition = Definitions.FirstOrDefault(d => d.id == id);
            if (definition == null) return false;
            if (definition.surface == FinishSurface.Wall) WallId = id; else FloorId = id;
            Changed?.Invoke(); return true;
        }
        public bool Apply()
        {
            if (!Allowed || !Previewing) return false;
            context.Save.housing.wallFinishId = WallId; context.Save.housing.floorFinishId = FloorId;
            Previewing = false; context.SaveTo(); Changed?.Invoke(); return true;
        }
        public void Cancel()
        {
            Previewing = false;
            WallId = Resolve(context.Save.housing.wallFinishId, FinishSurface.Wall).id;
            FloorId = Resolve(context.Save.housing.floorFinishId, FinishSurface.Floor).id;
            Changed?.Invoke();
        }
        public FinishDefinition Resolve(string id, FinishSurface surface) => Definitions.FirstOrDefault(d => d.id == id && d.surface == surface)
            ?? Definitions.First(d => d.id == (surface == FinishSurface.Wall ? "wood_dark" : "planks_light"));
        private void PhaseChanged() { if (!Allowed) Cancel(); }
        public void Dispose() { context.Loaded -= Cancel; phases.Changed -= PhaseChanged; }
    }
}
