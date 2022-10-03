using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerJumpState : PlayerBaseState
	{
		public PlayerJumpState(PlayerStateMachineContext ctx, PlayerStateFactory factory) 
			: base(ctx, factory, MovementState.Jumping) { }

		//#region Likely Unnessecary
		//public override void CheckSwitchState()
		//{
		//	throw new System.NotImplementedException();
		//}

		//public override void InitializeSubState()
		//{
		//	throw new System.NotImplementedException();
		//}
		//#endregion

		public override void EnterState()
		{
			var moveVector = Ctx.moveVector;
			if (!Ctx.moveVector.IsFinite())
				moveVector = Ctx.GetMoveVector();
			moveVector.y = 1;
			var fApplied = Vector2.Scale(moveVector, Ctx.MvmtSettings.jumpForce);
			Ctx.Rb.AddForce(fApplied, ForceMode2D.Impulse);
			Ctx.particleSystem.Emit(30);
			Ctx.ASource.PlayOneShot(Ctx.SFX_group_Jump.GetRandomClip());
			Ctx.movementState |= MovementState.Jumping;
		}

		public override void UpdateState()
		{
			Ctx.moveVector = Ctx.moveVector.IsFinite() ? Ctx.moveVector : Ctx.BasicMovement(Ctx.MoveForceAerial);
			if (Ctx.collisionState.HasFlag(CollisionState.EnemyCollider) && !Ctx.movementState.HasFlag(MovementState.Invincible))
			{
				SwitchState(Factory.StumbleState, StateSwitchBehaviour./*AllDownstream*/All);
				return;
			}
			else if (Ctx.Velocity.y <= 0)
				SwitchState(Factory.FallState, StateSwitchBehaviour.Self);
			else if (Ctx.collisionState.HasFlag(CollisionState.BGWall) && 
					!Ctx.movementState.HasFlag(MovementState.Wallrunning) && 
					 Ctx.JumpButton.InputPressedOnThisFrame && 
					 Ctx.Velocity.x != 0)
			{
				SwitchState(Factory.WallrunState, StateSwitchBehaviour.AllDownstream);
				return;
			}
			if ((Ctx.movementState.HasFlag(MovementState.Jumping) || Ctx.movementState.HasFlag(MovementState.Falling)) &&        //If we're in the air...
				!(Ctx.movementState.HasFlag(MovementState.Wallrunning) || Ctx.movementState.HasFlag(MovementState.Stumbling)) && //...and not wallrunning nor stumbling...
				!TryBoostJumping() &&																							 //...then try boost jumping. If that fails...
				Ctx.Input.IsPressed(InputNames.DownAction) &&																	 //...and the coil button is pressed...
				Ctx.coilConsumable &&																							 //...and we can coil...
				Ctx.MyPlayer.PlayerDistanceFromGround() > Ctx.MyPlayer.MyCapsule.bounds.size.y)                                  //...and we're farther than 1 bodylength from the ground...
			{
				if (SubState != null)
					SubState.ExitState(StateSwitchBehaviour.SelfAndAllDownstream);
				SubState = Factory.CoilState;
				SubState.EnterState();
			}
			SubState?.UpdateState();
		}

		protected override bool TryBoostJumping()
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
				SubState?.ExitState(StateSwitchBehaviour.SelfAndAllDownstream);
				EnterState();
				Ctx.MyPlayer.UpdateBoostMeter(-Ctx.MvmtSettings.boostJumpCost);
				Ctx.boostConsumable = false;
				return true;
			}
			return false;
		}
	}
}