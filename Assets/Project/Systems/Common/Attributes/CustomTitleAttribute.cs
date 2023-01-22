using System;
using UnityEngine;

namespace RR.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CustomTitleAttribute : PropertyAttribute
    {
        public readonly string Title;
        public readonly string Subtitle;
        public readonly int FontSize;
        public readonly int SubSize;
        public Color Color;
        public readonly bool Line;
        public readonly bool ForceEnableGUI;

        public CustomTitleAttribute(string title, float r = 1, float g = 1, float b = 1, float a = 1, string subtitle = "", bool line = true, bool forceEnableGUI = false, int fontSize = 12, int subSize = 10)
        {
            Title = title;
            Subtitle = subtitle;
            Color = new Color(r, g, b, a);
            Line = line;
            ForceEnableGUI = forceEnableGUI;
            FontSize = fontSize;
            SubSize = subSize;
        }
        
        public CustomTitleAttribute(string title, Color color, string subtitle = "", bool line = true, bool forceEnableGUI = false, int fontSize = 12, int subSize = 10)
        {
            Title = title;
            Subtitle = subtitle;
            Color = color;
            Line = line;
            ForceEnableGUI = forceEnableGUI;
            FontSize = fontSize;
            SubSize = subSize;
        }
    }
}
