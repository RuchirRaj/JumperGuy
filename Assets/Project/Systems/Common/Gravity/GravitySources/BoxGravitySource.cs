using Drawing;
using UnityEngine;

namespace RR.Common
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Gravity/Source/Box Gravity Source")]
    public class BoxGravitySource : GravitySource
    {
        public Vector3 direction = Vector3.down;
        public Vector3 scale = Vector3.one;
        public float gravity = 10;
        public GravityManager.GravityMask mask = (GravityManager.GravityMask)1;

        public override Vector3 GetGravity(Vector3 position, GravityManager.GravityMask mask)
        {
            return (this.mask & this.mask) != 0
                ? WithinBounds(position) ? transform.TransformDirection(direction) * gravity : Vector3.zero
                : Vector3.zero;
        }

        public override bool WithinBounds(Vector3 pos)
        {
            var b = new Bounds(Vector3.zero, scale);
            return b.Contains(transform.InverseTransformPoint(pos));
        }

        protected override void Draw(CommandBuilder draw)
        {
            using (draw.InLocalSpace(transform))
            {
                draw.WireBox(Vector3.zero, scale);
            }
        }
    }
}