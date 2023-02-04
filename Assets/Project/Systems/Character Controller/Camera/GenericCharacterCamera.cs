using Cinemachine;
using RR.UpdateSystem;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [AddComponentMenu("Ruchir/Character/Camera/Type/Generic Camera")]
    [SelectionBase]
    public class GenericCharacterCamera : PlayerCameraBase
    {
        
        public override bool Active => _active;
        
        [Space(10)]
        public CharacterController controller;
        public CharacterController.LookDirection lookDirection = CharacterController.LookDirection.MovementDirection;
        [Space] 
        public CursorLockMode lockMode = CursorLockMode.Locked;
        public bool hideCursor = true;
        
        //Private
        private CmCamera _camera;
        private bool _active;

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
                Cursor.lockState = lockMode;
                Cursor.visible = !hideCursor;
                controller.lookDirection = lookDirection;
            }
            _active = value;
            _camera.enabled = value;
        }

        public override void BatchLateUpdate(float dt, float sdt)
        {
            if(!_active) return;
            if(controller == null)
                return;
            controller.INPUT_ForwardDirection(_camera.State.RawOrientation);
        }
        public override void SetFOV(float value)
        {
            _camera.Lens.FieldOfView = value;
        }
    }   
}
