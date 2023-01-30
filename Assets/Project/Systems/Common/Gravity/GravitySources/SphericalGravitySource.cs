using Drawing;
using UnityEngine;

namespace RR.Common
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Gravity/Source/Spherical Gravity Source")]
    public class SphericalGravitySource : GravitySource
    {
        public AnimationCurve gravity;
        public float gravityMultiplier = -10;
        public float radius = 10;
        public GravityManager.GravityMask mask = (GravityManager.GravityMask)1;

        public override Vector3 GetGravity(Vector3 position, GravityManager.GravityMask mask)
        {
            var direction = position - transform.position;
            return (this.mask & this.mask) != 0
                ? WithinBounds(position)
                    ? direction.normalized * (gravityMultiplier * gravity.Evaluate(direction.magnitude))
                    : Vector3.zero
                : Vector3.zero;
        }

        public override bool WithinBounds(Vector3 pos)
        {
            return (pos - transform.position).magnitude < radius;
        }

        protected override void Draw(CommandBuilder draw)
        {
            draw.WireSphere(transform.position, radius);
        }
    }
}