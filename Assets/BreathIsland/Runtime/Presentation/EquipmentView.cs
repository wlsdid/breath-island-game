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
        public void Attack(WeaponDefinition weapon, bool skill, Vector3 target)
        {
            socket.GetComponent<WeaponActionView>().Play();
            var bow = held == null ? null : held.GetComponent<BowShotView>();
            if (bow != null) bow.Play(target, skill, weapon.skill != null ? weapon.skill.effectColor : Color.white);
            else if (skill) socket.GetComponent<WeaponSkillView>().Play(weapon.job.id, target, weapon.skill.effectColor);
        }
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
            anchor.AddComponent<WeaponActionView>().Initialize(player);
            anchor.AddComponent<WeaponSkillView>();
            equipment.Changed += Refresh; Refresh();
        }
        private void Refresh()
        {
            socket.GetComponent<ToolSwing>().SetHeldTool(equipment.Tool != LifeTool.None);
            socket.GetComponent<WeaponActionView>().Configure(equipment.Tool==LifeTool.None?equipment.Job?.id:null);
            if (held != null) { held.SetActive(false); Object.Destroy(held); }
            GameObject source = equipment.Tool switch
            {
                LifeTool.Axe => catalog.axeModel,
                LifeTool.Pickaxe => catalog.pickaxeModel,
                LifeTool.Hammer => catalog.hammerModel,
                _ => equipment.Weapon?.model
            };
            held = source == null ? null : ModelPresentation.Create(source, socket,
                equipment.Tool == LifeTool.None ? (equipment.Weapon?.skill != null ? null : catalog.weaponMaterial) : catalog.toolMaterial, equipment.Tool == LifeTool.None ? 1.05f : .58f, true);
            if (held != null)
            {
                var scale = socket.lossyScale;
                held.transform.localScale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);
                if (equipment.Tool == LifeTool.None && equipment.Job?.id == "warrior") held.transform.localRotation = Quaternion.Euler(0, 90, 0);
                if (equipment.Tool == LifeTool.None && equipment.Job?.id == "ranger") held.AddComponent<BowShotView>().Initialize(socket.GetComponent<WeaponActionView>().Owner, socket.GetComponent<WeaponActionView>());
            }
        }
        public void Dispose() { equipment.Changed -= Refresh; if (socket != null) Object.Destroy(socket.gameObject); }
    }
}
