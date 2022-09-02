using System;
using UnityEngine;

namespace Assets.Scripts.Utility
{
	[Serializable]
	public class SceneWrapper
	{
		public string scenePath;
		public string sceneName;
		public bool forceReload;
		#region For Editor Property Drawer Use Only
		[SerializeField]
		string newScenePath = "";
		#endregion
	}
}