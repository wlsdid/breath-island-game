using UnityEngine;
using UnityEngine.InputSystem;

namespace BreathIsland
{
    public static class GameInput
    {
        public static Vector2 Move
        {
            get
            {
                Vector2 value = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
                if (Keyboard.current == null) return value;
                Vector2 keyboard = new(
                    (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f),
                    (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f));
                return keyboard.sqrMagnitude > value.sqrMagnitude ? Vector2.ClampMagnitude(keyboard, 1f) : value;
            }
        }

        public static Vector2 Look
        {
            get
            {
                Vector2 stick = (Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero) * 115f * Time.unscaledDeltaTime;
                Vector2 mouse = Mouse.current?.delta.ReadValue() ?? Vector2.zero;
                return mouse.sqrMagnitude > .01f ? mouse * .08f : stick;
            }
        }

        public static bool InteractPressed() =>
            (Keyboard.current?.eKey.wasPressedThisFrame ?? false) || (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);
        public static bool PrimaryPressed() =>
            (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false) || (Keyboard.current?.fKey.wasPressedThisFrame ?? false) || (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);
        public static bool EasePressed() => (Keyboard.current?.sKey.wasPressedThisFrame ?? false) || (Gamepad.current?.buttonEast.wasPressedThisFrame ?? false);
        public static bool CancelPressed() => (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false) || (Gamepad.current?.startButton.wasPressedThisFrame ?? false);
        public static bool FacilityPressed() => Keyboard.current?.mKey.wasPressedThisFrame ?? false;
        public static bool CollectionPressed() => Keyboard.current?.tabKey.wasPressedThisFrame ?? false;
        public static bool SavePressed() => Keyboard.current?.f5Key.wasPressedThisFrame ?? false;
        public static bool LoadPressed() => Keyboard.current?.f9Key.wasPressedThisFrame ?? false;
        public static bool LanguagePressed() => Keyboard.current?.lKey.wasPressedThisFrame ?? false;
        public static bool PadSouthPressed() => Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false;
        public static bool PadWestPressed() => Gamepad.current?.buttonWest.wasPressedThisFrame ?? false;
        public static bool PadNorthPressed() => Gamepad.current?.buttonNorth.wasPressedThisFrame ?? false;
        public static bool PadEastPressed() => Gamepad.current?.buttonEast.wasPressedThisFrame ?? false;
        public static int PadVerticalPressed()
        {
            if (Gamepad.current?.dpad.up.wasPressedThisFrame == true) return -1;
            if (Gamepad.current?.dpad.down.wasPressedThisFrame == true) return 1;
            return 0;
        }
    }

    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonMotor : MonoBehaviour
    {
        public float speed = 5.2f;
        public float turnSpeed = 12f;
        private CharacterController controller;
        private Transform cameraTransform;
        private float verticalVelocity;
        public bool CanMove { get; set; } = true;

        private void Awake() => controller = GetComponent<CharacterController>();
        public void SetCamera(Transform target) => cameraTransform = target;

        private void Update()
        {
            if (!CanMove || cameraTransform == null) return;
            Vector2 input = GameInput.Move;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 desired = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
            if (desired.sqrMagnitude > .02f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desired), turnSpeed * Time.deltaTime);
            }
            verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + Physics.gravity.y * Time.deltaTime;
            controller.Move((desired * speed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }

    public sealed class IslandCameraRig : MonoBehaviour
    {
        public Transform target;
        public bool firstPerson;
        private float yaw = 20f;
        private float pitch = 20f;
        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector2 look = GameInput.Look;
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, -8f, 55f);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired;
            if (firstPerson)
            {
                desired = target.position + Vector3.up * 1.25f + rotation * new Vector3(0f, 0f, .15f);
            }
            else
            {
                desired = target.position + Vector3.up * 1.45f + rotation * new Vector3(0f, 0f, -5.8f);
            }
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, firstPerson ? .08f : .16f);
            transform.rotation = rotation;
        }
    }
}
