using System;
using UnityEngine;

namespace RR.Gameplay.CharacterController.StateController
{
    [AddComponentMenu("Ruchir/Character/Character State Controller")]
    [SelectionBase]
    [RequireComponent(typeof(CharacterController), typeof(CharacterBase))]
    public partial class CharacterStateController : MonoBehaviour
    {
        private CharacterController _controller;
        private CharacterBase _base;
        
        public float test;

        public event Action<PlayerStateBase, PlayerStateBase> StateChanged; 
        public event Action<PlayerStateBase> StateEntered;
        public event Action<PlayerStateBase> StateExited;

        private void OnEnable()
        {
            _controller = GetComponent<CharacterController>();
            _base = GetComponent<CharacterBase>();

            if (MovementState.Value == null)
            {
                MovementState.CreateNew();
            }
            if (ShapeState.Value == null)
            {
                ShapeState.CreateNew();
            }
            
            _controller.StateUpdateAction += UpdateState;
        }

        private void OnDisable()
        {
            if(_controller)
                _controller.StateUpdateAction -= UpdateState;
        }
        
        
        private void Start()
        {
            _movementIndex = _controller.MovementStates.AddState(MovementState);
            _shapeIndex = _controller.ShapeStates.AddState(ShapeState);
        }
    }
}