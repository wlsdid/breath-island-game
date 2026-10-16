using UnityEngine;
namespace BreathIsland
{
    public sealed class SkillFlipbook : MonoBehaviour
    {
        public int columns = 6, rows = 5, firstFrame = 1, frameCount = 29;
        public float duration = .8f;
        private Renderer surface;
        private MaterialPropertyBlock properties;
        private float elapsed;
        public bool Active => surface != null && surface.enabled;
        public static Vector4 FrameUv(int frame, int columns, int rows)
        {
            frame = Mathf.Clamp(frame, 0, columns * rows - 1);
            return new Vector4(1f / columns, 1f / rows, (float)(frame % columns) / columns,
                1f - (float)(frame / columns + 1) / rows);
        }
        private void Awake()
        {
            surface = GetComponent<Renderer>(); properties = new MaterialPropertyBlock(); Stop();
        }
        public void Play(Vector3 position)
        {
            transform.position = position; elapsed = 0; surface.enabled = true; ApplyFrame();
        }
        public void Stop() { if (surface != null) surface.enabled = false; }
        private void LateUpdate()
        {
            if (!Active) return;
            elapsed += Time.deltaTime;
            if (elapsed >= duration) { Stop(); return; }
            ApplyFrame();
            var camera = Camera.main;
            if (camera != null) transform.rotation = camera.transform.rotation;
        }
        private void ApplyFrame()
        {
            int frame = firstFrame + Mathf.Min(frameCount - 1, Mathf.FloorToInt(elapsed / duration * frameCount));
            properties.SetVector("_Frame", FrameUv(frame, columns, rows));
            surface.SetPropertyBlock(properties);
        }
        private void OnDisable() => Stop();
    }
}
