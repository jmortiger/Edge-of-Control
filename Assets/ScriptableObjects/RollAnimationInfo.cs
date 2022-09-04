using UnityEngine;

namespace Assets.ScriptableObjects
{
	/// <summary>
	/// Controls the player's collider during the roll based of animation frame timings and dimensions.
	/// </summary>
	[CreateAssetMenu(fileName = "player_roll_RollAnimationInfo", menuName = "ScriptableObjects/Roll Animation Info")]
	public class RollAnimationInfo : ScriptableObject
	{
		[Tooltip("How much of the total roll time is spent in the lead up to the roll?")]
		public float entranceLengthRatio = 1f / 3f;
		[Tooltip("How much of the total roll time is spent in the roll proper (the part where you actually roll)?")]
		public float properLengthRatio = 1f / 3f;
		[Tooltip("How much of the total roll time is spent in the exit of the roll?")]
		public float exitLengthRatio = 1f / 3f;
		[Tooltip("At the lowest point of the roll (during the roll proper), how much of your standing height do you maintain?")]
		public float minHeightRatio = .5f;
	}
}
