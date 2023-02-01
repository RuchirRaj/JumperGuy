using System;
using System.Collections.Generic;
using RR.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace RR.Gameplay.CharacterController.Camera
{
    [Serializable]
    public class CameraSpring
    {
        public SpringSettings positionSpring, angularSpring;
        [FormerlySerializedAs("pivotPos")] public Vector3 pivotOffset;
        public Vector3 pos, targetPos;
        public Quaternion rot, targetRot;

        public Vector3 velocity, angularVelocity;
        public Vector3 force, torque;
        
        [Serializable]
        public struct SoftForce
        {
            public Vector3 force;
            public float time;
            public float currentTime;

            public SoftForce(float t, Vector3 f)
            {
                force = f;
                time = t;
                currentTime = t;
            }
        }

        public List<SoftForce> softPositionForces = new();
        public List<SoftForce> softRotationForces = new();

        public void Update(float dt, Transform transform)
        {
            pos = transform.localPosition;
            rot = transform.localRotation;
            ApplySoftForce(dt);
            //Rotation Spring around Pivot
            {
                torque = MathUtils.SpringUtils.DampedTorsionalSpring(dt, angularSpring, rot, targetRot, -angularVelocity);
                angularVelocity += torque * dt;
                MathUtils.RotateAroundPivot(ref rot, ref pos, transform.parent.InverseTransformPoint(transform.TransformPoint(pivotOffset)),
                    Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * dt, angularVelocity.normalized));
            }
            //Position Spring
            {
                var deltaPos = targetPos - transform.parent.InverseTransformPoint(transform.TransformPoint(pivotOffset));
                force = MathUtils.SpringUtils.DamperSpring(dt, positionSpring, deltaPos, -velocity);
                velocity += force * dt;
                pos += velocity * dt;
            }
        }
        
        public void Update(float dt, Quaternion r, Vector3 p)
        {
            pos = p;
            rot = r;
            ApplySoftForce(dt);
            //Rotation Spring around Pivot
            {
                torque = MathUtils.SpringUtils.DampedTorsionalSpring(dt, angularSpring, rot, targetRot, -angularVelocity);
                angularVelocity += torque * dt;
                MathUtils.RotateAroundPivot(ref rot, ref pos, pos + (rot * pivotOffset),
                    Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * dt, angularVelocity.normalized));
            }
            //Position Spring
            {
                var deltaPos = targetPos - (pos + rot * pivotOffset);
                force = MathUtils.SpringUtils.DamperSpring(dt, positionSpring, deltaPos, -velocity);
                velocity += force * dt;
                pos += velocity * dt;
            }
        }

        void ApplySoftForce(float dt)
        {
            //Position Force
            for (int i = softPositionForces.Count - 1; i >= 0; i--)
            {
                var s = softPositionForces[i];
                
                AddForce(s.force * s.currentTime / s.time, ForceMode.Force, dt);
                s.currentTime -= dt;
                if(s.currentTime <= 0)
                    softPositionForces.RemoveAt(i);
                else
                    softPositionForces[i] = s;
            }
            
            //Position Force
            for (int i = softRotationForces.Count - 1; i >= 0; i--)
            {
                var s = softRotationForces[i];
                
                AddTorque(s.force * s.currentTime / s.time, ForceMode.Force, dt);
                s.currentTime -= dt;
                if(s.currentTime <= 0)
                    softRotationForces.RemoveAt(i);
                else
                    softRotationForces[i] = s;
            }
        }

        public void AddForce(Vector3 value, ForceMode mode, float dt)
        {
            switch (mode)
            {
                case ForceMode.Acceleration:
                case ForceMode.Force:
                    velocity += value * dt;
                    break;
                case ForceMode.Impulse:
                case ForceMode.VelocityChange:
                    velocity += value;
                    break;
            }
        }
        
        public void AddImpulse(Vector3 value)
        {
            velocity += value;
        }
        
        public void AddImpulseTorque(Vector3 value)
        {
            angularVelocity += value;
        }

        public void AddTorque(Vector3 value, ForceMode mode, float dt)
        {
            switch (mode)
            {
                case ForceMode.Acceleration:
                case ForceMode.Force:
                    angularVelocity += value * dt;
                    break;
                case ForceMode.Impulse:
                case ForceMode.VelocityChange:
                    angularVelocity += value;
                    break;
            }
        }

        public void AddSoftPositionForce(SoftForce s)
        {
            softPositionForces.Add(s);
        }

        public void AddSoftRotationForce(SoftForce s)
        {
            softRotationForces.Add(s);
        }
    }
}