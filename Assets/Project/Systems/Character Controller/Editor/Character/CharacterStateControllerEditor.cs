using System;
using System.Collections.Generic;
using System.Reflection;
using RR.Gameplay.CharacterController.StateController;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RR.Gameplay.Editor
{
    public class CharacterStateControllerEditor : OdinAttributeProcessor<CharacterStateController>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "<MovementState>k__BackingField":
                case "<ShapeState>k__BackingField":
                case "enableDebug":
                    attributes.Add(new ToggleGroupAttribute("enableDebug", 5, "Debug"));
                    break;
            }
        }
    }
}