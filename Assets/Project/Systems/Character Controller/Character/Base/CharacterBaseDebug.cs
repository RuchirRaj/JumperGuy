using System;
using Drawing;
using RR.UpdateSystem;
using RR.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    public partial class CharacterBase : IBatchUpdate
    {

        [Flags]
        public enum GizmoFlag
        {
            Shape = 1 << 1,
            Ground = 1 << 2,
            CenterOfMass = 1 << 3,
            Raycasts = 1 << 4,
            All = Shape | Ground | CenterOfMass | Raycasts
        }
        
        [SerializeField] private bool enableGizmos = true;
        [SerializeField] private bool enableRuntimeGizmos = true;
        [SerializeField] private GizmoFlag runtimeGizmoFlag = GizmoFlag.All;
        
        private Color _stableGroundColor = new Color(0.56f, 1f, 0.6f);
        private Color _unStableGroundColor = new Color(1f, 0.34f, 0.36f);
        
        public override void DrawGizmos()
        {
            if (GizmoContext.InSelection(this)) DrawDebug(Draw.editor, GizmoFlag.All);
            else DrawDebug(Draw.editor, enableGizmos ? GizmoFlag.All : GizmoFlag.Shape);
        }

        public void BatchUpdate(float dt, float sdt)
        {
            if (enableRuntimeGizmos) DrawDebug(Draw.ingame, runtimeGizmoFlag);
        }

        public void DrawDebug(CommandBuilder draw, GizmoFlag flag)
        {
            var colType = ShapeColliderType;
            var shape = Shape;
            if((flag & GizmoFlag.Shape) != 0)
                using (draw.InLocalSpace(transform))
                {
                    switch (colType)
                    {
                        case ColliderType.Capsule:
                        {
                            var pos = new Vector3(0, shape.height, 0) + Offset;
                            var height = Mathf.Clamp(shape.height, 0, shape.height * (1 - shape.stepHeightRatio));
                            var radius = Mathf.Clamp(shape.radius, 0, shape.height * 0.5f * (1 - shape.stepHeightRatio));
                            draw.WireCapsule(pos, Vector3.down, height, radius);
                        }
                            break;
                        case ColliderType.Box:
                        {
                            var pos = new Vector3(0, shape.height * (1 + shape.stepHeightRatio) / 2, 0) + Offset;
                            var height = Mathf.Clamp(shape.height, 0, shape.height * (1 - shape.stepHeightRatio));
                            var radius = Mathf.Clamp(shape.radius, 0, shape.height * 0.5f * (1 - shape.stepHeightRatio));
                            draw.WireBox(pos, new float3(radius * 2, height, radius * 2));
                        }
                            break;
                        case ColliderType.Sphere:
                        {
                            var center =
                                new Vector3(0, shape.height * (1 + shape.stepHeightRatio) / 2, 0) + Offset;
                            var height = Mathf.Clamp(shape.height, 0,
                                shape.height * (1 - shape.stepHeightRatio));
                            var radius = Mathf.Clamp(shape.radius, 0,
                                shape.height * 0.5f * (1 - shape.stepHeightRatio));

                            center -= (height/2 - radius) * Vector3.up;
                            draw.WireSphere(center, radius);
                        }
                            break;
                    }    
                }
            
            if(!Application.isPlaying) return;

            var hit = Sensor.averageHit;
            if((flag & GizmoFlag.Ground) != 0)
                switch (GroundState)
                {
                    case GroundedState.Stable:
                        draw.PlaneWithNormal(hit.point, hit.normal, new float2(0.1f,0.1f), _stableGroundColor);
                        break;
                    case GroundedState.UnStable:
                        draw.PlaneWithNormal(hit.point, hit.normal, new float2(0.1f,0.1f), _unStableGroundColor);
                        break;
                }

            if ((flag & GizmoFlag.CenterOfMass) != 0)
                using (draw.WithColor(Color.red))
                    draw.DrawSolidSphere(_rigidbody.worldCenterOfMass, Vector3.one * 0.05f);
        }
    }
}