using System;
using Cinemachine.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Gameplay.CharacterController.Camera
{
    [Serializable]
    public class Axis
    {
        [Min(0)] public float maxSpeed = 200;
        public float gain = 1;
        [MinMaxSlider("@range", true)] public Vector2 clamp;
        public bool wrap;
        public float accelTime = 0.2f, decelTime = 0.2f;
        [Header("Recenter")] public bool recenter;
        public float recenterWait = 2;
        public float recenterTime = 2;
        public float recenterValue;
        [Space] public float inputValue;
        public float outValue;

        [SerializeField] [EnableIf("@false")] public Vector2 range = new(-180, 180);

        /// Internal state
        private float m_CurrentSpeed;

        private const float k_Epsilon = UnityVectorExtensions.Epsilon;
        private float m_LastUpdateTime;
        private float m_RecenteringVelocity;
        private bool m_ForceRecenter;

        public float ClampValue(float value)
        {
            if (!wrap)
                return Mathf.Clamp(value, clamp.x, clamp.y);

            /*
            while (value < clamp.x) value = clamp.y + value - clamp.x;

            while (value > clamp.y) value = clamp.x + value - clamp.y;
            */
            
            var x = value;
            if (x < clamp.x || x > clamp.y)
            {
                x -= clamp.x;
                x %= (clamp.y - clamp.x);
                x += clamp.x;
            }

            value = x;
            return value;
        }

        public void SetInput(float value)
        {
            inputValue = value * gain;
        }

        /// <summary>Apply the input value to the axis value</summary>
        /// <param name="dt">current deltaTime</param>
        /// <param name="axis">The InputAxisValue to update</param>
        /// <param name="control">Parameter for controlling the behaviour of the axis</param>
        public void ProcessInput(float dt)
        {
            var input = inputValue;
            if (dt < 0)
            {
                m_CurrentSpeed = 0;
            }
            else
            {
                var dampTime = Mathf.Abs(input) < Mathf.Abs(m_CurrentSpeed) ? decelTime : accelTime;
                m_CurrentSpeed += Damper.Damp(input - m_CurrentSpeed, dampTime, dt);

                // Decelerate to the end points of the range if not wrapping
                var range = clamp.y - clamp.x;
                if (!wrap && decelTime > k_Epsilon && range > k_Epsilon)
                {
                    var v0 = ClampValue(outValue);
                    var v = ClampValue(v0 + m_CurrentSpeed * dt);
                    var d = m_CurrentSpeed > 0 ? clamp.y - v : v - clamp.x;
                    if (d < 0.1f * range && Mathf.Abs(m_CurrentSpeed) > k_Epsilon)
                        m_CurrentSpeed = Damper.Damp(v - v0, decelTime, dt) / dt;
                }
            }

            m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed, -maxSpeed, maxSpeed);
            outValue = ClampValue(outValue + m_CurrentSpeed * dt);

            if (Mathf.Abs(inputValue) > k_Epsilon)
                CancelRecentering();
            else
                DoRecentering(dt);
        }

        /// <summary>Call this to manage recentering axis value to axis center.</summary>
        /// <param name="deltaTime">Current deltaTime</param>
        /// <param name="axis">The axis to recenter</param>
        /// <param name="recentering">The recentering settings</param>
        public void DoRecentering(float deltaTime)
        {
            if (m_ForceRecenter ||
                (recenter
                 && CurrentTime - m_LastUpdateTime > recenterWait))
            {
                var v = ClampValue(outValue);
                var c = ClampValue(recenterValue);
                var distance = Mathf.Abs(c - v);
                if (distance < k_Epsilon || recenterTime < k_Epsilon)
                {
                    v = c;
                }
                else
                {
                    // Determine the direction
                    var r = clamp.y - clamp.x;
                    if (wrap && distance > r * 0.5f)
                        v += Mathf.Sign(c - v) * r;

                    // Damp our way there
                    v = Mathf.SmoothDamp(
                        v, c, ref m_RecenteringVelocity,
                        recenterTime * 0.5f, 9999, deltaTime);
                }

                outValue = ClampValue(v);

                // Are we there yet?
                if (Mathf.Abs(outValue - c) < k_Epsilon)
                    m_ForceRecenter = false;
            }
        }

        /// <summary>Cancel any current recentering in progress, and reset the wait time</summary>
        public void RecenterNow()
        {
            m_ForceRecenter = true;
        }

        /// <summary>Cancel any current recentering in progress, and reset the wait time</summary>
        public void CancelRecentering()
        {
            m_LastUpdateTime = CurrentTime;
            m_RecenteringVelocity = 0;
            m_ForceRecenter = false;
        }

        /// <summary>Reset axis to at-rest state</summary>
        /// <param name="axis">The axis to reset</param>
        /// <param name="recentering">The recentering settings</param>
        public void Reset()
        {
            m_LastUpdateTime = CurrentTime;
            m_CurrentSpeed = 0;
            m_RecenteringVelocity = 0;
            if (recenter)
                outValue = ClampValue(recenterValue);
            m_ForceRecenter = false;
        }

        private static float CurrentTime => Time.unscaledTime;
    }
}