using Cinemachine;
using RR.UpdateSystem;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [AddComponentMenu("Ruchir/Character/Camera/Generic Camera")]
    [SelectionBase]
    public class GenericCharacterCamera : MonoBehaviour, IBatchLateUpdate
    {
        public CharacterController controller;
        public CharacterController.LookDirection lookDirection = CharacterController.LookDirection.MovementDirection;
        [Space] 
        public CursorLockMode lockMode = CursorLockMode.Locked;
        public bool hideCursor = true;
        [Space]
        public UpdateMethod lateUpdate = new (){ autoUpdate = true };
        
        private CmCamera _camera;

        private void OnEnable()
        {
            _camera = GetComponent<CmCamera>();
            this.RegisterLateUpdate(lateUpdate);
            Cursor.lockState = lockMode;
            Cursor.visible = !hideCursor;
            controller.lookDirection = lookDirection;
        }

        private void OnDisable()
        {
            this.DeregisterLateUpdate();
        }

        public void BatchLateUpdate(float dt, float sdt)
        {
            if(controller == null)
                return;
            controller.InputState.INPUT_ForwardRotation(_camera.State.RawOrientation);
        }
    }   
}
