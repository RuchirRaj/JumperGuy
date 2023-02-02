using System;
using System.Collections.Generic;
using UnityEngine;

namespace RR.Common
{
    [SelectionBase]
    [AddComponentMenu("Ruchir/Gravity/Gravity Manager")]
    public class GravityManager : MonoBehaviour
    {
        [Flags] public enum GravityMask
        {
            Channel0 = 1 << 0,
            Channel1 = 1 << 2,
            Channel2 = 1 << 3,
            Channel3 = 1 << 4,
            Channel4 = 1 << 5,
            Channel5 = 1 << 6,
            Channel6 = 1 << 7,
            Channel7 = 1 << 8,
            Channel8 = 1 << 9,
            Channel9 = 1 << 10,
            Channel10 = 1 << 11,
            Channel11 = 1 << 12,
            Channel12 = 1 << 13,
            Channel13 = 1 << 14,
            Channel14 = 1 << 15,
            Channel15 = 1 << 16,
            Channel16 = 1 << 17,
            Channel17 = 1 << 18,
            Channel18 = 1 << 19,
            Channel19 = 1 << 20,
            Channel20 = 1 << 21,
            Channel21 = 1 << 22,
            Channel22 = 1 << 23,
            Channel23 = 1 << 24,
            Channel24 = 1 << 25,
            Channel25 = 1 << 26,
            Channel26 = 1 << 27,
            Channel27 = 1 << 28,
            Channel28 = 1 << 29,
            Channel29 = 1 << 30,
            Channel30 = 1 << 31
        }
        
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

        public Vector3 GetGravityAtPos(Vector3 position, GravityMask mask)
        {
            var gravity = Vector3.zero;
            var currentMaxPriority = 0;
            var validSources = 0;
            for (var i = 0; i < Sources.Count; i++)
            {
                var source = Sources[i];

                if (!source.ValidMask(mask)) continue;
                if (validSources == 0)
                    currentMaxPriority = source.priority;
                validSources++;
                if(source.priority < currentMaxPriority)
                    continue;
                if (!source.WithinBounds(position)) continue;
                if(source.priority == currentMaxPriority)
                    gravity += source.GetGravity(position, mask);
                else
                {
                    gravity = source.GetGravity(position, mask);
                    currentMaxPriority = source.priority;
                }
            }

            return gravity;
        }
    }   
}
