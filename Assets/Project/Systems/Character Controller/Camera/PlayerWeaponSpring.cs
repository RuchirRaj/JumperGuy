using RR.Attributes;
using RR.Gameplay.CharacterController.Camera;
using RR.UpdateSystem;
using RR.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace RR.Gameplay.CharacterController.Weapon
{
    [AddComponentMenu("Ruchir/Character/Weapon/Weapon Spring")]
    public class PlayerWeaponSpring : MonoBehaviour, IBatchLateUpdate
    {
        public Vector3 restPosition;
        public Quaternion restRotation = Quaternion.identity;
        
        [Header("Spring")] 
        public SpringSettings positionSpring;
        public SpringSettings angularSpring;

        [Space]
        public bool bob = true;
        [Range(0, 10)] public float bobAmpMultiplier = 1;
        public AnimationCurve bobMultiplierOverSpeed = new()
        {
            keys = new[]
            {
                new Keyframe(0, 0), new Keyframe(1, 1)
            }
        };


        [Space]
        [Range(0, 10)] public float bobRateMultiplier = 1;

        public AnimationCurve bobRateOverSpeed = new()
        {
            keys = new[]
            {
                new Keyframe(0, 0), new Keyframe(1, 1)
            }
        };

        [Space] 
        public float bobChangeSpeed = 2;
        public float bobExtraSpeed = 2;
        public Vector4 bobAmplitude = Vector4.one;
        public Vector4 bobRate = Vector4.one;

        [Header("Sway")] 
        public bool sway = true;
        [Space] 
        public float swayMultiplier = 1;
        public Vector2 swayLookMultiplier = Vector2.one;

        [Space] 
        public Vector3 rotationLookSway = Vector3.one;
        public Vector3 rotationStrafeSway = Vector3.one;
        public Vector3 rotationFallSway = Vector3.one;
        public float rotationSlopeSway = 1;
        [Space]
        [Range(0, 10)]public float positionFallRetract = 1;
        public Vector3 positionWalkSlide = Vector3.one;

        [Space] 
        public Vector3 pivotOffset = Vector3.zero;

        [Space]
        [CustomTitle("Update", 1f, 0.79f, 0.98f)]
        public UpdateMethod lateUpdate = new() { autoUpdate = true };

        //Private
        private CharacterController _controller;
        private float _bobSpeed, _lastBobSpeed;
        private Vector4 _currentBobAmp, _currentBobVal;
        private Vector4 _currentBobTime;
        private CameraSpring _bobSpring;

        //Const
        private static readonly Vector4 TimeClamp = new Vector4(1, 1, 1, 1) * 100;

        private void OnEnable()
        {
            _controller = GetComponentInParent<CharacterController>();
            _bobSpring ??= new CameraSpring();
            this.RegisterLateUpdate(lateUpdate);
        }

        private void OnDisable()
        {
            this.DeregisterLateUpdate();
        }

        public void BatchLateUpdate(float dt, float sdt)
        {
            if (!_controller) return;
            var grounded = _controller.Base.GroundState != CharacterBase.GroundedState.None;
            var relativeVel = _controller.Rigidbody.velocity - _controller.GroundVelUnProcessed;
            UpdateSway(dt, relativeVel, grounded);
            relativeVel = grounded
                ? Vector3.ProjectOnPlane(_controller.Rigidbody.velocity, _controller.Base.Sensor.averageHit.normal) -
                  _controller.GroundVel
                : Vector3.zero;
            UpdateBob(dt, relativeVel);
            UpdateSpring(dt);
        }

        private void UpdateBob(float dt, Vector3 vel)
        {
            if (!bob || bobAmplitude == Vector4.zero || bobRate == Vector4.zero)
                return;

            _bobSpeed = vel.magnitude + bobExtraSpeed;

            // if speed is zero, this means we should just fade out the last stored
            // speed value. NOTE: it's important to clamp it to the current max input
            // velocity since the preset may have changed since last bob!
            if (_bobSpeed == 0)
                _bobSpeed = Mathf.Min(_lastBobSpeed * (1 - bobChangeSpeed * dt), 0);

            var b = bobMultiplierOverSpeed.Evaluate(_bobSpeed);

            _currentBobTime += bobRate * (bobRateOverSpeed.Evaluate(_bobSpeed) * bobRateMultiplier * dt);

            _currentBobTime = MathUtils.WrapValue(-TimeClamp, TimeClamp, _currentBobTime);

            _currentBobAmp.x = b * bobAmplitude.x;
            _currentBobVal.x = Mathf.Cos(_currentBobTime.x * 2 * Mathf.PI) * _currentBobAmp.x * 10;

            _currentBobAmp.y = b * bobAmplitude.y;
            _currentBobVal.y = Mathf.Cos(_currentBobTime.y * 2 * Mathf.PI) * _currentBobAmp.y * 10;

            _currentBobAmp.z = b * bobAmplitude.z;
            _currentBobVal.z = Mathf.Cos(_currentBobTime.z * 2 * Mathf.PI) * _currentBobAmp.z * 10;

            _currentBobAmp.w = b * bobAmplitude.w;
            _currentBobVal.w = Mathf.Cos(_currentBobTime.w * 2 * Mathf.PI) * _currentBobAmp.w * 10;

            _bobSpring.AddForce(_currentBobVal * bobAmpMultiplier,
                ForceMode.Force, dt);
            _bobSpring.AddTorque(Vector3.forward * (_currentBobVal.w * bobAmpMultiplier),
                ForceMode.Force, dt);
        }

        private void UpdateSway(float dt, Vector3 vel, bool grounded)
        {
            if (!sway)
                return;
            var look = _controller.InputState.Look;
            look.Scale(swayLookMultiplier);

            // --- pitch & yaw rotational sway ---
            var torque = new Vector3(
                look.y * (rotationLookSway.x * 0.025f),
                look.x * (rotationLookSway.y * -0.025f),
                look.x * (rotationLookSway.z * -0.025f));

            // --- weapon strafe rotate ---
            // RotationStrafeSway x will rotate up when strafing (it can't push down)
            // RotationStrafeSway y will rotate sideways when strafing
            // RotationStrafeSway z will twist weapon around the forward vector when strafing
            var localVelocity = _controller.RefTransform.InverseTransformDirection(vel);
            if (grounded)
                torque += new Vector3(
                    -Mathf.Abs(localVelocity.x * rotationStrafeSway.x),
                    -(localVelocity.x * rotationStrafeSway.y),
                    localVelocity.x * rotationStrafeSway.z);

            // --- falling ---

            // rotate weapon while falling. this will take effect in reverse when being elevated,
            // for example walking up a ramp. however, the weapon will only rotate around the z
            // vector while going down
            var fallSway = rotationFallSway * localVelocity.y;
            // if grounded, optionally reduce fallsway
            if (grounded)
                fallSway *= rotationSlopeSway;
            fallSway.z = Mathf.Max(0.0f, fallSway.z);
            torque += fallSway;

            torque *= swayMultiplier;

            _bobSpring.AddTorque(torque, ForceMode.Force, dt);

            var force = Vector3.zero;

            // drag weapon towards ourselves
            force += Vector3.forward * -Mathf.Abs(localVelocity.y * positionFallRetract);

            // --- weapon strafe & walk slide ---
            // PositionWalkSlide x will slide sideways when strafing
            // PositionWalkSlide y will slide down when strafing (it can't push up)
            // PositionWalkSlide z will slide forward or backward when walking
            force += new Vector3(
                localVelocity.x * positionWalkSlide.x,
                -Mathf.Abs(localVelocity.magnitude * positionWalkSlide.y),
                -localVelocity.z * positionWalkSlide.z);

            _bobSpring.AddForce(force, ForceMode.Force, dt);
        }

        private void UpdateSpring(float dt)
        {
            var s = _bobSpring;
            s.angularSpring = angularSpring;
            s.positionSpring = positionSpring;
            s.pivotPos = pivotOffset;
            s.targetPos = restPosition + pivotOffset;

            s.targetRot = restRotation;
            s.Update(dt, transform.localRotation, transform.localPosition);

            transform.SetLocalPositionAndRotation(s.pos, s.rot);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.TransformPoint(pivotOffset), 0.1f);
        }
    }
}