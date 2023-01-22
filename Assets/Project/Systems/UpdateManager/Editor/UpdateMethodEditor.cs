using System;
using System.Collections.Generic;
using System.Reflection;
using RR.UpdateSystem;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RR.UpdateManager.Editor
{
    public class UpdateMethodEditor : OdinAttributeProcessor<UpdateMethod>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InlinePropertyAttribute());
            attributes.Add(new PropertySpaceAttribute(2,2));
        }
        
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "autoUpdate":
                    attributes.Add(new LabelTextAttribute("A"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new HorizontalGroupAttribute("UpdateMethod", width: 25));
                    attributes.Add(new PropertyTooltipAttribute("Enable auto update?"));
                    attributes.Add(new GUIColorAttribute(1, 0.75f, 0.75f));
                    break;
                case "slicedUpdate":
                    attributes.Add(new LabelTextAttribute("S"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new HorizontalGroupAttribute("UpdateMethod", width: 25));
                    attributes.Add(new PropertyTooltipAttribute("Enable Sliced Update"));
                    attributes.Add(new GUIColorAttribute(0.75f, 1, 0.75f));
                    break;
                case "bucketCount":
                    attributes.Add(new LabelTextAttribute("B"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new HorizontalGroupAttribute("UpdateMethod"));
                    attributes.Add(new PropertyTooltipAttribute("Bucket number to register this instance to"));
                    attributes.Add(new GUIColorAttribute(0.75f, 0.75f, 1));
                    attributes.Add(new EnableIfAttribute("@slicedUpdate"));
                    break;
            }
        }
    }
}
