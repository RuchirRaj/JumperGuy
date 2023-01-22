using System;
using RR.Utils;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Test
{
    public class PlanetGravityTest : MonoBehaviour
    {
        public CharacterController[] controllers = Array.Empty<CharacterController>();
        public string playerTag;
        public ScaledAnimationCurve gravityMultiplier = new();

        private void FixedUpdate()
        {
            foreach (var controller in controllers)
            {
                var direction = transform.position - controller.transform.position;
                controller.SetGravity(direction.normalized * gravityMultiplier.Evaluate(direction.magnitude));
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;
            var controller = other.GetComponent<CharacterController>();
            var direction = transform.position - other.transform.position;
            controller.SetGravity(direction.normalized * gravityMultiplier.Evaluate(direction.magnitude)); 
        }
    }
}