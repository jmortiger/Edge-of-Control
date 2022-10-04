using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerInvincibleState : PlayerBaseState
	{
		public PlayerInvincibleState(PlayerContext ctx, PlayerStateFactory factory)
			: base(ctx, factory, MovementState.Invincible)
		{
			void ExitInvincible()
			{
				Debug.Log("Exiting Invincible");
				var c = Ctx.MyPlayer.SRenderer.color;
				c.a = 1f;
				Ctx.MyPlayer.SRenderer.color = c;
				Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
				invincibleTimer = -1;
			}
			ExitAction = ExitInvincible;
		}

		#region State
		float invincibleTimer;
		#endregion

		#region Abstract Method Implementations
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
		#endregion
	}
}
