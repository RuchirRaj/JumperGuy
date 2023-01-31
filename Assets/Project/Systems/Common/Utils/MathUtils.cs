using UnityEngine;

namespace RR.Utils
{
    public static class MathUtils
    {
        
        public static float WrapValue(float min, float max, float value)
        {
            var x = value;
            if (!(x < min) && !(x > max)) return x;
            x -= min;
            x %= (max - min);
            x += min;

            return x;
        }

        public static Vector2 WrapValue(Vector2 min, Vector2 max, Vector2 value)
        {
            return new Vector2(
                WrapValue(min.x, max.x, value.x),
                WrapValue(min.y, max.y, value.y));
        }
        
        public static Vector3 WrapValue(Vector3 min, Vector3 max, Vector3 value)
        {
            return new Vector3(
                WrapValue(min.x, max.x, value.x),
                WrapValue(min.y, max.y, value.y),
                WrapValue(min.z, max.z, value.z));
        }
        
        public static Vector4 WrapValue(Vector4 min, Vector4 max, Vector4 value)
        {
            return new Vector4(
                WrapValue(min.x, max.x, value.x),
                WrapValue(min.y, max.y, value.y),
                WrapValue(min.z, max.z, value.z),
                WrapValue(min.w, max.w, value.w));
        }

        public static float ClampValue(float min, float max, float value)
        {
            return Mathf.Clamp(value, min, max);
        }
        public static Vector2 ClampValue(Vector2 min, Vector2 max, Vector2 value)
        {
            return new(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y)
            );
        }
        public static Vector3 ClampValue(Vector3 min, Vector3 max, Vector3 value)
        {
            return new(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z)
            );
        }
        public static Vector4 ClampValue(Vector4 min, Vector4 max, Vector4 value)
        {
            return new(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z),
                Mathf.Clamp(value.w, min.w, max.w)
            );
        }
        
        /// <summary>
        /// Rotate the vector to be tangential to normal along the (normal, Up) plane
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="normal"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        public static Vector3 ReorientToNormal(Vector3 vec, Vector3 normal, Vector3 up)
        {
            vec *= Mathf.Sign(Vector3.Dot(up, normal));
            Vector3 left = Vector3.Cross(vec, normal);
            vec = Vector3.Cross(normal, left).normalized;
            return vec;
        }
        
        /// <summary>
        /// Rotate a object around a given pivot
        /// </summary>
        /// <param name="rot">Current rotation</param>
        /// <param name="pos">Current position</param>
        /// <param name="pivot">Pivot point in worldSpace</param>
        /// <param name="deltaRot">Rotation to apply</param>
        public static void RotateAroundPivot (ref Quaternion rot, ref Vector3 pos, Vector3 pivot, Quaternion deltaRot)
        {
            pos = deltaRot * (pos - pivot) + pivot;
            rot = deltaRot * rot;
        }

        public static void RotateAroundPivot(this Transform transform, Vector3 pivot, Quaternion deltaRot)
        {
            transform.position = deltaRot * (transform.position - pivot) + pivot;
            transform.rotation = deltaRot * transform.rotation;
        }



        public static class QuaternionUtils
        {
            /// <summary>
            /// Gets the shortest rotation initial initial to final rotation
            /// </summary>
            /// <param name="initial"></param>
            /// <param name="final"></param>
            /// <returns></returns>
            public static Quaternion ShortestRotation(Quaternion initial, Quaternion final)
            {
                if (Quaternion.Dot(final, initial) < 0)
                    return final * Quaternion.Inverse(Multiply(initial, -1));

                return final * Quaternion.Inverse(initial);
            }
            
            /// <summary>
            /// Multiply the quaternion in an euler manner
            /// </summary>
            /// <param name="input"></param>
            /// <param name="scale"></param>
            /// <returns>Scaled rotation</returns>
            public static Quaternion Multiply(Quaternion input, float scale)
            {
                return new Quaternion(input.x * scale, input.y * scale, input.z * scale, input.w * scale);
            }
            
            public static Quaternion ClampRotation(Quaternion q, Vector3 bounds, Vector3 negativeBounds)
            {
                q.x /= q.w;
                q.y /= q.w;
                q.z /= q.w;
                q.w = 1.0f;
 
                float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
                angleX = Mathf.Clamp(angleX, negativeBounds.x, bounds.x);
                q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
 
                float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
                angleY = Mathf.Clamp(angleY, negativeBounds.y, bounds.y);
                q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);
 
                float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
                angleZ = Mathf.Clamp(angleZ, negativeBounds.z, bounds.z);
                q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);
 
                return q.normalized;
            }
        }

        public static class SpringUtils
        {
            public static (float spring, float damper) GetStableSpringConstants(float dt, float mass, float frequency, float damper, bool useForce = false)
            {
                var kp = useForce ? frequency : (6f * frequency) * (6f * frequency) * 0.25f;
                var kd = useForce ? damper : 4.5f * frequency * damper;
            
                float g = 1 / (1 + kd * dt + kp * dt * dt);
            
                float ksg = kp * g;
                float kdg = (kd + kp * dt) * g;

                if (ksg > mass / (dt * dt))
                    ksg = mass / (dt * dt);
                
                if (kdg > mass / dt)
                    kdg = mass / dt;
                
                return (ksg, kdg);
            }

            /// <summary>
            /// Force we need final apply final get spring like movement
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="f"></param>
            /// <param name="d"></param>
            /// <param name="deltaPos"></param>
            /// <param name="deltaVel"></param>
            /// <param name="useForce"></param>
            /// <returns>Spring Force</returns>
            public static Vector3 DamperSpring(float dt, float f, float d, Vector3 deltaPos, Vector3 deltaVel,
                bool useForce)
            {
                var s = GetStableSpringConstants(dt, 1, f, d, useForce);
                return deltaPos * s.spring + deltaVel * s.damper;
            }

            /// <summary>
            /// Force we need final apply final get spring like movement
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="spring"></param>
            /// <param name="deltaPos"></param>
            /// <param name="deltaVel"></param>
            /// <returns>Spring Force</returns>
            public static Vector3 DamperSpring(float dt, SpringSettings spring, Vector3 deltaPos, Vector3 deltaVel)
            {
                var s = GetStableSpringConstants(dt, 1, spring.frequency, spring.damper, spring.useForce);
                return deltaPos * s.spring + deltaVel * s.damper;
            }
            
            /// <summary>
            /// Force we need final apply final get spring like movement
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="f"></param>
            /// <param name="d"></param>
            /// <param name="deltaPos"></param>
            /// <param name="deltaVel"></param>
            /// <param name="useForce"></param>
            /// <returns>Spring Force</returns>
            public static float DamperSpring(float dt, float f, float d, float deltaPos, float deltaVel,
                bool useForce)
            {
                var s = GetStableSpringConstants(dt, 1,f, d, useForce);
                return deltaPos * s.spring + deltaVel * s.damper;
            }

            /// <summary>
            /// Force we need final apply final get spring like movement
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="spring"></param>
            /// <param name="deltaPos"></param>
            /// <param name="deltaVel"></param>
            /// <returns>Spring Force</returns>
            public static float DamperSpring(float dt, SpringSettings spring, float deltaPos, float deltaVel)
            {
                var s = GetStableSpringConstants(dt, 1, spring.frequency, spring.damper, spring.useForce);
                return deltaPos * s.spring + deltaVel * s.damper;
            }
            
            //BUG rigidbody variant of DampedTorsionalSpring isn't stable
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="f"></param>
            /// <param name="d"></param>
            /// <param name="currentRot"></param>
            /// <param name="targetRot"></param>
            /// <param name="deltaRotVelocity"></param>
            /// <param name="rigidbody"></param>
            /// <param name="useForce"></param>
            /// <returns>Torque Force</returns>
            public static Vector3 DampedTorsionalSpring(float dt, float f, float d, Quaternion currentRot,
                Quaternion targetRot, Vector3 deltaRotVelocity, Rigidbody rigidbody, bool useForce)
            {
                var deltaRot = targetRot * Quaternion.Inverse(currentRot);
                var k = GetStableSpringConstants(dt, 1, f, d, useForce);

                // Q can be the-long-rotation-around-the-sphere eg. 350 degrees
                // We want the equivalent short rotation eg. -10 degrees
                // Check if rotation is greater than 190 degrees == q.w is negative
                if (deltaRot.w < 0)
                {
                    // Convert the quaternion final equivalent "short way around" quaternion
                    deltaRot.x = -deltaRot.x;
                    deltaRot.y = -deltaRot.y;
                    deltaRot.z = -deltaRot.z;
                    deltaRot.w = -deltaRot.w;
                }
                deltaRot.ToAngleAxis (out var xMag, out var x);
                x.Normalize ();
                xMag *= Mathf.Deg2Rad;
                Vector3 torque = x * (k.spring * xMag) + k.damper * deltaRotVelocity;
                Quaternion rotInertia2World = rigidbody.inertiaTensorRotation * currentRot;
                torque = Quaternion.Inverse(rotInertia2World) * torque;
                torque.Scale(rigidbody.inertiaTensor);
                torque = rotInertia2World * torque;
                return torque;
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="spring"></param>
            /// <param name="currentRot"></param>
            /// <param name="targetRot"></param>
            /// <param name="deltaRotVelocity"></param>
            /// <param name="rigidbody"></param>
            /// <returns>Torque Force</returns>
            public static Vector3 DampedTorsionalSpring(float dt, SpringSettings spring, Quaternion currentRot,
                Quaternion targetRot, Vector3 deltaRotVelocity, Rigidbody rigidbody)
            {
                var deltaRot = targetRot * Quaternion.Inverse(currentRot);
                var k = GetStableSpringConstants(dt, 1, spring.frequency, spring.damper, spring.useForce);

                // Q can be the-long-rotation-around-the-sphere eg. 350 degrees
                // We want the equivalent short rotation eg. -10 degrees
                // Check if rotation is greater than 190 degrees == q.w is negative
                if (deltaRot.w < 0)
                {
                    // Convert the quaternion final equivalent "short way around" quaternion
                    deltaRot.x = -deltaRot.x;
                    deltaRot.y = -deltaRot.y;
                    deltaRot.z = -deltaRot.z;
                    deltaRot.w = -deltaRot.w;
                }
                deltaRot.ToAngleAxis (out var xMag, out var x);
                x.Normalize ();
                xMag *= Mathf.Deg2Rad;
                Vector3 torque = x * (k.spring * xMag) + k.damper * deltaRotVelocity;
                Quaternion rotInertia2World = rigidbody.inertiaTensorRotation * currentRot;
                torque = Quaternion.Inverse(rotInertia2World) * torque;
                torque.Scale(rigidbody.inertiaTensor);
                torque = rotInertia2World * torque;
                return torque;
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="f"></param>
            /// <param name="d"></param>
            /// <param name="currentRot"></param>
            /// <param name="targetRot"></param>
            /// <param name="deltaRotVelocity"></param>
            /// <param name="useForce"></param>
            /// <returns>Torque Force</returns>
            public static Vector3 DampedTorsionalSpring(float dt, float f, float d, Quaternion currentRot,
                Quaternion targetRot, Vector3 deltaRotVelocity, bool useForce)
            {
                var deltaRot = targetRot * Quaternion.Inverse(currentRot);
                var k = GetStableSpringConstants(dt, 1, f, d, useForce);

                // Q can be the-long-rotation-around-the-sphere eg. 350 degrees
                // We want the equivalent short rotation eg. -10 degrees
                // Check if rotation is greater than 190 degrees == q.w is negative
                if (deltaRot.w < 0)
                {
                    // Convert the quaternion final equivalent "short way around" quaternion
                    deltaRot.x = -deltaRot.x;
                    deltaRot.y = -deltaRot.y;
                    deltaRot.z = -deltaRot.z;
                    deltaRot.w = -deltaRot.w;
                }
                deltaRot.ToAngleAxis (out var xMag, out var x);
                x.Normalize ();
                xMag *= Mathf.Deg2Rad;
                Vector3 torque = x * (k.spring * xMag) + k.damper * deltaRotVelocity;
                return torque;
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="spring"></param>
            /// <param name="currentRot"></param>
            /// <param name="targetRot"></param>
            /// <param name="deltaRotVelocity"></param>
            /// <returns>Torque Force</returns>
            public static Vector3 DampedTorsionalSpring(float dt, SpringSettings spring, Quaternion currentRot,
                Quaternion targetRot, Vector3 deltaRotVelocity)
            {
                var deltaRot = targetRot * Quaternion.Inverse(currentRot);
                var k = GetStableSpringConstants(dt, 1, spring.frequency, spring.damper, spring.useForce);

                // Q can be the-long-rotation-around-the-sphere eg. 350 degrees
                // We want the equivalent short rotation eg. -10 degrees
                // Check if rotation is greater than 190 degrees == q.w is negative
                if (deltaRot.w < 0)
                {
                    // Convert the quaternion final equivalent "short way around" quaternion
                    deltaRot.x = -deltaRot.x;
                    deltaRot.y = -deltaRot.y;
                    deltaRot.z = -deltaRot.z;
                    deltaRot.w = -deltaRot.w;
                }
                deltaRot.ToAngleAxis (out var xMag, out var x);
                x.Normalize ();
                xMag *= Mathf.Deg2Rad;
                Vector3 torque = x * (k.spring * xMag) + k.damper * deltaRotVelocity;
                return torque;
            }
            
            
        }
    }   
}
