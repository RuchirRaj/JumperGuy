using RR.Attributes;
using RR.UpdateSystem;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [AddComponentMenu("")]
    public abstract class PlayerCameraBase : MonoBehaviour, IBatchLateUpdate
    {
        [CustomTitle("Update", 1f, 0.79f, 0.98f)]
        public UpdateMethod lateUpdate = new() { autoUpdate = true };
        public abstract bool Active { get; }
        
        public abstract void BatchLateUpdate(float dt, float sdt);

        protected virtual void OnEnable()
        {
            this.RegisterLateUpdate(lateUpdate);
        }

        protected virtual void OnDisable()
        {
            this.DeregisterLateUpdate();
        }

        /// <summary>
        /// Change the active state of the camera
        /// </summary>
        /// <param name="value"></param>
        /// <param name="characterController"></param>
        public abstract void SetActive(bool value, CharacterController characterController);
    }
}