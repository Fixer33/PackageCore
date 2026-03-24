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
    }
}