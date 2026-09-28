using UnityEngine;
namespace BreathIsland
{
    public sealed class InteriorFinishView : System.IDisposable
    {
        private readonly InteriorFinishSystem system;
        private readonly Transform room;
        private readonly Transform subfloor;
        private readonly Vector3 subfloorPosition;
        public InteriorFinishView(InteriorFinishSystem system, Transform room)
        {
            this.system = system; this.room = room;
            subfloor = room.Find("Subfloor"); if (subfloor != null) subfloorPosition = subfloor.localPosition;
            system.Changed += Refresh; Refresh();
        }
        private void Refresh()
        {
            var wall = system.Resolve(system.WallId, FinishSurface.Wall).material;
            var floor = system.Resolve(system.FloorId, FinishSurface.Floor).material;
            bool tiles = system.FloorId == "tiles";
            if (subfloor != null) subfloor.localPosition = subfloorPosition + (tiles ? Vector3.up * .03f : Vector3.zero);
            foreach (Transform part in room)
            {
                if (part.name == "planks") part.gameObject.SetActive(!tiles);
                Material material = part.name.StartsWith("wall-") ? wall : part.name == "planks" || part.name == "Subfloor" ? floor : null;
                if (material == null) continue;
                foreach (var renderer in part.GetComponentsInChildren<Renderer>())
                { var slots = renderer.sharedMaterials; for (int i = 0; i < slots.Length; i++) slots[i] = material; renderer.sharedMaterials = slots; }
            }
        }
        public void Dispose() => system.Changed -= Refresh;
    }
}
