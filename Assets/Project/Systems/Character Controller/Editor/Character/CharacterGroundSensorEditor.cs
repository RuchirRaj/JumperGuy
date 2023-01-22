using System;
using System.Collections.Generic;
using System.Reflection;
using RR.Gameplay.CharacterController;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace RR.Gameplay.Editor
{
    public class CharacterGroundSensorEditor : OdinAttributeProcessor<CharacterGroundSensor>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member,
            List<Attribute> attributes)
        {
            CharacterGroundSensor target = ((CharacterGroundSensor)parentProperty.ValueEntry.WeakSmartValue);
            var hColor = new Color(0.41f, 0.95f, 1f);
            var hColor2 = new Color(1f, 0.69f, 0.53f);
            switch (member.Name)
            {
                case "shape":
                    attributes.Add(new EnumToggleButtonsAttribute());
                    break;
                case "row":
                    attributes.Add(new GUIColorAttribute(hColor.r,hColor.g,hColor.b));
                    attributes.Add(new InfoBoxAttribute("$RaycastPointInfoText"));
                    attributes.Add(new ShowIfAttribute("@shape == Shape.Raycast"));
                    attributes.Add(new PropertyRangeAttribute(0,5));
                    break;
                case "column":
                    attributes.Add(new GUIColorAttribute(hColor.r,hColor.g,hColor.b));
                    attributes.Add(new ShowIfAttribute("@shape == Shape.Raycast"));
                    attributes.Add(new PropertyRangeAttribute(3,10));
                    break;
                case "offsetRow":
                case "flatBase":
                    attributes.Add(new GUIColorAttribute(hColor.r,hColor.g,hColor.b));
                    attributes.Add(new ShowIfAttribute("@shape == Shape.Raycast"));
                    attributes.Add(new TooltipAttribute("Cylindrical/Flat base?"));
                    break;
                case "radiusMultiplier":
                    attributes.Add(new PropertySpaceAttribute(5));
                    attributes.Add(new GUIColorAttribute(hColor.r,hColor.g,hColor.b));
                    break;
                case "maxIteration":
                    attributes.Add(new GUIColorAttribute(hColor2.r,hColor2.g,hColor2.b));
                    attributes.Add(new MinValueAttribute(1));
                    attributes.Add(new PropertyTooltipAttribute("Max no. of Iterations per Physics Cast"));
                    attributes.Add(new PropertySpaceAttribute(10));
                    break;
                case "ignoreCollider":
                    attributes.Add(new PropertySpaceAttribute(0,10));
                    attributes.Add(new GUIColorAttribute(1, 1, 0.75f));
                    break;
                case "stableGround":
                case "unStableGround":
                case "averageHit":
                case "castPointsLocal":
                case "castPointsWorld":
                case "hitInfos":
                case "totalRayCasts":
                case "slopes":
                case "enableDebug":
                case "hasDetectedHit":
                case "castPointToBottom":
                case "RaycastValidationFunc":
                attributes.Add(new ToggleGroupAttribute("enableDebug", "Debug"));
                break;
            }
        }
    }
}