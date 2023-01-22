using System;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    [Serializable]
    public struct ShapeSettings
    {
        public float radius;
        public float height;
        public float stepHeightRatio;
        public float comHeight;
        public PhysicMaterial material;
        
        public ShapeSettings(float r, float h, float ratio, float com)
        {
            radius = r;
            height = h;
            stepHeightRatio = ratio;
            comHeight = com;
            material = null;
        }

        public bool ShapeEquals(ShapeSettings other) => radius.Equals(other.radius) && height.Equals(other.height) &&
                                                        stepHeightRatio.Equals(other.stepHeightRatio);
    }
}