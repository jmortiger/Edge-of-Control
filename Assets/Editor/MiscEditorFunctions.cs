using UnityEditor;
using UnityEngine;

namespace Assets.EditorScripts
{
	public static class MiscEditorFunctions
	{
		[MenuItem("Tools/MyTool/Do It in C#")]
		static void DoIt()
		{
			EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
		}
	}
}