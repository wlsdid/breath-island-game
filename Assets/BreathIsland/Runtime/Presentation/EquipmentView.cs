using UnityEngine;
namespace BreathIsland
{
    public sealed class EquipmentView : System.IDisposable
    {
        private readonly EquipmentSystem equipment;
        private readonly IslandLifeCatalog catalog;
        private readonly Transform socket;
        private GameObject held;
        public GameObject Held => held;
        public void SetVisible(bool visible) { socket.gameObject.SetActive(visible); }
        public void Swing() => socket.GetComponent<ToolSwing>().Play();
        public EquipmentView(EquipmentSystem equipment, IslandLifeCatalog catalog, Transform player)
        {
            this.equipment = equipment; this.catalog = catalog;
            var anchor = new GameObject("Held Item Socket"); anchor.transform.SetParent(player, false);
            // Use the imported skeleton when available, otherwise a stable character-local socket.
            var transforms = player.GetComponentsInChildren<Transform>();
            var hand = System.Array.Find(transforms, t => t.name == "handslot.r")
                ?? System.Array.Find(transforms, t => t.name == "hand.r" || t.name == "hand_r" || t.name == "RightHand");
            if (hand != null) anchor.transform.SetParent(hand, false);
            else anchor.transform.localPosition = new Vector3(.5f, .85f, .25f);
            socket = anchor.transform;
            anchor.AddComponent<ToolSwing>().Initialize(player);
            equipment.Changed += Refresh; Refresh();
        }
        private void Refresh()
        {
            socket.GetComponent<ToolSwing>().SetHeldTool(equipment.Tool != LifeTool.None);
            if (held != null) { held.SetActive(false); Object.Destroy(held); }
            GameObject source = equipment.Tool switch
            {
                LifeTool.Axe => catalog.axeModel,
                LifeTool.Pickaxe => catalog.pickaxeModel,
                LifeTool.Hammer => catalog.hammerModel,
                _ => equipment.Weapon?.model
            };
            held = source == null ? null : ModelPresentation.Create(source, socket,
                equipment.Tool == LifeTool.None ? catalog.weaponMaterial : catalog.toolMaterial, .85f, true);
        }
        public void Dispose() { equipment.Changed -= Refresh; if (socket != null) Object.Destroy(socket.gameObject); }
    }
}
