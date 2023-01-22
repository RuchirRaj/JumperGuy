using System;
using System.Collections.Generic;
using System.Reflection;
using RR.StateSystem;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace RR.Common.Editor
{
    public class StateListPropertyProcessor<T> : OdinPropertyProcessor<StateList<T>> where T : ScriptableObject, IState<T>, new()
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            Action<string, string> renameByName = RenameByName;
            Action<int, string> renameByIndex = RenameByIndex;

            propertyInfos.AddDelegate("Rename(Name)", renameByName, new HorizontalGroupAttribute("Rename"),
                new GUIColorAttribute("Odin_RenameButtonColor"), new ButtonAttribute(), new IndentAttribute(-1));
            propertyInfos.AddDelegate("Rename(Index)", renameByIndex, new HorizontalGroupAttribute("Rename"),
                new GUIColorAttribute("Odin_RenameButtonColor"), new ButtonAttribute(), new IndentAttribute(-1));
        }

        private void RenameByName(string oldName, string newName)
        {
            ((StateList<T>)Property.ValueEntry.WeakSmartValue).RenameState(oldName, newName);
        }
        
        private void RenameByIndex(int index, string newName)
        {
            ((StateList<T>)Property.ValueEntry.WeakSmartValue).RenameState(index, newName);
        }
    }
    
    public class StateListEditor<T> : OdinAttributeProcessor<StateList<T>> where T : ScriptableObject, IState<T>, new()
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "states":
                    attributes.Add(new ListDrawerSettingsAttribute()
                    {
                        ShowIndexLabels = false, AddCopiesLastElement = false,
                        CustomAddFunction = nameof(StateList<T>.Odin_AddToList),
                        CustomRemoveIndexFunction = nameof(StateList<T>.Odin_RemoveIndex),
                        OnBeginListElementGUI = "Odin_ElementStartGUI", OnEndListElementGUI = "Odin_ElementEndGUI",
                        Expanded = true
                    });
                    
                    attributes.Add(new OnValueChangedAttribute(nameof(StateList<T>.Odin_StateUpdated), true)
                    {
                        InvokeOnInitialize = true,
                        InvokeOnUndoRedo = true
                    });
                    attributes.Add(new IndentAttribute(-1));
                    break;
            }
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            
        }
    }
}