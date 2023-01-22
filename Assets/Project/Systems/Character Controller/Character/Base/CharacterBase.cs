using Drawing;
using RR.UpdateSystem;
using RR.Utils;
using UnityEngine;
using UnityEngine.Animations;

namespace RR.Gameplay.CharacterController
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Character/Character Base")]
    [RequireComponent(typeof(Rigidbody))]
    public partial class CharacterBase : MonoBehaviourGizmos, IBatchFixedUpdate
    {
        public enum ColliderType
        {
            Capsule,
            Sphere,
            Box
        }

        public enum GroundedState
        {
            None,
            Stable,
            UnStable
        }
        
        [field: SerializeField] public CharacterGroundSensor Sensor { get; private set; } = new();
        [field: SerializeField] public CharacterGroundSettings GroundSettings { get; private set; } = new();
        [field: SerializeField] public SpringSettings PositionSpring { get; private set;} = new() { damper = 0.75f, frequency = 10, useForce = false };
        [field: SerializeField] public SpringSettings RotationSpring { get; private set;} = new() { damper = 0.75f, frequency = 10, useForce = false };
        [field: SerializeField] public Vector3 TorqueScale { get; private set;} = new(1,1,1);

        
        [field: SerializeField] public ColliderType ShapeColliderType { get; set; }
        [field: SerializeField] public ShapeSettings Shape { get; set; } = new() { radius = 0.5f, height = 2, comHeight = 0.5f, stepHeightRatio = 0.25f };
        [field: SerializeField] public Vector3 Offset { get; set; } = Vector3.zero;
        [field: SerializeField] public UpdateMethod FixedUpdate { get; private set; } = new() { autoUpdate = true };

        
        //Variables
        [field: SerializeField] public Transform RefTransform { get; set; }
        [field: SerializeField] public float DisableGroundingTimer { get; private set; }
        [field: SerializeField] public Vector2 DownForceMultiplier { get; private set; } = new(1, 1);
        [field: SerializeField] public GroundedState GroundState { get; private set; }

        //Properties
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once ConvertToAutoProperty
        public Vector3 CachedRefUp => _up;
        // ReSharper disable once ConvertToAutoProperty
        public Quaternion CachedRefRot => _rot;
        public float Slope => Vector3.Angle(Sensor.averageHit.normal, CachedRefUp);

        public Collider Collider =>
            _currentColliderType switch
            {
                ColliderType.Box => _boxCollider,
                ColliderType.Capsule => _capsuleCollider,
                ColliderType.Sphere => _sphereCollider,
                _ => _capsuleCollider
            };
        
        public Vector3 ColliderCenter
        {
            get
            {
                Vector3 center = _currentColliderType switch
                {
                    ColliderType.Box => _boxCollider.center,
                    ColliderType.Capsule => _capsuleCollider.center,
                    ColliderType.Sphere => _sphereCollider.center,
                    _ => Vector3.zero
                };
                return transform.TransformPoint(center);
            }
        }
        
        //Private
        [SerializeField]
        private bool enableDebug;
        private ShapeSettings _currentShape;
        private ColliderType _currentColliderType;
        private Vector3 _prevOffset;
        [Space]
        private Vector3 _up;
        private Quaternion _rot;
        [Space] 
        private Vector3 _vel;
        [Space]
        private Vector3 _finalForce;
        private Vector3 _finalTorque;
        private Vector3 _moveForce;
        private Vector3 _groundingForce;
        private Vector3 _rotationTorque;
        [Space]
        private float _sensorStartHeight;
        private float _sensorEndHeight;
        [Space]
        private CapsuleCollider _capsuleCollider;
        private BoxCollider _boxCollider;
        private SphereCollider _sphereCollider;

        private Rigidbody _rigidbody;

        private void OnEnable()
        {
            this.RegisterFixedUpdate(FixedUpdate);
            this.RegisterUpdate(new UpdateMethod(){autoUpdate = true, slicedUpdate = false});
            S1_InitReferences();
            S1_UpdateSettings(true);
        }

        private void OnDisable()
        {
            this.DeregisterFixedUpdate();
            this.DeregisterUpdate();
        }

        #region Helper Function

        #region Stage 1

        internal void S1_InitReferences()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if(!RefTransform)
            {
                RefTransform = new GameObject($"{name}_RefTransform").transform;
                var transform1 = transform;
                RefTransform.rotation = transform1.rotation;
                
                var source = new ConstraintSource();
                source.sourceTransform = transform1;
                source.weight = 1;
                
                { //Add position Constraint (Optional)
                    var pos = RefTransform.gameObject.AddComponent<PositionConstraint>();
                    pos.weight = 1;
                    pos.constraintActive = true;
                    pos.translationOffset = Vector3.zero;
                    pos.locked = true;
                    pos.AddSource(source);
                }
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void S1_UpdateCache()
        {
            if (RefTransform)
            {
                _up = RefTransform.up;
                _rot = RefTransform.rotation;
            }

            _vel = _rigidbody.velocity;
            Sensor.InitCache(_currentShape.radius);
            Sensor.UpdateCache(transform, _sensorStartHeight, _sensorEndHeight, Offset, _currentShape.radius);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Update all related component/Object settings
        /// </summary>
        /// <param name="initialize">Should we Force Initialize (array cache)/Shape/etc or just update it(resize/Scale/etc)</param>
        /// <remarks>Call this before updating cache if any setting was updated</remarks>
        public void S1_UpdateSettings(bool initialize = false)
        {
            bool updatedShape = false;
            
            UpdateShapeType(initialize);
            if (!updatedShape)
                UpdateShape(false);
            
            void UpdateShapeType(bool forceUpdate)
            {
                if (!forceUpdate && _currentColliderType == ShapeColliderType)
                    return;

                if (Collider)
                {
                    Sensor.ignoreCollider.Remove(Collider);
                    Destroy(Collider);
                }

                switch (ShapeColliderType)
                {
                    case ColliderType.Box:
                        _boxCollider = gameObject.AddComponent<BoxCollider>();
                        break;
                    case ColliderType.Capsule:
                        _capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                        break;
                    case ColliderType.Sphere:
                        _sphereCollider = gameObject.AddComponent<SphereCollider>();
                        break;
                }

                _currentColliderType = ShapeColliderType;
                UpdateShape(true);
                Sensor.ignoreCollider.Add(Collider);
            }

            void UpdateShape(bool forceUpdate)
            {
                updatedShape = true;

                if (!forceUpdate && _currentShape.ShapeEquals(Shape)) return;

                _currentShape = Shape;
                var shapeSettings = _currentShape;
                
                //Update Center of mass
                {
                    _rigidbody.centerOfMass = Vector3.up * (Shape.height * Shape.comHeight) + Offset;
                }

                switch (_currentColliderType)
                {
                    //Box
                    case ColliderType.Box:
                        _boxCollider.center =
                            new Vector3(0, shapeSettings.height * (1 + shapeSettings.stepHeightRatio) / 2, 0) + Offset;
                        _boxCollider.size = new Vector3(shapeSettings.radius * 2,
                            shapeSettings.height * (1 - shapeSettings.stepHeightRatio), shapeSettings.radius * 2);
                        _sensorStartHeight = _boxCollider.center.y - _boxCollider.size.y * 0.5f + shapeSettings.radius;
                        _sensorEndHeight = shapeSettings.radius + Offset.y;
                        break;
                    //Capsule
                    case ColliderType.Capsule:
                        _capsuleCollider.center =
                            new Vector3(0, shapeSettings.height * (1 + shapeSettings.stepHeightRatio) / 2, 0) + Offset;
                        _capsuleCollider.height = Mathf.Clamp(shapeSettings.height, 0,
                            shapeSettings.height * (1 - shapeSettings.stepHeightRatio));
                        _capsuleCollider.radius = Mathf.Clamp(shapeSettings.radius, 0,
                            shapeSettings.height * 0.5f * (1 - shapeSettings.stepHeightRatio));

                        _sensorStartHeight = _capsuleCollider.center.y - _capsuleCollider.height * 0.5f +
                                             _capsuleCollider.radius;
                        _sensorEndHeight = shapeSettings.radius + Offset.y;
                        break;
                    //Sphere
                    case ColliderType.Sphere:
                        var center =
                            new Vector3(0, shapeSettings.height * (1 + shapeSettings.stepHeightRatio) / 2, 0) + Offset;
                        var height = Mathf.Clamp(shapeSettings.height, 0,
                            shapeSettings.height * (1 - shapeSettings.stepHeightRatio));
                        var radius = Mathf.Clamp(shapeSettings.radius, 0,
                            shapeSettings.height * 0.5f * (1 - shapeSettings.stepHeightRatio));

                        _sphereCollider.center = center - (height/2 - radius) * Vector3.up;
                        _sphereCollider.radius = radius;
                        
                        _sensorStartHeight = _sphereCollider.center.y;
                        _sensorEndHeight = shapeSettings.radius + Offset.y;
                        break;
                }

                Collider.sharedMaterial = shapeSettings.material;
            }
        }

        public void S1_GroundSensorDetect()
        {
            Sensor.Cast(transform, CachedRefUp, GroundSettings.stepDownDistance, Sensor.groundMask,
                GroundSettings.slopeLimit, _currentShape.radius, enableRuntimeGizmos, runtimeGizmoFlag);
            
            UpdateGroundedState();

            void UpdateGroundedState()
            {
                var hit = Sensor.averageHit.valid;
                var threshold = GroundSettings.stepDownDistance * GroundSettings.groundingThreshold;

                if (!hit)
                {
                    GroundState = GroundedState.None;
                    return;
                }
                var averageHit = Sensor.averageHit;
                switch (GroundState)
                {
                    case GroundedState.None:
                        if (averageHit.distance < threshold.x)
                        {
                            GroundState = Sensor.stableGround ? GroundedState.Stable : GroundedState.UnStable;
                        }
                        break;
                    case GroundedState.UnStable:
                    case GroundedState.Stable:
                        if (averageHit.distance > threshold.y)
                        {
                            GroundState = GroundedState.None;
                            break;
                        }
                        GroundState = Sensor.stableGround ? GroundedState.Stable : GroundedState.UnStable; 
                        break;
                }

                if (DisableGroundingTimer > float.Epsilon)
                    GroundState = GroundedState.None;
            }
        }

        #endregion

        #region Stage 2

        public void S2_UpdateGroundingForce(float dt)
        {
            var averageHit = Sensor.averageHit;
            _groundingForce = Vector3.zero;
            if(!Sensor.stableGround) return;
            var downDir = -CachedRefUp;

            Rigidbody hitBody = averageHit.col.attachedRigidbody;

            float otherDirVel = 0;
            float downDirVel;
            {
                downDirVel = Vector3.Dot(downDir, averageHit.normal);
                downDirVel *= Vector3.Dot(averageHit.normal, _vel);
            }
            if (hitBody)
            {
                var otherVel = hitBody.velocity;
                otherDirVel = Vector3.Dot(downDir, averageHit.normal);
                otherDirVel *= Vector3.Dot(averageHit.normal, otherVel);
            }
            
            float relativeDownVel = downDirVel - otherDirVel;

            //Force in up direction
            var force = -MathUtils.SpringUtils.DamperSpring(dt, PositionSpring.frequency, PositionSpring.damper,
                averageHit.distance, -relativeDownVel, PositionSpring.useForce);

            force = Mathf.Clamp(force, GroundSettings.groundingForceClamp.x, GroundSettings.groundingForceClamp.y);

            force *= force > 0 ? DownForceMultiplier.y : DownForceMultiplier.x;
            if(GroundState == GroundedState.None)
                force *= force > 0 ? GroundSettings.unGroundedForceMultiplier.y : GroundSettings.unGroundedForceMultiplier.x;
            
            _groundingForce = force * CachedRefUp;

            //Apply force on ground
            if (GroundSettings.applyForceOnGround && force > 0 && hitBody)
            {
                var appliedForce = force * _rigidbody.mass;
                appliedForce = Mathf.Clamp(appliedForce, 0, GroundSettings.maxForceToMassRatio * hitBody.mass);
                hitBody.AddForceAtPosition(downDir * appliedForce, averageHit.point);
            }
        }
        
        /// <summary>
        ///     Applies torque to _rigidbody to keep it in right orientation
        /// </summary>
        public void S2_UpdateRotationForce(float dt)
        {
            _rotationTorque = MathUtils.SpringUtils.DampedTorsionalSpring(dt, RotationSpring.frequency,
                RotationSpring.damper,
                _rigidbody.rotation, RefTransform.rotation, -_rigidbody.angularVelocity, _rigidbody,
                RotationSpring.useForce);
            _rotationTorque.Scale(TorqueScale);
        }

        #endregion

        #region Stage 3

        public void S3_HandleFinalForce()
        {
            //Calculate final Force
            _finalForce = Vector3.zero;
            _finalForce += _groundingForce * _rigidbody.mass;
            _finalForce += _moveForce;
            
            _rigidbody.AddForce(_finalForce, ForceMode.Force);
            
            //Calculate final Torque
            _finalTorque = Vector3.zero;
            _finalTorque += _rotationTorque;
            
            _rigidbody.AddTorque(_finalTorque, ForceMode.Force);
            
            //Reset Movement Force
            _moveForce = Vector3.zero;

        }

        #endregion
        
        #region Stage 4


        /// <summary>
        /// Update all timers related to this component
        /// </summary>
        /// <param name="dt"></param>
        /// <remarks>Call at the end of frame or After Detect/Update callbacks</remarks>
        public void S4_UpdateTimer(float dt)
        {
            
            if(DisableGroundingTimer > 0)
                DisableGroundingTimer -= dt;
            else
                DisableGroundingTimer = 0;
        }

        /// <summary>
        /// Reset all the external inputs
        /// </summary>
        /// <remarks>Call at the end of frame/Cycle</remarks>
        public void S4_ResetState()
        {
            DownForceMultiplier = Vector2.one;
            _moveForce = Vector3.zero;
        }
        

        #endregion
        
        #endregion

        #region Public Function

        public float GetSlope(Vector3 normal) => Vector3.Angle(normal, RefTransform.up);
        
        /// <summary>
        ///     Add force to final movement force
        /// </summary>
        /// <remarks>The final force uses "ForceMode.Force"</remarks>
        /// <param name="force"></param>
        public void AddMoveForce(Vector3 force)
        {
            _moveForce += force;
        }
        
        public void AddForce(Vector3 force, ForceMode mode)
        {
            // ReSharper disable once Unity.NoNullPropagation
            _rigidbody?.AddForce(force, mode);
        }

        #endregion
        
        public void BatchFixedUpdate(float dt, float sdt)
        {
            //Update Settings
            S1_UpdateSettings(false);
            S1_UpdateCache();
            S1_GroundSensorDetect();

            //Update Forces
            S2_UpdateGroundingForce(dt);
            S2_UpdateRotationForce(dt);
            
            //ApplyForces
            S3_HandleFinalForce();
            
            //Post update methods
            S4_UpdateTimer(dt);
            S4_ResetState();
        }
    }
}