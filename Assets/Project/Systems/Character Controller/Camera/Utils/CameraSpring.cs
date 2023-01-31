using System;
using RR.Utils;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [Serializable]
    public class CameraSpring
    {
        public SpringSettings positionSpring, angularSpring;
        public Vector3 pivotPos;
        public Vector3 pos, targetPos;
        public Quaternion rot, targetRot;

        public Vector3 velocity, angularVelocity;
        public Vector3 force, torque;

        public void Update(float dt, Transform transform)
        {
            pos = transform.localPosition;
            rot = transform.localRotation;
            //Rotation Spring around Pivot
            {
                torque = MathUtils.SpringUtils.DampedTorsionalSpring(dt, angularSpring, rot, targetRot, -angularVelocity);
                angularVelocity += torque * dt;
                MathUtils.RotateAroundPivot(ref rot, ref pos, transform.parent.InverseTransformPoint(transform.TransformPoint(pivotPos)),
                    Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * dt, angularVelocity.normalized));
            }
            //Position Spring
            {
                var deltaPos = targetPos - transform.parent.InverseTransformPoint(transform.TransformPoint(pivotPos));
                force = MathUtils.SpringUtils.DamperSpring(dt, positionSpring, deltaPos, -velocity);
                velocity += force * dt;
                pos += velocity * dt;
            }
        }
        
        public void Update(float dt, Quaternion r, Vector3 p)
        {
            pos = p;
            rot = r;
            //Rotation Spring around Pivot
            {
                torque = MathUtils.SpringUtils.DampedTorsionalSpring(dt, angularSpring, rot, targetRot, -angularVelocity);
                angularVelocity += torque * dt;
                MathUtils.RotateAroundPivot(ref rot, ref pos, pos + (rot * pivotPos),
                    Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * dt, angularVelocity.normalized));
            }
            //Position Spring
            {
                var deltaPos = targetPos - (pos + rot * pivotPos);
                force = MathUtils.SpringUtils.DamperSpring(dt, positionSpring, deltaPos, -velocity);
                velocity += force * dt;
                pos += velocity * dt;
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
    }
}