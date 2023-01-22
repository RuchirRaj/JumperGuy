using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RR.StateSystem
{
    [Serializable]
    public class State<T> where T : ScriptableObject, IState<T>, new()
    {
        
        [SerializeField] internal bool enable;
        [SerializeField] internal bool blocked;
        [SerializeField] private bool forceEnable;
        [SerializeField] internal string name;
        [SerializeReference] internal T value;
        public List<string> blockedBy = new();
        //Internal
        [SerializeField] private bool partOfStateList; 
        
        //Events
        public delegate void StateActivationEvent(State<T> state, bool value);
        public event StateActivationEvent EnableEvent;
        public event StateActivationEvent ActivationEvent;

        //Properties
        public bool Active => (forceEnable || !blocked) && enable;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public T Value
        {
            get => value;
            set => this.value = value;
        }

        public bool ForceEnable
        {
            get => forceEnable;
            set => forceEnable = value;
        }

        public bool IsBlocked => blocked;

        public bool PartOfStateList
        {
            get => partOfStateList;
            set => partOfStateList = value;
        }

        #region Editor

        private void OnEnableChange(bool e)
        {
            EnableEvent?.Invoke(this, e);
            enable = e;
            Debug.Log(e);
        }

        private Color GetToggleColor() => value == null ? StateColor.EmptyStateColor : StateColor.ValidStateColor;
        
        private Color GetBlockedColor() => StateColor.BlockingStateColor;

        private Color GetNameColor() => Active ? StateColor.EnabledColor :
            blocked ? enable ? StateColor.BlockedColor : StateColor.BlockedDisabledColor : StateColor.DisabledColor;

        #endregion
        
        public void SetStateActive(bool e)
        {
            if (e != enable) EnableEvent?.Invoke(this, e);
            var act = Active;
            enable = e;
            
            if(act != Active)
                ActivationEvent?.Invoke(this, !act);
        }

        public void BlockState(bool b)
        {
            var act = Active;
            blocked = b;
            
            if(act != Active)
                ActivationEvent?.Invoke(this, !act);
        }
        
        public void CreateNew()
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(name))
            {
                EditorUtility.DisplayDialog("Unnamed State", "Assign a name to state before proceeding", "Ok");
                return;
            }
#endif
            Debug.Log($"Created \"{name}\" State");
            value = ScriptableObject.CreateInstance<T>();
            value.name = name;
        }

        public void SaveToAsset()
        {
#if UNITY_EDITOR
            if (value == null)
            {
                EditorUtility.DisplayDialog("Empty State", "Create a new state to save it", "Ok");
                return;
            }
            var path = EditorUtility.SaveFilePanel(
                "Save State as Asset",
                Application.dataPath,
                value.name + ".asset",
                "asset");
            if (path.StartsWith(Application.dataPath)) {
                path =  "Assets" + path.Substring(Application.dataPath.Length);
            }
            else
            {
                Debug.LogWarning("The path must be in assets folder");
                return;
            }
            AssetDatabase.CreateAsset(value, path);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}
