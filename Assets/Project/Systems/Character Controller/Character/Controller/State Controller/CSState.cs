using System.Collections.Generic;
using RR.Attributes;
using RR.StateSystem;
using UnityEngine;

namespace RR.Gameplay.CharacterController.StateController
{
    public partial class CharacterStateController
    {
        private int _movementIndex, _shapeIndex;

        //Private
        [SerializeField]
        private bool enableDebug;
        
        [CustomTitle("Settings", 1f, 0.73f, 0.6f)]
        [field: SerializeField]
        public State<MovementState> MovementState { get; private set; } = new() { Name = "DynamicMovement", blockedBy = new List<string>(0) };

        [field: SerializeField]
        public State<ShapeState> ShapeState { get; private set; } = new() { Name = "DynamicShape", blockedBy = new List<string>(0)};

        
        private void UpdateState(float dt, float sdt)
        {
            
        }
    }
}