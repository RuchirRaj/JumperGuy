using System.Collections.Generic;
using RR.Attributes;
using RR.StateSystem;
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

        private int _movementIndex, _shapeIndex;

        //Private
        [SerializeField]
        private bool enableDebug;
        
        [CustomTitle("Settings", 1f, 0.73f, 0.6f)]
        [field: SerializeField]
        public State<MovementState> MovementState { get; private set; } = new() { Name = "DynamicMovement", blockedBy = new List<string>(0) };

        [field: SerializeField]
        public State<ShapeState> ShapeState { get; private set; } = new() { Name = "DynamicShape", blockedBy = new List<string>(0)};

        public float test;
        
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

        private void UpdateState(float dt, float sdt)
        {
            
        }
        

        private void Start()
        {
            _movementIndex = _controller.MovementStates.AddState(MovementState);
            _shapeIndex = _controller.ShapeStates.AddState(ShapeState);
        }
    }
}