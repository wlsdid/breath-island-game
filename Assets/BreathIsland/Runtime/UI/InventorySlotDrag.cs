using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace BreathIsland
{
    public sealed class InventoryDragState
    {
        public int From=-1;
        public string Key;
        public GameObject Ghost;
        public void Cancel(){From=-1;Key=null;if(Ghost!=null)UnityEngine.Object.Destroy(Ghost);Ghost=null;}
    }
    public sealed class InventorySlotDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler
    {
        private InventoryDragState state;
        private int index=-1;
        private InventoryItem item;
        private Action<int,int,string> move;
        public void Bind(InventoryDragState shared,int slot,InventoryItem value,Action<int,int,string> command)
        {state=shared;index=slot;item=value;move=command;}
        public void Unbind(){index=-1;item=null;move=null;}
        public void OnBeginDrag(PointerEventData data)
        {
            if(index<0 || item==null || data.button!=PointerEventData.InputButton.Left)return;
            state.Cancel();state.From=index;state.Key=item.Key;data.eligibleForClick=false;
            var canvas=GetComponentInParent<Canvas>();
            var ghost=new GameObject("Dragged item",typeof(RectTransform),typeof(CanvasRenderer),typeof(IslandIcon));
            ghost.transform.SetParent(canvas.transform,false);
            var rect=(RectTransform)ghost.transform;rect.sizeDelta=new Vector2(76,76);
            var icon=ghost.GetComponent<IslandIcon>();icon.symbol=item.Id;icon.color=NotebookSurface.Coral;icon.raycastTarget=false;
            state.Ghost=ghost;OnDrag(data);
        }
        public void OnDrag(PointerEventData data)
        {
            if(state?.Ghost==null)return;
            var parent=(RectTransform)state.Ghost.transform.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,data.position,data.pressEventCamera,out var point);
            ((RectTransform)state.Ghost.transform).anchoredPosition=point;
            state.Ghost.transform.SetAsLastSibling();
        }
        public void OnDrop(PointerEventData data)
        {
            if(index<0 || state==null || state.From<0)return;
            move?.Invoke(state.From,index,state.Key);state.Cancel();
        }
        public void OnEndDrag(PointerEventData data)=>state?.Cancel();
        private void OnDisable(){if(state!=null && state.From==index)state.Cancel();}
    }
}
