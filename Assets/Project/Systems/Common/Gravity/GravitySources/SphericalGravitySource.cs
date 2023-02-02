﻿using Drawing;
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
        
        private void OnDrawGizmos()
        {
            if (!editorGizmo) return;
            DrawGizmo();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (editorGizmo) return;
            DrawGizmo();
        }

        private void DrawGizmo()
        {
            var c = gizmoColor;
            c.a *= 0.1f;
            Gizmos.color = c;
            Matrix4x4 trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = trs;
            
            Gizmos.DrawSphere(Vector3.zero, radius);
        }
    }
}