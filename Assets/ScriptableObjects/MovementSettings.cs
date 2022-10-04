using UnityEngine;

namespace Assets.ScriptableObjects
{
	[CreateAssetMenu(fileName = "mvt_PlayerMovement0", menuName = "Scriptable Object/Movement Settings")]
    public class MovementSettings : ScriptableObject
	{
		#region Constant Movement Fields
		[Tooltip("\"Endless Runner Mode\". Apply a constant displacement and use a different move force.")]
		public bool isConstantMovementEnabled = false;
		[Tooltip("The constant displacement applied under endless runner mode.")]
		public Vector2 constantMovementDisplacement = new(5f, 0f);
		[Tooltip("The force applied when using the movement actions if endless runner mode is on.")]
		public Vector2 constantMoveForce = new(5f, 1f);
		#endregion

		#region Rolling
		[Tooltip("The exit speed when rolling.")]
		[Range(.0001f, 20f)]
		public float rollExitSpeed = 3f;
		[Tooltip("How long does a roll last?")]
		[Range(.0001f, 20f)]
		public float rollTimerLength = 1f;
		#endregion

		#region Jump Fields
		public Vector2 jumpForce = new(.5f, 15f);
		public Vector2 wallJumpForce = new(.5f, 5f);
		[Tooltip("The amount of velocity conserved when jumping off enemies' heads.")]
		[Range(0, 1)]
		public float conservedVelocity = .8f;
		#endregion

		#region Boost Fields
		[Range(0, 200)]
		public int boostJumpCost = 50;
		[Range(0, 200)]
		public int boostRunCost = 20;
		#endregion
		
		[Tooltip("The force applied when using the movement actions.")]
		public Vector2 moveForce = new(1f, 1f);

		#region Unique Aerial And Grounded Movement
		[Tooltip("If true, use the aerial and grounded move forces instead of their generic equivalents.")]
		public bool isAerialAndGroundedMovementUnique = false;
		[InspectorName("Ground Move Force")]
		[Tooltip("The force applied when using the movement actions on the ground.")]
		public Vector2 moveForceGround = new(1f, 1f);
		[InspectorName("Aerial Move Force")]
		[Tooltip("The force applied when using the movement actions in the air.")]
		public Vector2 moveForceAerial = new(1f, 1f);
		[InspectorName("Wallrun Move Force")]
		[Tooltip("The force applied when using the movement actions while wallrunning.")]
		public Vector2 moveForceWallrun = new(1f, 0);
		#endregion

		[Tooltip("How long does a stumble last?")]
		[Range(.0001f, 20f)]
		public float stumbleTimerLength = 1f;
		
		[Tooltip("How long does invincibility last?")]
		[Range(.0001f, 20f)]
		public float invincibleTimerLength = 1f;

		[Tooltip("How long do coils (curling when aerial to clear obstacles) last?")]
		[Range(.0001f, 10f)]
		public float coilTimerLength = 1f;

		[Tooltip("If the velocity's y component is greater than this (absolute value), landing on the ground w/o rolling will cause them to stumble.")]
		[Range(float.MinValue, 0)]
		public float maxSafeFallSpeed = -20f;

		[Tooltip("The angle of attack, in degrees, for wall runs. Determines loss of horizontal speed and hang time.")]
		[Range(0, 90)]
		public float wallRunAngle = 5f;
	}
}
