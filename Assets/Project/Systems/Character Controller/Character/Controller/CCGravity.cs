using RR.Common;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    public partial class CharacterController
    {
        [Header("Gravity")] 
        [SerializeField] private float gravityMultiplier = 1;

        [field: SerializeField]
        public GravityManager.GravityMask GravityMask { get; set; } = GravityManager.GravityMask.Channel0;
        [field: SerializeField] public Vector3 ExternalGravity { get; set; } = new(0,-30F,0);
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
            _gravityForce = ExternalGravity +
                            GravityManager.GetInstance().GetGravityAtPos(Rigidbody.worldCenterOfMass, GravityMask) *
                            gravityMultiplier;
        }
    }
}