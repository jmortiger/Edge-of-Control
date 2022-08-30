using Assets.ScriptableObjects;
using Syrus.Plugins.ChartEditor;
using UnityEditor;
using UnityEngine;

namespace Assets.EditorScripts
{
	[CustomEditor(typeof(CameraSettings))]
	public class CameraSettingsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var obj = (CameraSettings)target;
			GUIChartEditor.BeginChart(10, 100, 100, 200, Color.black,
				GUIChartEditorOptions.ChartBounds(0, 20, 0, 20),
				GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft),
				GUIChartEditorOptions.ShowAxes(Color.white),
				GUIChartEditorOptions.ShowGrid(1, 1, Color.grey, true)
			);
			GUIChartEditor.PushFunction(obj.OrthographicSizeFunction, 0, 20, (obj.isOrthographicSizeFunctionActive) ? Color.green : Color.gray);
			GUIChartEditor.PushFunction((x) => obj.defaultOrthographicSize, 0, 20, (!obj.isOrthographicSizeFunctionActive) ? Color.red : Color.gray);
			GUIChartEditor.EndChart();
		}
	}
}