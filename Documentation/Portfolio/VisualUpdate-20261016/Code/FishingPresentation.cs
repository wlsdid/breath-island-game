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
        private FishingWaterEffects waterEffects;
        private LineRenderer line, ripple;
        private Transform bobber;
        private float clock, castTime, gestureTime, gesture, bend, reelAngle;
        private FishingStage previousStage;
        private bool wasVisible;
        private Vector3 castStart, waterTarget;
        private Vector3 castTarget, castForward=Vector3.forward;
        private bool released;
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
            waterEffects = rig.AddComponent<FishingWaterEffects>();
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
                released=false;
                castForward=Vector3.ProjectOnPlane(world.camera.transform.forward,Vector3.up).normalized;
                castTarget=world.player.transform.position+castForward*10;
                castTarget.z=Mathf.Max(28,castTarget.z);
                waterEffects.ResetCast();
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
            Vector3 forward = castForward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 origin = world.player.transform.position;
            var nearTarget=origin+forward*4;nearTarget.z=Mathf.Max(26.3f,nearTarget.z);
            waterTarget = Vector3.Lerp(castTarget,nearTarget,hooked ? fishing.Progress/100 : 0);
            waterTarget.y = OceanSurface.Height(waterTarget, Time.time) + .035f;
            float jump = hooked ? FishingPose.JumpHeight(clock) : 0;
            Vector3 endpoint = waterTarget + right * (hooked ? Mathf.Sin(clock * 1.8f) * 1.3f : 0);
            if (biting) endpoint += right * (Mathf.Sin(clock * 19) * .09f);
            endpoint.y = OceanSurface.FloatHeight(endpoint,Time.time,biting,clock);
            if (waiting && castTime < FishingPose.CastDuration)
                endpoint = FishingPose.CastPoint(castStart, waterTarget, Mathf.InverseLerp(.28f,FishingPose.CastDuration,castTime));
            float pulse = gestureTime < .55f ? Mathf.Sin(gestureTime / .55f * Mathf.PI) * gesture : 0;
            float cast = waiting ? FishingPose.CastSwing(castTime) : 0;
            bend = Mathf.MoveTowards(bend, hooked ? fishing.Tension : 0, dt * 110);
            float pull = hooked ? Mathf.Sin(clock * 1.8f) * Mathf.Clamp01(bend / 100) : 0;
            rod.transform.SetPositionAndRotation(camera.TransformPoint(new Vector3(.38f, .08f + pulse * .035f, 1.05f - pulse * .12f)),
                camera.rotation * Quaternion.Euler(-21 - pulse * 14 - cast * 32 + (hooked ? Mathf.Sin(clock * 5) * bend * .025f : 0), -12 + pull * 10, -6 - pull * 6));
            if (pulse > 0) reelAngle += dt * pulse * 850;
            rod.Pose(bend, reelAngle);
            if(waiting && !released)
            {
                castStart=rod.Tip;endpoint=castStart;
                if(castTime>=.28f)released=true;
            }
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
            Vector3 heading=hooked ? right*Mathf.Cos(clock*1.8f)-forward*.25f : -forward;
            fish.transform.rotation=Quaternion.Slerp(fish.transform.rotation,Quaternion.LookRotation(heading)*Quaternion.Euler(jumpPitch,0,8*Mathf.Sin(clock*6)),dt*8);
            waterEffects.Sample(fishing.Stage,castTime,endpoint,fishPosition,dt);
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
