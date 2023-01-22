using System;
using UnityEngine;

namespace RR.StateSystem
{
    [Serializable]
    public class StateVariable<T>
    {
        [SerializeField]
        public bool enable;
        [SerializeField]
        public T value;
        
        public static implicit operator T(StateVariable<T> value)
        {
            return value.value;
        }

        public override string ToString()
        {
            return $"Enable: {enable.ToString()}, Value: ({value.ToString()})";
        }
        /// <summary>
        /// Return state value based on whether it's enabled
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public T GetValue(T old)
        {
            return enable ? value : old;
        }

        /// <summary>
        /// Return state value based on whether it's enabled
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public void Apply(StateVariable<T> newValue)
        {
            enable |= newValue.enable;
            value = newValue.enable ? newValue : value;
        }

        /// <summary>
        /// Disable Enabled state
        /// </summary>
        public void Disable() => enable = false;
    }
}