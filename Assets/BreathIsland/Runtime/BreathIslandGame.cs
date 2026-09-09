using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BreathIsland
{
    public sealed class BreathIslandGame : MonoBehaviour
    {
        private enum FishingStage { Ready, Waiting, Hooked, Failed }

        private GamePhase phase = GamePhase.Explore;
        private GamePhase phaseBeforePause = GamePhase.Explore;
        private GameSaveData save;
        private SaveService saves;
        private WorldHandles world;
        private readonly List<FishDefinition> fish = new();
        private readonly Dictionary<FacilityId, FacilityDefinition> facilities = new();
        private readonly Dictionary<NpcId, NpcDefinition> npcs = new();
        private FishingStage fishingStage;
        private FishDefinition activeFish;
        private NpcId activeNpc;
        private int dialogueIndex;
        private float biteTimer;
        private float fishingProgress;
        private float tension;
        private int predatorHealth;
        private int playerHearts;
        private float predatorCharge;
        private string eventMessage = string.Empty;
        private float eventMessageUntil;
        private bool korean = true;
        private bool showHelp = true;
        private int selectedFacility;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;
        private GUIStyle primaryButtonStyle;
        private GUIStyle centeredStyle;
        private Texture2D panelTexture;
        private Texture2D darkTexture;
        private Texture2D tealTexture;
        private Texture2D paleTexture;

        public GamePhase Phase => phase;
        public GameSaveData SaveData => save;

        private void Awake()
        {
            name = "Breath Island Game";
            saves = new SaveService();
            save = saves.Load();
            CreateDefinitions();
            world = ProceduralWorld.Build(transform);
            gameObject.AddComponent<ProceduralSeaAudio>();
            world.player.transform.position = new Vector3(save.player.x, Mathf.Max(1.2f, save.player.y), save.player.z == 0f ? 4f : save.player.z);
            SetPhase(GamePhase.Explore);
            ShowMessage(T("welcome"));
        }

        private void Update()
        {
            if (GameInput.LanguagePressed()) { korean = !korean; ShowMessage(T("language")); }
            if (GameInput.SavePressed()) { CapturePosition(); saves.Save(save, 0); ShowMessage(T("saved")); }
            if (GameInput.LoadPressed()) { save = saves.Load(0); RestorePosition(); ShowMessage(T("loaded")); }
            if (GameInput.CancelPressed()) HandleCancel();
            if (phase == GamePhase.Explore) UpdateExplore();
            else if (phase == GamePhase.Fishing) UpdateFishing();
            else if (phase == GamePhase.PredatorCombat) UpdateCombat();
            else if (phase == GamePhase.CatchResolution) UpdateCatchResolutionInput();
            else if (phase == GamePhase.Dialogue) UpdateDialogueInput();
            else if (phase == GamePhase.Facility) UpdateFacilityInput();
        }

        private void UpdateExplore()
        {
            if (GameInput.FacilityPressed()) SetPhase(GamePhase.Facility);
            if (GameInput.CollectionPressed()) SetPhase(GamePhase.Collection);
            if (GameInput.InteractPressed())
            {
                NpcId? nearby = NearbyNpc();
                if (nearby.HasValue) OpenDialogue(nearby.Value);
            }
            if (GameInput.PrimaryPressed() && Vector3.Distance(world.player.transform.position, world.fishingPoint) < 7.5f) StartFishing();
        }

        private void UpdateFishing()
        {
            if (fishingStage == FishingStage.Ready && GameInput.PrimaryPressed()) Cast();
            else if (fishingStage == FishingStage.Waiting)
            {
                biteTimer -= Time.deltaTime;
                if (biteTimer <= 0f) HookFish();
            }
            else if (fishingStage == FishingStage.Hooked)
            {
                tension = FishingMath.PassiveTension(tension, activeFish.pullStrength, Time.deltaTime);
                fishingProgress = FishingMath.PassiveProgress(fishingProgress, Time.deltaTime);
                if (GameInput.PrimaryPressed()) Reel();
                if (GameInput.EasePressed()) FishingMath.Ease(ref tension);
                if (tension >= 99.9f)
                {
                    fishingStage = FishingStage.Failed;
                    ShowMessage(T("line_broke"));
                }
            }
            else if (fishingStage == FishingStage.Failed && GameInput.PrimaryPressed()) fishingStage = FishingStage.Ready;
        }

        private void UpdateCombat()
        {
            if (GameInput.PadSouthPressed()) CombatAction(0);
            if (GameInput.PadWestPressed()) CombatAction(1);
            if (GameInput.PadNorthPressed()) CombatAction(2);
            predatorCharge += Time.deltaTime / 3.4f * 100f;
            if (predatorCharge >= 100f)
            {
                predatorCharge = 0f;
                playerHearts--;
                ShowMessage(T("hit"));
                if (playerHearts <= 0)
                {
                    save.inventory.bait = Mathf.Max(0, save.inventory.bait - 1);
                    world.player.transform.position = new Vector3(0f, 1.2f, 8f);
                    AutoSave(); SetPhase(GamePhase.Explore); ShowMessage(T("washed_ashore"));
                }
            }
        }

        private void UpdateCatchResolutionInput()
        {
            if (GameInput.PadWestPressed() && !activeFish.IsPredator) ResolveCatch(CatchDisposition.Sell);
            else if (GameInput.PadSouthPressed()) ResolveCatch(CatchDisposition.Record);
            else if (GameInput.PadNorthPressed()) ResolveCatch(CatchDisposition.Display);
            else if (GameInput.PadEastPressed()) ResolveCatch(CatchDisposition.Material);
        }

        private void UpdateDialogueInput()
        {
            if (GameInput.PadSouthPressed()) Talk();
            else if (GameInput.PadWestPressed()) GiveGift();
        }

        private void UpdateFacilityInput()
        {
            int direction = GameInput.PadVerticalPressed();
            if (direction != 0) selectedFacility = Mathf.Clamp(selectedFacility + direction, 0, 1);
            if (GameInput.PadSouthPressed()) Upgrade((FacilityId)selectedFacility);
        }

        private void HandleCancel()
        {
            if (phase == GamePhase.Explore)
            {
                phaseBeforePause = phase;
                SetPhase(GamePhase.Pause);
            }
            else if (phase == GamePhase.Pause) SetPhase(phaseBeforePause);
            else SetPhase(GamePhase.Explore);
        }

        private void SetPhase(GamePhase next)
        {
            phase = next;
            if (world == null) return;
            bool movable = next == GamePhase.Explore;
            world.motor.CanMove = movable;
            world.cameraRig.firstPerson = next == GamePhase.Fishing || next == GamePhase.PredatorCombat;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void StartFishing()
        {
            if (save.inventory.bait <= 0) { ShowMessage(T("no_bait")); return; }
            world.player.transform.position = world.fishingPoint;
            world.player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            fishingStage = FishingStage.Ready;
            fishingProgress = 0f; tension = 32f;
            SetPhase(GamePhase.Fishing);
        }

        private void Cast()
        {
            if (save.inventory.bait <= 0) { ShowMessage(T("no_bait")); return; }
            save.inventory.bait--;
            save.player.hour = (save.player.hour + 1) % 24;
            fishingStage = FishingStage.Waiting;
            biteTimer = UnityEngine.Random.Range(1.2f, 2.2f);
        }

        private void HookFish()
        {
            bool predator = save.inventory.totalCatchCount >= 2 && save.inventory.totalCatchCount % 4 == 2;
            var pool = fish.Where(definition => !definition.IsPredator && IsAvailable(definition)).ToList();
            activeFish = predator ? fish.First(definition => definition.IsPredator) : pool[UnityEngine.Random.Range(0, pool.Count)];
            fishingStage = FishingStage.Hooked;
            fishingProgress = 8f; tension = 34f;
            ShowMessage(string.Format(T("hooked"), activeFish.LocalizedName(korean)));
        }

        private bool IsAvailable(FishDefinition definition)
        {
            int hour = save.player.hour;
            return definition.startHour <= definition.endHour
                ? hour >= definition.startHour && hour <= definition.endHour
                : hour >= definition.startHour || hour <= definition.endHour;
        }

        private void Reel()
        {
            FishingMath.Reel(ref fishingProgress, ref tension);
            if (fishingProgress < 99.9f) return;
            if (activeFish.IsPredator)
            {
                predatorHealth = 100; playerHearts = 3; predatorCharge = 12f;
                SetPhase(GamePhase.PredatorCombat); ShowMessage(T("predator"));
            }
            else SetPhase(GamePhase.CatchResolution);
        }

        private void ResolveCatch(CatchDisposition disposition)
        {
            GameEconomy.ApplyCatch(save, activeFish, disposition);
            AutoSave();
            SetPhase(GamePhase.Explore);
            ShowMessage(disposition switch
            {
                CatchDisposition.Sell => string.Format(T("sold"), activeFish.coinValue),
                CatchDisposition.Display => T("displayed"),
                CatchDisposition.Material => string.Format(T("materials"), activeFish.materialValue),
                _ => T("recorded")
            });
        }

        private void CombatAction(int action)
        {
            if (action == 0)
            {
                predatorCharge = Mathf.Max(0f, predatorCharge - 68f);
                ShowMessage(T("dodged")); return;
            }
            int damage = action == 1 ? 18 : 28;
            predatorHealth = Mathf.Max(0, predatorHealth - damage);
            ShowMessage(action == 1 ? T("net") : T("shock"));
            if (predatorHealth == 0) SetPhase(GamePhase.CatchResolution);
        }

        private void OpenDialogue(NpcId id)
        {
            activeNpc = id; dialogueIndex = 0; SetPhase(GamePhase.Dialogue);
        }

        private void Talk()
        {
            dialogueIndex++;
            save.relationships.Add(activeNpc, .25f);
            AutoSave();
        }

        private void GiveGift()
        {
            if (save.inventory.coins < 12) { ShowMessage(T("gift_fail")); return; }
            save.inventory.coins -= 12;
            save.relationships.Add(activeNpc, 1f);
            AutoSave(); ShowMessage(T("gift_ok"));
        }

        private void Upgrade(FacilityId id)
        {
            if (!GameEconomy.TryUpgrade(save, facilities[id])) { ShowMessage(T("upgrade_fail")); return; }
            AutoSave(); ShowMessage(id == FacilityId.Boatyard && save.facilities.boatyard == 1 ? T("boat_unlocked") : T("upgrade_ok"));
        }

        private NpcId? NearbyNpc()
        {
            foreach (var pair in world.npcs) if (Vector3.Distance(world.player.transform.position, pair.Value.position) < 3.3f) return pair.Key;
            return null;
        }

        private void CapturePosition()
        {
            Vector3 position = world.player.transform.position;
            save.player.x = position.x; save.player.y = position.y; save.player.z = position.z;
        }

        private void RestorePosition()
        {
            world.player.transform.position = new Vector3(save.player.x, Mathf.Max(1.2f, save.player.y), save.player.z);
        }

        private void AutoSave() { CapturePosition(); saves.Save(save); }
        private void OnApplicationQuit() { if (save != null && world != null) AutoSave(); }
        private void ShowMessage(string text) { eventMessage = text; eventMessageUntil = Time.unscaledTime + 2.6f; }

        private void CreateDefinitions()
        {
            fish.Add(Fish("glimmer", "윤슬멸치", "Glimmer Anchovy", "햇빛을 따라 반짝이는 작은 물고기", "A small fish that follows sunlit ripples.", FishRarity.Common, new Color(.35f, .88f, .82f), 18, 1, 4f, 6, 18));
            fish.Add(Fish("cloud", "구름복어", "Cloud Puffer", "놀라면 몽글몽글 부풀어 오른다", "It puffs up like a tiny cloud.", FishRarity.Uncommon, new Color(1f, .78f, .35f), 28, 2, 6f, 7, 20));
            fish.Add(Fish("coral", "산호나비어", "Coral Butterfly", "얕은 산호 곁을 맴도는 온순한 물고기", "A gentle fish that circles shallow coral.", FishRarity.Uncommon, new Color(1f, .43f, .35f), 34, 2, 7f, 8, 17));
            fish.Add(Fish("moon", "달빛가오리", "Moon Ray", "저녁 바다에서 푸른 빛을 남긴다", "It leaves a blue trail across evening water.", FishRarity.Rare, new Color(.5f, .48f, 1f), 56, 4, 9f, 17, 23));
            fish.Add(Fish("pearl", "진주해마", "Pearl Seahorse", "해초 사이에 몸을 숨기는 희귀종", "A rare creature hiding among sea grass.", FishRarity.Rare, new Color(.95f, .55f, .76f), 64, 5, 10f, 6, 23));
            fish.Add(Fish("breaker", "파도송곳상어", "Breaker Shark", "거친 파도를 가르는 외해의 포식자", "An open-sea predator that cuts through rough waves.", FishRarity.Predator, new Color(.2f, .55f, .66f), 0, 9, 13f, 0, 23));
            facilities[FacilityId.Boatyard] = Facility(FacilityId.Boatyard, "바람 조선소", "Wind Boatyard");
            facilities[FacilityId.Aquarium] = Facility(FacilityId.Aquarium, "파도 수족관", "Tide Aquarium");
            npcs[NpcId.Nari] = Npc(NpcId.Nari, "나리", "Nari", "조선공", "Boatwright", new Color(.15f, .65f, .58f),
                new[] { "바다는 기다릴 줄 아는 사람을 좋아해.", "조선소를 손보면 산호초까지 갈 수 있어.", "오늘 파도는 얌전하네. 줄 소리에 귀 기울여 봐." },
                new[] { "The sea likes people who know how to wait.", "Repair the boatyard and we can reach the reef.", "The waves are gentle today. Listen to your line." });
            npcs[NpcId.Maru] = Npc(NpcId.Maru, "마루", "Maru", "수족관지기", "Aquarium Keeper", new Color(.95f, .64f, .31f),
                new[] { "처음 만난 생물은 도감에 기록해 둬.", "수족관이 밝아지면 마을 사람들도 찾아올 거야.", "구름복어는 둥글지만 마음은 아주 용감하대." },
                new[] { "Record every creature you meet for the first time.", "A brighter aquarium will bring people back.", "Cloud puffers are round, but very brave." });
        }

        private static FishDefinition Fish(string id, string ko, string en, string koDesc, string enDesc, FishRarity rarity, Color color, int coin, int material, float pull, int start, int end)
        {
            var value = ScriptableObject.CreateInstance<FishDefinition>();
            value.id = id; value.koreanName = ko; value.englishName = en; value.koreanDescription = koDesc; value.englishDescription = enDesc;
            value.rarity = rarity; value.color = color; value.coinValue = coin; value.materialValue = material; value.pullStrength = pull; value.startHour = start; value.endHour = end;
            return value;
        }

        private static FacilityDefinition Facility(FacilityId id, string ko, string en)
        {
            var value = ScriptableObject.CreateInstance<FacilityDefinition>(); value.id = id; value.koreanName = ko; value.englishName = en; return value;
        }

        private static NpcDefinition Npc(NpcId id, string ko, string en, string koRole, string enRole, Color color, string[] koLines, string[] enLines)
        {
            var value = ScriptableObject.CreateInstance<NpcDefinition>(); value.id = id; value.koreanName = ko; value.englishName = en; value.koreanRole = koRole; value.englishRole = enRole; value.color = color; value.koreanLines = koLines; value.englishLines = enLines; return value;
        }

        private string T(string key)
        {
            return (key, korean) switch
            {
                ("welcome", true) => "숨결섬에 오신 것을 환영해요.", ("welcome", false) => "Welcome to Breath Island.",
                ("language", true) => "언어: 한국어", ("language", false) => "Language: English",
                ("saved", true) => "1번 슬롯에 저장했습니다.", ("saved", false) => "Saved to slot 1.",
                ("loaded", true) => "1번 슬롯을 불러왔습니다.", ("loaded", false) => "Loaded slot 1.",
                ("no_bait", true) => "미끼가 없어요.", ("no_bait", false) => "You are out of bait.",
                ("line_broke", true) => "줄이 끊어졌어요. 미끼만 소모됩니다.", ("line_broke", false) => "The line snapped. Only the bait was lost.",
                ("hooked", true) => "{0}이(가) 미끼를 물었어요!", ("hooked", false) => "A {0} took the bait!",
                ("predator", true) => "포식자 조우! 공격 신호를 보고 회피하세요.", ("predator", false) => "Predator! Watch the signal and dodge.",
                ("hit", true) => "꼬리치기에 맞았어요!", ("hit", false) => "The tail strike hit you!",
                ("washed_ashore", true) => "해안으로 돌아왔어요. 미끼 1개를 잃었습니다.", ("washed_ashore", false) => "You washed ashore and lost one bait.",
                ("sold", true) => "조개코인 {0}개를 받았어요.", ("sold", false) => "Received {0} shell coins.",
                ("displayed", true) => "수족관 전시 목록에 추가했어요.", ("displayed", false) => "Added to the aquarium display.",
                ("materials", true) => "재료 {0}개를 얻었어요.", ("materials", false) => "Received {0} materials.",
                ("recorded", true) => "도감을 자세히 기록했어요.", ("recorded", false) => "The field guide has been updated.",
                ("dodged", true) => "물보라를 피해 굴렀어요.", ("dodged", false) => "You rolled clear of the spray.",
                ("net", true) => "그물이 움직임을 늦췄어요.", ("net", false) => "The net slowed it down.",
                ("shock", true) => "진정 충격으로 기운을 뺐어요.", ("shock", false) => "The calming pulse weakened it.",
                ("gift_fail", true) => "선물을 살 코인이 부족해요.", ("gift_fail", false) => "Not enough coins for a gift.",
                ("gift_ok", true) => "호감도가 올랐어요.", ("gift_ok", false) => "Friendship increased.",
                ("upgrade_fail", true) => "업그레이드 재료가 부족해요.", ("upgrade_fail", false) => "Not enough upgrade materials.",
                ("upgrade_ok", true) => "시설이 성장했어요.", ("upgrade_ok", false) => "The facility has grown.",
                ("boat_unlocked", true) => "작은 보트가 완성됐어요!", ("boat_unlocked", false) => "The small boat is ready!",
                _ => key
            };
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
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave }; texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1600f, Screen.height / 900f), .62f, 1.35f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale; float height = Screen.height / scale;
            DrawHud(width, height);
            if (phase == GamePhase.Explore) DrawExplore(width, height);
            else if (phase == GamePhase.Fishing) DrawFishing(width, height);
            else if (phase == GamePhase.CatchResolution) DrawCatch(width, height);
            else if (phase == GamePhase.PredatorCombat) DrawCombat(width, height);
            else if (phase == GamePhase.Dialogue) DrawDialogue(width, height);
            else if (phase == GamePhase.Facility) DrawFacilities(width, height);
            else if (phase == GamePhase.Collection) DrawCollection(width, height);
            else if (phase == GamePhase.Pause) DrawPause(width, height);
            if (Time.unscaledTime < eventMessageUntil) GUI.Box(new Rect(width / 2f - 260f, 22f, 520f, 48f), eventMessage, DarkBox());
        }

        private void DrawHud(float width, float height)
        {
            GUI.Box(new Rect(24, 22, 235, 68), korean ? "숨결섬\n고요한 해안마을" : "BREATH ISLAND\nQuiet coastal village", DarkBox());
            GUI.Box(new Rect(width - 330, 22, 306, 68), $"◉ {save.inventory.coins}     ◆ {save.inventory.materials}     🎣 {save.inventory.bait}", DarkBox());
            GUI.Box(new Rect(width / 2f - 92f, 22, 184, 42), $"{(korean ? "시간" : "TIME")} {save.player.hour:00}:00", DarkBox());
        }

        private void DrawExplore(float width, float height)
        {
            var nearby = NearbyNpc();
            if (nearby.HasValue) GUI.Box(new Rect(width / 2f - 190f, height - 170f, 380f, 48f), korean ? $"E  {npcs[nearby.Value].koreanName}와 대화" : $"E  Talk to {npcs[nearby.Value].englishName}", DarkBox());
            if (Vector3.Distance(world.player.transform.position, world.fishingPoint) < 7.5f) GUI.Box(new Rect(width / 2f - 190f, height - 220f, 380f, 48f), korean ? "F / SPACE  낚시 시작" : "F / SPACE  Start fishing", DarkBox());
            GUI.Box(new Rect(24, height - 118f, 390f, 92f), korean ? "WASD 이동 · 마우스 시점 · E 대화\nM 마을 성장 · TAB 도감 · L 언어" : "WASD Move · Mouse Look · E Talk\nM Village · TAB Guide · L Language", DarkBox());
            if (showHelp && GUI.Button(new Rect(width - 182f, height - 72f, 158f, 46f), korean ? "도움말 숨기기" : "Hide help", buttonStyle)) showHelp = false;
        }

        private void DrawFishing(float width, float height)
        {
            GUI.Box(new Rect(24, 105, 210, 48), korean ? "1인칭 낚시 · 햇살 해안" : "FIRST-PERSON · SUNNY SHORE", DarkBox());
            Rect panel = new(width / 2f - 275f, height - 290f, 550f, 245f);
            GUI.Box(panel, string.Empty, panelStyle);
            if (fishingStage == FishingStage.Ready)
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 25, 500, 42), korean ? "잔잔한 물결" : "Gentle ripples", titleStyle);
                GUI.Label(new Rect(panel.x + 25, panel.y + 72, 500, 56), korean ? "물고기 그림자가 지나갈 때 낚싯줄을 던져 보세요." : "Cast when a fish shadow passes through the water.", bodyStyle);
                if (GUI.Button(new Rect(panel.x + 135, panel.y + 150, 280, 58), korean ? "낚싯줄 던지기" : "CAST LINE", primaryButtonStyle)) Cast();
            }
            else if (fishingStage == FishingStage.Waiting)
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 52, 500, 42), korean ? "입질을 기다리는 중…" : "Waiting for a bite…", titleStyle);
                GUI.Label(new Rect(panel.x + 25, panel.y + 105, 500, 50), korean ? "수면의 움직임에 집중하세요." : "Watch the movement of the water.", centeredStyle);
            }
            else if (fishingStage == FishingStage.Hooked)
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 18, 500, 34), activeFish.LocalizedName(korean), titleStyle);
                DrawMeter(new Rect(panel.x + 28, panel.y + 67, 494, 25), korean ? "끌어오기" : "REEL", fishingProgress, new Color(.2f, .75f, .66f));
                DrawMeter(new Rect(panel.x + 28, panel.y + 111, 494, 25), korean ? "줄 장력" : "TENSION", tension, tension > 74f ? new Color(.95f, .38f, .28f) : new Color(.95f, .67f, .22f));
                if (GUI.Button(new Rect(panel.x + 28, panel.y + 164, 235, 52), korean ? "S  줄 느슨하게" : "S  EASE LINE", buttonStyle)) FishingMath.Ease(ref tension);
                if (GUI.Button(new Rect(panel.x + 278, panel.y + 164, 244, 52), korean ? "SPACE  줄 감기" : "SPACE  REEL", primaryButtonStyle)) Reel();
            }
            else
            {
                GUI.Label(new Rect(panel.x + 25, panel.y + 35, 500, 42), korean ? "줄이 끊어졌어요" : "The line snapped", titleStyle);
                GUI.Label(new Rect(panel.x + 25, panel.y + 91, 500, 48), korean ? "괜찮아요. 잃은 것은 미끼 하나뿐이에요." : "It is okay. You only lost one bait.", centeredStyle);
                if (GUI.Button(new Rect(panel.x + 135, panel.y + 157, 280, 52), korean ? "다시 해보기" : "TRY AGAIN", primaryButtonStyle)) fishingStage = FishingStage.Ready;
            }
        }

        private void DrawCatch(float width, float height)
        {
            Rect panel = new(width / 2f - 300f, height / 2f - 285f, 600f, 570f); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 25, 540, 38), korean ? "새로운 발견" : "NEW DISCOVERY", centeredStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 70, 540, 52), activeFish.LocalizedName(korean), titleStyle);
            GUI.Label(new Rect(panel.x + 60, panel.y + 130, 480, 85), activeFish.LocalizedDescription(korean), centeredStyle);
            string rarity = korean ? activeFish.rarity switch { FishRarity.Common => "흔함", FishRarity.Uncommon => "보통", FishRarity.Rare => "희귀", _ => "포식자" } : activeFish.rarity.ToString();
            GUI.Label(new Rect(panel.x + 60, panel.y + 218, 480, 36), rarity, centeredStyle);
            int columnWidth = 255;
            if (!activeFish.IsPredator && GUI.Button(new Rect(panel.x + 35, panel.y + 290, columnWidth, 68), korean ? $"판매  +{activeFish.coinValue} 코인" : $"SELL  +{activeFish.coinValue}", buttonStyle)) ResolveCatch(CatchDisposition.Sell);
            if (GUI.Button(new Rect(panel.x + 310, panel.y + 290, columnWidth, 68), korean ? "도감 등록" : "RECORD", buttonStyle)) ResolveCatch(CatchDisposition.Record);
            if (GUI.Button(new Rect(panel.x + 35, panel.y + 378, columnWidth, 68), korean ? "수족관 전시" : "DISPLAY", buttonStyle)) ResolveCatch(CatchDisposition.Display);
            if (GUI.Button(new Rect(panel.x + 310, panel.y + 378, columnWidth, 68), korean ? $"재료 획득  +{activeFish.materialValue}" : $"MATERIAL  +{activeFish.materialValue}", primaryButtonStyle)) ResolveCatch(CatchDisposition.Material);
        }

        private void DrawCombat(float width, float height)
        {
            GUI.Box(new Rect(width / 2f - 330, 100, 660, 100), string.Empty, DarkBox());
            GUI.Label(new Rect(width / 2f - 280, 113, 560, 34), korean ? "포식자 조우 · 파도송곳상어" : "PREDATOR · BREAKER SHARK", CenteredLight());
            DrawMeter(new Rect(width / 2f - 275, 154, 550, 24), korean ? "체력" : "HEALTH", predatorHealth, new Color(.95f, .42f, .28f));
            GUI.Box(new Rect(width / 2f - 250, height - 240, 500, 58), korean ? $"다음 공격 {predatorCharge:0}%" : $"NEXT ATTACK {predatorCharge:0}%", DarkBox());
            Rect bar = new(width / 2f - 360, height - 160, 720, 92); GUI.Box(bar, string.Empty, DarkBox());
            GUI.Label(new Rect(bar.x + 18, bar.y + 28, 120, 35), new string('♥', playerHearts), CenteredLight());
            if (GUI.Button(new Rect(bar.x + 150, bar.y + 17, 165, 58), korean ? "1  회피" : "1  DODGE", buttonStyle)) CombatAction(0);
            if (GUI.Button(new Rect(bar.x + 325, bar.y + 17, 165, 58), korean ? "2  그물" : "2  NET", buttonStyle)) CombatAction(1);
            if (GUI.Button(new Rect(bar.x + 500, bar.y + 17, 195, 58), korean ? "3  진정 충격" : "3  CALM PULSE", primaryButtonStyle)) CombatAction(2);
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard?.digit1Key.wasPressedThisFrame == true) CombatAction(0);
            if (keyboard?.digit2Key.wasPressedThisFrame == true) CombatAction(1);
            if (keyboard?.digit3Key.wasPressedThisFrame == true) CombatAction(2);
        }

        private void DrawDialogue(float width, float height)
        {
            Rect panel = new(120, height - 300, width - 240, 245); GUI.Box(panel, string.Empty, panelStyle);
            var npc = npcs[activeNpc];
            GUI.Label(new Rect(panel.x + 30, panel.y + 24, 350, 42), npc.LocalizedName(korean), titleStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 64, 350, 28), npc.LocalizedRole(korean), smallStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 106, panel.width - 60, 55), $"“{npc.LocalizedLine(korean, dialogueIndex)}”", bodyStyle);
            GUI.Label(new Rect(panel.x + panel.width - 230, panel.y + 30, 190, 35), $"♥ {save.relationships.Get(activeNpc):0.0} / 3", bodyStyle);
            if (GUI.Button(new Rect(panel.x + panel.width - 430, panel.y + 177, 180, 48), korean ? "선물  12 코인" : "GIFT  12 COINS", buttonStyle)) GiveGift();
            if (GUI.Button(new Rect(panel.x + panel.width - 230, panel.y + 177, 200, 48), korean ? "계속 대화" : "KEEP TALKING", primaryButtonStyle)) Talk();
        }

        private void DrawFacilities(float width, float height)
        {
            Rect panel = new(width - 570, 0, 570, height); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 35, 45, 500, 46), korean ? "마을 성장" : "VILLAGE GROWTH", titleStyle);
            GUI.Label(new Rect(panel.x + 35, 100, 500, 52), korean ? "원하는 시설부터 복구하세요. 모든 선택은 나중에 완성할 수 있어요." : "Restore either facility first. Every choice can be completed later.", bodyStyle);
            DrawFacilityCard(new Rect(panel.x + 35, 185, 500, 190), FacilityId.Boatyard, selectedFacility == 0);
            DrawFacilityCard(new Rect(panel.x + 35, 395, 500, 190), FacilityId.Aquarium, selectedFacility == 1);
            if (GUI.Button(new Rect(panel.x + 35, height - 82, 500, 52), korean ? "닫기  ESC" : "CLOSE  ESC", buttonStyle)) SetPhase(GamePhase.Explore);
        }

        private void DrawFacilityCard(Rect rect, FacilityId id, bool selected)
        {
            GUI.Box(rect, string.Empty, new GUIStyle(panelStyle) { normal = { background = paleTexture, textColor = Color.black } });
            FacilityDefinition definition = facilities[id]; int level = save.facilities.Get(id);
            GUI.Label(new Rect(rect.x + 20, rect.y + 15, 450, 36), $"{definition.LocalizedName(korean)}  ·  LEVEL {level}", titleStyle);
            GUI.Label(new Rect(rect.x + 20, rect.y + 61, 450, 45), id == FacilityId.Boatyard ? (korean ? "작은 보트와 산호초 항로를 개방합니다." : "Unlocks a small boat and the reef route.") : (korean ? "잡은 생물을 전시하고 주민을 불러옵니다." : "Displays creatures and brings villagers back."), smallStyle);
            if (level < 2)
            {
                string cost = $"{definition.coinCosts[level]} ◉  +  {definition.materialCosts[level]} ◆";
                if (GUI.Button(new Rect(rect.x + 20, rect.y + 122, 460, 48), (korean ? "복구하기  " : "RESTORE  ") + cost, primaryButtonStyle)) Upgrade(id);
            }
            else GUI.Label(new Rect(rect.x + 20, rect.y + 130, 460, 38), korean ? "복구 완료" : "RESTORATION COMPLETE", centeredStyle);
            if (selected) GUI.Label(new Rect(rect.x + 440, rect.y + 12, 40, 28), "●", bodyStyle);
        }

        private void DrawCollection(float width, float height)
        {
            Rect panel = new(width - 650, 0, 650, height); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 35, 40, 570, 46), korean ? "바다 도감" : "FIELD GUIDE", titleStyle);
            GUI.Label(new Rect(panel.x + 35, 91, 570, 32), $"{save.inventory.discoveredFish.Count} / {fish.Count} {(korean ? "발견" : "DISCOVERED")}", bodyStyle);
            float y = 145;
            foreach (FishDefinition definition in fish)
            {
                bool found = save.inventory.discoveredFish.Contains(definition.id);
                GUI.Box(new Rect(panel.x + 35, y, 570, 72), string.Empty, new GUIStyle(panelStyle) { normal = { background = paleTexture } });
                GUI.Label(new Rect(panel.x + 55, y + 12, 360, 28), found ? definition.LocalizedName(korean) : (korean ? "아직 만나지 못한 생물" : "UNDISCOVERED CREATURE"), bodyStyle);
                GUI.Label(new Rect(panel.x + 55, y + 40, 470, 24), found ? definition.LocalizedDescription(korean) : (korean ? "물결과 시간대를 관찰해 보세요." : "Watch the ripples and time of day."), smallStyle);
                if (save.inventory.displayedFish.Contains(definition.id)) GUI.Label(new Rect(panel.x + 490, y + 12, 90, 25), korean ? "전시 중" : "ON SHOW", smallStyle);
                y += 82;
            }
            if (GUI.Button(new Rect(panel.x + 35, height - 70, 570, 48), korean ? "닫기  ESC" : "CLOSE  ESC", buttonStyle)) SetPhase(GamePhase.Explore);
        }

        private void DrawPause(float width, float height)
        {
            Rect panel = new(width / 2f - 310, height / 2f - 280, 620, 560); GUI.Box(panel, string.Empty, panelStyle);
            GUI.Label(new Rect(panel.x + 30, panel.y + 28, 440, 45), korean ? "잠시 쉬어가기" : "TAKE A BREATH", titleStyle);
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 92, 490, 50), korean ? "계속하기" : "CONTINUE", primaryButtonStyle)) SetPhase(phaseBeforePause);
            for (int slot = 0; slot < 3; slot++)
            {
                int capturedSlot = slot;
                float y = panel.y + 164 + slot * 70;
                if (GUI.Button(new Rect(panel.x + 65, y, 235, 50), $"{(korean ? "저장" : "SAVE")} {slot + 1}", buttonStyle)) { CapturePosition(); saves.Save(save, capturedSlot); ShowMessage(T("saved")); }
                if (GUI.Button(new Rect(panel.x + 320, y, 235, 50), $"{(korean ? "불러오기" : "LOAD")} {slot + 1}", buttonStyle)) { save = saves.Load(capturedSlot); RestorePosition(); ShowMessage(T("loaded")); }
            }
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 390, 490, 50), korean ? "언어 변경 / LANGUAGE" : "LANGUAGE / 한국어", buttonStyle)) korean = !korean;
            if (GUI.Button(new Rect(panel.x + 65, panel.y + 462, 490, 50), korean ? "게임 종료" : "QUIT GAME", buttonStyle)) Application.Quit();
        }

        private void DrawMeter(Rect rect, string label, float value, Color color)
        {
            GUI.Label(new Rect(rect.x, rect.y - 22, 160, 22), label, smallStyle);
            GUI.Box(rect, string.Empty, new GUIStyle(GUI.skin.box) { normal = { background = darkTexture } });
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 3, (rect.width - 6) * Mathf.Clamp01(value / 100f), rect.height - 6), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(rect, $"{value:0}%", CenteredLight());
        }

        private GUIStyle DarkBox() => new(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleCenter, normal = { background = darkTexture, textColor = Color.white }, padding = new RectOffset(10, 10, 8, 8) };
        private GUIStyle CenteredLight() => new(centeredStyle) { normal = { textColor = Color.white }, fontStyle = FontStyle.Bold };
    }
}
