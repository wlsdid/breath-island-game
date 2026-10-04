using UnityEngine;
namespace BreathIsland
{
    // Rebuilds only when a construction command or finish changes, never each frame.
    public sealed class HomeConstructionView : System.IDisposable
    {
        private readonly HomeConstructionSystem system;
        private readonly InteriorFinishSystem finishes;
        private readonly Transform authored;
        private readonly IslandCameraRig camera;
        private GameObject generated;
        public HomeConstructionView(HomeConstructionSystem system,InteriorFinishSystem finishes,Transform authored,IslandCameraRig camera)
        {
            this.system=system;this.finishes=finishes;this.authored=authored;this.camera=camera;
            system.Changed+=Refresh;finishes.Changed+=Refresh;Refresh();
        }
        private void Refresh()
        {
            if(generated!=null){generated.SetActive(false);Object.Destroy(generated);}
            var state=system.Display;camera.homeStructure=state;
            bool custom=state?.customized==true;authored.gameObject.SetActive(!custom);if(!custom)return;
            generated=new GameObject("Edited Interior");generated.transform.SetParent(authored.parent,false);
            generated.transform.position=HousingSystem.InteriorOrigin;
            var floor=Resources.Load<GameObject>("World/BuildFloor");var wall=Resources.Load<GameObject>("World/BuildWall");
            var floorMaterial=finishes.Resolve(finishes.FloorId,FinishSurface.Floor).material;
            var wallMaterial=finishes.Resolve(finishes.WallId,FinishSurface.Wall).material;
            foreach(var cell in state.cells)
            {
                Part(floor,new Vector3(cell.x+.5f,0,cell.z+.5f),0,floorMaterial);
                if(!HomeFootprint.Cell(state,cell.x-1,cell.z))Boundary(wall,cell.x,cell.z,true,wallMaterial);
                if(!HomeFootprint.Cell(state,cell.x+1,cell.z))Boundary(wall,cell.x+1,cell.z,true,wallMaterial);
                if(!HomeFootprint.Cell(state,cell.x,cell.z-1))Boundary(wall,cell.x,cell.z,false,wallMaterial);
                if(!HomeFootprint.Cell(state,cell.x,cell.z+1))Boundary(wall,cell.x,cell.z+1,false,wallMaterial);
            }
            foreach(var edge in state.walls)Boundary(wall,edge.x,edge.z,edge.vertical,wallMaterial);
            Physics.SyncTransforms();
        }
        private void Boundary(GameObject prefab,int x,int z,bool vertical,Material material)
        {
            // Low foreground/partition walls preserve the fixed camera's view; collision stays full height.
            var obj=Part(prefab,new Vector3(x+(vertical?0:.5f),0,z+(vertical?.5f:0)),vertical?90:0,material);
            bool doorway=!vertical&&z==0&&x>=3&&x<5;
            var model=obj.transform.Find("Visual");model.localScale=new Vector3(1,doorway?.025f:.34f,1);
        }
        private GameObject Part(GameObject prefab,Vector3 p,float yaw,Material material)
        {
            var obj=Object.Instantiate(prefab,generated.transform);obj.transform.localPosition=p;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);
            foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;
            return obj;
        }
        public void Dispose()
        {
            system.Changed-=Refresh;finishes.Changed-=Refresh;camera.homeStructure=null;
            if(generated!=null){generated.SetActive(false);Object.Destroy(generated);}
        }
    }
}
