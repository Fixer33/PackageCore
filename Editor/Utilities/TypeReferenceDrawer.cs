using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor.Utilities
{
    [CustomPropertyDrawer(typeof(TypeReference<>))]
    public class TypeReferenceDrawer : PropertyDrawer
    {
        private const string PROPERTY_NAME = "_typeName";
        private static IEnumerable<Type> _allTypes;
        
        private SerializedProperty _typeName;
        private List<Type> _possibleTypes;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _typeName = property.FindPropertyRelative(PROPERTY_NAME);
            return base.CreatePropertyGUI(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(new Rect(position.x, position.y, position.width, 20), label, property);

            if (property.boxedValue is not TypeReference typeReference)
            {
                EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 3, 20), label.text);
                EditorGUI.LabelField(new Rect(position.x + position.width / 3, position.y, position.width * 2 / 3, 20), "Type error");
                EditorGUI.EndProperty();
                return;
            }
            
            _typeName ??= property.FindPropertyRelative(PROPERTY_NAME);
            _allTypes ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

            if (_possibleTypes == null)
            {
                _possibleTypes = _allTypes.Where(t => typeReference.Editor_GetTypeBase().IsAssignableFrom(t) && !t.IsAbstract).ToList();
                _possibleTypes.Insert(0, null);
            }
            
            if (string.IsNullOrEmpty(_typeName.stringValue))
                _typeName.stringValue = "No type";
                
            float width = position.width;
            EditorGUI.LabelField(new Rect(position.x, position.y, width / 3, 20), label.text);

            string typeName = _typeName.stringValue;
            var index = _possibleTypes
                .Select(i => i != null ? i.FullName : "No type")
                .ToList()
                .IndexOf(typeName);
            if (index < 0)
                index = 0;
                
            var newIndex =
                EditorGUI.Popup(new Rect(position.x + width / 3, position.y, position.width * 2 / 3, 20), 
                    index, _possibleTypes.Select(i => i != null ? i.FullName : "No type").ToArray());

            if (newIndex > 0 && newIndex < _possibleTypes.Count)
            {
                typeName = _possibleTypes[newIndex].FullName;
                _typeName.stringValue = typeName;
            }
            
            EditorGUI.EndProperty();
        }
    }
}