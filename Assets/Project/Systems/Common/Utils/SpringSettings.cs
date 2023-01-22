using System;

namespace RR.Utils
{
    [Serializable]
    public class SpringSettings
    {
        public bool useForce;
        public float frequency = 10;
        public float damper = 0.75f;

        public override string ToString()
        {
            return $"Force: {useForce.ToString()}, Frequency: {frequency}, Damper: {damper}";
        }
    }
}