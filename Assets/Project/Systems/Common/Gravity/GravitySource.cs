using System;
using Drawing;
using UnityEngine;

namespace RR.Common
{
    /// <summary>
    /// Base class for all gravity Sources
    /// </summary>
    public abstract class GravitySource : MonoBehaviourGizmos
    {
        private void OnEnable()
        {
            GravityManager.GetInstance().RegisterSource(this);
        }

        private void OnDisable()
        {
            if(GravityManager.Instance)
                GravityManager.Instance.DeRegisterSource(this);
        }

        /// <summary>
        /// Get gravity at a position (it returns Vector3.zero if out of bounds
        /// </summary>
        /// <param name="position"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public virtual Vector3 GetGravity(Vector3 position, int mask)
        {
            Vector3 g = default;
            return g;
        }

        /// <summary>
        /// Is the position within bounds of this Gravity Source
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>"True" if the position is within bounds</returns>
        public abstract bool WithinBounds(Vector3 pos);
    }
}