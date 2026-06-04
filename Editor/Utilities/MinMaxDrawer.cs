using Core.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor.Utilities
{
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            var label = new Label(property.displayName);
            label.style.width = 120; // Standard label width or similar
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            container.Add(label);

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            var minField = new FloatField("Min")
            {
                bindingPath = minProp.propertyPath,
                style = { flexGrow = 1 }
            };
            minField.labelElement.style.minWidth = 30;

            var maxField = new FloatField("Max")
            {
                bindingPath = maxProp.propertyPath,
                style = { flexGrow = 1 }
            };
            maxField.labelElement.style.minWidth = 30;

            minField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue > maxProp.floatValue)
                {
                    maxProp.floatValue = evt.newValue;
                    maxProp.serializedObject.ApplyModifiedProperties();
                }
            });

            maxField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < minProp.floatValue)
                {
                    minProp.floatValue = evt.newValue;
                    minProp.serializedObject.ApplyModifiedProperties();
                }
            });

            container.Add(minField);
            container.Add(maxField);

            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var minRect = new Rect(position.x, position.y, position.width / 2 - 5, position.height);
            var maxRect = new Rect(position.x + position.width / 2 + 5, position.y, position.width / 2 - 5, position.height);

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            EditorGUI.BeginChangeCheck();
            float min = EditorGUI.FloatField(minRect, "Min", minProp.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                minProp.floatValue = min;
                if (minProp.floatValue > maxProp.floatValue)
                {
                    maxProp.floatValue = minProp.floatValue;
                }
            }

            EditorGUI.BeginChangeCheck();
            float max = EditorGUI.FloatField(maxRect, "Max", maxProp.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                maxProp.floatValue = max;
                if (maxProp.floatValue < minProp.floatValue)
                {
                    minProp.floatValue = maxProp.floatValue;
                }
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(MinMaxInt))]
    public class MinMaxIntDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            var label = new Label(property.displayName);
            label.style.width = 120; // Standard label width or similar
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            container.Add(label);

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            var minField = new IntegerField("Min")
            {
                bindingPath = minProp.propertyPath,
                style = { flexGrow = 1 }
            };
            minField.labelElement.style.minWidth = 30;

            var maxField = new IntegerField("Max")
            {
                bindingPath = maxProp.propertyPath,
                style = { flexGrow = 1 }
            };
            maxField.labelElement.style.minWidth = 30;

            minField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue > maxProp.intValue)
                {
                    maxProp.intValue = evt.newValue;
                    maxProp.serializedObject.ApplyModifiedProperties();
                }
            });

            maxField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < minProp.intValue)
                {
                    minProp.intValue = evt.newValue;
                    minProp.serializedObject.ApplyModifiedProperties();
                }
            });

            container.Add(minField);
            container.Add(maxField);

            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var minRect = new Rect(position.x, position.y, position.width / 2 - 5, position.height);
            var maxRect = new Rect(position.x + position.width / 2 + 5, position.y, position.width / 2 - 5, position.height);

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            EditorGUI.BeginChangeCheck();
            int min = EditorGUI.IntField(minRect, "Min", minProp.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                minProp.intValue = min;
                if (minProp.intValue > maxProp.intValue)
                {
                    maxProp.intValue = minProp.intValue;
                }
            }

            EditorGUI.BeginChangeCheck();
            int max = EditorGUI.IntField(maxRect, "Max", maxProp.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                maxProp.intValue = max;
                if (maxProp.intValue < minProp.intValue)
                {
                    minProp.intValue = maxProp.intValue;
                }
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}