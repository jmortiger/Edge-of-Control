using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.ScriptableObjects
{
	[CreateAssetMenu(fileName = "sce_NewCollection", menuName = "ScriptableObjects/Scene Collection")]
	public class SceneCollection : ScriptableObject
	{
		public string collectionName;
		public SceneWrapper[] scenes;
	}
}