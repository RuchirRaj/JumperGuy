using System.Collections.Generic;
using UnityEngine;

namespace RR.Common
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Gravity/Gravity Manager")]
    public class GravityManager : MonoBehaviour
    {
        public static GravityManager Instance { get; private set; }

        [field: SerializeField]
        public List<GravitySource> Sources { get; private set; } = new();


        public static GravityManager GetInstance()
        {
            if(Instance == null)
                CreateInstance();
            return Instance;
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        private static void CreateInstance()
        {
            var managerGo = new GameObject("Gravity Manager");
            DontDestroyOnLoad(managerGo);
            managerGo.AddComponent<GravityManager>();
        }

        public void RegisterSource(GravitySource source)
        {
            if(!Sources.Contains(source))
                Sources.Add(source);
        }

        public void DeRegisterSource(GravitySource source)
        {
            Sources.Remove(source);
        }

        public Vector3 GetGravityAtPos(Vector3 position, int mask)
        {
            var gravity = Vector3.zero;
            foreach (var source in Sources)
            {
                if(source.WithinBounds(position))
                    gravity += source.GetGravity(position, mask);
            }
            return gravity;
        }
    }   
}
