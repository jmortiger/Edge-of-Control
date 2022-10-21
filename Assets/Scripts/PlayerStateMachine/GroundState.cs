using Assets.Scripts.Utility;
using JMor.Utility;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class GroundState : BaseState
	{
		public GroundState(PlayerContext ctx, StateFactory factory) 
			: base(ctx, factory, MovementState.Grounded)
		{
			void GroundExitAction()
			{
				Ctx.ASource.Stop();
				Ctx.particleSystem.Stop(/*false, ParticleSystemStopBehavior.StopEmitting*/);
			}
			ExitAction = GroundExitAction;
		}

		#region Abstract Method Implementations
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
			if (!Ctx.collisionState.HasFlag(CollisionState.Ground)/* && !Ctx.collisionState.HasFlag(CollisionState.EnemyCollider)*/)// TODO: Check on removal of enemy collider as valid grounded state.
			{
				SwitchState(Factory.FallState, StateSwitchBehaviour.All);
				return;
			}
			else if (Ctx.collisionState.HasFlag(CollisionState.EnemyCollider) && !Ctx.movementState.HasFlag(MovementState.Invincible))
			{
				SwitchState(Factory.StumbleState, StateSwitchBehaviour.All);
				return;
			}
			else if (Ctx.JumpButton.InputPressedOnThisFrame && Ctx.movementState.IsInJumpableState())
			{
				SwitchState(Factory.JumpState, StateSwitchBehaviour.All);
				return;
			}
			#endregion

			#region Update State
			// If rolling (or potentially some other states), normal grounded movement is ignored. Otherwise, perform my normal movement.
			if (!Ctx.movementState.HasFlag(MovementState.Rolling))
			{
				Ctx.moveVector = Ctx.moveVector.IsFinite() ? Ctx.moveVector : Ctx.BasicMovement(Ctx.MoveForceGround);
				// TODO: Debug running sound & particle effect start and stop.
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
				//Debug.Log($"isPlaying:{aSource.isPlaying}");
				// TODO: Test added boost run
				if (Ctx.MyPlayer.boostButton.InputPressedOnThisFrame && /*Ctx.boostConsumable && */Ctx.MyPlayer.boostMeter >= Ctx.MvmtSettings.boostRunCost && Ctx.moveVector.x != 0)
				{
					var dir = Ctx.moveVector.Round();
					Ctx.Rb.AddForce(dir * Ctx.MvmtSettings.boostRunImpulse, ForceMode2D.Impulse);
				}
			}
			SubState?.UpdateState();
			#endregion
		}
		#endregion
	}
}
