using Assets.Scripts.Utility;
using UnityEditor;
using UnityEngine;

namespace Assets.EditorScripts
{
	[CustomPropertyDrawer(typeof(SceneWrapper))]
	public class ScenePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var scenePathProperty = property.FindPropertyRelative("newScenePath");
			if (scenePathProperty.stringValue == "" || scenePathProperty.stringValue == null)
				scenePathProperty.stringValue = EditorBuildSettings.scenes[0].path;

			var lastValidProp = property.FindPropertyRelative("scenePath");
			if (lastValidProp.stringValue == "" || lastValidProp.stringValue == null)
				lastValidProp.stringValue = EditorBuildSettings.scenes[0].path;

			var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProperty.stringValue);
			property.serializedObject.Update();
			void DrawAddToBuildButtons()
			{
				if (!property.FindPropertyRelative("sceneInBuildIndex").boolValue)
				{
					position.width /= 2;
					var yButtonTooltip = $"Add {AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProperty.stringValue).name} to the build at index {EditorBuildSettings.scenes.Length}?";
					if (GUI.Button(position, new GUIContent("Add To Build", yButtonTooltip)))
					{
						property.FindPropertyRelative("sceneInBuildIndex").boolValue = true;
						property.FindPropertyRelative("sceneName").stringValue = sceneAsset.name;
						property.serializedObject.ApplyModifiedProperties();
						AddToBuild(scenePathProperty.stringValue);
					}
					position.x += position.width;
					if (GUI.Button(position, "No"))
					{
						scenePathProperty.stringValue = lastValidProp.stringValue;
						property.FindPropertyRelative("sceneInBuildIndex").boolValue = true;
						property.serializedObject.ApplyModifiedProperties();
					}
					position.x -= position.width;
					position.width *= 2;
				}
			}
			// Hacky solution: Drawing the buttons before the ObjectField allows the buttons
			// interaction to take precedence, but draws the object field over them. Drawing the buttons
			// after the ObjectField draws the the buttons over the object field. but then the object
			// field interaction takes precedence. Removing the object field during attempts to draw the buttons
			// won't erase the editor window, but will stop the change check from setting the values. Drawing the
			// buttons twice, first to prioritize their interactions, then to draw them over the object field visually,
			// gives the intended behaviour.
			DrawAddToBuildButtons();
			EditorGUI.BeginChangeCheck();
			var removeButtonXWidth = position.height;
			var posOrigWidth = position.width;
			var posOrigXPos = position.x;
			position.width = removeButtonXWidth;
			var buildIndex = FindBuildIndex(scenePathProperty.stringValue);
			if (GUI.Button(position, new GUIContent("X", $"Remove scene from build; index = {buildIndex}")))
			{
				RemoveFromBuild(buildIndex);
				lastValidProp.stringValue = EditorBuildSettings.scenes[0].path;
				property.FindPropertyRelative("sceneInBuildIndex").boolValue = false;
			}
			position.width = posOrigWidth - removeButtonXWidth;
			position.x += removeButtonXWidth;
			sceneAsset = EditorGUI.ObjectField(position/*, sceneAsset.name*/, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
			position.width = posOrigWidth;
			position.x = posOrigXPos;
			if (EditorGUI.EndChangeCheck())
			{
				property.FindPropertyRelative("sceneInBuildIndex").boolValue = false;
				var newPath = AssetDatabase.GetAssetPath(sceneAsset);
				scenePathProperty.stringValue = newPath;
				for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
				{
					if (EditorBuildSettings.scenes[i].path == newPath)
					{
						//scenePathProperty.stringValue = newPath;
						lastValidProp.stringValue = newPath;
						property.FindPropertyRelative("sceneInBuildIndex").boolValue = true;
						property.FindPropertyRelative("sceneName").stringValue = sceneAsset.name;
						property.serializedObject.ApplyModifiedProperties();
						break;
					}
				}
			}
			DrawAddToBuildButtons();
		}
		int FindBuildIndex(string scenePath)
		{
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
				if (EditorBuildSettings.scenes[i].path == scenePath)
					return i;
			return -1;
		}
		void RemoveFromBuild(int index)
		{
			if (index < 0 || index >= EditorBuildSettings.scenes.Length)
				Debug.LogWarning($"Index {index} out of bounds; no scenes will be removed from the build.");
			var newBuildScenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length - 1];
			for (int i = 0; i < newBuildScenes.Length; i++)
				newBuildScenes[i] = (i >= index) ? 
					EditorBuildSettings.scenes[i + 1] : 
					EditorBuildSettings.scenes[i];
			EditorBuildSettings.scenes = newBuildScenes;
		}
		void AddToBuild(string scenePath)
		{
			var newBuildScenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length + 1];
			EditorBuildSettings.scenes.CopyTo(newBuildScenes, 0);
			newBuildScenes[^1] = new EditorBuildSettingsScene(scenePath, true);
			EditorBuildSettings.scenes = newBuildScenes;
		}
	}
}