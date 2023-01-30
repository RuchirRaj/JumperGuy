using System;
using Drawing;
using UnityEngine;

namespace RR.Common
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Gravity/Source/Global Gravity Source")]
    public class GlobalGravitySource : GravitySource
    {
        public Vector3 direction = Vector3.down;
        public float gravity = 10;
        public GravityManager.GravityMask mask = (GravityManager.GravityMask)1;


        public override Vector3 GetGravity(Vector3 position, GravityManager.GravityMask mask)
        {
            return (this.mask & this.mask) != 0 ? transform.TransformDirection(direction) * gravity : Vector3.zero;
        }

        public override bool WithinBounds(Vector3 pos)
        {
            return true;
        }

        protected override void Draw(CommandBuilder draw)
        {
            var position = transform.position;
            draw.Arrow(position, position + transform.TransformDirection(direction) * gravity);
        }
    }
}