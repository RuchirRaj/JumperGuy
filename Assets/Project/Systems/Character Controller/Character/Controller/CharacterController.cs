using RR.Attributes;
using RR.UpdateSystem;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    [AddComponentMenu("Ruchir/Character/Character Controller")]
    [SelectionBase]
    [RequireComponent(typeof(CharacterBase))]
    public partial class CharacterController : MonoBehaviour, IBatchFixedUpdate
    {
        [field: CustomTitle("Update", 1f, 0.79f, 0.98f)] 
        [field: SerializeField] public UpdateMethod FixedUpdate { get; private set; } = new() { autoUpdate = true };

        private void OnEnable()
        {
            this.RegisterFixedUpdate(FixedUpdate);

            CCController_OnEnable();
            CCInput_OnEnable();
        }

        private void OnDisable()
        {
            this.DeregisterFixedUpdate();
            CCInput_OnDisable();
        }

        private void Awake()
        {
            MovementStates.ValidateStates();
            ShapeStates.ValidateStates();
            CCState_Awake();
            CCController_Awake();
        }

        private void Start()
        {
            CCController_Start();
        }

        public void BatchFixedUpdate(float dt, float sdt)
        {
            CCState_FixedUpdate();
            CCController_FixedUpdate(dt, sdt);
        }
    }   
}
