using System;
using RR.Attributes;
using RR.UpdateSystem;
using UnityEngine;

namespace RR
{
    public class Conveyor : MonoBehaviour, IBatchFixedUpdate
    {
        private static readonly int VelocityHash = Shader.PropertyToID("_SurfaceSpeed");
        private Vector2 _surfaceVelocity;
        private Material _mat;
            
        [SerializeField] private Rigidbody rb;
        [SerializeField] private bool rotate;
        [SerializeField] private Vector3 angularVelocity;
        [SerializeField] private Vector3 velocity;
        [SerializeField] private Vector2 materialVelocity;
        
        [CustomTitle("Update", 1f, 0.79f, 0.98f)]
        public UpdateMethod fixedUpdate = new()
        {
            autoUpdate = true
        };

        private void OnEnable()
        {
            rb ??= GetComponent<Rigidbody>();
            this.RegisterFixedUpdate(fixedUpdate);
            _mat = GetComponent<Renderer>().material;
        }

        private void Reset()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            this.DeregisterFixedUpdate();
        }

        public void BatchFixedUpdate(float dt, float sdt)
        {
            var pos = rb.position;
            var dir = transform.TransformDirection(velocity);
            rb.position -= dir * dt;
            rb.MovePosition(pos);

            if(angularVelocity.sqrMagnitude > float.Epsilon)
            {
                var rot = rb.rotation;
                if (rotate)
                {
                    rb.MoveRotation(rot * Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * dt, angularVelocity.normalized));
                }
                else
                {
                    rb.rotation = Quaternion.AngleAxis(-angularVelocity.magnitude * Mathf.Rad2Deg * dt, angularVelocity.normalized);
                    rb.MoveRotation(rot);
                }
            }
            
            // rb.angularVelocity = angularVelocity;
            
            if (_surfaceVelocity != materialVelocity)
            {
                _surfaceVelocity = materialVelocity;
                _mat.SetVector(VelocityHash, _surfaceVelocity);
            }
        }
    }
}
