using System;
using UnityEngine;

namespace RR.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CustomLineSpaceAttribute : PropertyAttribute
    {
        
        public Color color;
        public float height;
        public float lineHeight;

        public CustomLineSpaceAttribute(float r = 1, float g = 1, float b = 1, float a = 1, float h = 3, float l = 0.5f)
        {
            height = h;
            color = new Color(r, g, b, a);
            lineHeight = l;
        }
        
        /*public CustomTitleAttribute(string title, Color color, bool line)
        {
            this.title = title;
            this.color = color;
            this.line = line;
        }*/
    }
}
