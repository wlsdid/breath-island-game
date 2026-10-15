using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BreathIsland.Tests
{
    // Virtual keyboard/mouse integration tests must not depend on Editor window focus.
    // Use a temporary settings clone so neither the game nor the project asset changes.
    [SetUpFixture]
    public sealed class PlayModeInputEnvironment
    {
        private InputSettings original;
        private InputSettings temporary;
        private bool wasRunningInBackground;

        [OneTimeSetUp] public void Begin()
        {
            original=InputSystem.settings;
            temporary=Object.Instantiate(original);
            temporary.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            temporary.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings=temporary;
            wasRunningInBackground=Application.runInBackground;
            Application.runInBackground=true;
        }

        [OneTimeTearDown] public void End()
        {
            InputSystem.settings=original;
            Application.runInBackground=wasRunningInBackground;
            Object.Destroy(temporary);
        }
    }
}
