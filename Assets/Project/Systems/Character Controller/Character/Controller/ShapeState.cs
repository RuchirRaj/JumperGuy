using RR.StateSystem;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    [CreateAssetMenu(fileName = "Shape State", menuName = "Ruchir/Character Controller/Shape State", order = 0)]
    public class ShapeState : ScriptableObject, IState<ShapeState>
    {
        public StateVariable<ShapeSettings> shape = new();
        public void Apply(ShapeState state)
        {
            shape.Apply(state.shape);
        }

        public void Reset()
        {
            shape.Disable();
        }
    }
}