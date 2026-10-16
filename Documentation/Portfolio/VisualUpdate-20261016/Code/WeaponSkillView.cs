using UnityEngine;
namespace BreathIsland
{
    // Presentation only: rules remain in TrainingCombatSystem.
    public sealed class WeaponSkillView : MonoBehaviour
    {
        private SkillFlipbook effect;
        private string currentKind;
        public bool Active => effect != null && effect.Active;
        public void Play(string kind, Vector3 target, Color color)
        {
            if (effect == null || currentKind != kind)
            {
                if (effect != null) Destroy(effect.gameObject);
                var prefab = Resources.Load<SkillFlipbook>("World/SkillVfx/" + kind);
                if (prefab == null) return;
                effect = Instantiate(prefab); currentKind = kind;
            }
            effect.Play(target);
        }
        private void OnDisable() { if (effect != null) effect.Stop(); }
        private void OnDestroy() { if (effect != null) Destroy(effect.gameObject); }
    }
}
