using RR.Attributes;
using UnityEditor;
using UnityEngine;

namespace Project.Systems.Gameplay.Editor.Editor
{
    [CustomPropertyDrawer(typeof(CustomTitleAttribute))]
    public class CustomTitleDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight * 0.2f;
        }

        public override void OnGUI(Rect position)
        {
            // First get the attribute since it contains the range for the slider
            CustomTitleAttribute titleAttribute = attribute as CustomTitleAttribute;
            var enabled = GUI.enabled;
            GUI.enabled = titleAttribute.ForceEnableGUI || enabled;
            var style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = titleAttribute.FontSize;
            style.normal.textColor = Color.Lerp(titleAttribute.Color, style.normal.textColor, 0.3f);
            /*
            style.margin.bottom = 0;
            style.border.bottom = 0;
            style.padding.bottom = 0;
            */
            EditorGUILayout.LabelField(titleAttribute.Title, style);
            
            var sub = titleAttribute.Subtitle != "";
            if(sub)
            {
                style.fontSize = titleAttribute.SubSize;
                GUI.enabled = false;
                /*
                style.margin.top = 0;
                style.border.top = 0;
                style.padding.top = 0;
                */
                EditorGUILayout.LabelField(titleAttribute.Subtitle, style);
                GUI.enabled = true;
            }
            if(titleAttribute.Line)
            {
                var guiColor = GUI.backgroundColor;
                GUI.backgroundColor = style.normal.textColor;

                var lineRect = EditorGUILayout.GetControlRect(false, 3);
                lineRect.y -= 2;
                lineRect.height = 1;
                
                var boxStyle = new GUIStyle
                {
                    normal =
                    {
                        background = Texture2D.whiteTexture
                    }
                };

                GUI.Box(lineRect, "", boxStyle);
                
                GUI.backgroundColor = guiColor;
            }
            GUI.enabled = enabled;
        }
    }
}