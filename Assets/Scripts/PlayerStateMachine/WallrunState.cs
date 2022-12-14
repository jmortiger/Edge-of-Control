using Assets.Scripts.Utility;
using JMor.Utility;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class WallrunState : BaseState
	{
		public WallrunState(PlayerContext ctx, StateFactory factory)
			: base(ctx, factory, MovementState.Wallrunning)
		{
			void ExitWallrunning()
			{
				// TODO: Add wallrun sfx;
				Ctx.particleSystem.Stop(/*false, ParticleSystemStopBehavior.StopEmitting*/);
				hangTime = 0;
				wallrunStartDir = 0;
				Debug.Log("Exiting Wallrunning");
			}
			ExitAction = ExitWallrunning;
		}

		#region State
		float hangTime = 1f;
		float wallrunStartDir = 0f;
		#endregion

		#region Abstract Method Implementations
		public override void EnterState()
		{
			var vel = Ctx.Velocity;
			hangTime = Mathf.Abs(vel.x * Mathf.Sin(Ctx.MvmtSettings.wallRunAngle));
			wallrunStartDir = (vel.x > 0) ? 1 : -1;
			vel.x *= Mathf.Cos(Ctx.MvmtSettings.wallRunAngle);
			Ctx.Rb.velocity = vel;
			Ctx.Rb.AddForce(new(0, hangTime));
			Ctx.boostConsumable = true;
			Ctx.coilConsumable = true;
			Ctx.particleSystem.Play(true);
			// TODO: Add wallrun sfx;
			//aSource.PlayOneShot(sfx_group_Wallrun.GetRandomClip());
			Ctx.movementState |= MovementState.Wallrunning;
		}

		public override void UpdateState()
		{
			// TODO: Add wallrun sfx;
			// Check Switch States
			if (!Ctx.collisionState.HasFlag(CollisionState.BGWall) ||
				hangTime <= 0 ||
				(Ctx.Velocity.x >= 0 && wallrunStartDir < 0) ||
				(Ctx.Velocity.x <= 0 && wallrunStartDir > 0))
				SwitchState((BaseState)null, StateSwitchBehaviour.SelfAndDownstream);
			else if (Ctx.JumpButton.InputPressedOnThisFrame)
			{
				Factory.JumpState.JumpForce = Ctx.MvmtSettings.wallJumpForce;
				SwitchState(new BaseState[] { Factory.JumpState }, StateSwitchBehaviour.All);
			}
			// Update
			else
			{
				Ctx.moveVector = Ctx.moveVector.IsFinite() ? Ctx.moveVector : Ctx.BasicMovement(Ctx.MoveForceWallrun);
				// Counter gravity
				Ctx.Rb.AddForce(Physics2D.gravity * -1);
				Ctx.Rb.AddForce(new(0, hangTime));
				hangTime -= Time.fixedDeltaTime;
			}
		}
		#endregion
	}
}