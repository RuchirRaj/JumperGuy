using System;
using System.Collections.Generic;
using System.Reflection;
using RR.Gameplay.CharacterController;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RR.Gameplay.Editor
{
    public class ShapeSettingsEditor : OdinAttributeProcessor<ShapeSettings>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InlinePropertyAttribute());
            attributes.Add(new PropertySpaceAttribute(3,3));
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "radius":
                    attributes.Add(new LabelTextAttribute("R"));
                    attributes.Add(new PropertyTooltipAttribute("Radius"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new VerticalGroupAttribute("shape"));
                    attributes.Add(new HorizontalGroupAttribute("shape/a"));
                    attributes.Add(new SuffixLabelAttribute("m", overlay:true));
                    attributes.Add(new GUIColorAttribute(0.75f, 1, 0.75f));
                    break;
                case "height":
                    attributes.Add(new LabelTextAttribute("H"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new PropertyTooltipAttribute("Height"));
                    attributes.Add(new VerticalGroupAttribute("shape"));
                    attributes.Add(new HorizontalGroupAttribute("shape/a"));
                    attributes.Add(new SuffixLabelAttribute("m", overlay:true));
                    attributes.Add(new GUIColorAttribute(0.75f, 0.75f, 1));
                    break;
                case "stepHeightRatio":
                    attributes.Add(new LabelTextAttribute("R"));
                    attributes.Add(new PropertyTooltipAttribute("Step-Offset to Height ratio"));
                    attributes.Add(new VerticalGroupAttribute("shape"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new PropertyRangeAttribute(0,1));
                    attributes.Add(new HorizontalGroupAttribute("shape/b"));
                    // attributes.Add(new GUIColorAttribute(0.75f, 1, 1));
                    break;
                case "comHeight":
                    attributes.Add(new LabelTextAttribute("C"));
                    attributes.Add(new PropertyTooltipAttribute("Height of center of mass from bottom"));
                    attributes.Add(new VerticalGroupAttribute("shape"));
                    attributes.Add(new LabelWidthAttribute(15));
                    attributes.Add(new PropertyRangeAttribute(0,1));
                    attributes.Add(new HorizontalGroupAttribute("shape/b"));
                    // attributes.Add(new GUIColorAttribute(0.75f, 1, 1));
                    break;
                case "material":
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new PropertySpaceAttribute(0, 5));
                    break;
            }
        }
    }
}