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
        [Space]
        public GameObject normalShape, secondShape;
        public CharacterController controller;
        [Space]
        public TextMeshPro textMeshPro;
        [Space]
        public InputAction cameraAction = new InputAction("CameraChange");

        public List<GameObject> cameras = new();

        public Transform camParent;
        public bool changePose;
        public float camDuration = 0.2f;
        public Ease camEase = Ease.OutQuad;

        private bool _moveStateEnabled;
        private bool _shapeStateEnabled;
        private int _currentCam = 0;
        private ShapeSettings _currentShape;
        
        private void Update()
        {
            if (!changePose || !camParent) return;
            if(_currentShape.ShapeEquals(controller.Base.Shape)) return;
            _currentShape = controller.Base.Shape;
            // camParent.DOLocalMoveY(_currentShape.height - _currentShape.radius, camDuration).SetEase(camEase);
        }

        private void OnEnable()
        {
            DOTween.Init();
            moveAction.Enable();
            moveAction.performed += MoveActionPerformed;
            shapeAction.Enable();
            shapeAction.performed += ShapeActionPerformed;
            cameraAction.Enable();
            cameraAction.performed += CameraActionPerformed;
            SetCurrentCamera(0);
        }

        private void CameraActionPerformed(InputAction.CallbackContext obj)
        {
            if (obj.performed) ToggleCameras();
        }

        public void SetCurrentCamera(int index)
        {
            if (cameras.Count == 0)
            {
                _currentCam = 0;
                return;
            }
            _currentCam = index;
            _currentCam %= cameras.Count;
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].SetActive(i == _currentCam);
            }
        }
        public void ToggleCameras()
        {
            if (cameras.Count == 0)
            {
                _currentCam = 0;
                return;
            }
            cameras[_currentCam].SetActive(false);
            
            _currentCam++;
            _currentCam %= cameras.Count;
            cameras[_currentCam].SetActive(true);
        }

        private void OnDisable()
        {
            moveAction.Disable();
            moveAction.performed -= MoveActionPerformed;
            shapeAction.Disable();
            shapeAction.performed -= MoveActionPerformed;
            cameraAction.Disable();
            cameraAction.performed -= CameraActionPerformed;
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