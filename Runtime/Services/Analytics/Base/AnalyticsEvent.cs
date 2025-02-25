#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Services.Analytics.Base
{
    [CreateAssetMenu(menuName = "Analytics event", fileName = "AnalyticsEvent")]
    public class AnalyticsEvent : ScriptableObject
    {
        public IEnumerable<(string name, object value)> Parameters { get; private set; }
        public DateTime LastFired { get; private set; }
        public Type BoundType { get; private set; }
        public string LastParameterValues { get; private set; }
        
        [field: SerializeField] public string EventId { get; private set; }

        private void OnEnable()
        {
            LastFired = DateTime.MinValue;
            LastParameterValues = string.Empty;
        }

        public void BindTo<T>() where T : AnalyticsEventParameterCollectionBase
        {
            BoundType = typeof(T);
        }

        public void Send()
        {
            if (BoundType == null)
            {
                Parameters = AnalyticsEventParameterCollectionBase.Get<EmptyAnalyticsEventParameterCollectionBase>();
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                MethodInfo getMethod = typeof(AnalyticsEventParameterCollectionBase).GetMethod("Get").MakeGenericMethod(BoundType);
                Parameters = (IEnumerable<(string name, object value)>)getMethod.Invoke(null, null);
            }
            Core.Services.Analytics.Analytics.CallEvent(this);
            CacheLastParameterValues();
            LastFired = DateTime.Now;
        }

        private void CacheLastParameterValues()
        {
            StringBuilder sv = new();
            foreach (var parameter in Parameters)
            {
                sv.Append(parameter.name);
                sv.Append(": ");
                sv.AppendLine(parameter.value.ToString());
            }
            LastParameterValues = sv.ToString();
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(AnalyticsEvent), true)]
    public class AnalyticsEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default properties (excluding value)
            DrawDefaultInspector();

            // Get a reference to the target object and its type
            var targetVariable = target;
            var targetType = target.GetType();

            if (Application.isPlaying == false)
                return;
            
            // Get the Value property using reflection
            DrawProperty<DateTime>(targetType, targetVariable,
                "LastFired", 
                "Can't retrieve information", 
                "Can't retrieve information", 
                "Last fired:", 
                (dateTime) => dateTime.ToString("HH:mm:ss"),
                true, "Hasn't been fired yet", DateTime.MinValue);
            
            DrawProperty<Type>(targetType, targetVariable,
                "BoundType",
                "Can't retrieve parameter type information",
                "Bound to empty parameter collection",
                "Bound parameter collection:", type => type.Name,
                false, null, null);
            
            DrawPropertyCustomFunc<string>(targetType, targetVariable,
                "LastParameterValues",
                "Can't fetch last parameter values",
                "No parameter values passed",
                "Last passed parameter values:", values => values, (headerText, bodyText, val) =>
                {
                    var defaultColor = GUI.contentColor;
                    GUI.contentColor = Color.magenta;
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField(headerText, "");
                    var lines = bodyText.Split("\n");
                    foreach (var line in lines)
                    {
                        var dividedLine = line.Split(": ");
                        if (dividedLine is not {Length: 2})
                            continue;
                        
                        EditorGUILayout.LabelField(dividedLine[0], dividedLine[1]);
                    }
                    
                    GUI.contentColor = defaultColor;
                },
                true, "No parameter values passed", string.Empty);
        }

        private void DrawProperty<T>(Type targetType, Object targetVariable, 
            string propertyName, 
            string wrongTypeText,
            string nullValueText,
            string labelHeader, Func<T, string> labelTextGet,
            bool useDefaultValue, string defaultValueText, T defaultValue)
        {
            DrawPropertyCustomFunc(targetType, targetVariable, propertyName, wrongTypeText, nullValueText,
                labelHeader, labelTextGet, (headerText, bodyText, val) =>
                {
                    EditorGUILayout.LabelField(headerText, bodyText);
                },
                useDefaultValue, defaultValueText, defaultValue);
        }
        
        private void DrawPropertyCustomFunc<T>(Type targetType, Object targetVariable, 
            string propertyName, 
            string wrongTypeText,
            string nullValueText,
            string labelHeader, Func<T, string> labelTextGet, Action<string, string, T> drawLabelFunction,
            bool useDefaultValue, string defaultValueText, T defaultValue)
        {
            var property = targetType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property != null)
            {
                // Get the current value
                var currentValue = property.GetValue(targetVariable, null);

                if (currentValue is null)
                {
                    EditorGUILayout.LabelField(nullValueText);
                }
                else if (currentValue is not T val)
                {
                    EditorGUILayout.LabelField(wrongTypeText);
                }
                else if (useDefaultValue && val.Equals(defaultValue))
                {
                    EditorGUILayout.LabelField(defaultValueText);
                }
                else
                {
                    drawLabelFunction?.Invoke(labelHeader, labelTextGet?.Invoke(val), val);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"{propertyName} property not found");
            }
        }
    }    
#endif
}