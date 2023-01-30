using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RR.Gameplay.CharacterController.Test
{
    [AddComponentMenu("Ruchir/Character/Test/Character State Change Test")]
    public class CharacterStateChangeTest : MonoBehaviour
    {
        public InputAction moveAction;
        public string moveStateName;
        [Space]
        public InputAction shapeAction;
        public string shapeStateName;
        public GameObject normalShape;
        public GameObject secondShape;
        
        [Space]
        public CharacterController controller;
        [Space]
        public TextMeshPro textMeshPro;
        

        private bool _moveStateEnabled;
        private bool _shapeStateEnabled;
        private ShapeSettings _currentShape;

        private void OnEnable()
        {
            DOTween.Init();
            moveAction.Enable();
            moveAction.performed += MoveActionPerformed;
            shapeAction.Enable();
            shapeAction.performed += ShapeActionPerformed;
        }


        private void OnDisable()
        {
            moveAction.Disable();
            moveAction.performed -= MoveActionPerformed;
            shapeAction.Disable();
            shapeAction.performed -= MoveActionPerformed;
        }

        private void ShapeActionPerformed(InputAction.CallbackContext obj)
        {
            if (obj.performed || obj.canceled)
                ToggleShapeState();
        }

        private void MoveActionPerformed(InputAction.CallbackContext obj)
        {
            if (obj.performed || obj.canceled)
                ToggleMoveState();
        }

        private void ToggleMoveState()
        {
            if (_moveStateEnabled)
            {
                controller.MovementStates.DisableState(moveStateName);
            }
            else
            {
                controller.MovementStates.EnableState(moveStateName);
            }
            if(textMeshPro)
                textMeshPro.text = $"Performed {controller.MovementStates.states[1].Value.maxSpeed.value} {controller.MovementStates.states[1].Active}";

            _moveStateEnabled = !_moveStateEnabled;
        }
        
        private void ToggleShapeState()
        {
            if (_shapeStateEnabled)
            {
                controller.ShapeStates.DisableState(shapeStateName);
                normalShape.SetActive(true);
                secondShape.SetActive(false);
            }
            else
            {
                controller.ShapeStates.EnableState(shapeStateName);
                normalShape.SetActive(false);
                secondShape.SetActive(true);
            }
            
            _shapeStateEnabled = !_shapeStateEnabled;
        }
    }
   
}