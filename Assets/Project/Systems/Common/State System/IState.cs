using UnityEngine;

namespace RR.StateSystem
{
    public interface IState<T> where T : ScriptableObject, new()
    {
        public void Apply(T state);
        public void Reset();
    }
}