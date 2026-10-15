using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    // Rebind only visible skins; locomotion, hand sockets and seated poses retain their skeleton.
    public sealed class JobAppearanceView : IDisposable
    {
        private readonly EquipmentSystem equipment;
        private readonly SkinnedMeshRenderer[] original;
        private readonly Dictionary<string, GameObject> variants = new();
        public string CurrentJob { get; private set; }
        public JobAppearanceView(EquipmentSystem equipment, Transform player)
        {
            this.equipment = equipment;
            var visual = player.GetComponentsInChildren<Transform>().First(t => t.name == "Hero Visual" && t.gameObject.activeSelf);
            original = visual.GetComponentsInChildren<SkinnedMeshRenderer>();
            var bones = visual.GetComponentsInChildren<Transform>().GroupBy(t => t.name).ToDictionary(g => g.Key, g => g.First());
            foreach (var job in new[] { "warrior", "mage" })
            {
                var prefab = Resources.Load<GameObject>("World/JobAppearances/" + job);
                var group = new GameObject(job + " outfit"); group.transform.SetParent(original[0].transform.parent, false);
                foreach (var source in prefab.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var part = new GameObject(source.name); part.transform.SetParent(group.transform, false);
                    part.transform.localPosition = source.transform.localPosition;
                    part.transform.localRotation = source.transform.localRotation;
                    part.transform.localScale = source.transform.localScale;
                    var skin = part.AddComponent<SkinnedMeshRenderer>(); skin.sharedMesh = source.sharedMesh;
                    skin.sharedMaterials = source.sharedMaterials;
                    skin.bones = source.bones.Select(b => bones[b.name]).ToArray();
                    skin.rootBone = bones[source.rootBone.name]; skin.localBounds = source.localBounds;
                    skin.updateWhenOffscreen = true;
                }
                group.SetActive(false); variants.Add(job, group);
            }
            equipment.Changed += Refresh; Refresh();
        }
        private void Refresh()
        {
            CurrentJob = equipment.Job?.id ?? "ranger";
            bool custom = variants.ContainsKey(CurrentJob);
            foreach (var skin in original) skin.enabled = !custom;
            foreach (var pair in variants) pair.Value.SetActive(pair.Key == CurrentJob);
        }
        public void Dispose()
        {
            equipment.Changed -= Refresh;
            foreach (var skin in original) if (skin != null) skin.enabled = true;
            foreach (var group in variants.Values) UnityEngine.Object.Destroy(group);
        }
    }
}
