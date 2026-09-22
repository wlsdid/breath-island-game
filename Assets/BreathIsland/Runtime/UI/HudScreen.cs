using UnityEngine;
namespace BreathIsland
{
    public abstract class HudScreen
    {
        protected readonly GameContext context;
        protected readonly GamePhaseController phases;
        protected GUIStyle panelStyle, titleStyle, bodyStyle, smallStyle, buttonStyle, primaryButtonStyle, centeredStyle;
        protected Texture2D panelTexture, darkTexture, tealTexture, paleTexture;
        protected HudScreen(GameContext context, GamePhaseController phases) { this.context = context; this.phases = phases; }
        public abstract void Draw(float width, float height);
        protected void DrawMeter(Rect rect, string label, float value, Color color)
        {
            GUI.Label(new Rect(rect.x, rect.y - 22, 160, 22), label, smallStyle);
            GUI.Box(rect, string.Empty, new GUIStyle(GUI.skin.box) { normal = { background = darkTexture } });
            Color previousColor = GUI.color; GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 3, (rect.width - 6) * Mathf.Clamp01(value / 100f), rect.height - 6), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(rect, $"{value:0}%", CenteredLight());
        }
        protected void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelTexture = Solid(new Color(.97f, .96f, .88f, .96f)); darkTexture = Solid(new Color(.04f, .16f, .18f, .9f));
            tealTexture = Solid(new Color(.05f, .55f, .6f, .97f)); paleTexture = Solid(new Color(.87f, .95f, .91f, .97f));
            panelStyle = new GUIStyle(GUI.skin.box) { normal = { background = panelTexture, textColor = new Color(.08f, .22f, .25f) }, padding = new RectOffset(24, 24, 22, 22) };
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.07f, .24f, .28f) } };
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true, normal = { textColor = new Color(.14f, .32f, .34f) } };
            smallStyle = new GUIStyle(bodyStyle) { fontSize = 14, normal = { textColor = new Color(.33f, .48f, .48f) } };
            centeredStyle = new GUIStyle(bodyStyle) { alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold, normal = { background = paleTexture, textColor = new Color(.08f, .3f, .32f) }, hover = { background = panelTexture }, padding = new RectOffset(12, 12, 10, 10) };
            primaryButtonStyle = new GUIStyle(buttonStyle) { normal = { background = tealTexture, textColor = Color.white }, hover = { background = tealTexture, textColor = Color.white } };
        }
        protected static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }
        protected GUIStyle DarkBox() => new(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleCenter, normal = { background = darkTexture, textColor = Color.white }, padding = new RectOffset(10, 10, 8, 8) };
        protected GUIStyle CenteredLight() => new(centeredStyle) { normal = { textColor = Color.white }, fontStyle = FontStyle.Bold };
        public virtual void Dispose()
        {
            foreach (var texture in new[] { panelTexture, darkTexture, tealTexture, paleTexture })
                if (texture != null) Object.Destroy(texture);
        }
    }
}
