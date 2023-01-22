using System;
using RR.StateSystem;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    public partial class CharacterController
    {
        [field: SerializeField] public StateList<ShapeState> ShapeStates { get; private set; } = new ();
        [field: SerializeField] public StateList<MovementState> MovementStates { get; private set; } = new();
        
        [field: SerializeField] public ShapeState CurrentShapeState { get; private set; }
        [field: SerializeField] public MovementState CurrentMovementState { get; private set; }

        public event Action<float, float> StateUpdateAction;

        private void CCState_Awake()
        {
            CurrentMovementState ??= ScriptableObject.CreateInstance<MovementState>();
            CurrentShapeState ??= ScriptableObject.CreateInstance<ShapeState>();
            
            GetMovementState();
            GetShapeState();
            Debug.Log(CurrentMovementState.acceleration);
        }

        private void GetShapeState()
        {
            ShapeStates.ApplyState(CurrentShapeState);
        }
        
        private void GetMovementState()
        {
            MovementStates.ApplyState(CurrentMovementState);
        }

        void CCState_FixedUpdate()
        {
            GetMovementState();
            GetShapeState();
        }
        
        
    }
}