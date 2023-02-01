

using UnityEngine;

namespace RR.Utils
{
    public static class ExtensionMethods {
 
        public static float Remap (this float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        public static Vector2 Remap (this Vector2 value, Vector2 from1, Vector2 to1, Vector2 from2, Vector2 to2) {
            return new(
                (value.x - from1.x) / (to1.x - from1.x) * (to2.x - from2.x) + from2.x,
                (value.y - from1.y) / (to1.y - from1.y) * (to2.y - from2.y) + from2.y
                );
        }
        public static Vector3 Remap (this Vector3 value, Vector3 from1, Vector3 to1, Vector3 from2, Vector3 to2) {
            return new(
                (value.x - from1.x) / (to1.x - from1.x) * (to2.x - from2.x) + from2.x,
                (value.y - from1.y) / (to1.y - from1.y) * (to2.y - from2.y) + from2.y,
                (value.z - from1.z) / (to1.z - from1.z) * (to2.z - from2.z) + from2.z
            );
        }
        public static Vector4 Remap (this Vector4 value, Vector4 from1, Vector4 to1, Vector4 from2, Vector4 to2) {
            return new(
                (value.x - from1.x) / (to1.x - from1.x) * (to2.x - from2.x) + from2.x,
                (value.y - from1.y) / (to1.y - from1.y) * (to2.y - from2.y) + from2.y,
                (value.z - from1.z) / (to1.z - from1.z) * (to2.z - from2.z) + from2.z,
                (value.w - from1.w) / (to1.w - from1.w) * (to2.w - from2.w) + from2.w
            );
        }
    }
}