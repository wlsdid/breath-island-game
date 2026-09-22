using UnityEngine;
namespace BreathIsland
{
    public readonly struct HudCanvas : System.IDisposable
    {
        private readonly Matrix4x4 previous;
        public float Width { get; }
        public float Height { get; }
        public HudCanvas(bool begin)
        {
            previous = GUI.matrix;
            float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1600f, Screen.height / 900f), .1f, 1.35f);
            Width = Screen.width / scale; Height = Screen.height / scale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
        }
        public void Dispose() => GUI.matrix = previous;
    }
}
