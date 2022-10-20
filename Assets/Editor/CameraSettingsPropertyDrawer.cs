using Assets.ScriptableObjects;
using JMor.EditorScripts.Utility;
using Syrus.Plugins.ChartEditor;
using UnityEditor;
using UnityEngine;

namespace Assets.EditorScripts
{
	[CustomPropertyDrawer(typeof(CameraSettings), true)]
	public class CameraSettingsPropertyDrawer : ExtendedScriptableObjectDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//generateSubIMGUI = (position, property, label) =>
			//{
			//	var obj = (CameraSettings)property.serializedObject.targetObject;
			//	GUIChartEditor.BeginChart(10, 100, 100, 200, Color.black,
			//		GUIChartEditorOptions.ChartBounds(0, 20, 0, 20),
			//		GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft),
			//		GUIChartEditorOptions.ShowAxes(Color.white),
			//		GUIChartEditorOptions.ShowGrid(1, 1, Color.grey, true)
			//	);
			//	GUIChartEditor.PushFunction(obj.OrthographicSizeFunction, 0, 20, (obj.isOrthographicSizeFunctionActive) ? Color.green : Color.gray);
			//	GUIChartEditor.PushFunction((x) => obj.defaultOrthographicSize, 0, 20, (!obj.isOrthographicSizeFunctionActive) ? Color.red : Color.gray);
			//	GUIChartEditor.EndChart();
			//};
			base.OnGUI(position, property, label);
		}
	}
}
