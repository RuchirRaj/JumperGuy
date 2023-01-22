using System;
using Drawing;
using RR.StateSystem;
using RR.UpdateSystem;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


namespace RR.Utils
{
    [AddComponentMenu("Ruchir/Utils/MouseDraggable")]
    public class MouseDraggable : MonoBehaviour//, IBatchFixedUpdate, IBatchUpdate
    {
        /*//Settings
        public float scrollSensitivity = 1;
        public bool isKinematic = false;
        public bool angularConstraint = true;

        public StateVariable<SpringSettings> angularSpring;
        public StateVariable<SpringSettings> positionalSpring;
        
        //Update Method
        public UpdateMethod update = new (){autoUpdate = true};
        public UpdateMethod fixedUpdate = new (){autoUpdate = true};
        
        //Gizmos
        public bool runtimeGizmo = true;


        //Private
        private Vector3 _mousePosition;

        private (bool valid, Rigidbody rb) _rb;
        private (Vector3 postion, Quaternion rotation) _initialTransform;
        private Vector3 _targetPos;
        private bool _dragging;


        private void OnEnable()
        {
            this.RegisterUpdate(update);
            this.RegisterFixedUpdate(update);
            
            var rb = GetComponent<Rigidbody>();
            _rb = rb ? (true, rb) : (false, null);
        }

        private void OnDisable()
        {
            this.DeregisterUpdate();
            this.DeregisterFixedUpdate();
        }

        private Vector3 ScreenPos()
        {
            return (_rb.valid && !isKinematic) ? Camera.main.WorldToScreenPoint(_rb.rb.worldCenterOfMass) : Camera.main.WorldToScreenPoint(transform.position);
        }

        private void OnMouseDown()
        {
            _dragging = true;
            _initialTransform.postion = (Vector3)Mouse.current.position.ReadValue() - ScreenPos();
            _initialTransform.rotation = transform.rotation;
        }

        private void RenderLine()
        {
            var mouse = (Vector3)Mouse.current.position.ReadValue();
            var target = mouse - new Vector3(_initialTransform.postion.x, _initialTransform.postion.y);
            var current = (Vector3)((Vector2)ScreenPos());
            var force = target - current;
            using (var draw = DrawingManager.GetBuilder(true))
            {
                // Draw the curves in 2D using pixel coordinates
                using (draw.InScreenSpace(Camera.main))
                {
                    draw.Line(mouse, target, Color.gray);
                    draw.Arrow(current, target, new float3(0,0,1), 10f, Color.red);
                }
            }
        }
        private void OnMouseUp()
        {
            _dragging = false;
        }

        public void BatchFixedUpdate(float dt, float sdt)
        {
            if (!_dragging) return;
            if (isKinematic || !_rb.valid) return;

            if (angularSpring.enable)
            {
                var torque = MathUtils.SpringUtils.DampedTorsionalSpring(dt, angularSpring, _rb.rb.rotation,
                    _initialTransform.rotation, -_rb.rb.angularVelocity, _rb.rb);
                _rb.rb.AddTorque(torque);
            }
            else if (angularConstraint)
            {
                _rb.rb.rotation = _initialTransform.rotation;
            }

            if (positionalSpring.enable)
            {
                var force = MathUtils.SpringUtils.DamperSpring(dt, positionalSpring,
                    _targetPos - _rb.rb.position, -_rb.rb.velocity);
                _rb.rb.AddForce(force, positionalSpring.value.useForce ? ForceMode.Force : ForceMode.Acceleration);
            }
        }

        public void BatchUpdate(float dt, float sdt)
        {
            if(!_dragging)
                return;
            
            var scroll = Mouse.current.scroll.ReadValue().y * scrollSensitivity;
            _initialTransform.postion.z -= scroll * dt;
            
            _targetPos =
                Camera.main.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue() - _initialTransform.postion);

            if(runtimeGizmo)
                RenderLine();
            
            if (!isKinematic && _rb.valid) return;

            if (_rb.rb)
                _rb.rb.position = _targetPos;
            else
                transform.position = _targetPos;
        }*/
    }   
}
