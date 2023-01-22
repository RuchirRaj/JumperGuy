using System;
using System.Collections.Generic;
using System.Reflection;
using RR.Gameplay.CharacterController;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RR.Gameplay.Editor
{
    public class GroundingSettingEditor : OdinAttributeProcessor<CharacterGroundSettings>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(CharacterGroundSettings.stepDownDistance):
                    attributes.Add(new SuffixLabelAttribute("m", true));
                    attributes.Add(new PropertySpaceAttribute(0, 10));
                    break;
                case nameof(CharacterGroundSettings.applyForceOnGround):
                    attributes.Add(new GUIColorAttribute(1, 0.75f, 0.75f));
                    attributes.Add(new HorizontalGroupAttribute("ApplyForceOnGround", 25));
                    break;
                case nameof(CharacterGroundSettings.maxForceToMassRatio):
                    attributes.Add(new LabelTextAttribute("Max Ratio"));
                    attributes.Add(new LabelWidthAttribute(65));
                    attributes.Add(new EnableIfAttribute("applyForceOnGround"));
                    attributes.Add(new HorizontalGroupAttribute("ApplyForceOnGround"));
                    attributes.Add(new GUIColorAttribute(0.75f, 1, 0.75f));
                    attributes.Add(new PropertyTooltipAttribute("Max Force to mass Ratio"));
                    break;
                case nameof(CharacterGroundSettings.slopeLimit):
                    attributes.Add(new PropertySpaceAttribute(10, 0));
                    attributes.Add(new PropertyRangeAttribute(0, 90));
                    attributes.Add(new SuffixLabelAttribute("degree"));
                    break;
                case nameof(CharacterGroundSettings.groundingThreshold):
                    attributes.Add(new MinMaxSliderAttribute(0, 1, true));
                    break;
                case nameof(CharacterGroundSettings.groundingForceClamp):
                    attributes.Add(new MinMaxSliderAttribute(-500, 500, true));
                    attributes.Add(new PropertySpaceAttribute(0, 10));
                    break;
            }
        }
    }
}