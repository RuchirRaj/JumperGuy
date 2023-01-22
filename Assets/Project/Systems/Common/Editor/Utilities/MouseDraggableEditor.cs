using System;
using System.Collections.Generic;
using System.Reflection;
using RR.Attributes;
using RR.Utils;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace RR.Common.Editor
{
    public class MouseDraggableEditor : OdinAttributeProcessor<MouseDraggable>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "scrollSensitivity":
                    attributes.Add(new CustomTitleAttribute("Settings", new Color(0.64f, 0.87f, 1f)));
                    break;
                case "update":
                    attributes.Add(new CustomTitleAttribute("Update", new Color(1f, 0.62f, 0.91f)));
                    break;
                case "runtimeGizmo":
                    attributes.Add(new CustomTitleAttribute("Gizmos", new Color(1f, 0.8f, 0.62f)));
                    break;
            }
        }
    }
}
