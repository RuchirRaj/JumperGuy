using RR.Attributes;
using RR.Utils;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    public partial class CharacterController 
    {
        #region Enum

        public enum LookDirection
        {
            MovementDirection,
            InputDirection,
            InputWhenMovementDetected,
            Custom
        }

        #endregion

        [CustomTitle("Settings", 1f, 0.73f, 0.6f)]
        [SerializeField] public LookDirection lookDirection = LookDirection.InputWhenMovementDetected;
        [SerializeField] public bool stopRotatingByDefault;
        [SerializeField] public float rotationSpeed = 10;
        [SerializeField] public float maxAngularVelocity = 2;
        [field: SerializeField] public Transform RefTransform { get; private set; }
        [field: SerializeField] public Transform BaseRefTransform { get; private set; }

        //References
        public Rigidbody Rigidbody { get; private set; }
        //TODO Change Grounder to CharacterBase
        public CharacterBase Base { get; private set; }
        
        //Properties
        public bool IsGrounded => Base.GroundState == CharacterBase.GroundedState.Stable;
        public float Slope => Base.Slope;
        public Quaternion RefRotation => Base.RefTransform.rotation;
        
        //Private variables
        private bool _horizontalInputsDetected;
        
        private Vector3 _moveInput;
        private Vector3 _moveDirection;
        private Vector3 _horizontalForce;
        private Vector3 _groundVel, _groundAngularVel;
        private Vector3 _targetVelocity;
        private Vector3 _finalForce;

        private Vector3 _targetRotFwdDirection, _targetUpDirection;
        private Vector3 _externalExtraVelocity;
        
        
        //Properties

        public Vector3 GroundVel => _groundVel;
        public Vector3 GroundAngularVel => _groundAngularVel;
        public bool HorizontalInputDetected => _horizontalInputsDetected;
        public Vector3 MoveDirection
        {
            get => _moveDirection;
            set => _moveDirection = value;
        }

        private void CCController_OnEnable()
        {
            if (!RefTransform)
            {
                RefTransform = new GameObject($"{name}_ControllerRefTransform").transform;
                var transform1 = transform;
                RefTransform.rotation = transform1.rotation;

                if (!Base.RefTransform)
                    Base.S1_InitReferences();
                RefTransform.SetParent(Base.RefTransform);
            }
            
            BaseRefTransform = Base.RefTransform;
            Base.RefTransform = RefTransform;
        }


        private void CCController_OnDisable()
        {
            if(!RefTransform)
                return;
            Base.RefTransform = BaseRefTransform;
        }

        private void CCController_Awake()
        {
            Base = GetComponent<CharacterBase>();
            Rigidbody = GetComponent<Rigidbody>();
        }

        void CCController_Start()
        {
            _targetRotFwdDirection = transform.forward;
        }

        void CCController_FixedUpdate(float dt, float sdt)
        {
            ResetExternalValues(dt);
            
            //Modify
            ModifyRigidBody();
            
            //Detect
            DetectGround(dt);
            DetectInputDirection();
            DetectGravity();
            DetectForwardRotation();
            DetectUpRotation();
            
            //Update States
            StateUpdateAction?.Invoke(dt, sdt);

            //Post-process values
            UpdateGrounderSettings();
            HandleHorizontalMovement(dt);
            
            //Apply
            ApplyForce(dt);
            RotateRefTransform(dt);
            
            inputState.ResetState();
        }
        
        private void ModifyRigidBody()
        {
            Rigidbody.maxAngularVelocity = maxAngularVelocity;
        }
        #region Reset

        private void ResetExternalValues(float dt)
        {
            _externalExtraVelocity = Vector3.zero;
        }
        
        #endregion
        
        #region Detect

        void DetectGround(float dt)
        {
            Base.S1_UpdateSettings(false);
            Base.S1_UpdateCache();
            Base.S1_GroundSensorDetect();
        }
        
        private void DetectInputDirection()
        {
            _moveInput = new Vector3(inputState.Move.x, 0, inputState.Move.y);

            // Detect horizontal player inputs
            _horizontalInputsDetected = (_moveInput.magnitude >= inputOffset);

            _moveDirection = Vector3.zero;

            if (_horizontalInputsDetected)
            {
                _moveDirection = inputState.ForwardRotation * _moveInput;
                _moveDirection = Vector3.ProjectOnPlane(_moveDirection, RefRotation * Vector3.up).normalized;
            }
        }
        
        private void DetectForwardRotation()
        {
            if (stopRotatingByDefault)
                _targetRotFwdDirection = RefTransform.forward;
            switch (lookDirection)
            {
                case LookDirection.MovementDirection:
                    if (_horizontalInputsDetected)
                        _targetRotFwdDirection = _moveDirection;
                    break;
                case LookDirection.InputDirection:
                    _targetRotFwdDirection = inputState.ForwardRotation * Vector3.forward;
                    break;
                case LookDirection.InputWhenMovementDetected:
                    if (_horizontalInputsDetected)
                        _targetRotFwdDirection = inputState.ForwardRotation * Vector3.forward;
                    break;
                case LookDirection.Custom:
                default: //Custom
                    _targetRotFwdDirection = RefTransform.forward;
                    break;
            }
        }
        
        private void DetectUpRotation()
        {
            //TODO add support for variable gravity
            _targetUpDirection = -FinalGravity.normalized;
        }
        
        #endregion

        #region Post-Process

        private void UpdateGrounderSettings()
        {
            //Change Shape settings
            Base.Shape = CurrentShapeState.shape;
        }
        
        private void HandleHorizontalMovement(float dt)
        {
            _horizontalForce = Vector3.zero;
            _groundVel = GetGroundVelocity();

            //Reference transform's up vector
            var refUp = RefRotation * Vector3.up;

            //The ground Plane's normal 
            var up = CurrentMovementState.reorientToGround && IsGrounded ? Base.Sensor.averageHit.normal : refUp;
            
            if(IsGrounded)
                _moveDirection = MathUtils.ReorientToNormal(_moveDirection, up, refUp);
            
            //Ground velocity tangent to it's normal
            var surfaceVel = Vector3.ProjectOnPlane(_groundVel, up);
            
            var horizontalVel = Vector3.ProjectOnPlane(Rigidbody.velocity, refUp);
            horizontalVel = ProjectOnPlane(horizontalVel, up, refUp);
            
            float acc = 1f;
            acc *= _horizontalInputsDetected ? CurrentMovementState.acceleration : CurrentMovementState.deceleration;

            var targetVel = surfaceVel + _moveDirection * (CurrentMovementState.maxSpeed * inputState.Move.magnitude);
            

            var velDot = Vector3.Dot(targetVel.normalized, horizontalVel);
            acc *= CurrentMovementState.accelerationCurve.value.Evaluate(velDot);

            _targetVelocity = horizontalVel;
            
            // Calculate the target velocity for next frame.
            _targetVelocity = Vector3.MoveTowards(
                horizontalVel, 
                targetVel + (_externalExtraVelocity),
                acc * dt);
            
            // Calculate the force needed for the next movement.
            Vector3 neededAccel = (_targetVelocity - horizontalVel) / dt;

            // Special case for zero deceleration
            if (!_horizontalInputsDetected && CurrentMovementState.deceleration == 0f) neededAccel = Vector3.zero;
            
            
            
            neededAccel = Vector3.ClampMagnitude(neededAccel,
                (_horizontalInputsDetected ? CurrentMovementState.maxAcceleration : CurrentMovementState.maxDeceleration) * CurrentMovementState.maxAccelerationCurve.value.Evaluate(velDot));

            _horizontalForce = Vector3.ProjectOnPlane(neededAccel, up);
        }

        #endregion
        
        #region Apply

        private void RotateRefTransform(float dt)
        {
            //BaseRefTransform
            {
                var rot = BaseRefTransform.rotation;
                var up = Vector3.Slerp(rot * Vector3.up, _targetUpDirection, dt * rotationSpeed);
                var fwd = rot * Vector3.forward;
                Vector3.OrthoNormalize(ref up, ref fwd);
                BaseRefTransform.rotation = Quaternion.LookRotation(fwd, up);
            }
            //RefTransform
            {

                if (lookDirection != LookDirection.Custom)
                {
                    var rot = RefTransform.rotation;
                    var fwd = rot * Vector3.forward;
                    fwd = Vector3.Slerp(fwd, ProjectDirectionInPlane(_targetRotFwdDirection, rot * Vector3.up),
                        dt * rotationSpeed);
                    fwd = BaseRefTransform.InverseTransformDirection(fwd).normalized;
                    var up = Vector3.up;
                    Vector3.OrthoNormalize(ref up, ref fwd);
                    RefTransform.localRotation = Quaternion.LookRotation(fwd, up);
                }
                
                if(CurrentMovementState.maxAngularSpeed > float.Epsilon)
                {
                    _groundAngularVel = GetGroundAngularVelocity();
                    var deltaRot = Vector3.Project(_groundAngularVel, RefTransform.up);
                    var angle = Mathf.Clamp(deltaRot.magnitude, -CurrentMovementState.maxAngularSpeed,
                        CurrentMovementState.maxAngularSpeed) * Mathf.Rad2Deg * dt;
                    //
                    // if (lookDirection != LookDirection.Custom)
                    //     angle *= 2;
                    
                    Quaternion r = Quaternion.AngleAxis(
                        angle, Vector3.up);
                    
                    RefTransform.localRotation *= r;
                }
            }
        }
        
        private void ApplyForce(float dt)
        {
            _finalForce = (_horizontalForce + FinalGravity) * Rigidbody.mass;

            Base.AddMoveForce(_finalForce);
            UpdateForces(dt);
        }

        #endregion
        
        #region Support Functions

        private void UpdateForces(float dt)
        {
            //Update Forces
            Base.S2_UpdateGroundingForce(dt);
            Base.S2_UpdateRotationForce(dt);
            
            //ApplyForces
            Base.S3_HandleFinalForce();
            
            //Post update methods
            Base.S4_UpdateTimer(dt);
            Base.S4_ResetState();
        }

        private Vector3 ProjectDirectionInPlane(Vector3 direction)
        {
            direction = Vector3.ProjectOnPlane(direction, Base.RefTransform.up);
            return direction.normalized;
        }
        
        private Vector3 ProjectDirectionInPlane(Vector3 direction, Vector3 up)
        {
            direction = Vector3.ProjectOnPlane(direction, up);
            return direction.normalized;
        }
        
        private Vector3 GetGroundVelocity()
        {
            var vel = Vector3.zero;
            var averageHit = Base.Sensor.averageHit;
            // Moving platforms
            if (averageHit.valid)
            {
                var rb = averageHit.col.attachedRigidbody;
                if (rb != null)
                {
                    vel = rb.GetPointVelocity(averageHit.point);
                    vel = Vector3.ProjectOnPlane(vel, averageHit.normal);
                    // return PhysicsUtils.GetRelativePointVelocity(transform, _groundHit.collider.gameObject, _groundHit.point);
                }
            }

            return vel;
        }

        private Vector3 GetGroundAngularVelocity()
        {
            var angular = Vector3.zero;
            var averageHit = Base.Sensor.averageHit;
            // Moving platforms
            if (averageHit.valid)
            {
                var rb = averageHit.col.attachedRigidbody;
                if (rb != null)
                {
                    angular = rb.angularVelocity;
                }
            }

            return angular;
        }
        
        
        /// <summary>
        /// Project a vector onto a plane along given vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="normal"></param>
        /// <param name="refUp"></param>
        /// <returns></returns>
        private Vector3 ProjectOnPlane(Vector3 vector, Vector3 normal, Vector3 refUp)
        {
            var vecDot = Vector3.Dot(vector, normal);
            var upDot = Vector3.Dot(refUp, normal);

            return upDot > 0 ? vector - refUp * (vecDot / upDot) : Vector3.zero;
        }
        #endregion
    }
}