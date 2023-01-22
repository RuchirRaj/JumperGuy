using RR.Attributes;
using UnityEditor;
using UnityEngine;

namespace Project.Systems.Gameplay.Editor.Editor
{
    [CustomPropertyDrawer(typeof(CustomLineSpaceAttribute))]
    public class CustomLineSpaceDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position)
        {
            // First get the attribute since it contains the range for the slider
            CustomLineSpaceAttribute lineAttribute = attribute as CustomLineSpaceAttribute;
            
            var guiColor = GUI.backgroundColor;
            var style = new GUIStyle(GUI.skin.label);
            GUI.backgroundColor = Color.Lerp(lineAttribute.color , style.normal.textColor, 0.4f);

            var lineRect = EditorGUILayout.GetControlRect(false, lineAttribute.height);
            lineRect.y -= EditorGUIUtility.singleLineHeight/2;
            lineRect.height = lineAttribute.lineHeight;
                
            var boxStyle = new GUIStyle();
            boxStyle.normal.background = Texture2D.whiteTexture;
                
            GUI.Box(lineRect, "", boxStyle);
                
            GUI.backgroundColor = guiColor;
        }
    }
}