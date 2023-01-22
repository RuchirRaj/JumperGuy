#if !DEBUG
#define PERFORMANCE_MODE
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

// ReSharper disable once CheckNamespace
namespace RR.UpdateSystem
{

    [Serializable]
    public struct UpdateMethod
    {
        public bool autoUpdate;

        [SerializeField]
        public bool slicedUpdate;

        [SerializeField] [Range(0, UpdateManager.BucketCount - 1)]
        public int bucketCount;
    }

    public static class UpdateUtils
    {
        /// <summary>
        ///     Register this class to be update according to updateMethod provided
        /// </summary>
        /// <param name="update"></param>
        /// <param name="method"></param>
        public static void RegisterFixedUpdate(this IBatchFixedUpdate update, UpdateMethod method)
        {
            if(!method.autoUpdate) return;
            UpdateManager.VerifyInstance();
            if(method.slicedUpdate)
                UpdateManager.Instance.RegisterFixedUpdateSliced(update, method.bucketCount);
            else
                UpdateManager.Instance.RegisterFixedUpdate(update);
        }
        
        /// <summary>
        ///     Register this class to be update according to updateMethod provided
        /// </summary>
        /// <param name="update"></param>
        public static void DeregisterFixedUpdate(this IBatchFixedUpdate update)
        {
            if(UpdateManager.Instance)
                UpdateManager.Instance.DeregisterFixedUpdate(update);
        }
        
        /// <summary>
        ///     Register this class to be update according to updateMethod provided
        /// </summary>
        /// <param name="update"></param>
        /// <param name="method"></param>
        public static void RegisterUpdate(this IBatchUpdate update, UpdateMethod method)
        {
            if(!method.autoUpdate) return;
            UpdateManager.VerifyInstance();
            if(method.slicedUpdate)
                UpdateManager.Instance.RegisterUpdateSliced(update, method.bucketCount);
            else
                UpdateManager.Instance.RegisterUpdate(update);
        }
        
        /// <summary>
        ///     Register this class to be update according to updateMethod provided
        /// </summary>
        /// <param name="update"></param>
        public static void DeregisterUpdate(this IBatchUpdate update)
        {
            if(UpdateManager.Instance)
                UpdateManager.Instance.DeregisterUpdate(update);
        }
        
        /// <summary>
        ///     Register this class to be update according to updateMethod provided
        /// </summary>
        /// <param name="update"></param>
        /// <param name="method"></param>
        public static void RegisterLateUpdate(this IBatchLateUpdate update, UpdateMethod method)
        {
            if(!method.autoUpdate) return;
            UpdateManager.VerifyInstance();
            if(method.slicedUpdate)
                UpdateManager.Instance.RegisterLateUpdateSliced(update, method.bucketCount);
            else
                UpdateManager.Instance.RegisterLateUpdate(update);
        }
        
        /// <summary>
        ///     Register this class to be update according to updateMethod provided
        /// </summary>
        /// <param name="update"></param>
        public static void DeregisterLateUpdate(this IBatchLateUpdate update)
        {
            if(UpdateManager.Instance)
                UpdateManager.Instance.DeregisterLateUpdate(update);
        }
    }
    
    public interface IBatchUpdate
    {
        void BatchUpdate(float dt, float sdt);
    }

    public interface IBatchLateUpdate
    {
        void BatchLateUpdate(float dt, float sdt);
    }

    public interface IBatchFixedUpdate
    {
        void BatchFixedUpdate(float dt, float sdt);
    }

    public class UpdateManager : MonoBehaviour
    {
        public const int BucketCount = 3;
        public static UpdateManager Instance { get; private set; }

        private readonly float[] _fixedUpdateDT = new float[BucketCount];
        private readonly float[] _updateDT = new float[BucketCount ];

#if PERFORMANCE_MODE
        private readonly HashSet<IBatchUpdate>[] _slicedUpdateBehavioursBuckets =
 new HashSet<IBatchUpdate>[BucketCount + 1];
        private readonly HashSet<IBatchLateUpdate>[] _slicedLateUpdateBehavioursBuckets =
 new HashSet<IBatchLateUpdate>[BucketCount + 1];
        private readonly HashSet<IBatchFixedUpdate>[] _slicedFixedUpdateBehavioursBuckets =
 new HashSet<IBatchFixedUpdate>[BucketCount + 1];
#else
        private readonly Dictionary<Type, HashSet<IBatchUpdate>>[] _slicedUpdateBehavioursBuckets =
            new Dictionary<Type, HashSet<IBatchUpdate>>[BucketCount + 1];

        private readonly Dictionary<Type, HashSet<IBatchLateUpdate>>[] _slicedLateUpdateBehavioursBuckets =
            new Dictionary<Type, HashSet<IBatchLateUpdate>>[BucketCount + 1];

        private readonly Dictionary<Type, HashSet<IBatchFixedUpdate>>[] _slicedFixedUpdateBehavioursBuckets =
            new Dictionary<Type, HashSet<IBatchFixedUpdate>>[BucketCount + 1];
#endif

        private int _currentUpdateAndLateUpdateBucket;
        private int _currentFixedUpdateBucket;

        #region Update

        public void RegisterUpdate(IBatchUpdate batchUpdateBehaviour)
        {
            RegisterUpdate_Internal(batchUpdateBehaviour, BucketCount);
        }

        public void RegisterUpdateSliced(IBatchUpdate batchUpdateBehaviour, int bucketNumber)
        {
            if (IsValidBucketNumber(bucketNumber)) RegisterUpdate_Internal(batchUpdateBehaviour, bucketNumber);
        }

        public void DeregisterUpdate(IBatchUpdate batchUpdateBehaviour)
        {
            DeregisterUpdate_Internal(batchUpdateBehaviour);
        }

        #endregion

        #region LateUpdate

        public void RegisterLateUpdate(IBatchLateUpdate batchLateUpdateBehaviour)
        {
            RegisterBatchLateUpdate_Internal(batchLateUpdateBehaviour, BucketCount);
        }

        public void RegisterLateUpdateSliced(IBatchLateUpdate batchLateUpdateBehaviour, int bucketNumber)
        {
            if (IsValidBucketNumber(bucketNumber))
                RegisterBatchLateUpdate_Internal(batchLateUpdateBehaviour, bucketNumber);
        }

        public void DeregisterLateUpdate(IBatchLateUpdate batchLateUpdateBehaviour)
        {
            DeregisterBatchLateUpdate_Internal(batchLateUpdateBehaviour);
        }

        #endregion

        #region FixedUpdate

        public void RegisterFixedUpdate(IBatchFixedUpdate batchFixedUpdateBehaviour)
        {
            RegisterBatchFixedUpdate_Internal(batchFixedUpdateBehaviour, BucketCount);
        }

        public void RegisterFixedUpdateSliced(IBatchFixedUpdate batchFixedUpdateBehaviour, int bucketNumber)
        {
            if (IsValidBucketNumber(bucketNumber))
                RegisterBatchFixedUpdate_Internal(batchFixedUpdateBehaviour, BucketCount);
        }

        public void DeregisterFixedUpdate(IBatchFixedUpdate batchFixedUpdateBehaviour)
        {
            DeregisterBatchFixedUpdate_Internal(batchFixedUpdateBehaviour);
        }

        #endregion

        public static void VerifyInstance()
        {
            if(Instance != null)
                return;
            GameObject updateManager = new GameObject("Update Manager");
            DontDestroyOnLoad(updateManager);
            Instance = updateManager.AddComponent<UpdateManager>();
        }
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            for (var i = 0; i < BucketCount + 1; i++)
            {
#if PERFORMANCE_MODE
            _slicedUpdateBehavioursBuckets[i] = new HashSet<IBatchUpdate>();
            _slicedLateUpdateBehavioursBuckets[i] = new HashSet<IBatchLateUpdate>();
            _slicedFixedUpdateBehavioursBuckets[i] = new HashSet<IBatchFixedUpdate>();
#else
                _slicedUpdateBehavioursBuckets[i] = new Dictionary<Type, HashSet<IBatchUpdate>>();
                _slicedLateUpdateBehavioursBuckets[i] = new Dictionary<Type, HashSet<IBatchLateUpdate>>();
                _slicedFixedUpdateBehavioursBuckets[i] = new Dictionary<Type, HashSet<IBatchFixedUpdate>>();
#endif
            }
        }

        private void Update()
        {
            for (var i = 0; i < BucketCount; i++)
            {
                _updateDT[i] += Time.deltaTime;
            }
#if PERFORMANCE_MODE
        foreach (var batchUpdateBehaviour in _slicedUpdateBehavioursBuckets[_currentUpdateAndLateUpdateBucket]) batchUpdateBehaviour.BatchUpdate(_updateDT[_currentUpdateAndLateUpdateBucket], Time.smoothDeltaTime);
        foreach (var batchUpdateBehaviour in _slicedUpdateBehavioursBuckets[BucketCount]) batchUpdateBehaviour.BatchUpdate(Time.deltaTime, Time.smoothDeltaTime);
#else
            foreach (var keyValue in _slicedUpdateBehavioursBuckets[_currentUpdateAndLateUpdateBucket])
            {
                Profiler.BeginSample(keyValue.Key.Name);
                foreach (var behaviour in keyValue.Value) behaviour.BatchUpdate(_updateDT[_currentUpdateAndLateUpdateBucket], Time.smoothDeltaTime);
                Profiler.EndSample();
            }

            foreach (var keyValue in _slicedUpdateBehavioursBuckets[BucketCount])
            {
                Profiler.BeginSample(keyValue.Key.Name);
                foreach (var behaviour in keyValue.Value) behaviour.BatchUpdate(Time.deltaTime, Time.smoothDeltaTime);
                Profiler.EndSample();
            }
#endif
        }

        private void LateUpdate()
        {
#if PERFORMANCE_MODE
        foreach (var batchLateUpdateBehaviour in _slicedLateUpdateBehavioursBuckets[_currentUpdateAndLateUpdateBucket]) batchLateUpdateBehaviour.BatchLateUpdate(_updateDT[_currentUpdateAndLateUpdateBucket], Time.smoothDeltaTime);
        foreach (var batchLateUpdateBehaviour in _slicedLateUpdateBehavioursBuckets[BucketCount]) batchLateUpdateBehaviour.BatchLateUpdate(Time.deltaTime, Time.smoothDeltaTime);
#else
            foreach (var keyValue in _slicedLateUpdateBehavioursBuckets[_currentUpdateAndLateUpdateBucket])
            {
                Profiler.BeginSample(keyValue.Key.Name);
                foreach (var behaviour in keyValue.Value) behaviour.BatchLateUpdate(_updateDT[_currentUpdateAndLateUpdateBucket], Time.smoothDeltaTime);
                Profiler.EndSample();
            }

            foreach (var keyValue in _slicedLateUpdateBehavioursBuckets[BucketCount])
            {
                Profiler.BeginSample(keyValue.Key.Name);
                foreach (var behaviour in keyValue.Value) behaviour.BatchLateUpdate(Time.deltaTime, Time.smoothDeltaTime);
                Profiler.EndSample();
            }
#endif
            _updateDT[_currentUpdateAndLateUpdateBucket] = 0;
            _currentUpdateAndLateUpdateBucket = (_currentUpdateAndLateUpdateBucket + 1) % BucketCount;
        }

        private void FixedUpdate()
        {
            for (var i = 0; i < BucketCount; i++)
            {
                _fixedUpdateDT[i] += Time.fixedDeltaTime;
            }
#if PERFORMANCE_MODE
        foreach (var batchFixedUpdateBehaviour in _slicedFixedUpdateBehavioursBuckets[_currentFixedUpdateBucket]) batchFixedUpdateBehaviour.BatchFixedUpdate(_fixedUpdateDT[_currentFixedUpdateBucket], Time.smoothDeltaTime);
        foreach (var batchFixedUpdateBehaviour in _slicedFixedUpdateBehavioursBuckets[BucketCount]) batchFixedUpdateBehaviour.BatchFixedUpdate(Time.fixedDeltaTime, Time.smoothDeltaTime);
#else
            foreach (var keyValue in _slicedFixedUpdateBehavioursBuckets[_currentFixedUpdateBucket])
            {
                Profiler.BeginSample(keyValue.Key.Name);
                foreach (var behaviour in keyValue.Value) behaviour.BatchFixedUpdate(_fixedUpdateDT[_currentFixedUpdateBucket], Time.smoothDeltaTime);
                Profiler.EndSample();
            }

            foreach (var keyValue in _slicedFixedUpdateBehavioursBuckets[BucketCount])
            {
                Profiler.BeginSample(keyValue.Key.Name);
                foreach (var behaviour in keyValue.Value) behaviour.BatchFixedUpdate(Time.fixedDeltaTime, Time.smoothDeltaTime);
                Profiler.EndSample();
            }
#endif
            _fixedUpdateDT[_currentFixedUpdateBucket] = 0;
            _currentFixedUpdateBucket = (_currentFixedUpdateBucket + 1) % BucketCount;
        }

        private static bool IsValidBucketNumber(int bucketNumber)
        {
            var isValidBucketNumber = bucketNumber is >= 0 and < BucketCount;
            Assert.IsTrue(isValidBucketNumber, $"Bucket must be between 0 and {BucketCount - 1}");
            return true;
        }

        private void RegisterUpdate_Internal(IBatchUpdate batchUpdateBehaviour, int bucketNumber)
        {
#if PERFORMANCE_MODE
        _slicedUpdateBehavioursBuckets[bucketNumber].Add(batchUpdateBehaviour);
#else
            var type = batchUpdateBehaviour.GetType();
            var targetBucket = _slicedUpdateBehavioursBuckets[bucketNumber];
            if (targetBucket.ContainsKey(type) == false) targetBucket[type] = new HashSet<IBatchUpdate>();
            targetBucket[type].Add(batchUpdateBehaviour);
#endif
        }

        private void RegisterBatchLateUpdate_Internal(IBatchLateUpdate batchLateUpdateBehaviour, int bucketNumber)
        {
#if PERFORMANCE_MODE
        _slicedLateUpdateBehavioursBuckets[bucketNumber].Add(batchLateUpdateBehaviour);
#else
            var type = batchLateUpdateBehaviour.GetType();
            var targetBucket = _slicedLateUpdateBehavioursBuckets[bucketNumber];
            if (targetBucket.ContainsKey(type) == false) targetBucket[type] = new HashSet<IBatchLateUpdate>();
            targetBucket[type].Add(batchLateUpdateBehaviour);
#endif
        }

        private void RegisterBatchFixedUpdate_Internal(IBatchFixedUpdate batchFixedUpdateBehaviour, int bucketNumber)
        {
#if PERFORMANCE_MODE
        _slicedFixedUpdateBehavioursBuckets[bucketNumber].Add(batchFixedUpdateBehaviour);
#else
            var type = batchFixedUpdateBehaviour.GetType();
            var targetBucket = _slicedFixedUpdateBehavioursBuckets[bucketNumber];
            if (targetBucket.ContainsKey(type) == false) targetBucket[type] = new HashSet<IBatchFixedUpdate>();
            targetBucket[type].Add(batchFixedUpdateBehaviour);
#endif
        }

        private void DeregisterUpdate_Internal(IBatchUpdate batchUpdateBehaviour)
        {
#if PERFORMANCE_MODE
        foreach (var batchUpdateBehavioursBucket in _slicedUpdateBehavioursBuckets)
        {
            batchUpdateBehavioursBucket.Remove(batchUpdateBehaviour);
        }
#else
            foreach (var keyValue in _slicedUpdateBehavioursBuckets)
            {
                var type = batchUpdateBehaviour.GetType();
                if (keyValue.ContainsKey(type)) keyValue[type].Remove(batchUpdateBehaviour);
            }
#endif
        }

        private void DeregisterBatchLateUpdate_Internal(IBatchLateUpdate batchLateUpdateBehaviour)
        {
#if PERFORMANCE_MODE
        foreach (var batchLateUpdateBehaviourBucket in _slicedLateUpdateBehavioursBuckets)
        {
            batchLateUpdateBehaviourBucket.Remove(batchLateUpdateBehaviour);
        }
#else
            foreach (var keyValue in _slicedLateUpdateBehavioursBuckets)
            {
                var type = batchLateUpdateBehaviour.GetType();
                if (keyValue.ContainsKey(type)) keyValue[type].Remove(batchLateUpdateBehaviour);
            }
#endif
        }

        private void DeregisterBatchFixedUpdate_Internal(IBatchFixedUpdate batchFixedUpdateBehaviour)
        {
#if PERFORMANCE_MODE
        foreach (var batchFixedUpdateBehaviourBucket in _slicedFixedUpdateBehavioursBuckets)
        {
            batchFixedUpdateBehaviourBucket.Remove(batchFixedUpdateBehaviour);
        }
#else
            foreach (var keyValue in _slicedFixedUpdateBehavioursBuckets)
            {
                var type = batchFixedUpdateBehaviour.GetType();
                if (keyValue.ContainsKey(type)) keyValue[type].Remove(batchFixedUpdateBehaviour);
            }
#endif
        }
    }
}