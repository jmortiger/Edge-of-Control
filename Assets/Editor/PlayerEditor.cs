using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Syrus.Plugins.ChartEditor;

using Assets.Scripts;

namespace Assets.EditorScripts
{
	[CustomEditor(typeof(Player))]
	public class PlayerEditor : Editor
	{
		private Player player;
		private Player Player
		{
			get
			{
				if (player == null)
					player = (Player)target;
				return player;
			}
		}
		public override VisualElement CreateInspectorGUI()
		{
			//return base.CreateInspectorGUI();
			VisualElement root = new();
			VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UILayouts/PlayerInspector.uxml");
			visualTree.CloneTree(root);
			var info = root.Q<Foldout>("info");
			for (int i = 0; i < info.childCount; i++)
				info.ElementAt(i)?.SetEnabled(false);
			//var camIMGUI = root.Q<IMGUIContainer>("camChart");
			//camIMGUI.onGUIHandler += () =>
			//{
			//	Debug.Log($"w:{camIMGUI.contentRect.width} h:{camIMGUI.contentRect.height}");
			//	var chartRect = camIMGUI.contentRect;
			//	chartRect.height = chartRect.width;
			//	GUILayout.BeginHorizontal(EditorStyles.helpBox);
			//	GUIChartEditor.BeginChart(chartRect, Color.black,
			//		GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft),
			//		GUIChartEditorOptions.ChartBounds(0, 10, 0, 20),
			//		GUIChartEditorOptions.ShowAxes(Color.white),
			//		GUIChartEditorOptions.ShowGrid(1, 1, Color.gray, true));
			//	//var chartFunc = player.OrthographicSizeFunction;
			//	GUIChartEditor.PushFunction(player.OrthographicSizeFunction, 0, 10, Color.red);
			//	GUIChartEditor.EndChart();
			//	GUILayout.EndHorizontal();
			//};

			//List<string> GetBindingPathsRecursive(VisualElement elem)
			//{
			//	var bps = new List<string>();
			//	if (!string.IsNullOrEmpty(((BindableElement)elem).bindingPath))
			//		bps.Add(((BindableElement)elem).bindingPath);
			//	for (int i = 0; i < elem.childCount; i++)
			//		bps.AddRange(GetBindingPathsRecursive(elem.ElementAt(i)));
			//	return bps;
			//}
			//var bPaths = GetBindingPathsRecursive(root);
			//var misc = root.Q<Foldout>("misc");
			//var currProp = serializedObject.GetIterator();
			//if (!bPaths.Contains(currProp.name))
			//{
			//	var newElem = new BindableElement();
			//	newElem.bindingPath = currProp.name;
			//	newElem.
			//}

			//root.
			//var camIMGUI = root.Q<IMGUIContainer>("cameraSettingsIMGUI");
			//camIMGUI.onGUIHandler = CreateEditor(Player.cameraSettings).OnInspectorGUI;
			return root;
		}
	}
}