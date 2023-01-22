using System;
using UnityEngine;

namespace RR.Utils
{
    /// <summary>
    ///     Stores RaycastHit data for faster access
    /// </summary>
    [Serializable]
    public struct RayHitInfo
    {
        public Collider col;
        public Vector3 origin, point, normal;
        public float distance;
        public bool valid;
        private RaycastHit _hit;

        public RayHitInfo(RaycastHit hit)
        {
            _hit = hit;
            col = hit.collider;
            point = hit.point;
            normal = hit.normal;
            distance = hit.distance;
            origin = Vector3.zero;
            valid = col != null;
        }

        public static explicit operator RayHitInfo(RaycastHit hit)
        {
            return new RayHitInfo(hit);
        }

        /// <summary>
        ///     Reset the Data stored in the struct
        /// </summary>
        /// <returns>Reset struct</returns>
        public RayHitInfo Reset()
        {
            _hit = new RaycastHit();
            col = null;
            point = Vector3.zero;
            normal = Vector3.zero;
            distance = 0;
            origin = Vector3.zero;
            valid = false;
            return this;
        }
    }
}