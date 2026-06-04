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
            label.AddToClassList("unity-property-field__label");
            // Instead of fixed width, let's use the standard property label behavior
            label.style.width = StyleKeyword.Auto;
            label.style.minWidth = 120;
            label.style.flexShrink = 0;
            container.Add(label);

            var contentContainer = new VisualElement();
            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.flexGrow = 1;

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            var minField = new FloatField("Min")
            {
                bindingPath = minProp.propertyPath,
                style = { flexGrow = 1, marginLeft = 2, marginRight = 2 }
            };
            minField.labelElement.style.minWidth = 30;

            var maxField = new FloatField("Max")
            {
                bindingPath = maxProp.propertyPath,
                style = { flexGrow = 1, marginLeft = 2, marginRight = 2 }
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

            contentContainer.Add(minField);
            contentContainer.Add(maxField);
            container.Add(contentContainer);

            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            float gap = 5;
            float width = (position.width - gap) / 2;
            var minRect = new Rect(position.x, position.y, width, position.height);
            var maxRect = new Rect(position.x + width + gap, position.y, width, position.height);

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            // Store original label width to restore it for sub-fields
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 30; // Small label for "Min"/"Max"

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

            EditorGUIUtility.labelWidth = originalLabelWidth;
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
            label.AddToClassList("unity-property-field__label");
            label.style.width = StyleKeyword.Auto;
            label.style.minWidth = 120;
            label.style.flexShrink = 0;
            container.Add(label);

            var contentContainer = new VisualElement();
            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.flexGrow = 1;

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            var minField = new IntegerField("Min")
            {
                bindingPath = minProp.propertyPath,
                style = { flexGrow = 1, marginLeft = 2, marginRight = 2 }
            };
            minField.labelElement.style.minWidth = 30;

            var maxField = new IntegerField("Max")
            {
                bindingPath = maxProp.propertyPath,
                style = { flexGrow = 1, marginLeft = 2, marginRight = 2 }
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

            contentContainer.Add(minField);
            contentContainer.Add(maxField);
            container.Add(contentContainer);

            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float gap = 5;
            float width = (position.width - gap) / 2;
            var minRect = new Rect(position.x, position.y, width, position.height);
            var maxRect = new Rect(position.x + width + gap, position.y, width, position.height);

            var minProp = property.FindPropertyRelative("Min");
            var maxProp = property.FindPropertyRelative("Max");

            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 30;

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

            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}