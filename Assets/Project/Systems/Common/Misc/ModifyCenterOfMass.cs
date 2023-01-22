using System;
using UnityEngine;

namespace RR.Utils
{
    [AddComponentMenu("Ruchir/Utils/Modify Center Of Mass")]
    public class ModifyCenterOfMass : MonoBehaviour
    {
        public Transform com;
        private void OnEnable()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.automaticCenterOfMass = false;
            rb.centerOfMass = transform.InverseTransformPoint(com.position);
        }

        private void OnDrawGizmosSelected()
        {
            if(com)
                Gizmos.DrawSphere(com.position, 0.1f);
        }
    }
}