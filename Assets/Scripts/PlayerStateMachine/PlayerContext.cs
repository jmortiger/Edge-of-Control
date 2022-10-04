using Assets.ScriptableObjects;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerContext
	{
		#region Initialization
		private PlayerContext(Player player) => MyPlayer = player;
		public static Tuple<PlayerContext, PlayerStateFactory> CreateStateMachine(Player player, bool initialize = true)
		{
			var ctx = new PlayerContext(player);
			ctx.StateFactory = new(ctx);
			if (initialize)
				ctx.StateFactory.InitializeStateMachine();
			return new(ctx, ctx.StateFactory);
		}
		//public void InitializeStateMachine() => StateFactory.InitializeStateMachine();
		#endregion
		#region Aliases
		public Player MyPlayer { get; private set; }
		public PlayerInput Input { get => MyPlayer.input; }
		public MovementSettings MvmtSettings { get => MyPlayer.movementSettings; }
		public Rigidbody2D Rb { get => MyPlayer.rb; }
		public Vector2 Velocity { get => MyPlayer.Velocity; }
		public Vector2 CachedVelocity { get => MyPlayer.CachedVelocity; }
		public Transform PlayerTransform { get => MyPlayer.transform; }
		public List<Enemy> CurrentEnemyCollisions { get => MyPlayer.currentEnemyCollisions; }
		public bool JumpPressedOnThisFrame { get => MyPlayer.jumpButton.InputPressedOnThisFrame; }
		public ButtonInfo JumpButton { get => MyPlayer.jumpButton; }
		public ParticleSystem particleSystem { get => MyPlayer.PSystem; }
		public AudioSource ASource { get => MyPlayer.ASource; }
		public AudioClip SFX_Running { get => MyPlayer.sfx_Running; }
		public AudioClip SFX_Hurt { get => MyPlayer.sfx_Hurt; }
		public SFX_Group SFX_group_Jump { get => MyPlayer.sfx_group_Jump; }
		public CollisionState collisionState { get => MyPlayer.CState; }
		public CollisionState[] CollisionStateBuffer { get => MyPlayer.CollisionStateBuffer; }
		#endregion
		#region State Machine
		public PlayerStateFactory StateFactory { get; private set; }
		public PlayerBaseState CurrentBaseState { get; set; }
		public PlayerBaseState CurrentDisjointState { get; set; }

		#region State
		public MovementState movementState;// { get => MyPlayer.MState; set => MyPlayer.MState = value; }
										   //public readonly CollisionState[] collisionStateBuffer = new CollisionState[10];
		public readonly MovementState[] movementStateBuffer = new MovementState[10];
		public bool boostConsumable = false;
		public bool coilConsumable = false;
		#endregion

		#region UpdateStates
		public void UpdateStates()
		{
			CurrentBaseState?.UpdateState();
			CurrentDisjointState?.UpdateState();
			moveVector = Vector2.positiveInfinity;

			// Slide new state onto buffer
			movementStateBuffer.SlideElementsDown(movementState);
			CollisionStateBuffer.SlideElementsDown(collisionState);
		}

		public void UpdateStates_DEBUG()
		{
			UpdateStates();
			Debug.Log(movementState);
			/*void Iterate(System.Collections.IEnumerator enumerator)
			{
				//enumerator.MoveNext();
				Debug.Assert(enumerator.MoveNext());
				var prev = (PlayerBaseState)enumerator.Current;
				if (((PlayerBaseState)enumerator.Current).SubState != null)
					Debug.Assert(((PlayerBaseState)enumerator.Current).SubState.SuperState == ((PlayerBaseState)enumerator.Current));
				while (enumerator.MoveNext())
				{
					Debug.Assert(((PlayerBaseState)enumerator.Current).SuperState != null && ((PlayerBaseState)enumerator.Current).SuperState == prev);
					if (((PlayerBaseState)enumerator.Current).SubState != null)
						Debug.Assert(((PlayerBaseState)enumerator.Current).SubState.SuperState == ((PlayerBaseState)enumerator.Current));
					prev = (PlayerBaseState)enumerator.Current;
				}
			}
			var e = CurrentBaseState.GetEnumerator();
			Iterate(e);
			e = CurrentDisjointState?.GetEnumerator();
			if (e != null)
				Iterate(e);*/
			Debug.Assert(CurrentBaseState != null, "STATE MACHINE ENTERED INVALID STATE: CurrentBaseState == null");
			foreach (PlayerBaseState state in StateFactory)
			{
				Debug.Assert(state?.SuperState == null || state?.SuperState?.SubState == state, $"STATE MACHINE DESYNC: {state}.SuperState.SubState != {state}");
				Debug.Assert(state?.SubState == null || state?.SubState?.SuperState == state, $"STATE MACHINE DESYNC: {state}.SubState.SuperState != {state}");
			}
		}
		#endregion
		#endregion
		#region MoveForces
		public Vector2 MoveForceGround { get => (MvmtSettings.isAerialAndGroundedMovementUnique) ? MvmtSettings.moveForceGround : MvmtSettings.moveForce; }
		public Vector2 MoveForceAerial { get => (MvmtSettings.isAerialAndGroundedMovementUnique) ? MvmtSettings.moveForceAerial : MvmtSettings.moveForce; }
		public Vector2 MoveForceWallrun { get => (MvmtSettings.isAerialAndGroundedMovementUnique) ? MvmtSettings.moveForceWallrun : MvmtSettings.moveForce; }
		#endregion
		#region Movement
		public Vector2 moveVector;
		public Vector2 GetMoveVector()
		{
			var moveVector = Input.FindAction("Move").ReadValue<Vector2>();
			moveVector.x = (moveVector.x == 0) ? 0 : ((moveVector.x > 0) ? 1 : -1);
			moveVector.y = (moveVector.y == 0) ? 0 : ((moveVector.y > 0) ? 1 : -1);
			return moveVector;
		}
		public Vector2 BasicMovement(Vector2 moveForce)
		{
			var moveVector = this.moveVector.IsFinite() ? this.moveVector : GetMoveVector();
			if (MvmtSettings.isConstantMovementEnabled)
			{
				Rb.AddForce(Vector2.Scale(moveVector, MvmtSettings.constantMoveForce));
				PlayerTransform.position += (Vector3)MvmtSettings.constantMovementDisplacement * Time.fixedDeltaTime;
			}
			else
				Rb.AddForce(Vector2.Scale(moveVector, moveForce));
			return moveVector;
		}
		#endregion
	}
	#region BitFlags
	[Flags]
	public enum MovementState
	{
		None = 0b0000_0000_0000,
		Grounded = 0b0000_0000_0001,
		Jumping = 0b0000_0000_0010,
		Wallrunning = 0b0000_0000_0100,
		Falling = 0b0000_0000_1000,
		Stumbling = 0b0000_0001_0000,
		Invincible = 0b0000_0010_0000,
		Rolling = 0b0000_0100_0000,
		Coiling = 0b0000_1000_0000,
		Aerial = 0b0001_0000_0000,
	}
	[Flags]
	public enum CollisionState// TODO: Relocate
	{
		None = 0x0000_0000,
		BGWall = 0x0000_0001,
		Ground = 0x0000_0010,
		EnemyCollider = 0x0000_0100,
		EnemyTrigger = 0x0000_1000,
	}
	#endregion
}
