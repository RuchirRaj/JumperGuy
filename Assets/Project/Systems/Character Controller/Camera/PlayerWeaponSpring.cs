using System;
using RR.Attributes;
using RR.Gameplay.CharacterController.Camera;
using RR.UpdateSystem;
using RR.Utils;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Weapon
{
    [AddComponentMenu("Ruchir/Character/Weapon/Weapon Spring")]
    public class PlayerWeaponSpring : MonoBehaviour, IBatchLateUpdate
    {
        [field: SerializeField] public Vector3 RestPosition { get; set; }
        [field: SerializeField] public Quaternion RestRotation { get; set; } = Quaternion.identity;
        [Header("Spring")]
        public SpringSettings positionSpring;
        public SpringSettings angularSpring;
        [field: Header("Bob")]
        [field: SerializeField] public bool Bob { get; set; } = true;
        [field: SerializeField] public float BobAmpMultiplier { get; set; } = 1;
        [field: SerializeField] public float BobRateMultiplier { get; set; } = 1;
        [field: SerializeField] public AnimationCurve BobMultiplierOverSpeed { get; set; } = new()
        {
            keys = new []
            {
                new Keyframe(0, 0), new Keyframe(1,1)
            }
        };
        [field: SerializeField] public AnimationCurve BobRateOverSpeed { get; set; } = new()
        {
            keys = new []
            {
                new Keyframe(0, 0), new Keyframe(1,1)
            }
        };
        [field: SerializeField] public float BobChangeSpeed { get; set; } = 2;
        [field: SerializeField] public float BobExtraSpeed { get; set; } = 2;
        [field: SerializeField] public Vector4 BobAmplitude { get; private set; }
        [field: SerializeField] public Vector4 BobRate { get; private set; }

        [field: Header("Sway")]
        [field: SerializeField] public bool Sway { get; set; } = true;
        [field: Space]
        [field: SerializeField] public float SwayMultiplier { get; set; } = 1;
        [field: SerializeField] public Vector2 SwayLookMultiplier { get; set; } = Vector2.one;
        
        [field: Space]
        [field: SerializeField] public Vector3 RotationLookSway { get; set; } = Vector3.one;
        [field: SerializeField] public Vector3 RotationStrafeSway { get; set; } = Vector3.one;
        [field: SerializeField] public Vector3 RotationFallSway { get; set; } = Vector3.one;
        [field: SerializeField] public float RotationSlopeSway { get; set; } = 1;
        [field: Space]
        [field: SerializeField] public float PositionFallRetract { get; set; } = 1;
        [field: SerializeField] public Vector3 PositionWalkSlide { get; set; } = Vector3.one;

        [field: Space]
        
        [field: SerializeField] public Vector3 PivotOffset { get; set; } = Vector3.zero;

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
        private static readonly Vector4 TimeClamp = new Vector4(1,1,1,1) * 100;
        
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
            if(!_controller) return;
            var grounded = _controller.Base.GroundState != CharacterBase.GroundedState.None;
            var relativeVel = _controller.Rigidbody.velocity - _controller.GroundVelUnProcessed;
            UpdateSway(dt, relativeVel, grounded);
            relativeVel = grounded ?
                Vector3.ProjectOnPlane(_controller.Rigidbody.velocity, _controller.Base.Sensor.averageHit.normal) -
                _controller.GroundVel : Vector3.zero;
            UpdateBob(dt, relativeVel);
            UpdateSpring(dt);
        }

        private void UpdateBob(float dt, Vector3 vel)
        {
            if (!Bob || BobAmplitude == Vector4.zero || BobRate == Vector4.zero)
                return;
            
            _bobSpeed = vel.magnitude + BobExtraSpeed;

            // if speed is zero, this means we should just fade out the last stored
            // speed value. NOTE: it's important to clamp it to the current max input
            // velocity since the preset may have changed since last bob!
            if (_bobSpeed == 0)
                _bobSpeed = Mathf.Min(_lastBobSpeed * (1 - (BobChangeSpeed * dt)), 0);

            var b = BobMultiplierOverSpeed.Evaluate(_bobSpeed);
            
            _currentBobTime += BobRate * (BobRateOverSpeed.Evaluate(_bobSpeed) * BobRateMultiplier * dt);

            _currentBobTime = MathUtils.WrapValue(-TimeClamp, TimeClamp, _currentBobTime);

            _currentBobAmp.x = (b * (BobAmplitude.x));
            _currentBobVal.x = (Mathf.Cos(_currentBobTime.x * 2 * Mathf.PI)) * _currentBobAmp.x * 10;

            _currentBobAmp.y = (b * (BobAmplitude.y));
            _currentBobVal.y = (Mathf.Cos(_currentBobTime.y * 2 * Mathf.PI)) * _currentBobAmp.y * 10;

            _currentBobAmp.z = (b * (BobAmplitude.z));
            _currentBobVal.z = (Mathf.Cos(_currentBobTime.z * 2 * Mathf.PI)) * _currentBobAmp.z * 10;

            _currentBobAmp.w = (b * (BobAmplitude.w));
            _currentBobVal.w = (Mathf.Cos(_currentBobTime.w * 2 * Mathf.PI)) * _currentBobAmp.w * 10;

            _bobSpring.AddForce(_currentBobVal * BobAmpMultiplier,
                ForceMode.Force, dt);
            _bobSpring.AddTorque(Vector3.forward * (_currentBobVal.w * BobAmpMultiplier),
                ForceMode.Force, dt);
        }

        private void UpdateSway(float dt, Vector3 vel, bool grounded)
        {
            if(!Sway)
                return;
            var look = _controller.InputState.Look;
            look.Scale(SwayLookMultiplier);
            
            // --- pitch & yaw rotational sway ---
            var torque = new Vector3(
                (look.y * (RotationLookSway.x * 0.025f)),
                (look.x * (RotationLookSway.y * -0.025f)),
                look.x * (RotationLookSway.z * -0.025f));

            // --- weapon strafe rotate ---
            // RotationStrafeSway x will rotate up when strafing (it can't push down)
            // RotationStrafeSway y will rotate sideways when strafing
            // RotationStrafeSway z will twist weapon around the forward vector when strafing
            var localVelocity = _controller.RefTransform.InverseTransformDirection(vel);
            if(grounded)
                torque += new Vector3(
                    -Mathf.Abs(localVelocity.x * RotationStrafeSway.x),
                    -(localVelocity.x * RotationStrafeSway.y),
                    localVelocity.x * RotationStrafeSway.z);
                
            // --- falling ---

            // rotate weapon while falling. this will take effect in reverse when being elevated,
            // for example walking up a ramp. however, the weapon will only rotate around the z
            // vector while going down
            var fallSway = RotationFallSway * localVelocity.y;
            // if grounded, optionally reduce fallsway
            if (grounded)
                fallSway *= RotationSlopeSway;
            fallSway.z = Mathf.Max(0.0f, fallSway.z);
            torque += (fallSway);
            
            torque *= SwayMultiplier;
            
            _bobSpring.AddTorque(torque, ForceMode.Force, dt);

            var force = Vector3.zero;
            
            // drag weapon towards ourselves
            force += Vector3.forward * -Mathf.Abs(localVelocity.y * PositionFallRetract);

            // --- weapon strafe & walk slide ---
            // PositionWalkSlide x will slide sideways when strafing
            // PositionWalkSlide y will slide down when strafing (it can't push up)
            // PositionWalkSlide z will slide forward or backward when walking
            force += new Vector3(
                (localVelocity.x * (PositionWalkSlide.x)),
                -(Mathf.Abs(localVelocity.magnitude * (PositionWalkSlide.y))),
                (-localVelocity.z * (PositionWalkSlide.z)));
            
            _bobSpring.AddForce(force, ForceMode.Force, dt);
        }
        
        private void UpdateSpring(float dt)
        {
            var s = _bobSpring;
            s.angularSpring = angularSpring;
            s.positionSpring = positionSpring;
            s.pivotPos = PivotOffset;
            s.targetPos = RestPosition + PivotOffset;

            s.targetRot = RestRotation;
            s.Update(dt, transform.localRotation, transform.localPosition);
            
            transform.SetLocalPositionAndRotation(s.pos, s.rot);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.TransformPoint(PivotOffset), 0.1f);
        }
    }   
}
