using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    [Serializable] public sealed class FloorCell { public int x,z; public bool paid; }
    // Horizontal edges run along X at z; vertical edges run along Z at x.
    [Serializable] public sealed class WallEdge { public int x,z; public bool vertical; }
    [Serializable] public sealed class HomeStructureState
    {
        public bool customized;
        public List<FloorCell> cells = new();
        public List<WallEdge> walls = new();
    }
    public static class HomeFootprint
    {
        public const int Limit=32;
        public static HomeStructureState Copy(HomeStructureState state)
        {
            if(state?.customized==true)return JsonUtility.FromJson<HomeStructureState>(JsonUtility.ToJson(state));
            var result=new HomeStructureState{customized=true}; var layout=RoomLayoutDefinition.Default;
            for(int z=0;z<layout.depth;z++)for(int x=0;x<layout.width;x++)
                if(layout.Contains(new Rect(x,z,1,1)))result.cells.Add(new FloorCell{x=x,z=z});
            return result;
        }
        public static bool Cell(HomeStructureState state,int x,int z)=>state?.customized==true
            ? state.cells.Any(c=>c.x==x&&c.z==z):RoomLayoutDefinition.Default.Contains(new Rect(x,z,1,1));
        public static Rect WallArea(WallEdge w)=>w.vertical?new Rect(w.x-.08f,w.z,.16f,1):new Rect(w.x,w.z-.08f,1,.16f);
        public static bool Contains(HomeStructureState state,Rect area)
        {
            for(int z=Mathf.FloorToInt(area.yMin);z<Mathf.CeilToInt(area.yMax);z++)
                for(int x=Mathf.FloorToInt(area.xMin);x<Mathf.CeilToInt(area.xMax);x++)if(!Cell(state,x,z))return false;
            return state?.customized!=true||!state.walls.Any(w=>area.Overlaps(WallArea(w)));
        }
        public static Vector2 Size(HomeStructureState state)=>state?.customized==true&&state.cells.Count>0
            ?new Vector2(state.cells.Max(c=>c.x)+1,state.cells.Max(c=>c.z)+1)
            :new Vector2(RoomLayoutDefinition.Default.width,RoomLayoutDefinition.Default.depth);
        public static bool Connected(HomeStructureState state)
        {
            var remaining=new HashSet<Vector2Int>(state.cells.Select(c=>new Vector2Int(c.x,c.z)));
            var arrival=Vector2Int.FloorToInt(RoomLayoutDefinition.Default.arrival);
            if(!remaining.Remove(arrival))return false;
            var queue=new Queue<Vector2Int>();queue.Enqueue(arrival);
            while(queue.Count>0)
            {
                var p=queue.Dequeue();
                foreach(var step in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})
                {
                    var n=p+step;bool vertical=step.x!=0;
                    int x=vertical?Mathf.Max(p.x,n.x):p.x,z=vertical?p.y:Mathf.Max(p.y,n.y);
                    if(state.walls.Any(w=>w.x==x&&w.z==z&&w.vertical==vertical))continue;
                    if(remaining.Remove(n))queue.Enqueue(n);
                }
            }
            return remaining.Count==0;
        }
    }
}
