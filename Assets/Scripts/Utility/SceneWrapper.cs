using System;
using UnityEngine;

namespace Assets.Scripts.Utility
{
	[Serializable]
	public class SceneWrapper
	{
		public string scenePath;
		public string sceneName;
		#region For Editor Property Drawer Use Only
		[SerializeField]
		bool sceneInBuildIndex = true;
		[SerializeField]
		string newScenePath = "";
		#endregion
	}
}