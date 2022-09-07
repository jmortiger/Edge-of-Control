using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{
	[CreateAssetMenu(fileName = "lvl_Level", menuName = "Scriptable Object/LevelData")]
	public class LevelData : ScriptableObject
	{
		public string levelName;
		public float emptyClearTime = 26.68f;
		public float timeLimitMultiplier = 1.75f;
		public float PlayerTimeLimit { get => emptyClearTime * timeLimitMultiplier; }
		public UnityEvent<string> LevelLoaded = new();
		public UnityEvent<string> LevelCompleted = new();
	}
}