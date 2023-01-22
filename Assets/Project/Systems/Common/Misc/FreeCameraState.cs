using RR.StateSystem;
using UnityEngine;

namespace RR.Utils
{
    public class FreeCameraState : ScriptableObject, IState<FreeCameraState>
    {
        
        public StateVariable<float> lookSpeedController = new();
        public StateVariable<float> lookSpeedMouse = new();
        public StateVariable<float> moveSpeed = new();
        public StateVariable<float> moveSpeedIncrement = new();
        public StateVariable<float> turbo = new();
        public StateVariable<Space> space = new();

        public void Apply(FreeCameraState state)
        {
            lookSpeedController.Apply(state.lookSpeedController);
            lookSpeedMouse.Apply(state.lookSpeedMouse);
            moveSpeed.Apply(state.moveSpeed);
            moveSpeedIncrement.Apply(state.moveSpeedIncrement);
            turbo.Apply(state.turbo);
            space.Apply(state.space);
        }

        public void Reset()
        {
            lookSpeedController.Disable();
            lookSpeedMouse.Disable();
            moveSpeed.Disable();
            moveSpeedIncrement.Disable();
            turbo.Disable();
            space.Disable();
        }
    }
}