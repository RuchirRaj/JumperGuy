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
        [Space]
        public bool runtimeGizmo;
        public bool editorGizmo = true;

        public override Vector3 GetGravity(Vector3 position, int mask)
        {
            return transform.TransformDirection(direction) * gravity;
        }

        public override bool WithinBounds(Vector3 pos)
        {
            Bounds b = new Bounds(Vector3.zero, scale);
            return b.Contains(transform.InverseTransformPoint(pos));
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
            using (draw.InLocalSpace(transform))
            using (draw.WithColor(Color.yellow))
                draw.WireBox(Vector3.zero, scale);
            
        }
    }
}