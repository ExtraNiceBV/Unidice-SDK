using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unidice.SDK.Utilities
{
    /// <summary>
    /// Automatically references the object of the field's type when the inspector is drawn.
    /// </summary>
    [CustomPropertyDrawer(typeof(InjectAttribute))]
    public class InjectAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.BeginProperty(position, GUIContent.none, property);
                var reference = Object.FindObjectOfType(fieldInfo.FieldType, true);
                if (reference && reference != property.objectReferenceValue)
                {
                    property.objectReferenceValue = reference;
                }

                var enabled = GUI.enabled;
                GUI.enabled = false;
                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, true);
                GUI.enabled = enabled;
                EditorGUI.EndProperty();
            }
            else
            {
                Debug.LogError($"Property {property.name} can't be injected. It has to be a reference type.");

                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}