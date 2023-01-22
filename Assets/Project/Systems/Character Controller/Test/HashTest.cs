using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Systems.Character_Controller.Test
{
    public class HashTest : MonoBehaviour
    {
        public int hashOut;
        
        [Button]
        public int HashString(string text)
        {
            // TODO: Determine nullity policy.

            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }

                hashOut = hash;
                return hash;
            }
        }
    }
}