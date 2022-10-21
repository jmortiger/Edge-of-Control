using UnityEngine;

namespace Assets.ScriptableObjects
{
	[CreateAssetMenu(fileName = "sce_NewCollection", menuName = "Scriptable Object/Scene Collection")]
	public class SceneCollection : ScriptableObject
	{
		public string collectionName;
		public JMor.Utility.SceneWrapper[] scenes;
	}
}