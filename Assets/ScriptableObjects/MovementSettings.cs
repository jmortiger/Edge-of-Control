using UnityEngine;

namespace Assets.ScriptableObjects
{
	[CreateAssetMenu(fileName = "mvt_PlayerMovement0", menuName = "ScriptableObjects/Movement Settings")]
    public class MovementSettings : ScriptableObject
    {
		[Tooltip("The force applied when using the movement actions.")]
		public Vector2 moveForce = new(1f, 1f);
		[Tooltip("\"Endless Runner Mode\". Apply a constant displacement and use a different move force.")]
		public bool isConstantMovementEnabled = false;
		[Tooltip("The constant displacement applied under endless runner mode.")]
		public Vector2 constantMovementDisplacement = new(5f, 0f);
		[Tooltip("The force applied when using the movement actions if endless runner mode is on.")]
		public Vector2 constantMoveForce = new(5f, 1f);
		[Tooltip("The exit speed when rolling.")]
		public float rollExitSpeed = 3f;
		public float rollTimerLength = 1f;
		public float stumbleTimerLength = 1f;
		public float invincibleTimerLength = 1f;

		public Vector2 jumpForce = new(.5f, 15f);
		public float maxSafeFallSpeed = -20f;
		[Tooltip("The amount of velocity conserved when jumping off enemies' heads.")]
		[Range(0, 1)]
		public float conservedVelocity = .8f;
		[Tooltip("The angle of attack, in degrees, for wall runs. Determines loss of horizontal speed and hang time.")]
		[Range(0, 90)]
		public float wallRunAngle = 5f;

		#region Boost Fields
		public int boostJumpCost = 50;
		public int boostRunCost = 20;
		#endregion
	}
}
