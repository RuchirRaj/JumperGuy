using RR.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RR.Gameplay.CharacterController
{
    public partial class CharacterController
    {
        [CustomTitle("Input", 0.9f, 0.9f, 1)] 
        [SerializeField] private InputState inputState = new();
        [SerializeField] private float inputOffset = 0.2f;
        [SerializeField] private PlayerInput playerInput;

        //Properties
        public InputState InputState => inputState;
        public InputState LastInputState { get; private set; } = new();

        void CCInput_OnEnable()
        {
            if(playerInput)
                playerInput.onActionTriggered += PlayerInputOnActionTriggered;
        }
        
        void CCInput_OnDisable()
        {
            if(playerInput)
                playerInput.onActionTriggered -= PlayerInputOnActionTriggered;
        }

        private void PlayerInputOnActionTriggered(InputAction.CallbackContext obj)
        {
            // Gamepad.current?.SetMotorSpeeds	(motorSpeed.x, motorSpeed.y);
            //Debug.Log(obj.action.name);
            switch (obj.action.name)
            {
                case "Look":
                    OnLook(obj);
                    break;
                case "Move":
                    OnMove(obj);
                    break;
                case "Run":
                    OnRun(obj);
                    break;
                case "Jump":
                    OnJump(obj);
                    break;
                case "Crouch":
                    OnCrouch(obj);
                    break;
                case "Dash":
                    OnDash(obj);
                    break;
                case "Cam":
                    OnCam(obj);
                    break;
                default:
                    // Debug.Log($"\"{obj.action.name}\" Not assigned a function");
                    break;
            }
        }

        public void ResetInput()
        {
            LastInputState.CopyValue(inputState);
            InputState.ResetState();
        }

        public void UpdateForwardDirection(Quaternion direction)
        {
            //TODO move forward rotation to its own function
            inputState.INPUT_ForwardRotation(direction);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            // Debug.Log($"Move");
            inputState.INPUT_Move(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            inputState.INPUT_Look(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            inputState.INPUT_Jump(context.performed);
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            inputState.INPUT_Run(context.performed);
        }
        
        public void OnCrouch(InputAction.CallbackContext context)
        {
            inputState.INPUT_Crouch(context.performed);
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            inputState.INPUT_Dash(context.performed);
        }
        public void OnCam(InputAction.CallbackContext context)
        {
            inputState.INPUT_Cam(context.performed);
        }

        #region Send Message

        public void OnMove(InputValue context)
        {
            inputState.INPUT_Move(context.Get<Vector2>());
        }

        public void OnLook(InputValue context)
        {
            inputState.INPUT_Look(context.Get<Vector2>());
        }

        public void OnJump(InputValue context)
        {
            inputState.INPUT_Jump(context.isPressed);
        }

        public void OnRun(InputValue context)
        {
            inputState.INPUT_Run(context.isPressed);
        }
        
        public void OnCrouch(InputValue context)
        {
            inputState.INPUT_Crouch(context.isPressed);
        }

        public void OnDash(InputValue context)
        {
            inputState.INPUT_Dash(context.isPressed);
        }

        #endregion
    }
}