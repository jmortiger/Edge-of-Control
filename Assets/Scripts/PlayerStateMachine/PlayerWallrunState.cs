using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerWallrunState : PlayerBaseState
	{
		#region Ctor
		void ExitWallrunning()
		{
			// TODO: Add wallrun sfx;
			Ctx.particleSystem.Stop(/*false, ParticleSystemStopBehavior.StopEmitting*/);
			hangTime = 0;
			wallrunStartDir = 0;
			Debug.Log("Exiting Wallrunning");
		}
		public PlayerWallrunState(PlayerStateMachineContext ctx, PlayerStateFactory factory)
			: base(ctx, factory, MovementState.Wallrunning) { ExitAction = ExitWallrunning; }
		#endregion

		#region State
		float hangTime = 1f;
		float wallrunStartDir = 0f;
		#endregion

		#region Likely Unnecessary
		public override void CheckSwitchState()
		{
			throw new System.NotImplementedException();
		}

		public override void InitializeSubState()
		{
			throw new System.NotImplementedException();
		}
		#endregion

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
			Debug.Log("Entering Wallrunning");
		}

		public override void UpdateState()
		{
			// TODO: Add wallrun sfx;
			// TODO: Add wall jump
			Ctx.moveVector = Ctx.moveVector.IsFinite() ? Ctx.moveVector : Ctx.BasicMovement(Ctx.MoveForceWallrun);
			if (!Ctx.collisionState.HasFlag(CollisionState.BGWall) ||
				hangTime <= 0 ||
				(Ctx.Velocity.x >= 0 && wallrunStartDir < 0) ||
				(Ctx.Velocity.x <= 0 && wallrunStartDir > 0))
			{
				//ExitState(StateSwitchBehaviour.Self);
				SwitchState((PlayerBaseState)null, StateSwitchBehaviour.SelfAndAllDownstream);
			}
			else
			{
				// Counter gravity
				Ctx.Rb.AddForce(Physics2D.gravity * -1);
				Ctx.Rb.AddForce(new(0, hangTime));
				hangTime -= Time.fixedDeltaTime;
			}
		}
	}
}