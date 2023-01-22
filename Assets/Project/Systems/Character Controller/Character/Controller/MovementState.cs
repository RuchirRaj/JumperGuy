using RR.StateSystem;
using RR.Utils;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    [CreateAssetMenu(fileName = "MovementState", menuName = "Ruchir/Character Controller/Movement State", order = 0)]
    public class MovementState : ScriptableObject, IState<MovementState>
    {
        public StateVariable<bool> reorientToGround = new();
        public StateVariable<float> maxSpeed = new();
        public StateVariable<float> maxAngularSpeed = new();
        public StateVariable<float> acceleration = new();
        public StateVariable<float> deceleration = new();
        public StateVariable<float> maxAcceleration = new();
        public StateVariable<float> maxDeceleration = new();
        public StateVariable<ScaledAnimationCurve> accelerationCurve = new();
        public StateVariable<ScaledAnimationCurve> maxAccelerationCurve = new();

        public void Apply(MovementState state)
        {
            reorientToGround.Apply(state.reorientToGround);
            maxSpeed.Apply(state.maxSpeed);
            maxAngularSpeed.Apply(state.maxAngularSpeed);
            acceleration.Apply(state.acceleration);
            deceleration.Apply(state.deceleration);
            maxAcceleration.Apply(state.maxAcceleration);
            maxDeceleration.Apply(state.maxDeceleration);
            accelerationCurve.Apply(state.accelerationCurve);
            maxAccelerationCurve.Apply(state.maxAccelerationCurve);
        }

        public void Reset()
        {
            reorientToGround.Disable();
            maxSpeed.Disable();
            maxAngularSpeed.Disable();
            acceleration.Disable();
            deceleration.Disable();
            maxAcceleration.Disable();
            maxDeceleration.Disable();
            accelerationCurve.Disable();
            maxAccelerationCurve.Disable();
        }
    }
}