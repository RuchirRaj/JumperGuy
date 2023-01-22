using RR.StateSystem;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine;

namespace RR.Utils
{
    /// <summary>
    /// Utility Free Camera component.
    /// </summary>
    [AddComponentMenu("Ruchir/Utils/Free-Camera")]
    public class FreeCamera : MonoBehaviour
    {
        const float KMouseSensitivityMultiplier = 0.01f;

        /// <summary>
        /// Rotation speed when using a controller.
        /// </summary>
        public float lookSpeedController = 15f;
        /// <summary>
        /// Rotation speed when using the mouse.
        /// </summary>
        public float lookSpeedMouse = 4.0f;
        /// <summary>
        /// Movement speed.
        /// </summary>
        public float moveSpeed = 10.0f;
        /// <summary>
        /// Value added to the speed when incrementing.
        /// </summary>
        public float moveSpeedIncrement = 2.5f;
        /// <summary>
        /// Scale factor of the turbo mode.
        /// </summary>
        public float turbo = 10.0f;

        public CursorLockMode lockMode = CursorLockMode.Locked;
        public bool hideCursor = true;

        [HorizontalGroup("Space")] public Space space = Space.World;
        [HorizontalGroup("Space")] [HideLabel] [EnableIf("@space == Space.Self")] public Transform localSpace;

        public bool leftShiftBoost;
        public bool leftMouseBoost;
        
        private float _inputRotateAxisX, _inputRotateAxisY;
        private float _inputChangeSpeed;
        private float _inputVertical, _inputHorizontal, _inputYAxis;
        private bool _leftShift;
        private bool _fire1;
        
        
        InputAction lookAction;
        InputAction moveAction;
        InputAction speedAction;
        InputAction yMoveAction;


        public StateList<FreeCameraState> state = new();

        
        private FreeCameraState _currentState;

        private void Awake()
        {
            _currentState = ScriptableObject.CreateInstance<FreeCameraState>();
            state.ValidateStates();
        }

        void OnEnable()
        {
            RegisterInputs();
            state.ValidateStates();

            ApplyState();
            ApplyCursorState();
        }

        void ApplyCursorState()
        {
            Cursor.lockState = lockMode;
            Cursor.visible = !hideCursor;
        }
        private void ApplyState()
        {
            _currentState.Reset();
            if(state.states.Count == 0)
                return;
            state.ApplyState(_currentState);
            space = _currentState.space.GetValue(space);
            lookSpeedController = _currentState.lookSpeedController.GetValue(lookSpeedController);
            lookSpeedMouse = _currentState.lookSpeedMouse.GetValue(lookSpeedMouse);
            moveSpeed = _currentState.moveSpeed.GetValue(moveSpeed);
            moveSpeedIncrement = _currentState.moveSpeedIncrement.GetValue(moveSpeedIncrement);
            turbo = _currentState.turbo.GetValue(turbo);
        }

        void RegisterInputs()
        {
            var map = new InputActionMap("Free Camera");

            lookAction = map.AddAction("look");
            moveAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
            speedAction = map.AddAction("speed", binding: "<Gamepad>/dpad");
            yMoveAction = map.AddAction("yMove");

            lookAction.AddBinding("<Gamepad>/rightStick").WithProcessor($"scaleVector2(x={lookSpeedController}, y={lookSpeedController})");
            lookAction.AddBinding("<Mouse>/delta").WithProcessor($"scaleVector2(x={lookSpeedMouse * KMouseSensitivityMultiplier}, y={lookSpeedMouse * KMouseSensitivityMultiplier})");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d")
                .With("Right", "<Keyboard>/rightArrow");
            speedAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/home")
                .With("Down", "<Keyboard>/end");
            yMoveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/pageUp")
                .With("Down", "<Keyboard>/pageDown")
                .With("Up", "<Keyboard>/e")
                .With("Down", "<Keyboard>/q")
                .With("Up", "<Gamepad>/rightshoulder")
                .With("Down", "<Gamepad>/leftshoulder");

            moveAction.Enable();
            lookAction.Enable();
            speedAction.Enable();
            yMoveAction.Enable();
        }

        void UpdateInputs()
        {
            _inputRotateAxisX = 0.0f;
            _inputRotateAxisY = 0.0f;
            _fire1 = false;
            _leftShift = false;

            var lookDelta = lookAction.ReadValue<Vector2>();
            _inputRotateAxisX = lookDelta.x;
            _inputRotateAxisY = lookDelta.y;

            _leftShift = Keyboard.current?.leftShiftKey?.isPressed ?? false;
            _fire1 = Mouse.current?.leftButton?.isPressed == true || Gamepad.current?.xButton?.isPressed == true;

            _inputChangeSpeed = speedAction.ReadValue<Vector2>().y;

            var moveDelta = moveAction.ReadValue<Vector2>();
            _inputVertical = moveDelta.y;
            _inputHorizontal = moveDelta.x;
            _inputYAxis = yMoveAction.ReadValue<Vector2>().y;
        }

        void Update()
        {
            ApplyState();
            UpdateInputs();

            if (_inputChangeSpeed != 0.0f)
            {
                moveSpeed += _inputChangeSpeed * moveSpeedIncrement;
                if (moveSpeed < moveSpeedIncrement) moveSpeed = moveSpeedIncrement;
            }

            bool moved = _inputRotateAxisX != 0.0f || _inputRotateAxisY != 0.0f || _inputVertical != 0.0f || _inputHorizontal != 0.0f || _inputYAxis != 0.0f;
            if (moved)
            {
                var localEulerAngles = transform.localEulerAngles;
                
                float rotationX = localEulerAngles.x;
                float newRotationY = localEulerAngles.y + _inputRotateAxisX;

                // Weird clamping code due to weird Euler angle mapping...
                float newRotationX = (rotationX - _inputRotateAxisY);
                if (rotationX <= 90.0f && newRotationX >= 0.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
                if (rotationX >= 270.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

                transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

                float moveSpeed = Time.deltaTime * this.moveSpeed;
                if ((_fire1 && leftMouseBoost) || (leftShiftBoost && _leftShift))
                    moveSpeed *= turbo;

                var transform1 = transform;
                var position = transform1.position;

                if (space == Space.Self && localSpace)
                {
                    var up = localSpace.up;
                    position += up * (moveSpeed * _inputYAxis);
                    var fwd = Vector3.ProjectOnPlane(transform1.forward, up).normalized;
                    var rt = Vector3.ProjectOnPlane(transform1.right, up).normalized;
                    position += fwd * (moveSpeed * _inputVertical);
                    position += rt * (moveSpeed * _inputHorizontal);
                    transform1.position = position;
                }
                else
                {
                    position += transform1.forward * (moveSpeed * _inputVertical);
                    position += transform1.right * (moveSpeed * _inputHorizontal);
                    position += Vector3.up * (moveSpeed * _inputYAxis);
                    transform1.position = position;
                }
            }
        }
    }
}
