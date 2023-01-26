using System;
using Drawing;
using RR.Utils;
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
        [Space]
        public bool runtimeGizmo;
        public bool editorGizmo = true;


        public override Vector3 GetGravity(Vector3 position, int mask)
        {
            var direction = position - transform.position;
            return WithinBounds(position) ? direction.normalized * (gravityMultiplier * gravity.Evaluate(direction.magnitude)): Vector3.zero;
        }

        public override bool WithinBounds(Vector3 pos)
        {
            return (pos - transform.position).magnitude < radius;
        }

        public override void DrawGizmos()
        {
            if (editorGizmo || GizmoContext.InSelection(this))
            {
                Draw(Drawing.Draw.editor);
            }
        }

        private void Update()
        {
            if(runtimeGizmo)
                Draw(Drawing.Draw.ingame);
        }

        private void Draw(CommandBuilder draw)
        {
            using (draw.WithColor(Color.yellow))
                draw.WireSphere(transform.position, radius);
        }
    }
}