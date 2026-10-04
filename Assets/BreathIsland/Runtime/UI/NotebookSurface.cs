using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace BreathIsland
{
    public enum NotebookStyle { Paper, Cloth, Title, Body, Small, Center, Light, Button, Primary, Selected }
    // Retained uGUI nodes are reconciled in place, never rebuilt per frame.
    public sealed class NotebookSurface : IDisposable
    {
        public static readonly Color Ink = new(.12f,.27f,.28f), Paper = new(.98f,.95f,.86f), Sea = new(.25f,.57f,.56f), Coral = new(.80f,.34f,.25f);
        private readonly GameObject root;
        private readonly RectTransform content;
        private readonly List<GameObject> nodes = new();
        private readonly Font titleFont, bodyFont;
        private int used;
        private readonly CanvasGroup group;
        private readonly GameObject elementPrefab;
        private float opened;
        private static int instances;
        private static GameObject ownedEvents;
        public bool Visible => root.activeSelf;
        public NotebookSurface(string name, int order = 0)
        {
            var prefab=Resources.Load<GameObject>("UI/Screens/"+name);
            if(prefab==null) throw new InvalidOperationException("Missing authored screen prefab: "+name);
            root=UnityEngine.Object.Instantiate(prefab); root.name=name;
            var canvas=root.GetComponent<Canvas>(); canvas.sortingOrder=order;
            content=root.GetComponent<RectTransform>(); group=root.GetComponent<CanvasGroup>();
            titleFont=Resources.Load<Font>("UI/Fonts/Gaegu-Bold"); bodyFont=Resources.Load<Font>("UI/Fonts/GowunDodum-Regular");
            elementPrefab=Resources.Load<GameObject>("UI/Elements/NotebookElement");
            if(elementPrefab==null)throw new InvalidOperationException("Missing authored notebook element prefab");
            if(titleFont==null || bodyFont==null) throw new InvalidOperationException("Missing notebook fonts");
            root.SetActive(false);
            instances++;
            if(EventSystem.current==null)
            {
                var events=new GameObject("Notebook Event System",typeof(EventSystem),typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                ownedEvents=events;
            }
        }
        public void Begin()
        {
            if(!root.activeSelf) {root.SetActive(true);opened=Time.unscaledTime;}
            group.alpha=Mathf.Clamp01((Time.unscaledTime-opened)/.14f);
            used=0;
        }
        public void End(){ for(int i=used;i<nodes.Count;i++)nodes[i].SetActive(false); }
        public void Hide()=>root.SetActive(false);
        private GameObject Node(Rect rect,int kind=0)
        {
            GameObject node;
            if(used==nodes.Count)
            {
                node=UnityEngine.Object.Instantiate(elementPrefab,content,false);nodes.Add(node);
            } else node=nodes[used];
            used++;node.SetActive(true);
            var transform=(RectTransform)node.transform;transform.anchorMin=transform.anchorMax=new Vector2(0,1);transform.pivot=new Vector2(0,1);transform.anchoredPosition=new Vector2(rect.x,-rect.y);transform.sizeDelta=rect.size;
            node.name="Element";
            node.GetComponent<InventorySlotDrag>()?.Unbind();
            node.GetComponent<NotebookGraphic>().enabled=kind==0||kind==1;node.GetComponent<Button>().enabled=kind==1;
            node.GetComponent<NotebookButtonMotion>().enabled=kind==1;
            node.GetComponent<NotebookGraphic>().buttonFace=false;
            node.GetComponentInChildren<Text>(true).gameObject.SetActive(kind==1||kind==2);node.GetComponentInChildren<IslandIcon>(true).gameObject.SetActive(kind==3);
            return node;
        }
        public void Panel(Rect rect,string text="",NotebookStyle style=NotebookStyle.Paper)
        {
            var node=Node(rect);var graphic=node.GetComponent<NotebookGraphic>();graphic.enabled=true;graphic.raycastTarget=false;
            graphic.color=style==NotebookStyle.Cloth?Ink:Paper;graphic.stitched=rect.height>90;graphic.waves=rect.height>180;graphic.SetVerticesDirty();
            if(!string.IsNullOrEmpty(text)) SetLabel(node,text,style==NotebookStyle.Cloth?NotebookStyle.Light:NotebookStyle.Body);
        }
        public void Label(Rect rect,string text,NotebookStyle style=NotebookStyle.Body)
        {
            // Korean font line metrics need more height than the visible glyph body.
            if(rect.height<40){rect.y-=(40-rect.height)/2;rect.height=40;}
            SetLabel(Node(rect,2),text,style);
        }
        private void SetLabel(GameObject node,string value,NotebookStyle style)
        {
            var text=node.GetComponentInChildren<Text>(true);text.gameObject.SetActive(true);text.text=value;text.font=style==NotebookStyle.Title?titleFont:bodyFont;
            text.fontSize=style==NotebookStyle.Title?42:style==NotebookStyle.Small?18:22;
            text.resizeTextForBestFit=true;text.resizeTextMinSize=style==NotebookStyle.Title?24:16;text.resizeTextMaxSize=text.fontSize;
            text.color=style==NotebookStyle.Light?Paper:Ink;text.alignment=style==NotebookStyle.Center||style==NotebookStyle.Light?TextAnchor.MiddleCenter:TextAnchor.MiddleLeft;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
            text.rectTransform.offsetMin=new Vector2(14,3);text.rectTransform.offsetMax=new Vector2(-14,-3);
        }
        public void Button(Rect rect,string text,Action action,NotebookStyle style=NotebookStyle.Button,bool enabled=true,bool compact=false)
        {
            var node=Node(rect,1); node.name="Action: "+text;
            var graphic=node.GetComponent<NotebookGraphic>();graphic.enabled=true;graphic.raycastTarget=true;graphic.stitched=false;graphic.waves=false;
            graphic.buttonFace=!compact && !string.IsNullOrEmpty(text);
            graphic.color=style==NotebookStyle.Primary?Coral:style==NotebookStyle.Selected?Sea:new Color(.88f,.88f,.76f);graphic.SetVerticesDirty();
            var button=node.GetComponent<Button>();button.enabled=true;button.targetGraphic=graphic;button.interactable=enabled;button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action());
            var colors=button.colors;colors.highlightedColor=new Color(1.08f,1.08f,1.08f);colors.selectedColor=Color.white;colors.disabledColor=new Color(.7f,.7f,.7f,.55f);button.colors=colors;
            SetLabel(node,text,style==NotebookStyle.Primary||style==NotebookStyle.Selected?NotebookStyle.Light:NotebookStyle.Center);
            var label=node.GetComponentInChildren<Text>();label.font=titleFont;label.fontSize=30;label.resizeTextMaxSize=30;label.resizeTextMinSize=20;
            label.rectTransform.offsetMin=new Vector2(compact?8:22,3);label.rectTransform.offsetMax=new Vector2(compact?-8:-36,-3);
        }
        public void Icon(Rect rect,string symbol,Color? color=null)
        {var node=Node(rect,3);var icon=node.GetComponentInChildren<IslandIcon>(true);icon.gameObject.SetActive(true);icon.symbol=symbol;icon.color=color??Ink;icon.SetVerticesDirty();icon.SetMaterialDirty();}
        public void InventorySlot(Rect rect,InventoryDragState drag,int index,InventoryItem item,Action select,Action<int,int,string> move,bool selected)
        {
            Button(rect,"",select,selected?NotebookStyle.Selected:NotebookStyle.Button,true,true);
            var node=nodes[used-1];node.name="Inventory slot "+index;
            var handler=node.GetComponent<InventorySlotDrag>()??node.AddComponent<InventorySlotDrag>();
            handler.Bind(drag,index,item,move);
        }
        public void Quantity(Rect rect,int count)
        {
            Panel(rect);
            var node=Node(rect,2);SetLabel(node,count.ToString(),NotebookStyle.Center);
            var text=node.GetComponentInChildren<Text>();text.font=titleFont;text.fontSize=26;
            text.resizeTextMaxSize=26;text.resizeTextMinSize=14;
            text.rectTransform.offsetMin=new Vector2(3,0);text.rectTransform.offsetMax=new Vector2(-3,0);
        }
        public void Meter(Rect rect,string label,float percent,Color color,NotebookStyle labelStyle=NotebookStyle.Small)
        { Label(new Rect(rect.x,rect.y-30,rect.width,28),label,labelStyle);Panel(rect);var node=Node(new Rect(rect.x+3,rect.y+3,Mathf.Max(1,(rect.width-6)*Mathf.Clamp01(percent/100)),rect.height-6));var g=node.GetComponent<NotebookGraphic>();g.enabled=true;g.color=color;g.raycastTarget=false;g.stitched=g.waves=false;g.SetVerticesDirty(); }
        public void Dispose(){Hide();UnityEngine.Object.Destroy(root);if(--instances==0&&ownedEvents!=null){UnityEngine.Object.Destroy(ownedEvents);ownedEvents=null;}}
    }
}
