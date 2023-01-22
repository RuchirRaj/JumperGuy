using Cinemachine;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [AddComponentMenu("Ruchir/Character/Camera/Camera Brain")]
    [RequireComponent(typeof(CinemachineBrain))]
    public class MainCamManager : MonoBehaviour
    {
        private CinemachineBrain _brain;
        public CharacterController controller;

        private void OnEnable()
        {
            _brain = GetComponent<CinemachineBrain>();
        }

        private void Update()
        {
            if(!controller || !_brain)
                return;
            _brain.WorldUpOverride = controller.BaseRefTransform;
        }
    }
}
