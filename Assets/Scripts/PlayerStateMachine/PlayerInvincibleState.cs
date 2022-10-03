using System;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerInvincibleState : PlayerBaseState
	{
		#region State
		float invincibleTimer;
		#endregion

		#region Ctor
		void ExitInvincible()
		{
			Debug.Log("Exiting Invincible");
			var c = Ctx.MyPlayer.SRenderer.color;
			c.a = 1f;
			Ctx.MyPlayer.SRenderer.color = c;
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
			invincibleTimer = -1;
		}
		public PlayerInvincibleState(PlayerStateMachineContext ctx, PlayerStateFactory factory)
			: base(ctx, factory, MovementState.Invincible) { ExitAction = ExitInvincible; }
		#endregion

		public override void EnterState()
		{
			Debug.Log("Entering Invincible");
			//Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
			invincibleTimer = Ctx.MvmtSettings.invincibleTimerLength;
			Ctx.movementState |= MovementState.Invincible;
		}

		public override void UpdateState()
		{
			invincibleTimer -= Time.fixedDeltaTime;
			if (invincibleTimer <= 0f ||
				(Ctx.Rb.OverlapCollider(GlobalConstants.EnemyLayer, Ctx.MyPlayer.enemyCollidersOverlapped) <= 0 &&
				Ctx.collisionState.HasFlag(CollisionState.Ground)))
				ExitState(StateSwitchBehaviour.All);
		}

		#region Likely Unnecessary
		public override void CheckSwitchState()
		{
			throw new NotImplementedException();
		}

		public override void InitializeSubState()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
