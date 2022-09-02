using Assets.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Assets.EditorScripts
{
	//[CustomPropertyDrawer(typeof(SceneCollection))]
	//public class SceneCollectionPropertyDrawer : PropertyDrawer
	//{
	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		//base.OnGUI(position, property, label);
	//		var prop_collectionName = property.FindPropertyRelative("collectionName");
	//		property.serializedObject.Update();
	//		EditorGUI.BeginChangeCheck();
	//		var elemRect = new Rect(position);
	//		//elemRect.width = position.width / 2
	//		Debug.Log($"{prop_collectionName}");
	//		var n = prop_collectionName.stringValue;
	//		var newName = EditorGUI.TextField(elemRect, new GUIContent("", "The name of the SceneCollection"), n);
	//		if (EditorGUI.EndChangeCheck())
	//		{
	//			prop_collectionName.stringValue = newName;
	//			property.serializedObject.ApplyModifiedProperties();
	//		}
	//	}
	//}
}