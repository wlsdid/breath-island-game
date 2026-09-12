using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BreathIsland
{
    public sealed class GameHud
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly FishingSystem fishing;
        private readonly PredatorCombatSystem combat;
        private readonly RelationshipSystem relationships;
        private readonly FacilitySystem facilities;
        private readonly Func<string> eventMessage;
        private readonly Func<float> eventMessageUntil;
        private bool showHelp = true;
        private GUIStyle panelStyle, titleStyle, bodyStyle, smallStyle, buttonStyle, primaryButtonStyle, centeredStyle;
        private Texture2D panelTexture, darkTexture, tealTexture, paleTexture;

        public GameHud(GameContext context, GamePhaseController phases, FishingSystem fishing, PredatorCombatSystem combat,
            RelationshipSystem relationships, FacilitySystem facilities, Func<string> eventMessage, Func<float> eventMessageUntil)
        {
            this.context = context;
            this.phases = phases;
            this.fishing = fishing;
            this.combat = combat;
            this.relationships = relationships;
            this.facilities = facilities;
            this.eventMessage = eventMessage;
            this.eventMessageUntil = eventMessageUntil;
        }

        public void Draw()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1600f, Screen.height / 900f), .62f, 1.35f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            DrawHud(width);
            switch (phases.Current)
            {
                case GamePhase.Explore: DrawExplore(width, height); break;
                case GamePhase.Fishing: DrawFishing(width, height); break;
                case GamePhase.CatchResolution: DrawCatch(width, height); break;
                case GamePhase.PredatorCombat: DrawCombat(width, height); break;
                case GamePhase.Dialogue: DrawDialogue(width, height); break;
                case GamePhase.Facility: DrawFacilities(width, height); break;
                case GamePhase.Collection: DrawCollection(width, height); break;
                case GamePhase.Pause: DrawPause(width, height); break;
            }
            if (Time.unscaledTime < eventMessageUntil()) GUI.Box(new Rect(width / 2f - 260f, 22f, 520f, 48f), eventMessage(), DarkBox());
        }

        private void DrawHud(float width)
        {
            bool ko = context.Korean;
            GUI.Box(new Rect(24, 22, 235, 68), ko ? "숨결섬\n고요한 해안마을" : "BREATH ISLAND\nQuiet coastal village", DarkBox());
            GUI.Box(new Rect(width - 330, 22, 306, 68), $"◉ {context.Save.inventory.coins}     ◆ {context.Save.inventory.materials}     🎣 {context.Save.inventory.bait}", DarkBox());
            GUI.Box(new Rect(width / 2f - 92f, 22, 184, 42), $"{(ko ? "시간" : "TIME")} {context.Save.player.hour:00}:00", DarkBox());
        }

        private void DrawExplore(float width, float height)
        {
            bool ko = context.Korean;
            NpcId? nearby = relationships.NearbyNpc();
            if (nearby.HasValue)
            {
                NpcDefinition npc = context.Content.Npcs[nearby.Value];
                GUI.Box(new Rect(width / 2f - 190f, height - 170f, 380f, 48f), ko ? $"E  {npc.koreanName}와 대화" : $"E  Talk to {npc.englishName}", DarkBox());
            }
            if (Vector3.Distance(context.World.player.transform.position, context.World.fishingPoint) < 7.5f)
                GUI.Box(new Rect(width / 2f - 190f, height - 220f, 380f, 48f), ko ? "F / SPACE  낚시 시작" : "F / SPACE  Start fishing", DarkBox());
            GUI.Box(new Rect(24, height - 118f, 390f, 92f), ko ? "WASD 이동 · 마우스 시점 · E 대화\nM 마을 성장 · TAB 도감 · L 언어" : "WASD Move · Mouse Look · E Talk\nM Village · TAB Guide · L Language", DarkBox());
            if (showHelp && GUI.Button(new Rect(width - 182f, height - 72f, 158f, 46f), ko ? "도움말 숨기기" : "Hide help", buttonStyle)) showHelp = false;
        }

        private void DrawFishing(float width, float height)
        {
            bool ko = context.Korean;
            GUI.Box(new Rect(24, 105, 210, 48), ko ? "1인칭 낚시 · 햇살 해안" : "FIRST-PERSON · SUNNY SHORE", DarkBox());
            Rect panel = new(width / 2f - 275f, height - 290f, 550f, 245f);
            GUI.Box(panel, string.Empty, panelStyle);
            if (fishing.Stage == FishingStage.Ready)
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 25, 500, 42), ko ? "잔잔한 물결" : "Gentle ripples", titleStyle);
                GUI.Label(new Rect(panel.x + 25, panel.y + 72, 500, 56), ko ? "물고기 그림자가 지나갈 때 낚싯줄을 던져 보세요." : "Cast when a fish shadow passes through the water.", bodyStyle);
                if (GUI.Button(new Rect(panel.x + 135, panel.y + 150, 280, 58), ko ? "낚싯줄 던지기" : "CAST LINE", primaryButtonStyle)) fishing.Cast();
            }
            else if (fishing.Stage == FishingStage.Waiting)
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 52, 500, 42), ko ? "입질을 기다리는 중…" : "Waiting for a bite…", titleStyle);
                GUI.Label(new Rect(panel.x + 25, panel.y + 105, 500, 50), ko ? "수면의 움직임에 집중하세요." : "Watch the movement of the water.", centeredStyle);
            }
            else if (fishing.Stage == FishingStage.Hooked)
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 18, 500, 34), fishing.ActiveFish.LocalizedName(ko), titleStyle);
                DrawMeter(new Rect(panel.x + 28, panel.y + 67, 494, 25), ko ? "끌어오기" : "REEL", fishing.Progress, new Color(.2f, .75f, .66f));
                DrawMeter(new Rect(panel.x + 28, panel.y + 111, 494, 25), ko ? "줄 장력" : "TENSION", fishing.Tension, fishing.Tension > 74f ? new Color(.95f, .38f, .28f) : new Color(.95f, .67f, .22f));
                if (GUI.Button(new Rect(panel.x + 28, panel.y + 164, 235, 52), ko ? "S  줄 느슨하게" : "S  EASE LINE", buttonStyle)) fishing.Ease();
                if (GUI.Button(new Rect(panel.x + 278, panel.y + 164, 244, 52), ko ? "SPACE  줄 감기" : "SPACE  REEL", primaryButtonStyle)) fishing.Reel();
            }
            else
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 35, 500, 42), ko ? "줄이 끊어졌어요" : "The line snapped", titleStyle);
                GUI.Label(new Rect(panel.x + 25, panel.y + 91, 500, 48), ko ? "괜찮아요. 잃은 것은 미끼 하나뿐이에요." : "It is okay. You only lost one bait.", centeredStyle);
                if (GUI.Button(new Rect(panel.x + 135, panel.y + 157, 280, 52), ko ? "다시 해보기" : "TRY AGAIN", primaryButtonStyle)) fishing.Stage = FishingStage.Ready;
            }
        }

        private void DrawCatch(float width, float height)
        {
            bool ko = context.Korean;
            FishDefinition fish = fishing.ActiveFish;
            Rect panel = new(width / 2f - 300f, height / 2f - 285f, 600f, 570f);
            GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 25, 540, 38), ko ? "새로운 발견" : "NEW DISCOVERY", centeredStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 70, 540, 52), fish.LocalizedName(ko), titleStyle);
            GUI.Label(new Rect(panel.x + 60, panel.y + 130, 480, 85), fish.LocalizedDescription(ko), centeredStyle);
            string rarity = ko ? fish.rarity switch { FishRarity.Common => "흔함", FishRarity.Uncommon => "보통", FishRarity.Rare => "희귀", _ => "포식자" } : fish.rarity.ToString();
            GUI.Label(new Rect(panel.x + 60, panel.y + 218, 480, 36), rarity, centeredStyle);
            const int columnWidth = 255;
            if (!fish.IsPredator && GUI.Button(new Rect(panel.x + 35, panel.y + 290, columnWidth, 68), ko ? $"판매  +{fish.coinValue} 코인" : $"SELL  +{fish.coinValue}", buttonStyle)) fishing.Resolve(CatchDisposition.Sell);
            if (GUI.Button(new Rect(panel.x + 310, panel.y + 290, columnWidth, 68), ko ? "도감 등록" : "RECORD", buttonStyle)) fishing.Resolve(CatchDisposition.Record);
            if (GUI.Button(new Rect(panel.x + 35, panel.y + 378, columnWidth, 68), ko ? "수족관 전시" : "DISPLAY", buttonStyle)) fishing.Resolve(CatchDisposition.Display);
            if (GUI.Button(new Rect(panel.x + 310, panel.y + 378, columnWidth, 68), ko ? $"재료 획득  +{fish.materialValue}" : $"MATERIAL  +{fish.materialValue}", primaryButtonStyle)) fishing.Resolve(CatchDisposition.Material);
        }

        private void DrawCombat(float width, float height)
        {
            bool ko = context.Korean;
            GUI.Box(new Rect(width / 2f - 330, 100, 660, 100), string.Empty, DarkBox());
            GUI.Label(new Rect(width / 2f - 280, 113, 560, 34), ko ? "포식자 조우 · 파도송곳상어" : "PREDATOR · BREAKER SHARK", CenteredLight());
            DrawMeter(new Rect(width / 2f - 275, 154, 550, 24), ko ? "체력" : "HEALTH", combat.Health, new Color(.95f, .42f, .28f));
            GUI.Box(new Rect(width / 2f - 250, height - 240, 500, 58), ko ? $"다음 공격 {combat.Charge:0}%" : $"NEXT ATTACK {combat.Charge:0}%", DarkBox());
            Rect bar = new(width / 2f - 360, height - 160, 720, 92); GUI.Box(bar, string.Empty, DarkBox());
            GUI.Label(new Rect(bar.x + 18, bar.y + 28, 120, 35), new string('♥', combat.PlayerHearts), CenteredLight());
            if (GUI.Button(new Rect(bar.x + 150, bar.y + 17, 165, 58), ko ? "1  회피" : "1  DODGE", buttonStyle)) combat.Act(0);
            if (GUI.Button(new Rect(bar.x + 325, bar.y + 17, 165, 58), ko ? "2  그물" : "2  NET", buttonStyle)) combat.Act(1);
            if (GUI.Button(new Rect(bar.x + 500, bar.y + 17, 195, 58), ko ? "3  진정 충격" : "3  CALM PULSE", primaryButtonStyle)) combat.Act(2);
            Keyboard keyboard = Keyboard.current;
            if (keyboard?.digit1Key.wasPressedThisFrame == true) combat.Act(0);
            if (keyboard?.digit2Key.wasPressedThisFrame == true) combat.Act(1);
            if (keyboard?.digit3Key.wasPressedThisFrame == true) combat.Act(2);
        }

        private void DrawDialogue(float width, float height)
        {
            bool ko = context.Korean;
            Rect panel = new(120, height - 300, width - 240, 245); GUI.Box(panel, string.Empty, panelStyle);
            NpcDefinition npc = context.Content.Npcs[relationships.ActiveNpc];
            GUI.Label(new Rect(panel.x + 30, panel.y + 24, 350, 42), npc.LocalizedName(ko), titleStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 64, 350, 28), npc.LocalizedRole(ko), smallStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 106, panel.width - 60, 55), $"“{npc.LocalizedLine(ko, relationships.DialogueIndex)}”", bodyStyle);
            GUI.Label(new Rect(panel.x + panel.width - 230, panel.y + 30, 190, 35), $"♥ {context.Save.relationships.Get(relationships.ActiveNpc):0.0} / 3", bodyStyle);
            if (GUI.Button(new Rect(panel.x + panel.width - 430, panel.y + 177, 180, 48), ko ? "선물  12 코인" : "GIFT  12 COINS", buttonStyle)) relationships.GiveGift();
            if (GUI.Button(new Rect(panel.x + panel.width - 230, panel.y + 177, 200, 48), ko ? "계속 대화" : "KEEP TALKING", primaryButtonStyle)) relationships.Talk();
        }

        private void DrawFacilities(float width, float height)
        {
            bool ko = context.Korean;
            Rect panel = new(width - 570, 0, 570, height); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 35, 45, 500, 46), ko ? "마을 성장" : "VILLAGE GROWTH", titleStyle);
            GUI.Label(new Rect(panel.x + 35, 100, 500, 52), ko ? "원하는 시설부터 복구하세요. 모든 선택은 나중에 완성할 수 있어요." : "Restore either facility first. Every choice can be completed later.", bodyStyle);
            DrawFacilityCard(new Rect(panel.x + 35, 185, 500, 190), FacilityId.Boatyard, facilities.SelectedIndex == 0);
            DrawFacilityCard(new Rect(panel.x + 35, 395, 500, 190), FacilityId.Aquarium, facilities.SelectedIndex == 1);
            if (GUI.Button(new Rect(panel.x + 35, height - 82, 500, 52), ko ? "닫기  ESC" : "CLOSE  ESC", buttonStyle)) phases.Set(GamePhase.Explore);
        }

        private void DrawFacilityCard(Rect rect, FacilityId id, bool selected)
        {
            bool ko = context.Korean;
            GUI.Box(rect, string.Empty, new GUIStyle(panelStyle) { normal = { background = paleTexture, textColor = Color.black } });
            FacilityDefinition definition = context.Content.Facilities[id];
            int level = context.Save.facilities.Get(id);
            GUI.Label(new Rect(rect.x + 20, rect.y + 15, 450, 36), $"{definition.LocalizedName(ko)}  ·  LEVEL {level}", titleStyle);
            GUI.Label(new Rect(rect.x + 20, rect.y + 61, 450, 45), id == FacilityId.Boatyard ? (ko ? "작은 보트와 산호초 항로를 개방합니다." : "Unlocks a small boat and the reef route.") : (ko ? "잡은 생물을 전시하고 주민을 불러옵니다." : "Displays creatures and brings villagers back."), smallStyle);
            if (level < 2)
            {
                string cost = $"{definition.coinCosts[level]} ◉  +  {definition.materialCosts[level]} ◆";
                if (GUI.Button(new Rect(rect.x + 20, rect.y + 122, 460, 48), (ko ? "복구하기  " : "RESTORE  ") + cost, primaryButtonStyle)) facilities.Upgrade(id);
            }
            else GUI.Label(new Rect(rect.x + 20, rect.y + 130, 460, 38), ko ? "복구 완료" : "RESTORATION COMPLETE", centeredStyle);
            if (selected) GUI.Label(new Rect(rect.x + 440, rect.y + 12, 40, 28), "●", bodyStyle);
        }

        private void DrawCollection(float width, float height)
        {
            bool ko = context.Korean;
            Rect panel = new(width - 650, 0, 650, height); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 35, 40, 570, 46), ko ? "바다 도감" : "FIELD GUIDE", titleStyle);
            GUI.Label(new Rect(panel.x + 35, 91, 570, 32), $"{context.Save.inventory.discoveredFish.Count} / {context.Content.Fish.Count} {(ko ? "발견" : "DISCOVERED")}", bodyStyle);
            float y = 145;
            foreach (FishDefinition definition in context.Content.Fish)
            {
                bool found = context.Save.inventory.discoveredFish.Contains(definition.id);
                GUI.Box(new Rect(panel.x + 35, y, 570, 72), string.Empty, new GUIStyle(panelStyle) { normal = { background = paleTexture } });
                GUI.Label(new Rect(panel.x + 55, y + 12, 360, 28), found ? definition.LocalizedName(ko) : (ko ? "아직 만나지 못한 생물" : "UNDISCOVERED CREATURE"), bodyStyle);
                GUI.Label(new Rect(panel.x + 55, y + 40, 470, 24), found ? definition.LocalizedDescription(ko) : (ko ? "물결과 시간대를 관찰해 보세요." : "Watch the ripples and time of day."), smallStyle);
                if (context.Save.inventory.displayedFish.Contains(definition.id)) GUI.Label(new Rect(panel.x + 490, y + 12, 90, 25), ko ? "전시 중" : "ON SHOW", smallStyle);
                y += 82;
            }
            if (GUI.Button(new Rect(panel.x + 35, height - 70, 570, 48), ko ? "닫기  ESC" : "CLOSE  ESC", buttonStyle)) phases.Set(GamePhase.Explore);
        }

        private void DrawPause(float width, float height)
        {
            bool ko = context.Korean;
            Rect panel = new(width / 2f - 310, height / 2f - 280, 620, 560); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 28, 440, 45), ko ? "잠시 쉬어가기" : "TAKE A BREATH", titleStyle);
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 92, 490, 50), ko ? "계속하기" : "CONTINUE", primaryButtonStyle)) phases.Set(phases.BeforePause);
            for (int slot = 0; slot < 3; slot++)
            {
                int capturedSlot = slot;
                float y = panel.y + 164 + slot * 70;
                if (GUI.Button(new Rect(panel.x + 65, y, 235, 50), $"{(ko ? "저장" : "SAVE")} {slot + 1}", buttonStyle)) { context.SaveTo(capturedSlot); context.Message(context.Text("saved")); }
                if (GUI.Button(new Rect(panel.x + 320, y, 235, 50), $"{(ko ? "불러오기" : "LOAD")} {slot + 1}", buttonStyle)) { context.LoadFrom(capturedSlot); context.Message(context.Text("loaded")); }
            }
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 390, 490, 50), ko ? "언어 변경 / LANGUAGE" : "LANGUAGE / 한국어", buttonStyle)) context.Korean = !context.Korean;
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 462, 490, 50), ko ? "게임 종료" : "QUIT GAME", buttonStyle)) Application.Quit();
        }

        private void DrawMeter(Rect rect, string label, float value, Color color)
        {
            GUI.Label(new Rect(rect.x, rect.y - 22, 160, 22), label, smallStyle);
            GUI.Box(rect, string.Empty, new GUIStyle(GUI.skin.box) { normal = { background = darkTexture } });
            Color previousColor = GUI.color; GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 3, (rect.width - 6) * Mathf.Clamp01(value / 100f), rect.height - 6), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(rect, $"{value:0}%", CenteredLight());
        }

        private void EnsureStyles()
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

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }

        private GUIStyle DarkBox() => new(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleCenter, normal = { background = darkTexture, textColor = Color.white }, padding = new RectOffset(10, 10, 8, 8) };
        private GUIStyle CenteredLight() => new(centeredStyle) { normal = { textColor = Color.white }, fontStyle = FontStyle.Bold };
    }
}
