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
        
        [field: SerializeField]
        public Vector3 RestPosition { get; set; }
        [Header("Spring")]
        public SpringSettings positionSpring;
        public SpringSettings angularSpring;

        [Space] 
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
        [Space(10)]
        public List<PlayerCameraBase> cameras = new();

        [Space]
        [CustomTitle("Update", 1f, 0.79f, 0.98f)]
        public UpdateMethod lateUpdate = new() { autoUpdate = true };

        //Private
        private CharacterController _controller;
        private float _bobSpeed, _lastBobSpeed;
        private Vector4 _currentBobAmp, _currentBobVal;
        private Vector4 _currentBobTime;
        private CameraSpring _bobSpring;
        private int _currentCam;
        
        //Const
        private static readonly Vector4 TimeClamp = new Vector4(1,1,1,1) * 100;

        private void OnEnable()
        {
            _controller = GetComponentInParent<CharacterController>();
            _bobSpring ??= new CameraSpring();
            _controller.InputState.onCamEvent += OnCamEvent;
            this.RegisterLateUpdate(lateUpdate);
            SetActiveCamera(_currentCam, true);
        }
        
        private void OnDisable()
        {
            _controller.InputState.onCamEvent -= OnCamEvent;
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
            //TODO Add a proper way to manage active character reference
            if (UnityEngine.Camera.main != null)
                UnityEngine.Camera.main.GetComponent<CinemachineBrain>().WorldUpOverride = _controller.BaseRefTransform;
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
        }

        private void UpdateSpring(float dt)
        {
            _bobSpring.angularSpring = angularSpring;
            _bobSpring.positionSpring = positionSpring;

            var s = _bobSpring;
            s.targetRot = Quaternion.identity;
            s.targetPos = new Vector3(0,
                _controller.CurrentShapeState.shape.value.height - _controller.CurrentShapeState.shape.value.radius, 0);
            s.Update(dt, transform.localRotation, transform.localPosition);
            
            transform.SetLocalPositionAndRotation(s.pos, s.rot);
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

            var b = bobMultiplierOverSpeed.Evaluate(_bobSpeed);
            
            _currentBobTime += bobRate * (bobRateOverSpeed.Evaluate(_bobSpeed) * bobRateMultiplier * dt);

            _currentBobTime = MathUtils.WrapValue(-TimeClamp, TimeClamp, _currentBobTime);

            _currentBobAmp.x = (b * (bobAmplitude.x));
            _currentBobVal.x = (Mathf.Cos(_currentBobTime.x * 2 * Mathf.PI)) * _currentBobAmp.x * 10;

            _currentBobAmp.y = (b * (bobAmplitude.y));
            _currentBobVal.y = (Mathf.Cos(_currentBobTime.y * 2 * Mathf.PI)) * _currentBobAmp.y * 10;

            _currentBobAmp.z = (b * (bobAmplitude.z));
            _currentBobVal.z = (Mathf.Cos(_currentBobTime.z * 2 * Mathf.PI)) * _currentBobAmp.z * 10;

            _currentBobAmp.w = (b * (bobAmplitude.w));
            _currentBobVal.w = (Mathf.Cos(_currentBobTime.w * 2 * Mathf.PI)) * _currentBobAmp.w * 10;

            _bobSpring.AddForce(_currentBobVal * bobAmpMultiplier,
                ForceMode.Force, dt);
            _bobSpring.AddTorque(Vector3.forward * (_currentBobVal.w * bobAmpMultiplier),
                ForceMode.Force, dt);
        }
    }
}