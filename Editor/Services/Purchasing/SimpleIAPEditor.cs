using Core.Services.Purchasing;
using UnityEditor;

namespace Core.Editor.Services.Purchasing
{
    [CustomEditor(typeof(SimpleIAP))]
    public class SimpleIAPEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
#if SIS_IAP == FALSE
            EditorGUILayout.HelpBox("No SimpleIAP imported!", MessageType.Warning);
            EditorGUILayout.Space();
            return;      
#else
            base.OnInspectorGUI();
#endif
        }
    }
}