using System;
using System.Collections.Generic;
using System.Reflection;
using RR.Attributes;
using RR.Gameplay.CharacterController;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace RR.Gameplay.Editor
{
    public class CharacterBaseEditor : OdinAttributeProcessor<CharacterBase>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new SearchableAttribute());
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "<Sensor>k__BackingField":
                    attributes.Add(new CustomTitleAttribute("Settings", new Color(0.64f, 0.87f, 1f)));
                    break;
                case "<PositionSpring>k__BackingField":
                    attributes.Add(new PropertySpaceAttribute(10, 0));
                    attributes.Add(new VerticalGroupAttribute("Spring"));
                    break;
                case "<RotationSpring>k__BackingField":
                    attributes.Add(new VerticalGroupAttribute("Spring"));
                    attributes.Add(new PropertySpaceAttribute(0, 5));
                    break;
                case "<ShapeColliderType>k__BackingField":
                    attributes.Add(new CustomTitleAttribute("Shape", new Color(0.87f, 1f, 0.64f)));
                    attributes.Add(new EnumToggleButtonsAttribute());
                    attributes.Add(new LabelTextAttribute("Shape Collider"));
                    break;
                case "<Shape>k__BackingField":
                    attributes.Add(new LabelTextAttribute("Shape"));
                    break;
                case "<FixedUpdate>k__BackingField":
                    attributes.Add(new CustomTitleAttribute("Update", new Color(1f, 0.62f, 0.91f)));
                    break;
                case "<RefTransform>k__BackingField":
                case "<DisableGroundingTimer>k__BackingField":
                case "<GroundState>k__BackingField":
                case "<DownForceMultiplier>k__BackingField":
                case "_vel":
                case "_finalForce":
                case "_finalTorque":
                case "_groundingForce":
                case "_moveForce":
                case "_rotationTorque":
                case "_sensorStartHeight":
                case "_sensorEndHeight":
                case "_up":
                case "_rot":
                case "enableDebug":
                    attributes.Add(new ToggleGroupAttribute("enableDebug", "Debug"));
                    // attributes.Add(new TitleGroupAttribute("Debug"));
                    // attributes.Add(new ReadOnlyAttribute());
                    attributes.Add(new ShowInInspectorAttribute());
                    break;
                case "enableGizmos":
                    attributes.Add(new CustomTitleAttribute("Gizmos", new Color(1f, 0.8f, 0.62f)));
                    break;
            }
        }
    }
}