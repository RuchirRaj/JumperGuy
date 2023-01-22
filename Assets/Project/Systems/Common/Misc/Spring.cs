using Drawing;
using RR.Attributes;
using RR.UpdateSystem;
using RR.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Utils/Spring")]
    public class Spring : MonoBehaviourGizmos, IBatchFixedUpdate, IBatchUpdate, IBatchLateUpdate
    {
        [CustomTitle("Reference")] 
        [SerializeField] public Rigidbody rb;
        [SerializeField] public Transform self;
        [SerializeField] public Vector3 pivotOffset;
        [SerializeField] public Transform goal;
        [SerializeField] public Vector3 goalOffset;
        [SerializeField] public Space goalOffsetSpace;

        [Space] 
        [CustomTitle("Position")]
        [SerializeField] public bool enablePositionSpring = true;
        [SerializeField] public Vector3 restPos;
        [SerializeField] public SpringSettings positionSpring = new ();
        [SerializeField] public Vector3 forceScale = Vector3.one;
        [SerializeField] public ForceMode positionForce = ForceMode.Acceleration;
        [Space]
        [CustomTitle("Rotation")]
        [SerializeField] public bool enableRotationSpring = true;
        [SerializeField] public Quaternion restRot = Quaternion.identity;
        [SerializeField] public SpringSettings rotationSpring = new ();
        [SerializeField] public Vector3 torqueScale = Vector3.one;
        [SerializeField] public ForceMode rotationForce = ForceMode.Force;

        [Header("Update Method")]
        [SerializeField] public UpdateMethod fixedUpdate = new UpdateMethod()
        {
            autoUpdate = true
        };
        [SerializeField] public UpdateMethod update = new UpdateMethod()
        {
            autoUpdate = false
        };
        [SerializeField] public UpdateMethod lateUpdate = new UpdateMethod()
        {
            autoUpdate = false
        };
        
        //Private
        private Vector3 _velocity;
        private Vector3 _angularVelocity;
        private bool _isRbNotNull;
        private Transform _tr;


        private void Reset()
        {
            _tr = transform;
            restPos = _tr.position;
            restRot = _tr.rotation;
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _isRbNotNull = rb != null;
            _tr = self;
        }

        private void OnEnable()
        {
            this.RegisterFixedUpdate(fixedUpdate);
            this.RegisterUpdate(update);
            this.RegisterLateUpdate(lateUpdate);
        }

        private void OnDisable()
        {
            this.DeregisterFixedUpdate();
            this.DeregisterUpdate();
            this.DeregisterLateUpdate();
        }

        public void BatchFixedUpdate(float dt, float sdt)
        {
            UpdateSpring(dt);
        }
        
        public void BatchUpdate(float dt, float sdt)
        {
            UpdateSpring(dt);
        }

        public void BatchLateUpdate(float dt, float sdt)
        {
            UpdateSpring(dt);
        }

        private void UpdateSpring(float dt)
        {
            _isRbNotNull = rb != null;

            if (_isRbNotNull && !rb.isKinematic)
            {
                _velocity = rb.velocity;
                _angularVelocity = rb.angularVelocity;
            }

            if (goal != null)
            {
                restPos = goalOffsetSpace switch
                {
                    Space.Self => goal.TransformPoint(goalOffset),
                    Space.World => goal.position + goalOffset,
                    _ => Vector3.zero
                };
                restRot = goal.rotation;
            }

            if (!self) self = _isRbNotNull ? rb.transform : transform;
            _tr = self;

            var pivotPos = _tr.TransformPoint(pivotOffset);
            var rot = _tr.rotation;
            var pos = _tr.position;

            //Note: We can only call rb.Move once during a fixed update
            //Rotation spring
            if (enableRotationSpring)
            {
                var torque = _isRbNotNull
                    ? MathUtils.SpringUtils.DampedTorsionalSpring(dt, rotationSpring,
                        _tr.rotation,
                        restRot,
                        -_angularVelocity,
                        rb)
                    : MathUtils.SpringUtils.DampedTorsionalSpring(dt, rotationSpring,
                        _tr.rotation,
                        restRot,
                        -_angularVelocity);

                torque.Scale(torqueScale);
                _angularVelocity += dt * torque;
                if (_isRbNotNull)
                {
                    if (rb.isKinematic)
                    {
                        MathUtils.RotateAroundPivot(ref rot, ref pos, pivotPos,
                            Quaternion.AngleAxis(_angularVelocity.magnitude * Mathf.Rad2Deg * dt, _angularVelocity.normalized));
                    }
                    else
                        rb.AddTorque(torque, rotationForce);
                }
                else
                {
                    MathUtils.RotateAroundPivot(ref rot, ref pos, pivotPos,
                        Quaternion.AngleAxis(_angularVelocity.magnitude * Mathf.Rad2Deg * dt, _angularVelocity.normalized));
                }
            }

            //Position Spring
            if (enablePositionSpring)
            {
                var deltaPos = restPos - pivotPos;
                var force = MathUtils.SpringUtils.DamperSpring(dt, positionSpring.frequency, positionSpring.damper, deltaPos,
                    -_velocity, positionSpring.useForce);
                force.Scale(forceScale);
                _velocity += force * dt;

                if (_isRbNotNull)
                    if (rb.isKinematic)
                        pos += _velocity * dt;
                    else
                        rb.AddForceAtPosition(force, pivotPos, positionForce);
                else
                    pos += _velocity * dt;
            }

            switch (_isRbNotNull)
            {
                case true when rb.isKinematic:
                    rb.Move(pos, rot);
                    break;
                case false:
                    _tr.position = pos;
                    _tr.rotation = rot;
                    break;
            }
        }

        [Button(ButtonStyle.FoldoutButton)]
        public void AddForce(Vector3 impulse, ForceMode mode)
        {
            _velocity += impulse;
            if(_isRbNotNull && !rb.isKinematic)
                rb.AddForce(impulse, mode);
        }
        
        [Button(ButtonStyle.FoldoutButton)]
        public void AddTorque(Vector3 torque, ForceMode mode)
        {
            _angularVelocity += torque;
            if(_isRbNotNull && !rb.isKinematic)
                rb.AddTorque(torque, mode);
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            if(GizmoContext.InSelection(this))
                DrawDebug(Draw.editor);
        }

        void DrawDebug(CommandBuilder commandBuilder)
        {
            if (!self) self = _isRbNotNull ? rb.transform : transform;
            _tr = self;

            var pivotPos = _tr.TransformPoint(pivotOffset);
            if (goal != null)
                restPos = goalOffsetSpace switch
                {
                    Space.Self => goal.TransformPoint(goalOffset),
                    Space.World => goal.position + goalOffset,
                    _ => Vector3.zero
                };
            
            commandBuilder.DrawSolidCube(pivotPos, Vector3.one * 0.02f, Quaternion.identity);
            using (commandBuilder.WithColor(new Color(0.59f, 1f, 0.5f)))
                commandBuilder.DrawSolidSphere(restPos, Vector3.one * 0.05f);
            using (commandBuilder.WithColor(new Color(1f, 0.61f, 0.41f)))
            {
                commandBuilder.DrawSolidLine(pivotPos, restPos, 0.002f);
            }
        }
    }
}
