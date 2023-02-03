using System;

namespace RR.Utils
{
    [Serializable]
    public struct SpringSettings
    {
        public bool useForce;
        public float frequency;
        public float damper;

        public SpringSettings(bool b = false, float f = 10, float d = 0.75f)
        {
            useForce = b;
            frequency = f;
            damper = d;
        }

        public override string ToString()
        {
            return $"Force: {useForce.ToString()}, Frequency: {frequency}, Damper: {damper}";
        }
    }
}