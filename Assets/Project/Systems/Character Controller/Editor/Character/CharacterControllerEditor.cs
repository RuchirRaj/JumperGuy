using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RR.Gameplay.Editor
{
    public class CharacterControllerEditor : OdinAttributeProcessor<CharacterController.CharacterController>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            // attributes.Add(new SearchableAttribute());
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "<ShapeStates>k__BackingField":
                case "<MovementStates>k__BackingField":
                    attributes.Add(new TabGroupAttribute("State Group", "States List"));
                    break;
                case "<CurrentShapeState>k__BackingField":
                case "<CurrentMovementState>k__BackingField":
                    attributes.Add(new TabGroupAttribute("State Group", "Current State"));
                    attributes.Add(new InlineEditorAttribute());
                    break;
            }
        }
    }
}