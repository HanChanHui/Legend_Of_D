using UnityEngine;
using UnityEditor;
using HornSpirit;

[CustomPropertyDrawer(typeof(Constants.CardinalPoints))]
public class EnumCardinalPointsAttributeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        property.intValue = EditorGUI.MaskField(position, label, property.intValue, System.Enum.GetNames(typeof(Constants.CardinalPoints)));
    }
}
