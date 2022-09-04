using Assets.Scripts;
using Assets.ScriptableObjects;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.EditorScripts
{
	[CustomEditor(typeof(MySceneManager))]
	public class MySceneManagerEditor : Editor
	{
		int currSceneCollectionSelection = 0;
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			/*var */manager = (MySceneManager)target;
			#region Select Scene Collection To Launch In Play Mode
			var sceneCollections = manager.sceneCollections;
			var collectionNames = sceneCollections.ConvertAll(new System.Converter<SceneCollection, string>((sc) => sc.collectionName)).ToArray();
			EditorGUILayout.LabelField("Select Scene Collection To Launch In Play Mode");
			currSceneCollectionSelection = EditorGUILayout.Popup(currSceneCollectionSelection, collectionNames);
			#endregion
			if (GUILayout.Button("Launch Scene In Play Mode") && !EditorApplication.isPlayingOrWillChangePlaymode)
			{
				if (SceneManager.GetSceneByName("SceneManagement") == null || !SceneManager.GetSceneByName("SceneManagement").IsValid())
					EditorSceneManager.OpenScene("SceneManagement", OpenSceneMode.Single);
				EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetSceneByName("SceneManagement").path);
				manager.firstCollectionToLoad = currSceneCollectionSelection;
				EditorApplication.EnterPlaymode();
			}
		}

		static MySceneManager manager;
		[MenuItem("CONTEXT/MySceneManager/Find Scene Collections")]
		static void FindSceneCollections()
		{
			var sceneCollections = AssetDatabase.FindAssets("t:SceneCollection", new string[]
			{
				"Assets/Resources",
				"Assets/Scenes",
				"Assets/Scenes/Collections",
				"Assets/ScriptableObjects",
				"Assets/ScriptableObjects/SceneCollections",
			});
			//var manager = (MySceneManager)target;
			var added = new List<string>();
			for (int i = 0; i < sceneCollections.Length; i++)
			{
				sceneCollections[i] = AssetDatabase.GUIDToAssetPath(sceneCollections[i]);
				var sc = AssetDatabase.LoadAssetAtPath<SceneCollection>(sceneCollections[i]);
				if (!manager.sceneCollections.Contains(sc))
				{
					manager.sceneCollections.Add(sc);
					added.Add(sceneCollections[i]);
				}
			}
			var dialog = "";
			added.ForEach((elem) => dialog += $"Added {elem}");
			EditorUtility.DisplayDialog("Added Scene Collections", (dialog == "") ? "No SceneCollections were added." : dialog, "OK");
		}
	}
}