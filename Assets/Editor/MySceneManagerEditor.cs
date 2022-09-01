using Assets.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.EditorScripts
{
	[CustomEditor(typeof(MySceneManager))]
	public class MySceneManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			//EditorGUILayout.BeginHorizontal();
			var manager = (MySceneManager)target;
			var sceneNames = manager.SceneNames.ToArray();
			EditorGUILayout.Popup(0, sceneNames);
			//EditorSceneManager.
		}
	}
}