using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace RR.StateSystem
{
    [Serializable]
    public class StateList<T> where T : ScriptableObject, IState<T>, new()
    {
        [SerializeField]
        public List<State<T>> states = new();
        public Dictionary<string, int> IndexMap = new();

        public T GetState(string name)
        {
            return IndexMap.ContainsKey(name) ? states[IndexMap[name]].value : null;
        }
        
        public T GetState(int index)
        {
            return index < states.Count ? states[index].value : null;
        }
        
        public T ApplyState(T state)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if(states[i].Active && states[i].value != null)
                    state.Apply(states[i].value);
            }
            return state;
        }

        public void RemoveState(int index)
        {
            Odin_RemoveIndex(index);
        }

        public void ValidateStates()
        {
            Odin_ValidateStateList();
        }
        
        /// <summary>
        /// Add a state to state list
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Index of added state</returns>
        public int AddState(State<T> state)
        {
            //Rename
            if (IndexMap.ContainsKey(state.name))
            {
                string name;
                var i = states.Count;
                do
                {
                    i++;
                    name = $"New State {states.Count + i}";
                } while (IndexMap.ContainsKey(name));

                state.name = name;
            }
            IndexMap.Add(state.name, states.Count);
            states.Add(state);
            return states.Count - 1;
        }

        public void ReOrderState(int oldIndex, int newIndex)
        {
            if(oldIndex >= states.Count || newIndex >= states.Count || oldIndex < 0 || newIndex < 0 || oldIndex == newIndex)
                return;
            (states[oldIndex], states[newIndex]) = (states[newIndex], states[oldIndex]);
            ResetIndexMap();
        }
        
        public bool EnableState(string name)
        {
            if (!IndexMap.ContainsKey(name))
                return false;
            var state = states[IndexMap[name]];
            state.SetStateActive(true);
            
            {
                //Update blocked state
                foreach (var blockedby in state.blockedBy)
                {
                    if (states[IndexMap[blockedby]].enable)
                    {
                        state.blocked = true;
                        break;
                    }
                }
                
                if(state.enable)
                    for (var index = 0; index < states.Count; index++)
                    {
                        if(index == IndexMap[name])
                            continue;
                        var s = states[index];
                        if (s.blockedBy.Contains(state.name))
                            s.blocked = true;
                    }
            }

            return state.Active;
        } 
        
        public void DisableState(string name)
        {
            if (!IndexMap.ContainsKey(name))
                return;
            var state = states[IndexMap[name]];
            var index = IndexMap[name];

            {
                //Check if other states blocked by this state should still be blocked
                if(state.enable)
                    for (var i = 0; i < states.Count; i++)
                    {
                        if(i == index)
                            continue;
                        
                        var s = states[i];
                        
                        if (s.blockedBy.Contains(state.name))
                        {
                            bool shouldBlock = false;
                            foreach (var bs in s.blockedBy)
                            {
                                if(states[IndexMap[bs]].name == state.name)
                                    continue;
                                if(states[IndexMap[bs]].enable)
                                {
                                    shouldBlock = true;
                                    break;
                                }
                            }

                            s.blocked = shouldBlock;
                        }
                    }
            }
            state.SetStateActive(false);

        }
        
        public bool EnableState(int index)
        {
            if (index < 0 || index > states.Count)
                return false;
            var state = states[index];
            state.SetStateActive(true);
            
            {
                //Update blocked state
                foreach (var blockedby in state.blockedBy)
                {
                    if (states[IndexMap[blockedby]].enable)
                    {
                        state.blocked = true;
                        break;
                    }
                }
                
                if(state.enable)
                    for (var i = 0; i < states.Count; i++)
                    {
                        if(i == index)
                            continue;
                        var s = states[i];
                        if (s.blockedBy.Contains(state.name))
                            s.blocked = true;
                    }
            }
            return state.Active;
        }

        public void DisableState(int index)
        {
            if (index < 0 || index > states.Count)
                return;
            var state = states[index];

            {
                //Check if other states blocked by this state should still be blocked
                if(state.enable)
                    for (var i = 0; i < states.Count; i++)
                    {
                        if(i == index)
                            continue;
                        
                        var s = states[i];
                        
                        if (s.blockedBy.Contains(state.name))
                        {
                            bool shouldBlock = false;
                            foreach (var bs in s.blockedBy)
                            {
                                if(states[IndexMap[bs]].name == state.name)
                                    continue;
                                if(states[IndexMap[bs]].enable)
                                {
                                    shouldBlock = true;
                                    break;
                                }
                            }

                            s.blocked = shouldBlock;
                        }
                    }
            }
            state.SetStateActive(false);

        }
        
        public (bool succes, string name) RenameState(string name, string newName)
        {
            if(!IndexMap.ContainsKey(name) || IndexMap.ContainsKey(newName)|| string.IsNullOrWhiteSpace(newName))
                return (false, name);

            if (String.Compare(name, newName, StringComparison.Ordinal) == 0)
            {
                return (true, name);
            }

            var index = IndexMap[name];
            var state = states[index];
            IndexMap.Remove(state.name);
            IndexMap.Add(newName, index);

            //Update blocked state names
            for (int i = 0; i < states.Count; i++)
            {
                if(i == index)
                    continue;
                for (int j = 0; j < states[i].blockedBy.Count; j++)
                {
                    if (String.Compare(name, states[i].blockedBy[j], StringComparison.Ordinal) == 0)
                        states[i].blockedBy[j] = newName;
                }
            }
            
            state.name = newName;
            return (true, newName);
        }
        
        public bool RenameState(int index, string newName)
        {
            if((index < 0 || index > states.Count) || IndexMap.ContainsKey(newName)|| string.IsNullOrWhiteSpace(newName))
                return false;

            if (String.Compare(states[index].name, newName, StringComparison.Ordinal) == 0)
            {
                return true;
            }
            
            var state = states[index];
            IndexMap.Remove(state.name);
            IndexMap.Add(newName, index);

            //Update blocked state names
            for (int i = 0; i < states.Count; i++)
            {
                if(i == index)
                    continue;
                for (int j = 0; j < states[i].blockedBy.Count; j++)
                {
                    if (String.Compare(states[index].name, states[i].blockedBy[j], StringComparison.Ordinal) == 0)
                        states[i].blockedBy[j] = newName;
                }
            }
            
            state.name = newName;
            return true;
        }

        public void ResetIndexMap()
        {
            IndexMap ??= new();
            IndexMap.Clear();
            for (int i = 0; i < states.Count; i++)
            {
                IndexMap.Add(states[i].name, i);
            }
        }
        
        #region Editor

        private Color Odin_RenameButtonColor() => StateColor.RenameButtonColor;
        
#if UNITY_EDITOR
        private void Odin_ElementStartGUI(int index, InspectorProperty property)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            var c = GUI.color;
            //GUILayout.Label(index.ToString());
            GUI.color = states[index].enable ? new Color(1f, 0.89f, 0.74f) : new Color(0.75f, 0.91f, 1f);
            EditorGUI.BeginChangeCheck();
            if (SirenixEditorGUI.ToolbarButton(states[index].enable ? EditorIcons.Stop : EditorIcons.Play))
            {
                property.MarkSerializationRootDirty();
                switch (states[index].enable)
                {
                    case true:
                        DisableState(index);
                        break;
                    case false:
                        EnableState(index);
                        break;
                }
            }

            EditorGUI.EndChangeCheck();
            GUI.color = c;
        }
#endif
       
#if UNITY_EDITOR
        private void Odin_ElementEndGUI(int index, InspectorProperty property)
        {

            var g = GUI.enabled;
            var c = GUI.color;
            var enable = states[index].value == null;
            if (states[index].value == null)
            {
                GUI.enabled = enable;
                GUI.color = enable ? new Color(0.66f, 1f, 0.6f) : new Color(0.48f, 0.43f, 0.33f);
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
                {
                    property.MarkSerializationRootDirty();
                    states[index].CreateNew();
                }
                GUI.enabled = g;
            }
            else
            {
                GUI.color = enable ? new Color(0.48f, 0.43f, 0.33f) :  new Color(1f, 0.96f, 0.64f);
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Folder))
                {
                    property.MarkSerializationRootDirty();
                    states[index].SaveToAsset();
                }
            }
            
            GUI.color = c;

            SirenixEditorGUI.EndHorizontalToolbar();


        }
#endif
        public void Odin_StateUpdated()
        {
            Odin_ValidateStateList();
        }

        void Odin_ValidateStateList()
        {
            IndexMap ??= new Dictionary<string, int>(states.Count);
            IndexMap.Clear();
            for (int i = 0; i < states.Count; i++)
            {
                if (IndexMap.ContainsKey(states[i].name))
                {
                    if (IndexMap[states[i].name] != i)
                    {
                        string name;
                        var j = i;
                        do
                        {
                            j++;
                            name = $"New State {j}";
                        } while (IndexMap.ContainsKey(name));
                        states[i].name = name;
                        IndexMap.Add(name, i);
                    }
                }
                else
                {
                    IndexMap.Add(states[i].name, i);
                }
                //Remove being blocked by themselves
                if (states[i].blockedBy.Contains(states[i].name))
                    states[i].blockedBy.Remove(states[i].name);
            }
            
            //Remove Invalid Names
            foreach (var t in states)
                for (var j = 0; j < t.blockedBy.Count; j++)
                {
                    var blockedBy = t.blockedBy[j];
                    if (IndexMap.ContainsKey(blockedBy))
                        continue;
                    t.blockedBy.RemoveAt(j);
                }
        }

        public State<T> Odin_AddToList()
        {
            string name;
            var i = 0;
            do
            {
                i++;
                name = $"New State {states.Count + i}";
            } while (IndexMap.ContainsKey(name));

            if(IndexMap.TryAdd(name, states.Count))
                return new State<T>
                {
                    name = name,
                    PartOfStateList = true
                };
            return null;
        }

        public void Odin_RemoveIndex(int index)
        {
            var state = states[index];
            foreach (var s in states)
            {
                if (s.blockedBy.Contains(state.name))
                {
                    //Update blocked state
                    //Check if other states blocked by this state should still be blocked
                    bool shouldBlock = false;
                    foreach (var bs in s.blockedBy)
                    {
                        if(states[IndexMap[bs]].name == state.name)
                            continue;
                        if(states[IndexMap[bs]].enable)
                        {
                            shouldBlock = true;
                            break;
                        }
                    }

                    s.blocked = shouldBlock;
                    s.blockedBy.Remove(state.name);
                    
                }
            }
            states.RemoveAt(index);
            IndexMap.Remove(state.name);
        }
        

        #endregion
    }
}