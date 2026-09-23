using UnityEngine;
namespace BreathIsland
{
    public abstract class HudScreen
    {
        protected readonly GameContext context;
        protected readonly GamePhaseController phases;
        protected NotebookSurface ui;
        protected const NotebookStyle panelStyle=NotebookStyle.Paper, titleStyle=NotebookStyle.Title,
            bodyStyle=NotebookStyle.Body, smallStyle=NotebookStyle.Small, centeredStyle=NotebookStyle.Center,
            buttonStyle=NotebookStyle.Button, primaryButtonStyle=NotebookStyle.Primary;
        protected HudScreen(GameContext context,GamePhaseController phases){this.context=context;this.phases=phases;}
        public abstract void Draw(float width,float height);
        protected void EnsureStyles(){ui ??= new NotebookSurface(GetType().Name, GetType()==typeof(HudOverlay)?20:10);}
        public void Render(float width,float height){EnsureStyles();ui.Begin();Draw(width,height);ui.End();}
        public void Hide()=>ui?.Hide();
        protected void DrawMeter(Rect rect,string label,float value,Color color)=>ui.Meter(rect,label,value,color);
        protected static NotebookStyle DarkBox()=>NotebookStyle.Cloth;
        protected static NotebookStyle CenteredLight()=>NotebookStyle.Light;
        public virtual void Dispose()=>ui?.Dispose();
    }
}
