using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RR.Utils.Editor
{
    public class SpringSettingsEditor : OdinAttributeProcessor<SpringSettings>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InlinePropertyAttribute());
            // attributes.Add(new PropertySpaceAttribute(2,2));
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "useForce":
                    attributes.Add(new LabelTextAttribute("U"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new HorizontalGroupAttribute("SpringSettings", width: 25));
                    attributes.Add(new PropertyTooltipAttribute("Should we force instead of frequency?"));
                    attributes.Add(new GUIColorAttribute(1, 0.75f, 0.75f));
                    break;
                case "frequency":
                    attributes.Add(new LabelTextAttribute("F"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new HorizontalGroupAttribute("SpringSettings"));
                    attributes.Add(new PropertyTooltipAttribute("@useForce?\"Force\":\"Frequency\""));
                    attributes.Add(new SuffixLabelAttribute("@useForce?\"N\":\"Hz\"", overlay:true));
                    attributes.Add(new GUIColorAttribute(0.75f, 1, 0.75f));
                    break;
                case "damper":
                    attributes.Add(new LabelTextAttribute("D"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new HorizontalGroupAttribute("SpringSettings"));
                    attributes.Add(new PropertyTooltipAttribute("Damper Value"));
                    attributes.Add(new GUIColorAttribute(0.75f, 0.75f, 1));
                    break;
            }
        }
    }
}