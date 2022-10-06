using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class RollState : BaseState
	{
		public RollState(PlayerContext ctx, StateFactory factory)
			: base(ctx, factory, MovementState.Rolling)
		{
			void ExitRolling()
			{
				// TOOD: Roll displacement prevention is jittery, tweak
				// Stop roll shrink from making player airborne
				//_ctx.transform.position = new(_ctx.transform.position.x, _ctx.transform.position.y - _ctx.MyPlayer.colliderInitialDimensions.y * .5f, _ctx.transform.position.z);
				var delta = Ctx.MyPlayer.MyCapsule.size.y - Ctx.MyPlayer.colliderInitialDimensions.y;
				Ctx.PlayerTransform.position = new(Ctx.PlayerTransform.position.x, Ctx.PlayerTransform.position.y - delta, Ctx.PlayerTransform.position.z);
				Ctx.MyPlayer.MyCapsule.size = Ctx.MyPlayer.colliderInitialDimensions;

				// TODO: Remove when using animations
				//var srb = spriteRenderer.bounds;
				//srb.size = rendererInitialDimensions;//new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
				//spriteRenderer.bounds = srb;

				//_ctx.movementState ^= MovementState.Rolling;
			}
			ExitAction = ExitRolling;
		}

		#region State
		bool rollRight = true;
		float rollTimer = 0;
		float rollInitialVx = 0;
		#endregion

		#region Abstract Method Implementations
		public override void EnterState()
		{
			Ctx.movementState |= MovementState.Rolling;
			Ctx.particleSystem.Play(true);
			rollTimer = Ctx.MvmtSettings.rollTimerLength;
			rollRight = Ctx./*Velocity*/CachedVelocity.x >= 0;
			rollInitialVx = Mathf.Abs(Ctx.CachedVelocity.x);
			Ctx.MyPlayer.AddUIMessage("Successful PK Roll");
		}

		public override void UpdateState()
		{
			rollTimer -= Time.fixedDeltaTime;
			if (Ctx.collisionState.HasFlag(CollisionState.EnemyCollider))
			{
				//ExitState(StateSwitchBehaviour.AllButBase);
				//EnterStumble();
				SwitchState(Factory.StumbleState, StateSwitchBehaviour.All);
				return;
			}
			else if (rollTimer <= 0f/* || (rb.OverlapCollider(enemyLayer, enemyCollidersOverlapped) <= 0 && collisionState.HasFlag(CollisionState.Ground))*/)
			{
				Ctx.Rb.velocity = new(Ctx.MvmtSettings.rollExitSpeed * (rollRight ? 1 : -1), Ctx.Velocity.y);

				ExitState(StateSwitchBehaviour.Self);
				return;
			}
			else
			{
				rollTimer = (rollTimer < 0) ? 0 : rollTimer;

				Ctx.Rb.velocity = new(Mathf.Lerp(Ctx.MvmtSettings.rollExitSpeed, rollInitialVx, rollTimer / Ctx.MvmtSettings.rollTimerLength) * (rollRight ? 1 : -1), Ctx.Velocity.y);

				float GetRollHeightAtTime(float rollTimer)
				{
					if (rollTimer <= Ctx.MyPlayer.rollInfo.entranceLengthRatio * Ctx.MvmtSettings.rollTimerLength)
						return Mathf.SmoothStep(Ctx.MyPlayer.colliderInitialDimensions.y, Ctx.MyPlayer.colliderInitialDimensions.y * Ctx.MyPlayer.rollInfo.minHeightRatio, rollTimer / (Ctx.MvmtSettings.rollTimerLength / 2));
					else if (rollTimer <= (Ctx.MyPlayer.rollInfo.entranceLengthRatio + Ctx.MyPlayer.rollInfo.properLengthRatio) * Ctx.MvmtSettings.rollTimerLength)
						return Ctx.MyPlayer.colliderInitialDimensions.y * Ctx.MyPlayer.rollInfo.minHeightRatio;
					else
						return Mathf.SmoothStep(Ctx.MyPlayer.colliderInitialDimensions.y * Ctx.MyPlayer.rollInfo.minHeightRatio, Ctx.MyPlayer.colliderInitialDimensions.y, (rollTimer / (Ctx.MvmtSettings.rollTimerLength / 2)) - 1);
				}
				var h = GetRollHeightAtTime(rollTimer);

				// TOOD: Roll displacement prevention is jittery, tweak
				// Stop roll shrink from making player airborne
				//transform.position = new(transform.position.x, transform.position.y - colliderInitialDimensions.y * .5f, transform.position.z);
				var delta = Ctx.MyPlayer.MyCapsule.size.y - h;
				Ctx.MyPlayer.MyCapsule.size = new(Ctx.MyPlayer.colliderInitialDimensions.x, h);
				Ctx.PlayerTransform.position = new(Ctx.PlayerTransform.position.x, Ctx.PlayerTransform.position.y - delta, Ctx.PlayerTransform.position.z);

				// TODO: Remove when using animations
				//var srb = spriteRenderer.bounds;
				//srb.size = new Vector3(colliderInitialDimensions.x, h, rendererInitialDimensions.z);//srb.size.z);
				//spriteRenderer.bounds = srb;
			}
		}
		#endregion
	}
}