using UnityEngine;
using UnityEngine.UI;
namespace BreathIsland
{
    // Original vector UI: rounded paper, stitched cloth and a wave signature.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class NotebookGraphic : MaskableGraphic
    {
        public bool stitched;
        public bool waves;
        public bool buttonFace;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = rectTransform.rect;
            float radius = Mathf.Min(22, Mathf.Min(r.width, r.height) / 3);
            int center = mesh.currentVertCount; mesh.AddVert(r.center, color, Vector2.zero);
            for (int corner = 0; corner < 4; corner++)
            {
                var point = new Vector2(corner < 2 ? r.xMax - radius : r.xMin + radius, corner == 0 || corner == 3 ? r.yMax - radius : r.yMin + radius);
                for (int step = 0; step <= 8; step++)
                {
                    float a = (90 - corner * 90 - step * 90f / 8) * Mathf.Deg2Rad;
                    mesh.AddVert(point + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, color, Vector2.zero);
                }
            }
            for (int i = 1; i <= 36; i++) mesh.AddTriangle(center, i, i == 36 ? 1 : i + 1);
            if (buttonFace)
            {
                var thread=new Color(.16f,.29f,.27f,.32f);
                for(float x=r.xMin+24;x<r.xMax-27;x+=14)
                {
                    Line(mesh,new Vector2(x,r.yMin+7),new Vector2(x+6,r.yMin+7),thread,1.3f);
                    Line(mesh,new Vector2(x,r.yMax-7),new Vector2(x+6,r.yMax-7),thread,1.3f);
                }
                // Folded label corner and a hand-drawn arrow instead of a stock capsule button.
                int fold=mesh.currentVertCount;
                Color foldColor=Color.Lerp(color,Color.white,.4f);
                mesh.AddVert(new Vector2(r.xMin+8,r.yMax-3),foldColor,Vector2.zero);
                mesh.AddVert(new Vector2(r.xMin+27,r.yMax-3),foldColor,Vector2.zero);
                mesh.AddVert(new Vector2(r.xMin+8,r.yMax-20),foldColor,Vector2.zero);
                mesh.AddTriangle(fold,fold+1,fold+2);
                var arrow=new Color(.98f,.95f,.86f,.8f);
                float ax=r.xMax-25,ay=r.center.y;
                Line(mesh,new Vector2(ax-9,ay-1),new Vector2(ax+5,ay+1),arrow,2);
                Line(mesh,new Vector2(ax,ay+7),new Vector2(ax+6,ay+1),arrow,2);
                Line(mesh,new Vector2(ax+6,ay+1),new Vector2(ax,ay-6),arrow,2);
            }
            if (stitched)
            {
                Color thread = new Color(.84f, .77f, .61f, .7f);
                for (float x = r.xMin + 30; x < r.xMax - 28; x += 18)
                { Line(mesh, new Vector2(x, r.yMax - 12), new Vector2(x + 8, r.yMax - 12), thread, 1); Line(mesh, new Vector2(x, r.yMin + 12), new Vector2(x + 8, r.yMin + 12), thread, 1); }
            }
            if (waves)
            {
                for (int row = 0; row < 2; row++) for (int i = 0; i < 50; i++)
                {
                    float x = r.xMax - 145 + i * 2;
                    Line(mesh, new Vector2(x, r.yMin + 24 + row * 8 + Mathf.Sin(i * .22f) * 3), new Vector2(x + 2, r.yMin + 24 + row * 8 + Mathf.Sin((i + 1) * .22f) * 3), new Color(.21f,.52f,.50f,.45f), 1.5f);
                }
            }
        }
        public static void Line(VertexHelper mesh, Vector2 a, Vector2 b, Color color, float width)
        {
            var normal = new Vector2(-(b-a).y, (b-a).x).normalized * width / 2;
            int start = mesh.currentVertCount;
            mesh.AddVert(a-normal,color,Vector2.zero); mesh.AddVert(a+normal,color,Vector2.zero);
            mesh.AddVert(b+normal,color,Vector2.zero); mesh.AddVert(b-normal,color,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2); mesh.AddTriangle(start,start+2,start+3);
        }
    }
}
