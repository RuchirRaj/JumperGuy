using System;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    [Serializable]
    public class CharacterGroundSettings
    {
        public float stepDownDistance = 0.5f;
        public bool applyForceOnGround = true;
        public float maxForceToMassRatio = 10;
        public float slopeLimit = 75;
        public Vector2 groundingThreshold = new(0, 1);
        public Vector2 groundingForceClamp = new (-500, 500);
        public Vector2 unGroundedForceMultiplier = new(0, 1);
    }
}