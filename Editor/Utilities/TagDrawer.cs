using Core.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor.Utilities
{
    [CustomPropertyDrawer(typeof(Tag))]
    public class TagDrawer : PropertyDrawer
    {
        private SerializedProperty _tag;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _tag = property.FindPropertyRelative("_name");
            return base.CreatePropertyGUI(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(new Rect(position.x, position.y, position.width, 20), label, property);

            if (_tag == null)
                _tag = property.FindPropertyRelative("_name");
            
            if (string.IsNullOrEmpty(_tag.stringValue))
                _tag.stringValue = "Untagged";
                
            float width = position.width;
            EditorGUI.LabelField(new Rect(position.x, position.y, width / 3, 20), label.text);
            _tag.stringValue = EditorGUI.TagField(new Rect(position.x + width / 3, position.y, position.width * 2 / 3, 20), _tag.stringValue);
            
            EditorGUI.EndProperty();
        }
    }
}