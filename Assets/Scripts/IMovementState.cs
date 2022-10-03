//using Assets.ScriptableObjects;
//using Assets.Scripts.Utility;
//using System;
//using UnityEngine;
//using UnityEngine.InputSystem;

//namespace Assets.Scripts
//{
//	public abstract class IMovementState
//	{
//		public abstract IMovementState Parent { get; }
//		protected Player MyPlayer { get; set; }
//		protected PlayerInput input { get => MyPlayer.input; }
//		protected MovementSettings movementSettings { get => MyPlayer.movementSettings; }
//		protected Rigidbody2D rb { get => MyPlayer.rb; }
//		protected Vector2 Velocity { get => MyPlayer.Velocity; }
//		protected Transform transform { get => MyPlayer.transform; }
//		protected bool JumpPressedOnThisFrame { get => MyPlayer./*JumpPressedOnThisFrame*/jumpButton.InputPressedOnThisFrame; }
//		protected ParticleSystem particleSystem { get => MyPlayer.PSystem; }
//		protected AudioSource aSource { get => MyPlayer.ASource; }
//		protected AudioClip sfx_Running { get => MyPlayer.sfx_Running; }
//		protected AudioClip sfx_Hurt { get => MyPlayer.sfx_Hurt; }
//		protected SFX_Group sfx_group_Jump { get => MyPlayer.sfx_group_Jump; }

//		public IMovementState(Player player)
//		{
//			MyPlayer = player;
//		}

//		public abstract void Enter();
//		public abstract void Exit();
//		public abstract IMovementState UpdateCore(Player player, Vector2 moveVector, CollisionState collisionState, MovementState movementState);
//		public abstract IMovementState UpdateFull(Player player, Vector2 moveVector, CollisionState collisionState, MovementState movementState);
//		// TOOD: Factor in aerial and grounded constant move force.
//		public Vector2 BasicMovement(Vector2 moveForce)
//		{
//			var moveVector = input.FindAction("Move").ReadValue<Vector2>();
//			moveVector.x = (moveVector.x == 0) ? 0 : ((moveVector.x > 0) ? 1 : -1);
//			moveVector.y = (moveVector.y == 0) ? 0 : ((moveVector.y > 0) ? 1 : -1);

//			if (movementSettings.isConstantMovementEnabled)
//			{
//				rb.AddForce(Vector2.Scale(moveVector, movementSettings.constantMoveForce));
//				transform.position += (Vector3)movementSettings.constantMovementDisplacement * Time.fixedDeltaTime;
//			}
//			else
//				rb.AddForce(Vector2.Scale(moveVector, moveForce/*movementSettings.moveForce*/));
//			return moveVector;
//		}
//	}
//	[Flags]
//	public enum MovementState
//	{
//		None		= 0b0000_0000_0000,
//		Grounded	= 0b0000_0000_0001,
//		Jumping		= 0b0000_0000_0010,
//		Wallrunning	= 0b0000_0000_0100,
//		Falling		= 0b0000_0000_1000,
//		Stumbling	= 0b0000_0001_0000,
//		//Invincible	= 0b0000_0010_0000,
//		Rolling		= 0b0000_0100_0000,
//		Coiling		= 0b0000_1000_0000,
//		Aerial		= 0b0001_0000_0000,
//	}
//	[Flags]
//	public enum CollisionState
//	{
//		None			= 0x0000_0000,
//		BGWall			= 0x0000_0001,
//		Ground			= 0x0000_0010,
//		EnemyCollider	= 0x0000_0100,
//		EnemyTrigger	= 0x0000_1000,
//	}
//	public enum StateAction
//	{
//		PopThisState,
//		EnterThisState,
//		PopTilThisState,
//	}

//	public class GroundMovementState : IMovementState
//	{
//		private IMovementState parent = null;
//		public override IMovementState Parent { get => parent; }

//		public IMovementState StumbleState { get; set; }
//		public IMovementState FallState { get; set; }
//		public IMovementState JumpState { get; set; }

//		public GroundMovementState(Player player)
//			: base(player)
//		{

//		}

//		public override void Enter()
//		{
//			throw new NotImplementedException();
//		}

//		public override void Exit()
//		{
//			throw new NotImplementedException();
//		}

//		public override IMovementState UpdateCore(Player player, Vector2 moveVector, CollisionState collisionState, MovementState movementState)
//		{
//			throw new NotImplementedException();
//		}

//		public override IMovementState UpdateFull(Player player, Vector2 moveVector, CollisionState collisionState, MovementState movementState)
//		{
//			Vector2 moveForceGround = (movementSettings.isAerialAndGroundedMovementUnique) ? movementSettings.moveForceGround : movementSettings.moveForce;
//			Vector2 moveForceAerial = (movementSettings.isAerialAndGroundedMovementUnique) ? movementSettings.moveForceAerial : movementSettings.moveForce;
//			moveVector = moveVector.IsFinite() ? moveVector : BasicMovement(moveForceGround);
//			aSource.clip = sfx_Running;
//			// TODO: Debug running sound & particle effect start and stop.
//			// TODO: Combine this collisionState.HasFlag(CollisionState.Ground) check with the next one.
//			if (collisionState.HasFlag(CollisionState.Ground))
//			{
//				if (Velocity.x != 0)
//				{
//					if (!aSource.isPlaying)
//						aSource.Play();
//					if (!particleSystem.isPlaying)
//						particleSystem.Play();
//				}
//				else if (Velocity.x == 0)
//				{
//					if (aSource.isPlaying)
//						aSource.Stop();
//					if (particleSystem.isPlaying)
//						particleSystem.Stop();
//				}
//			}
//			//Debug.Log($"isPlaying:{aSource.isPlaying}");
//			if (!collisionState.HasFlag(CollisionState.Ground) &&
//				!collisionState.HasFlag(CollisionState.EnemyCollider) &&
//				!collisionState.HasFlag(CollisionState.EnemyTrigger))
//			{
//				aSource.Stop();
//				particleSystem.Stop();
//				movementState |= MovementState.Falling;
//				movementState ^= MovementState.Grounded;
//				return FallState;
//			}
//			else if (collisionState.HasFlag(CollisionState.EnemyCollider) && !movementState.HasFlag(MovementState.Invincible))
//			{
//				aSource.Stop();
//				particleSystem.Stop();
//				movementState ^= MovementState.Grounded;
//				//EnterStumble();
//				return StumbleState;
//			}
//			else if (JumpPressedOnThisFrame)
//			{
//				//var moveVector = input.FindAction("Move").ReadValue<Vector2>();
//				//moveVector.x = (moveVector.x == 0) ? 0 : ((moveVector.x > 0) ? 1 : -1);
//				moveVector.y = 1;
//				// TODO: If fall speed too high, force to roll?
//				if (collisionState.HasFlag(CollisionState.EnemyTrigger) && Velocity.y <= 0)
//				{
//					var vel = Velocity;
//					vel.y *= -1;
//					rb.velocity = vel;
//					rb.velocity *= movementSettings.conservedVelocity;

//					// This process seems to automatically clear currentEnemyCollisions due
//					// to the deactivation of enemy colliders further down the chain. If
//					// the following assert fails, check there.
//					int lengthBefore = player.currentEnemyCollisions.Count;
//					var temp = player.currentEnemyCollisions.ToArray();
//					for (int i = 0; i < temp.Length; i++)
//						temp[i]?.OnPlayerStomp();
//					Debug.Assert(lengthBefore > player.currentEnemyCollisions.Count);
//					player.UpdateScore((lengthBefore - player.currentEnemyCollisions.Count) * 200);

//					player.AddUIMessage("Enemy Bounce");
//					player.UpdateBoostMeter(20);
//				}
//				var fApplied = Vector2.Scale(moveVector, movementSettings.jumpForce);
//				rb.AddForce(fApplied, ForceMode2D.Impulse);
//				aSource.Stop();
//				particleSystem.Emit(30);
//				particleSystem.Stop();
//				aSource.PlayOneShot(/*sfx_Jump*/sfx_group_Jump.GetRandomClip());
//				movementState |= MovementState.Jumping;
//				movementState ^= MovementState.Grounded;
//				return JumpState;
//			}
//			return this;
//		}
//	}
//}
