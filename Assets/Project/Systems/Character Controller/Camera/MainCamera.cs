using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RR.Gameplay.CharacterController.Camera
{
    [AddComponentMenu("Ruchir/Character/Camera/Main Camera")]
    public class MainCamera : MonoBehaviour
    {
        public static MainCamera Instance { get; private set; }
        public UnityEngine.Camera Camera { get; private set; } 
        
        public UniversalAdditionalCameraData CameraData { get; private set; } 

        private void Awake()
        {
            if(Instance && Instance != this)
                Destroy(this);
            Instance = this;
            Camera = GetComponent<UnityEngine.Camera>();
            CameraData = GetComponent<UniversalAdditionalCameraData>();
        }
        
        private void OnEnable()
        {
            if(Instance && Instance != this)
                Destroy(this);
            Instance = this;
            Camera = GetComponent<UnityEngine.Camera>();
            CameraData = GetComponent<UniversalAdditionalCameraData>();
        }
    }
}