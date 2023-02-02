using System.Collections.Generic;
using Cinemachine;
using RR.Attributes;
using RR.UpdateSystem;
using RR.Utils;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [AddComponentMenu("Ruchir/Character/Camera/Camera Manager")]
    public class PlayerCameraManager : MonoBehaviour, IBatchLateUpdate
    {
        [Header("Spring")]
        public SpringSettings positionSpring;
        public SpringSettings angularSpring;
        
        public float minCamHeight = 0.1f;
        public float maxVelocity = 5;
        [Header("Bob")] 
        public bool bob = true;
        [Range(0,10)] public float bobAmpMultiplier = 1;
        public AnimationCurve bobMultiplierOverSpeed = new()
        {
            keys = new []
            {
                new Keyframe(0, 0), new Keyframe(1,1)
            }
        };

        [Space]
        [Range(0,10)] public float bobRateMultiplier = 1;
        public AnimationCurve bobRateOverSpeed = new()
        {
            keys = new []
            {
                new Keyframe(0, 0), new Keyframe(1,1)
            }
        };
        [Space]
        public float bobChangeSpeed = 2;
        public float bobExtraSpeed = 2;
        public Vector4 bobAmplitude = Vector4.one;
        public Vector4 bobRate = Vector4.one;
        [Header("Kneeling")] 
        [Min(0)] public float maxImpactVelocity = 10f;
        [Range(0, 50)] public float maxPositionKneelingImpulse = 1f;
        public CameraSpring.SoftForce positionKneelingSoftForce;
        [Range(0, 10)] public float maxRotationKneelingImpulse = 1f;
        public CameraSpring.SoftForce rotationKneelingSoftForce;
        public Vector3 rotationKneelingImpulseDirection = Vector3.forward;
        [Header("Cameras")]
        public List<PlayerCameraBase> cameras = new();
        
        [Header("Field of View")]
        public float fovSpeed = 5;
        public Vector2 cameraFOV = new(80, 90);
        public Vector2 velocityRange = new(3, 6);
        
        [Space]
        [CustomTitle("Update", 1f, 0.79f, 0.98f)]
        public UpdateMethod lateUpdate = new() { autoUpdate = true };

        //Private
        private CharacterController _controller;
        private float _bobSpeed, _lastBobSpeed;
        private Vector4 _currentBobAmp, _currentBobVal;
        private Vector4 _currentBobTime;
        private CameraSpring _camSpring;
        private float _fov;
        private int _currentCam;
        private float _currentLowestPos;
        
        //Const
        private static readonly Vector4 TimeClamp = new Vector4(1,1,1,1) * 100;

        private void OnEnable()
        {
            _controller = GetComponentInParent<CharacterController>();
            _camSpring ??= new CameraSpring();
            _controller.InputState.onCamEvent += OnCamEvent;
            _controller.OnGroundImpact += ControllerOnGroundImpact;
            _currentLowestPos = Mathf.Infinity;
            this.RegisterLateUpdate(lateUpdate);
            if (!MainCamera.Instance)
            {
                if (UnityEngine.Camera.main != null) UnityEngine.Camera.main.gameObject.AddComponent<MainCamera>();
            }
            SetActiveCamera(_currentCam, true);
        }

        private void ControllerOnGroundImpact(Vector3 vel, Vector3 localVel)
        {
            if(localVel.y < 0)
                OnImpact(localVel);
        }

        private void OnDisable()
        {
            _controller.InputState.onCamEvent -= OnCamEvent;
            _controller.OnGroundImpact -= ControllerOnGroundImpact;
            this.DeregisterLateUpdate();
        }
        
        private void OnCamEvent(bool value)
        {
            if (value)
                SetActiveCamera(_currentCam + 1);
        }

        private void SetActiveCamera(int index, bool forceDisable = false)
        {
            index %= cameras.Count;
            Debug.Log(index);
            
            for (int i = 0; i < cameras.Count; i++)
            {
                if (i == index)
                    cameras[i].SetActive(true, _controller);
                else if(forceDisable || cameras[i].Active)
                    cameras[i].SetActive(false, _controller);
            }

            _currentCam = index;
        }

        private void Start()
        {
            if (MainCamera.Instance)
                MainCamera.Instance.GetComponent<CinemachineBrain>().WorldUpOverride = _controller.BaseRefTransform;
        }

        public void BatchLateUpdate(float dt, float sdt)
        {
            if(!_controller) return;
            var grounded = _controller.Base.GroundState != CharacterBase.GroundedState.None;
            var relativeVel = grounded ?
                Vector3.ProjectOnPlane(_controller.Rigidbody.velocity, _controller.Base.Sensor.averageHit.normal) -
                _controller.GroundVel : Vector3.zero;
            UpdateBob(dt, relativeVel);
            UpdateSpring(dt);
            UpdateFOV(dt, relativeVel);
        }

        private void UpdateFOV(float dt, Vector3 relativeVel)
        {
            //TODO Add Target FOV according to player state
            //Placeholder logic
            var vel = relativeVel.magnitude;
            _fov = Mathf.Clamp(Mathf.Lerp(_fov, Mathf.Clamp(vel, velocityRange.x, velocityRange.y)
                    .Remap(velocityRange.x, velocityRange.y, cameraFOV.x, cameraFOV.y), fovSpeed * dt), cameraFOV.x,
                cameraFOV.y);
            cameras[_currentCam].SetFOV(_fov);
        }

        private void UpdateSpring(float dt)
        {
            _camSpring.angularSpring = angularSpring;
            _camSpring.positionSpring = positionSpring;

            var s = _camSpring;
            s.targetRot = Quaternion.identity;
            s.targetPos = new Vector3(0,
                _controller.CurrentShapeState.shape.value.height - _controller.CurrentShapeState.shape.value.radius, 0);
            s.Update(dt, transform.localRotation, transform.localPosition);

            transform.SetLocalPositionAndRotation(
                MathUtils.ClampValue(
                    new(0, minCamHeight - _controller.Base.Sensor.averageHit.distance, 0), 
                    new(0, Mathf.Infinity /*_controller.CurrentShapeState.shape.value.height*/, 0), s.pos),
                s.rot);
        }

        void UpdateBob(float dt, Vector3 vel)
        {
            if (!bob || bobAmplitude == Vector4.zero || bobRate == Vector4.zero)
                return;
            
            _bobSpeed = vel.magnitude + bobExtraSpeed;

            // if speed is zero, this means we should just fade out the last stored
            // speed value. NOTE: it's important to clamp it to the current max input
            // velocity since the preset may have changed since last bob!
            if (_bobSpeed == 0)
                _bobSpeed = Mathf.Min(_lastBobSpeed * (1 - (bobChangeSpeed * dt)), 0);

            var b = bobMultiplierOverSpeed.Evaluate(_bobSpeed / maxVelocity);
            
            _currentBobTime += bobRate * (bobRateOverSpeed.Evaluate(_bobSpeed / maxVelocity) * bobRateMultiplier * dt);

            _currentBobTime = MathUtils.WrapValue(-TimeClamp, TimeClamp, _currentBobTime);

            _currentBobAmp.x = (b * (bobAmplitude.x));
            _currentBobVal.x = (Mathf.Cos(_currentBobTime.x * 2 * Mathf.PI)) * _currentBobAmp.x * 10;

            _currentBobAmp.y = (b * (bobAmplitude.y));
            _currentBobVal.y = (Mathf.Cos(_currentBobTime.y * 2 * Mathf.PI)) * _currentBobAmp.y * 10;

            _currentBobAmp.z = (b * (bobAmplitude.z));
            _currentBobVal.z = (Mathf.Cos(_currentBobTime.z * 2 * Mathf.PI)) * _currentBobAmp.z * 10;

            _currentBobAmp.w = (b * (bobAmplitude.w));
            _currentBobVal.w = (Mathf.Cos(_currentBobTime.w * 2 * Mathf.PI)) * _currentBobAmp.w * 10;

            _camSpring.AddForce(_currentBobVal * bobAmpMultiplier,
                ForceMode.Force, dt);
            _camSpring.AddTorque(Vector3.forward * (_currentBobVal.w * bobAmpMultiplier),
                ForceMode.Force, dt);
        }

        /// <summary>
        /// Add Impact kneeling
        /// </summary>
        /// <param name="impact">Impact in local co-ordinates</param>
        public void OnImpact(Vector3 impact)
        {
            float posImpact = Mathf.Abs(impact.y / maxImpactVelocity);
            float rotImpact = Mathf.Abs(impact.y / maxImpactVelocity);

            // smooth step the impacts to make the springs react more subtly
            // from short falls, and more aggressively from longer falls
            posImpact = Mathf.SmoothStep(0, 1, posImpact);
            rotImpact = Mathf.SmoothStep(0, 1, rotImpact);
            rotImpact = Mathf.SmoothStep(0, 1, rotImpact);

            _camSpring.AddImpulse(Vector3.down * (maxPositionKneelingImpulse * posImpact));
            _camSpring.AddImpulseTorque(rotationKneelingImpulseDirection * (maxRotationKneelingImpulse * rotImpact));

            _camSpring.AddSoftPositionForce(new CameraSpring.SoftForce(posImpact * positionKneelingSoftForce.time,
                posImpact * positionKneelingSoftForce.force));
            _camSpring.AddSoftRotationForce(new CameraSpring.SoftForce(rotImpact * rotationKneelingSoftForce.time,
                rotImpact * rotationKneelingSoftForce.force));
        }
    }
}