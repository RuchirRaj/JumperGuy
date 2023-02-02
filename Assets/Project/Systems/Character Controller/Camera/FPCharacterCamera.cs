using Cinemachine;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [AddComponentMenu("Ruchir/Character/Camera/Type/First Person Camera")]
    public class PlayerFPCamera : PlayerCameraBase
    {
        private static Axis DefaultPan => new()
        {
            outValue = 0, clamp = new Vector2(-180, 180), wrap = true, recenterValue = 0, gain = 12,
            range = new Vector2(-180, 180), accelTime = 0.1f, decelTime = 0.1f, recenterWait = 2, recenterTime = 1
        };

        private static Axis DefaultTilt => new()
        {
            outValue = 0, clamp = new Vector2(-70, 70), wrap = false, recenterValue = 0, gain = -12,
            range = new Vector2(-90, 90), accelTime = 0.1f, decelTime = 0.1f, recenterWait = 2, recenterTime = 1,
            recenter = true
        };
        
        public override bool Active => _active;

        [Space(10)]
        public Axis panAxis = DefaultPan;
        public Axis tiltAxis = DefaultTilt;

        [SerializeField] [Space] private CharacterController controller;
        [Space] public CursorLockMode lockMode = CursorLockMode.Confined;
        public bool hideCursor;
        [SerializeField] private UnityEngine.Camera weaponCamera;
        
        //Private
        private bool _active;
        private CmCamera _camera;
        protected override void OnEnable()
        {
            base.OnEnable();
            _camera = GetComponent<CmCamera>();
            _camera.enabled = _active;
        }

        public override void SetActive(bool value, CharacterController characterController)
        {
            controller = characterController;
            if (value)
            {
                controller.lookDirection = CharacterController.LookDirection.Custom;
                
                if(!_active)
                {
                    Cursor.lockState = lockMode;
                    Cursor.visible = hideCursor;
                    panAxis.Reset();
                    if(controller.RefTransform)
                        panAxis.outValue = controller.RefTransform.localRotation.eulerAngles.y;
                }
            }

            if (weaponCamera)
            {
                weaponCamera.enabled = value;
                if (MainCamera.Instance)
                    MainCamera.Instance.CameraData.renderPostProcessing = !value;
            }

            _active = value;
            _camera.enabled = value;
        }


        public override void BatchLateUpdate(float dt, float sdt)
        {
            if(!_active) return;
            panAxis.outValue = controller.RefTransform.localRotation.eulerAngles.y;
            panAxis.SetInput(controller.InputState.Look.x);
            panAxis.ProcessInput(dt);

            tiltAxis.SetInput(controller.InputState.Look.y);
            tiltAxis.ProcessInput(dt);
            transform.localRotation = Quaternion.Euler(tiltAxis.outValue, 0, 0);
            controller.RefTransform.localRotation = Quaternion.Euler(0, panAxis.outValue, 0);

            controller.InputState.INPUT_ForwardRotation(transform.rotation);
        }
    }
}