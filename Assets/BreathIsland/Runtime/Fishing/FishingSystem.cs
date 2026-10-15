using System.Linq;
using UnityEngine;

namespace BreathIsland
{
    public enum FishingStage { Ready, Waiting, Hooked, Failed }
    public enum FishingVisualAction { Cast, Reel, Ease }

    public sealed class FishingSystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly System.Action beginPredatorCombat;
        private float biteTimer;

        public FishingSystem(GameContext context, GamePhaseController phases, System.Action beginPredatorCombat)
        {
            this.context = context;
            this.phases = phases;
            this.beginPredatorCombat = beginPredatorCombat;
        }

        public FishingStage Stage { get; private set; }
        public FishDefinition ActiveFish { get; private set; }
        public float Progress { get; private set; }
        public float Tension { get; private set; }
        public event System.Action<FishingVisualAction> PresentAction;

        public void Start()
        {
            if (phases.Current != GamePhase.Explore) return;
            if (context.Save.inventory.bait <= 0) { context.Message(context.Text("no_bait")); return; }
            Stage = FishingStage.Ready;
            Progress = 0f;
            Tension = 32f;
            phases.Set(GamePhase.Fishing);
        }

        public void Tick() => Tick(Time.deltaTime);

        public void Tick(float deltaTime)
        {
            if (phases.Current != GamePhase.Fishing) return;
            if (Stage == FishingStage.Ready && GameInput.PrimaryPressed()) Cast();
            else if (Stage == FishingStage.Waiting)
            {
                biteTimer -= deltaTime;
                if (biteTimer <= 0f) HookFish();
            }
            else if (Stage == FishingStage.Hooked)
            {
                Tension = FishingMath.PassiveTension(Tension, ActiveFish.pullStrength, deltaTime);
                Progress = FishingMath.PassiveProgress(Progress, deltaTime);
                if (GameInput.PrimaryPressed()) Reel();
                if (GameInput.EasePressed()) Ease();
                if (Tension >= 99.9f)
                {
                    Stage = FishingStage.Failed;
                    context.Message(context.Text("line_broke"));
                }
            }
            else if (Stage == FishingStage.Failed && GameInput.PrimaryPressed()) Stage = FishingStage.Ready;
        }

        public void Retry() { if (Stage == FishingStage.Failed) Stage = FishingStage.Ready; }

        public void Cast()
        {
            if (phases.Current != GamePhase.Fishing || Stage != FishingStage.Ready) return;
            if (context.Save.inventory.bait <= 0) { context.Message(context.Text("no_bait")); return; }
            context.Save.inventory.bait--;
            context.Save.player.hour = (context.Save.player.hour + 1) % 24;
            Stage = FishingStage.Waiting;
            biteTimer = Random.Range(1.2f, 2.2f);
            PresentAction?.Invoke(FishingVisualAction.Cast);
        }

        public void Reel()
        {
            if (phases.Current != GamePhase.Fishing || Stage != FishingStage.Hooked) return;
            float progress = Progress;
            float tension = Tension;
            FishingMath.Reel(ref progress, ref tension);
            Progress = progress;
            Tension = tension;
            PresentAction?.Invoke(FishingVisualAction.Reel);
            if (Progress < 99.9f) return;
            if (ActiveFish.IsPredator) beginPredatorCombat();
            else phases.Set(GamePhase.CatchResolution);
        }

        public void Ease()
        {
            if (phases.Current != GamePhase.Fishing || Stage != FishingStage.Hooked) return;
            float tension = Tension;
            FishingMath.Ease(ref tension);
            Tension = tension;
            PresentAction?.Invoke(FishingVisualAction.Ease);
        }

        public void Resolve(CatchDisposition disposition)
        {
            if (phases.Current != GamePhase.CatchResolution || ActiveFish == null || (ActiveFish.IsPredator && disposition == CatchDisposition.Sell)) return;
            GameEconomy.ApplyCatch(context.Save, ActiveFish, disposition);
            context.SaveTo();
            phases.Set(GamePhase.Explore);
            context.Message(disposition switch
            {
                CatchDisposition.Sell => string.Format(context.Text("sold"), ActiveFish.coinValue),
                CatchDisposition.Display => context.Text("displayed"),
                CatchDisposition.Material => string.Format(context.Text("materials"), ActiveFish.materialValue),
                _ => context.Text("recorded")
            });
        }

        public void TickResolutionInput()
        {
            if (GameInput.PadWestPressed() && !ActiveFish.IsPredator) Resolve(CatchDisposition.Sell);
            else if (GameInput.PadSouthPressed()) Resolve(CatchDisposition.Record);
            else if (GameInput.PadNorthPressed()) Resolve(CatchDisposition.Display);
            else if (GameInput.PadEastPressed()) Resolve(CatchDisposition.Material);
        }

        private void HookFish()
        {
            bool predator = PredatorDue;
            var pool = context.Content.Fish.Where(definition => !definition.IsPredator && IsAvailable(definition, context.Save.player.hour)).ToList();
            // Preserve casting cost and authored spawn hours even when the night pool is empty.
            if (!predator && pool.Count == 0)
            {
                Stage = FishingStage.Ready;
                context.Message(context.Text("no_fish"));
                return;
            }
            ActiveFish = predator ? context.Content.Fish.First(definition => definition.IsPredator) : pool[Random.Range(0, pool.Count)];
            Stage = FishingStage.Hooked;
            Progress = 8f;
            Tension = 34f;
            context.Message(string.Format(context.Text("hooked"), ActiveFish.LocalizedName(context.Korean)));
        }

        private bool PredatorDue => context.Save.inventory.totalCatchCount >= 2 && context.Save.inventory.totalCatchCount % 4 == 2;

        private static bool IsAvailable(FishDefinition definition, int hour)
        {
            return definition.startHour <= definition.endHour
                ? hour >= definition.startHour && hour <= definition.endHour
                : hour >= definition.startHour || hour <= definition.endHour;
        }
    }
}
