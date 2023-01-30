using RR.Attributes;
using RR.UpdateSystem;
using UnityEngine;


namespace RR.Common
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Gravity/Rigidbody Gravity")]
    public class RigidbodyGravity : MonoBehaviour, IBatchFixedUpdate
    {
        public bool floatToSleep = true;
        public float floatDelay = 2f;
        public GravityManager.GravityMask mask = GravityManager.GravityMask.Channel0;
        
        [CustomTitle("Update", 1f, 0.62f, 0.91f)]
        public UpdateMethod fixedUpdate = new (){autoUpdate = true};
        
        private Rigidbody _rb;
        private float _float; //Current Float Time
        private void OnEnable()
        {
            this.RegisterFixedUpdate(fixedUpdate);
            _rb = GetComponent<Rigidbody>();
            // _rb.useGravity = false;
        }

        private void OnDisable()
        {
            this.DeregisterFixedUpdate();
        }

        public void BatchFixedUpdate(float dt, float sdt)
        {
            if(!_rb) return;

            if (floatToSleep)
            {
                if (_rb.IsSleeping())
                {
                    _float = 0;
                    return;
                }

                if (_rb.velocity.sqrMagnitude < 0.00001f)
                {
                    _float += dt;
                    if(_float > floatDelay)
                        return;
                }

                _float = 0;
            }
            
            _rb.AddForce(GetCustomGravity(), ForceMode.Acceleration);
        }

        Vector3 GetCustomGravity()
        {
            return GravityManager.GetInstance().GetGravityAtPos(transform.position, mask);
        }
    }
}