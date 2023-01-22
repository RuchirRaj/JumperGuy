using System;
using System.Collections.Generic;
using System.Reflection;
using RR.StateSystem;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RR.Common.Editor
{
    public class StateVariableEditor<T> : OdinAttributeProcessor<StateVariable<T>>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "enable":
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new HorizontalGroupAttribute(width:25));
                    return;
                case "value":
                    attributes.Add(new EnableIfAttribute(nameof(StateVariable<T>.enable)));
                    attributes.Add(new LabelTextAttribute($"      {parentProperty.NiceName}"));
                    attributes.Add(new HorizontalGroupAttribute(marginLeft:-25));
                    return;
                default:
                    attributes.Add(new HideIfAttribute("@true"));
                    return;
            }
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideLabelAttribute());
            attributes.Add(new PropertySpaceAttribute(3, 3));
        }
    }
}
