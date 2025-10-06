using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.Utilities
{
    [CustomPropertyDrawer(typeof(TypeReference<>))]
    public class TypeReferenceDrawer : PropertyDrawer
    {
        private const string PROPERTY_NAME = "_typeName";
        private static IEnumerable<Type> _allTypes;
        
        private List<Type> _possibleTypes;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty typeNameProp = property.FindPropertyRelative(PROPERTY_NAME);

            // Defensive: show error if structure is wrong
            if (property.boxedValue is not TypeReference typeReference)
            {
                EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 3, 20), label.text);
                EditorGUI.LabelField(new Rect(position.x + position.width / 3, position.y, position.width * 2 / 3, 20), "Type error");
                EditorGUI.EndProperty();
                return;
            }

            // Lazy-load the full list of types (shared across all drawers, that's okay)
            _allTypes ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

            // Recompute possible types for this element
            if (_possibleTypes == null)
            {
                _possibleTypes = _allTypes
                    .Where(t => typeReference.Editor_GetTypeBase().IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();
                _possibleTypes.Insert(0, null); // Add 'No type' option
            }

            if (string.IsNullOrEmpty(typeNameProp.stringValue))
                typeNameProp.stringValue = "No type";

            float width = position.width;
            EditorGUI.LabelField(new Rect(position.x, position.y, width / 3, 20), label.text);

            string currentTypeName = typeNameProp.stringValue;
            int currentIndex = _possibleTypes
                .Select(t => t != null ? t.FullName : "No type")
                .ToList()
                .IndexOf(currentTypeName);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUI.Popup(
                new Rect(position.x + width / 3, position.y, width * 2 / 3, 20),
                currentIndex,
                _possibleTypes.Select(t => t != null ? t.FullName : "No type").ToArray()
            );

            // Apply the selected type name
            typeNameProp.stringValue = newIndex > 0 && newIndex < _possibleTypes.Count
                ? _possibleTypes[newIndex].FullName
                : "No type";

            EditorGUI.EndProperty();
        }

    }
}