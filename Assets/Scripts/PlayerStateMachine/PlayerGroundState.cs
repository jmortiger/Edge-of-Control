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

		#region Likely Unnessecary
		public override void InitializeSubState()
		{
			throw new NotImplementedException();
		}

		public override void CheckSwitchState()
		{
			throw new NotImplementedException();
		}
		#endregion

		//public override void CheckSwitchState()
		//{
		//	if (!_ctx.collisionState.HasFlag(CollisionState.Ground) &&
		//		!_ctx.collisionState.HasFlag(CollisionState.EnemyCollider))// TODO: work on removing enemy collider as valid grounded state.
		//	{
		//		ExitState(StateSwitchBehaviour.KillSelfAndAllDownstream);//ExitGrounded();
		//		_ctx.CurrentState = _factory.FallState; _ctx.CurrentState.EnterState();//_ctx.movementState |= MovementState.Falling;
		//	}
		//	else if (_ctx.collisionState.HasFlag(CollisionState.EnemyCollider) && !_ctx.movementState.HasFlag(MovementState.Invincible))
		//	{
		//		ExitState();
		//		_ctx.CurrentState = _factory.StumbleState; _ctx.CurrentState.EnterState();//EnterStumble();
		//	}
		//	else if (_ctx.jumpButton.InputPressedOnThisFrame && _ctx.movementState.IsInJumpableState())
		//	{
		//		ExitState();
		//		_ctx.CurrentState = _factory.JumpState; _ctx.CurrentState.EnterState();//EnterJumping(moveVector);
		//	}
		//}

		//public override void UpdateState()
		//{
		//	if (_ctx.movementState.HasFlag(MovementState.Rolling))
		//		subState.UpdateState();//UpdateRolling();//goto case MovementState.Rolling;
		//	// Ground Specific
		//	else
		//	{
		//		_ctx.moveVector = _ctx.moveVector.IsFinite() ? _ctx.moveVector : _ctx.BasicMovement(_ctx.moveForceGround);
		//		// TODO: Debug running sound & particle effect start and stop.
		//		// TODO: Combine this collisionState.HasFlag(CollisionState.Ground) check with the next one.
		//		if (_ctx.collisionState.HasFlag(CollisionState.Ground))
		//		{
		//			if (_ctx.Velocity.x != 0)
		//			{
		//				if (!_ctx.aSource.isPlaying)
		//					_ctx.aSource.Play();
		//				if (!_ctx.particleSystem.isPlaying)
		//					_ctx.particleSystem.Play();
		//			}
		//			else if (_ctx.Velocity.x == 0)
		//			{
		//				if (_ctx.aSource.isPlaying)
		//					_ctx.aSource.Stop();
		//				if (_ctx.particleSystem.isPlaying)
		//					_ctx.particleSystem.Stop();
		//			}
		//		}
		//		//Debug.Log($"isPlaying:{aSource.isPlaying}");
		//	}
		//}
	}
}
