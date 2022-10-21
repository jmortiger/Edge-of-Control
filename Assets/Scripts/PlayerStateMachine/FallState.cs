using Assets.Scripts.Utility;
using JMor.Utility;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class FallState : BaseState
	{
		public FallState(PlayerContext ctx, StateFactory factory) : base(ctx, factory, MovementState.Falling) { }

		#region Abstract Method Implementations
		public override void EnterState()
		{
			Ctx.movementState |= MovementState.Falling;
		}

		public override void UpdateState()
		{
			Debug.Assert(Ctx.Velocity.y <= 0 || Ctx.CachedVelocity.y <= 0);

			// Check Switch State
			if ((Ctx.collisionState == CollisionState.None || Ctx.collisionState == CollisionState.BGWall) &&
				(!TryBoostJumping() && Ctx.Input.IsPressed(InputNames.DownAction) && Ctx.coilConsumable))
				SwitchState(Factory.CoilState, StateSwitchBehaviour.Downstream);
			// TODO: If fall speed too high, force to roll?
			else if (Ctx.JumpButton.InputPressedOnThisFrame && Ctx.collisionState.HasFlag(CollisionState.EnemyTrigger))
			{
				// Conserve and redirect velocity
				var vel = Ctx.Velocity;
				vel.y *= -1;
				Ctx.Rb.velocity = vel;
				Ctx.Rb.velocity *= Ctx.MvmtSettings.conservedVelocity;

				// This process seems to automatically clear currentEnemyCollisions due
				// to the deactivation of enemy colliders further down the chain. If
				// the following assert fails, check there.
				// Kill stomped-on enemies.
				int lengthBefore = Ctx.CurrentEnemyCollisions.Count;
				var temp = Ctx.CurrentEnemyCollisions.ToArray();
				for (int i = 0; i < temp.Length; i++)
					temp[i]?.OnPlayerStomp();
				Debug.Assert(lengthBefore > Ctx.CurrentEnemyCollisions.Count);

				// Update Score, UI, Boost meter accordingly.
				Ctx.MyPlayer.UpdateScore((lengthBefore - Ctx.CurrentEnemyCollisions.Count) * 200);
				Ctx.MyPlayer.AddUIMessage("Enemy Bounce");
				Ctx.MyPlayer.UpdateBoostMeter(20);

				// Switch State
				SwitchState(Factory.JumpState);
			}
			//else if (Ctx.collisionState.HasFlag(CollisionState.EnemyCollider) &&
			//		!Ctx.movementState.HasFlag(MovementState.Invincible))
			//{

			//}
			else if (
				((Ctx.collisionState.HasFlag(CollisionState.Ground) && Ctx.CachedVelocity.y < Ctx.MvmtSettings.maxSafeFallSpeed) ||
				Ctx.collisionState.HasFlag(CollisionState.EnemyCollider)) &&
					!Ctx.movementState.HasFlag(MovementState.Invincible))
			{
				if (Ctx.Input.IsPressed(InputNames.DownAction))
					SwitchState(new BaseState[] { Factory.GroundState, Factory.RollState }, StateSwitchBehaviour.All);
				else
					SwitchState(Factory.StumbleState, StateSwitchBehaviour.All);
			}
			else if (Ctx.collisionState.HasFlag(CollisionState.Ground))
				SwitchState(Factory.GroundState, StateSwitchBehaviour.All);

			// Update state
			if (Ctx.movementState.HasFlag(MovementState.Coiling) ||
				Ctx.movementState.HasFlag(MovementState.Wallrunning))
				SubState?.UpdateState();
			else
			{
				Ctx.moveVector = Ctx.moveVector.IsFinite() ? Ctx.moveVector : Ctx.BasicMovement(Ctx.MoveForceAerial);
				SubState?.UpdateState();
			}
		}
		#endregion

		protected bool TryBoostJumping()
		{
			if (CanBoostJump())
			{
				// If falling, zero out vertical velocity so the jump isn't fighting the downward momentum.
				if (Ctx.Velocity.y < 0)
				{
					var vel = Ctx.Velocity;
					vel.y = 0;
					Ctx.Rb.velocity = vel;
				}
				//aSource.Stop();// Should I do this?
				//particleSystem.Play();// Should I do this?
				SwitchState(Factory.JumpState, StateSwitchBehaviour.SelfAndDownstream);
				Ctx.MyPlayer.UpdateBoostMeter(-Ctx.MvmtSettings.boostJumpCost);
				Ctx.boostConsumable = false;
				return true;
			}
			return false;
		}
	}
}
