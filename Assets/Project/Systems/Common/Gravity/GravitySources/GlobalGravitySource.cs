using UnityEngine;

namespace RR.Common
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Gravity/Source/Global Gravity Source")]
    public class GlobalGravitySource : GravitySource
    {
        public Vector3 direction = Vector3.down;
        public float gravity = 10;

        public override Vector3 GetGravity(Vector3 position, int mask)
        {
            return transform.TransformDirection(direction) * gravity;
        }

        public override bool WithinBounds(Vector3 pos)
        {
            return true;
        }
    }
}