using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer (typeof(UniqueIdentifierAttribute))]
public class UniqueIdentifierDrawer : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
    {	
        if(prop.intValue == 0)
        {
            prop.intValue = CreateDefId();
        }
        Rect textFieldPosition = position;
        textFieldPosition.height = 20;
        DrawLabelField (textFieldPosition, prop, label);
        Rect buttonPosition = textFieldPosition;
        buttonPosition.height = 17;
        buttonPosition.width = 100;
        buttonPosition.x = 250;
        
        if(GUI.Button(buttonPosition,"Regenerate")) 
        {
            prop.intValue = CreateDefId();
        }
    }

    private int CreateDefId()
    {
        var guid = System.Guid.NewGuid();
        return guid.GetHashCode();
    }

    void DrawLabelField (Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.LabelField (position, label, new GUIContent (prop.intValue.ToString()));
    } 
}
