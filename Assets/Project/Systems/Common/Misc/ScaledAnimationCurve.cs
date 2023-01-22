using System;
using NaughtyAttributes;
using UnityEngine;

namespace RR.Utils
{
    [Serializable]
    public class ScaledAnimationCurve
    {
        [CurveRange(0, 0, 1, 1, EColor.Blue)]
        public AnimationCurve curve;

        public Vector2 offset;
        public Vector2 scale;

        public float Evaluate(float time) => curve.Evaluate((time - offset.x) / scale.x) * scale.y + offset.y;
        
        public void Copy(ScaledAnimationCurve source)
        {
            curve = source.curve;
            scale = source.scale;
            offset = source.offset;
        }
    }
}