using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    public partial class CharacterController
    {
        [Space]
        [SerializeField] private Vector3 gravity = new(0,-30F,0);
        [SerializeField] private float gravityMultiplier = 1;
        private Vector3 _gravityForce;
        
        //Properties
        public float GravityMultiplier
        {
            get => gravityMultiplier;
            set => gravityMultiplier = value;
        }

        public Vector3 FinalGravity => _gravityForce;

        private void DetectGravity()
        {
            _gravityForce = gravity * gravityMultiplier;
        }

        public void SetGravity(Vector3 value)
        {
            gravity = value;
        }
    }
}