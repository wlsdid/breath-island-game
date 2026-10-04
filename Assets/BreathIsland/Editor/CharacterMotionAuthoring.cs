using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BreathIsland.Editor
{
    public static class CharacterMotionAuthoring
    {
        public static void Build()
        {
            const string path = "Assets/BreathIsland/Art/ThirdParty/KayKit/Character/Rig_Medium_MovementBasic.fbx";
            var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().ToArray();
            var controller = (AnimatorController)Resources.Load<IslandLifeCatalog>("Content/IslandLife").animator;
            var machine = controller.layers[0].stateMachine;
            Parameter(controller, "Airborne", AnimatorControllerParameterType.Bool);
            Parameter(controller, "VerticalSpeed", AnimatorControllerParameterType.Float);
            var start = State(machine, "Jump Start", clips.First(c => c.name == "Jump_Start"));
            var air = State(machine, "Jump Air", clips.First(c => c.name == "Jump_Idle"));
            var land = State(machine, "Jump Land", clips.First(c => c.name == "Jump_Land"));
            var idle = machine.states.First(s => s.state.name == "Idle").state;
            var walk = machine.states.First(s => s.state.name == "Walk").state;
            foreach (var source in new[] { idle, walk })
            {
                Transition(source, start, false, "Airborne", AnimatorConditionMode.If);
                foreach (var t in source.transitions.Where(t => t.destinationState == idle || t.destinationState == walk))
                    if (!t.conditions.Any(c => c.parameter == "Airborne")) t.AddCondition(AnimatorConditionMode.IfNot, 0, "Airborne");
            }
            Transition(start, air, true);
            Transition(start, land, false, "Airborne", AnimatorConditionMode.IfNot);
            Transition(air, land, false, "Airborne", AnimatorConditionMode.IfNot);
            Transition(land, idle, true);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        private static void Parameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        { if (!controller.parameters.Any(p => p.name == name)) controller.AddParameter(name, type); }
        private static AnimatorState State(AnimatorStateMachine machine, string name, AnimationClip clip)
        {
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name) ?? machine.AddState(name);
            state.motion = clip; return state;
        }
        private static void Transition(AnimatorState from, AnimatorState to, bool exit, string parameter = null, AnimatorConditionMode mode = AnimatorConditionMode.If)
        {
            if (from.transitions.Any(t => t.destinationState == to)) return;
            var transition = from.AddTransition(to); transition.hasExitTime = exit;
            transition.exitTime = .85f; transition.duration = .08f;
            if (parameter != null) transition.AddCondition(mode, 0, parameter);
        }
    }
}
