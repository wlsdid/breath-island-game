using UnityEngine;

namespace BreathIsland
{
    [DefaultExecutionOrder(150)]
    public sealed class FishingPresentation : MonoBehaviour
    {
        private FishingSystem fishing;
        private GamePhaseController phases;
        private WorldHandles world;
        private GameObject rig, fish;
        private FishingRodView rod;
        private LineRenderer line, ripple;
        private Transform bobber;
        private float clock, castTime, gestureTime, gesture, bend, reelAngle;
        private FishingStage previousStage;
        private bool wasVisible;
        private Vector3 castStart, waterTarget;
        public bool Visible => rig != null && rig.activeSelf;

        public void Initialize(WorldHandles handles, FishingSystem system, GamePhaseController controller)
        {
            world = handles; fishing = system; phases = controller;
            rig = Instantiate(Resources.Load<GameObject>("World/FishingRig"), transform);
            rod = rig.GetComponentInChildren<FishingRodView>();
            line = rig.transform.Find("Line").GetComponent<LineRenderer>();
            ripple = rig.transform.Find("Ripple").GetComponent<LineRenderer>();
            bobber = rig.transform.Find("Bobber");
            fish = rig.transform.Find("Fish").gameObject;
            fishing.PresentAction += OnAction;
            phases.Changed += OnPhaseChanged;
            rig.SetActive(false);
        }

        private void OnPhaseChanged()
        {
            if (phases.Current != GamePhase.Fishing) { rig.SetActive(false); wasVisible = false; }
        }

        private void OnAction(FishingVisualAction action)
        {
            if (action == FishingVisualAction.Cast)
            {
                castTime = 0; castStart = rod.Tip;
            }
            else { gesture = action == FishingVisualAction.Ease ? -1 : action == FishingVisualAction.Hook ? 1.5f : 1; gestureTime = 0; }
        }

        private void LateUpdate()
        {
            if (fishing == null) return;
            bool visible = phases.Current == GamePhase.Fishing;
            rig.SetActive(visible);
            if (!visible) { wasVisible = false; return; }
            if (!wasVisible)
            {
                // Resume retains cast/gesture clocks; entering a new ready session resets them.
                if (fishing.Stage == FishingStage.Ready) { clock = 0; gestureTime = 1; bend = 0; }
                previousStage = fishing.Stage; wasVisible = true;
            }
            float dt = Time.deltaTime;
            clock += dt; castTime += dt; gestureTime += dt;
            bool hooked = fishing.Stage == FishingStage.Hooked;
            bool waiting = fishing.Stage == FishingStage.Waiting;
            bool biting = fishing.Stage == FishingStage.Bite;
            if ((hooked || biting) && previousStage != fishing.Stage) clock = 0;
            previousStage = fishing.Stage;
            var camera = world.camera.transform;
            Vector3 forward = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 origin = world.player.transform.position;
            waterTarget = origin + forward * Mathf.Lerp(10, 4, hooked ? fishing.Progress / 100 : 0);
            waterTarget.y = OceanSurface.Height(waterTarget, Time.time) + .035f;
            float jump = hooked ? FishingPose.JumpHeight(clock) : 0;
            Vector3 endpoint = waterTarget + right * (hooked ? Mathf.Sin(clock * 1.8f) * 1.3f : 0);
            if (biting) endpoint += right * (Mathf.Sin(clock * 19) * .09f);
            endpoint.y = OceanSurface.FloatHeight(endpoint,Time.time,biting,clock);
            if (waiting && castTime < FishingPose.CastDuration)
                endpoint = FishingPose.CastPoint(castStart, waterTarget, castTime / FishingPose.CastDuration);
            float pulse = gestureTime < .55f ? Mathf.Sin(gestureTime / .55f * Mathf.PI) * gesture : 0;
            float cast = waiting && castTime < FishingPose.CastDuration ? Mathf.Sin(castTime / FishingPose.CastDuration * Mathf.PI * 2) : 0;
            bend = Mathf.MoveTowards(bend, hooked ? fishing.Tension : 0, dt * 110);
            rod.transform.SetPositionAndRotation(camera.TransformPoint(new Vector3(.42f, -.19f + pulse * .035f, .95f - pulse * .12f)),
                camera.rotation * Quaternion.Euler(-21 - pulse * 14 - cast * 32 + (hooked ? Mathf.Sin(clock * 5) * bend * .025f : 0), -12, -6));
            if (pulse > 0) reelAngle += dt * pulse * 850;
            rod.Pose(bend, reelAngle);
            bool deployed = waiting || biting || hooked;
            line.enabled = deployed; bobber.gameObject.SetActive(deployed); ripple.enabled = deployed;
            fish.SetActive(hooked || biting);
            if (hooked || biting) fish.GetComponent<FishingCatchView>().Show(fishing.ActiveFish.id);
            bobber.position = endpoint;
            bobber.rotation = Quaternion.FromToRotation(Vector3.up,OceanSurface.Normal(endpoint,Time.time))
                * (biting ? Quaternion.Euler(0, 0, Mathf.Sin(clock * 17) * 24) : Quaternion.identity);
            Vector3 fishPosition=endpoint+forward*(biting ? (1-Mathf.Clamp01(clock/.6f))*2 : 0);
            fishPosition.y=OceanSurface.Height(fishPosition,Time.time)-.6f+jump*1.45f;
            float jumpPitch=jump>0 ? -Mathf.Cos(Mathf.Repeat(clock,4.8f)/.95f*Mathf.PI)*40 : 0;
            fish.transform.position=fishPosition;
            fish.transform.rotation=Quaternion.Slerp(fish.transform.rotation,Quaternion.Euler(jumpPitch,hooked ? (Mathf.Cos(clock*1.8f)>0 ? 90 : -90) : 180,8*Mathf.Sin(clock*6)),dt*8);
            for (int i = 0; i < line.positionCount; i++) line.SetPosition(i, FishingPose.LinePoint(rod.Tip, endpoint, i / (line.positionCount - 1f), bend));
            float radius = .25f + Mathf.Repeat(clock * (biting ? 2.8f : .7f), 1) * .8f;
            for (int i = 0; i < ripple.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / (ripple.positionCount - 1);
                var point=new Vector3(endpoint.x + Mathf.Cos(angle) * radius,0,endpoint.z + Mathf.Sin(angle) * radius);
                point.y=OceanSurface.Height(point,Time.time)+.015f;
                ripple.SetPosition(i,point);
            }
            ripple.enabled = deployed && !(waiting && castTime < FishingPose.CastDuration);
        }

        private void OnDestroy()
        {
            if (fishing != null) fishing.PresentAction -= OnAction;
            if (phases != null) phases.Changed -= OnPhaseChanged;
        }
    }
}
