using System.Collections.Generic;
using System.Reflection;

namespace Core.Services.Analytics.Base
{
    public abstract class AnalyticsEventParameterCollectionBase
    {
        private static FieldInfo[] _fields;
        
        public static IEnumerable<(string name, object value)> Get<T>() where T : AnalyticsEventParameterCollectionBase
        {
            var fields = GetFieldsRecursively(typeof(T));
            List<(string name, object value)> result = new();
            
            foreach (var fieldInfo in fields)
            {
                result.Add((fieldInfo.Name, fieldInfo.GetValue(null)));
            }

            return result;
        }

        private static IEnumerable<FieldInfo> GetFieldsRecursively(System.Type type)
        {
            if (type == null)
                return new List<FieldInfo>();

            var fields = new List<FieldInfo>(type.GetFields(BindingFlags.Public | BindingFlags.Static));

            fields.AddRange(GetFieldsRecursively(type.BaseType));

            return fields;
        }
    }

    public class EmptyAnalyticsEventParameterCollectionBase : AnalyticsEventParameterCollectionBase{}
}