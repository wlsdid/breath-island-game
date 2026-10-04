using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace BreathIsland
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class IslandIcon : MaskableGraphic
    {
        public string symbol;
        private VertexHelper vertices;
        private static readonly Dictionary<string,Texture2D> Textures=new();
        private Texture2D ItemTexture
        {
            get
            {
                if(string.IsNullOrEmpty(symbol))return null;
                if(!Textures.TryGetValue(symbol,out var texture)){texture=Resources.Load<Texture2D>("UI/ItemIcons/"+symbol);Textures[symbol]=texture;}
                return texture;
            }
        }
        public override Texture mainTexture=>ItemTexture!=null?ItemTexture:base.mainTexture;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); vertices = mesh;
            if(ItemTexture!=null)
            {
                var rect=rectTransform.rect;var tint=new Color(1,1,1,color.a);
                mesh.AddVert(new Vector2(rect.xMin,rect.yMin),tint,new Vector2(0,0));
                mesh.AddVert(new Vector2(rect.xMin,rect.yMax),tint,new Vector2(0,1));
                mesh.AddVert(new Vector2(rect.xMax,rect.yMax),tint,new Vector2(1,1));
                mesh.AddVert(new Vector2(rect.xMax,rect.yMin),tint,new Vector2(1,0));
                mesh.AddTriangle(0,1,2);mesh.AddTriangle(0,2,3);return;
            }
            string id = symbol ?? "bag";
            if(id=="reticle"){Circle(50,50,14);return;}
            if(id=="heart") {Path(50,17,15,51,15,71,29,84,44,80,50,71,56,80,71,84,85,71,85,51,50,17);return;}
            if(id=="materials") {Path(15,62,32,85,69,85,86,62,50,15,15,62,86,62);Path(32,85,38,62,50,15,62,62,69,85);return;}
            if (id.Contains("sword")) { Path(25,18,72,82,82,88,83,74,35,15); Path(19,30,44,11); }
            else if (id.Contains("bow")) { Path(30,14,62,29,73,50,62,71,30,86); Path(30,14,45,50,30,86); Path(18,50,85,50,72,62); }
            else if (id.Contains("staff")) { Path(35,12,61,70,50,82,60,92,73,86,69,73,61,70); }
            else if (id.Contains("Axe") || id.Contains("axe") || id.Contains("Pick"))
            { Path(27,12,65,81); if(id.Contains("Pick") || id.Contains("pick")) Path(30,72,57,85,81,79,91,64); else {Path(54,80,36,77,26,57,48,52,64,66); Path(54,80,64,66);}}
            else if (id.Contains("Hammer") || id.Contains("hammer")) { Path(37,12,58,69);Path(32,70,40,89,82,74,75,55,32,70); }
            else if(id.Contains("wood")) { Path(19,23,27,70,71,86,81,40,19,23); Path(27,70,81,40);Path(31,36,39,61);Path(52,42,59,55); }
            else if(id.Contains("stone")) { Path(13,28,30,71,61,85,87,57,78,19,13,28);Path(30,71,49,41,78,19);Path(49,41,87,57); }
            else if(id.Contains("herb")) {Path(44,14,52,74);Path(48,39,23,47,15,70,37,65,48,39);Path(51,58,72,66,83,88,60,81,51,58);}
            else if(id=="chest") {Path(15,20,15,65,25,82,75,82,85,65,85,20,15,20);Path(15,58,85,58);Path(43,66,57,66,57,48,43,48,43,66);Path(25,20,25,58);Path(75,20,75,58);}
            else if(id=="couch") {Path(13,18,13,58,24,58,24,79,76,79,76,58,87,58,87,18);Path(13,35,87,35);Path(24,58,76,58);}
            else if(id=="cabinet"||id=="shelf") {Path(20,15,20,87,80,87,80,15,20,15);Path(20,40,80,40);Path(20,64,80,64);if(id=="cabinet")Path(50,15,50,87);}
            else if(id.Contains("chair")) {Path(25,15,25,72,71,72,71,15);Path(25,42,74,42);Path(35,72,35,88,66,88,66,72);}
            else if(id.Contains("table")) {Path(12,61,86,61,76,78,24,78,12,61);Path(22,61,22,18);Path(76,61,76,18);}
            else if(id.Contains("bed")) {Path(15,18,15,66,85,66,85,18);Path(15,36,85,36);Path(15,66,15,86,45,86,45,66);}
            else if(id.Contains("lamp")) {Path(36,15,66,15);Path(51,15,51,57);Path(23,57,36,85,67,85,79,57,23,57);}
            else if(id.Contains("coin")) { Circle(50,50,32);Path(56,72,41,72,39,53,60,48,59,30,42,29); }
            else if(id.Contains("fish") || id.Contains("bait")) {Path(19,48,39,73,66,70,81,50,63,28,39,27,19,48,8,65,8,33,19,48);Circle(63,53,3);}
            else if(id.Contains("sun")) {Circle(50,50,18);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Path(50+Mathf.Cos(a)*28,50+Mathf.Sin(a)*28,50+Mathf.Cos(a)*38,50+Mathf.Sin(a)*38);}}
            else if(id.Contains("lock")) {Path(27,15,27,58,75,58,75,15,27,15);Path(36,58,36,79,50,90,66,79,66,58);}
            else {Path(18,14,18,69,82,69,82,14,18,14);Path(33,69,33,86,67,86,67,69);Path(18,51,82,51);Path(46,57,46,42,56,42,56,57);}
        }
        private void Circle(float x,float y,float radius) {for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;Path(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius);}}
        private void Path(params float[] points)
        {
            var r=rectTransform.rect; float scale=Mathf.Min(r.width,r.height)/100;
            for(int i=0;i<points.Length-2;i+=2)
                NotebookGraphic.Line(vertices,r.center+new Vector2(points[i]-50,points[i+1]-50)*scale,r.center+new Vector2(points[i+2]-50,points[i+3]-50)*scale,color,Mathf.Max(2,scale*4));
        }
    }
}
