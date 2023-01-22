using System;
using System.Collections.Generic;
using System.Reflection;
using RR.Attributes;
using RR.Utils;
using Sirenix.OdinInspector.Editor;

namespace RR.Common.Editor
{
    public class FreeCameraEditor : OdinAttributeProcessor<FreeCamera>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "lookSpeedController":
                    new CustomLineSpaceAttribute();
                    break;
            }
        }
    }
}