using System;
using System.Collections.Generic;
using System.Reflection;
using RR.StateSystem;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace RR.Common.Editor
{
    public class StatePropertyProcessor<T> : OdinPropertyProcessor<State<T>> where T : ScriptableObject, IState<T>, new()
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            propertyInfos.AddDelegate("+", ((State<T>)Property.ValueEntry.WeakSmartValue).CreateNew,
                new ButtonAttribute(SdfIconType.Plus),
                new GUIColorAttribute(0.66f, 1f, 0.6f),
                new HorizontalGroupAttribute("State",width:20),
                new EnableIfAttribute("@value == null"),
                new HideIfAttribute("@value != null || partOfStateList"));
            
            propertyInfos.AddDelegate("S", ((State<T>)Property.ValueEntry.WeakSmartValue).SaveToAsset,
                new ButtonAttribute(SdfIconType.Folder),
                new GUIColorAttribute(1f, 0.96f, 0.64f),
                new HorizontalGroupAttribute("State", width:20),
                new EnableIfAttribute("@value != null"),
                new HideIfAttribute("@value == null || partOfStateList"));
        }
    }

    public class StateEditor<T> : OdinAttributeProcessor<State<T>> where T : ScriptableObject, IState<T>, new()
    {
        
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            var pathName = "";
            /*if(!((State<T>)parentProperty.ValueEntry.WeakSmartValue).PartOfStateList)
            {
                pathName = parentProperty.Name + "/";
                attributes.Add(new BoxGroupAttribute(parentProperty.Name, false));
            }*/

            switch (member.Name)
            {
                case "enable":
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new HorizontalGroupAttribute(pathName + "State",width:25,marginRight:-5));
                    attributes.Add(new OnValueChangedAttribute("OnEnableChange"));
                    attributes.Add(new HideIfAttribute("partOfStateList"));
                    break;
                case "name":
                    attributes.Add(new HideLabelAttribute());
                    // attributes.Add(new HorizontalGroupAttribute(width:100));
                    attributes.Add(new VerticalGroupAttribute(pathName + "State/Name"));
                    attributes.Add(new GUIColorAttribute("GetNameColor"));
                    attributes.Add(new DisableIfAttribute("partOfStateList"));
                    attributes.Add(new LabelTextAttribute(parentProperty.NiceName));
                    break;
                case "value":
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new HorizontalGroupAttribute(pathName + "State/Name/Value"));
                    attributes.Add(new GUIColorAttribute("GetToggleColor"));
                    attributes.Add(new InlineEditorAttribute(InlineEditorModes.GUIAndHeader));
                    break;
                case "blockedBy":
                    attributes.Add(new HorizontalGroupAttribute(pathName + "State/Name/Value",width:100));
                    attributes.Add(new ListDrawerSettingsAttribute
                    {
                        DraggableItems = false, ElementColor = "GetBlockedColor", ShowIndexLabels = false, ShowItemCount = false   
                    });
                    break;
                default:
                    attributes.Add(new HideIfAttribute("@true"));
                    break;
            }

            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideLabelAttribute());
            
            base.ProcessSelfAttributes(property, attributes);
        }
    }
}