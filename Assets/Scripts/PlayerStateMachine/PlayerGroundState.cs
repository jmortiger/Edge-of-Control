using Assets.Scripts.Utility;
using System;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerGroundState : PlayerBaseState
	{
		private void GroundExitAction()
		{
			Ctx.ASource.Stop();
			Ctx.particleSystem.Stop(/*false, ParticleSystemStopBehavior.StopEmitting*/);
			//_ctx.movementState ^= MovementState.Grounded;
		}
		public PlayerGroundState(PlayerStateMachineContext ctx, PlayerStateFactory factory) 
			: base(ctx, factory, MovementState.Grounded) { ExitAction = GroundExitAction; }

		public override void EnterState()
		{
			Ctx.movementState |= MovementState.Grounded;
			Ctx.boostConsumable = true;
			Ctx.coilConsumable = true;
			Ctx.ASource.clip = Ctx.SFX_Running;
			Ctx.particleSystem.Play(true);
		}

		public override void UpdateState()
		{
			#region Check Switch State
			if (!Ctx.collisionState.HasFlag(CollisionState.Ground) &&
					!Ctx.collisionState.HasFlag(CollisionState.EnemyCollider))// TODO: work on removing enemy collider as valid grounded state.
			{
				//ExitState(StateSwitchBehaviour.All);
				//movementState |= MovementState.Falling;
				SwitchState(Factory.FallState, StateSwitchBehaviour.All);
			}
			else if (Ctx.collisionState.HasFlag(CollisionState.EnemyCollider) && !Ctx.movementState.HasFlag(MovementState.Invincible))
			{
				//ExitState(StateSwitchBehaviour.AllDownstream);
				//_ctx.CurrentDisjointState = _factory.StumbleState;
				//_ctx.CurrentDisjointState.EnterState();
				SwitchState(Factory.StumbleState, StateSwitchBehaviour.All);
			}
			else if (Ctx.JumpButton.InputPressedOnThisFrame && Ctx.movementState.IsInJumpableState())
			{
				//ExitState(StateSwitchBehaviour.All);
				//EnterJumping();
				SwitchState(Factory.JumpState, StateSwitchBehaviour.All);
			}
			#endregion

			// Update state
			if (Ctx.movementState.HasFlag(MovementState.Rolling))
				SubState?.UpdateState();
			else
			{
				Ctx.moveVector = Ctx.moveVector.IsFinite() ? Ctx.moveVector : Ctx.BasicMovement(Ctx.MoveForceGround);
				// TODO: Debug running sound & particle effect start and stop.
				// TODO: Combine this collisionState.HasFlag(CollisionState.Ground) check with the next one.
				if (Ctx.collisionState.HasFlag(CollisionState.Ground))
				{
					if (Ctx.Velocity.x != 0)
					{
						if (!Ctx.ASource.isPlaying)
							Ctx.ASource.Play();
						if (!Ctx.particleSystem.isPlaying)
							Ctx.particleSystem.Play();
					}
					else
					{
						if (Ctx.ASource.isPlaying)
							Ctx.ASource.Stop();
						if (Ctx.particleSystem.isPlaying)
							Ctx.particleSystem.Stop();
					}
				}
				//Debug.Log($"isPlaying:{aSource.isPlaying}");
				SubState?.UpdateState();
			}
		}
	}
}
