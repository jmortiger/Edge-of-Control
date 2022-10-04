using System;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerCoilState : PlayerBaseState
	{
		public PlayerCoilState(PlayerContext ctx, PlayerStateFactory factory) : base(ctx, factory, MovementState.Coiling)
		{
			void ExitCoil()
			{
				Ctx.MyPlayer.MyCapsule.size = Ctx.MyPlayer.colliderInitialDimensions;

				// TODO: Remove when using animations
				//var srb = spriteRenderer.bounds;
				//srb.size = rendererInitialDimensions;//new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
				//spriteRenderer.bounds = srb;

				//_ctx.movementState ^= MovementState.Coiling;
				coilTimer = 0;
			}
			ExitAction = ExitCoil;
		}

		#region State
		float coilTimer = 0;
		#endregion

		#region Abstract Method Implementations
		public override void EnterState()
		{
			Debug.Assert(!Ctx.movementState.HasFlag(MovementState.Coiling));
			Debug.Assert(Ctx.coilConsumable);
			Ctx.movementState |= MovementState.Coiling;
			coilTimer = Ctx.MvmtSettings.coilTimerLength;
			Ctx.coilConsumable = false;
			Ctx.MyPlayer.AddUIMessage("Coil Jump");
			// TODO: Only have aerial chain increment if coiling avoided a collsion.
			Ctx.MyPlayer.UpdateAerialChain(1);
		}

		public override void UpdateState()
		{
			if (!Ctx.movementState.HasFlag(MovementState.Jumping) &&
				!Ctx.movementState.HasFlag(MovementState.Falling))
			{
				ExitState(StateSwitchBehaviour.Self);
				return;
			}
			coilTimer -= Time.fixedDeltaTime;
			// TODO: The following should be redundant, remove
			if (Ctx.collisionState.HasFlag(CollisionState.EnemyCollider) && !Ctx.movementState.HasFlag(MovementState.Invincible))
			{
				ExitState(StateSwitchBehaviour.AllButBase);
				//EnterStumble();
				Ctx.CurrentDisjointState = Factory.StumbleState;
				Ctx.CurrentDisjointState.EnterState();
			}
			else if (coilTimer <= 0f || (Ctx.collisionState.HasFlag(CollisionState.Ground)/* && rb.OverlapCollider(GlobalConstants.EnemyLayer, enemyCollidersOverlapped) <= 0*/))
				ExitState(StateSwitchBehaviour.Self);
			else
			{
				coilTimer = (coilTimer < 0) ? 0 : coilTimer;

				float GetCoilHeightAtTime(float coilTimer)
				{
					if (coilTimer <= Ctx.MyPlayer.coilInfo.entranceLengthRatio * Ctx.MvmtSettings.coilTimerLength)
						return Mathf.SmoothStep(Ctx.MyPlayer.colliderInitialDimensions.y, Ctx.MyPlayer.colliderInitialDimensions.y * Ctx.MyPlayer.coilInfo.minHeightRatio, coilTimer / (Ctx.MvmtSettings.coilTimerLength / 2));
					else if (coilTimer <= (Ctx.MyPlayer.coilInfo.entranceLengthRatio + Ctx.MyPlayer.coilInfo.properLengthRatio) * Ctx.MvmtSettings.coilTimerLength)
						return Ctx.MyPlayer.colliderInitialDimensions.y * Ctx.MyPlayer.coilInfo.minHeightRatio;
					else
						return Mathf.SmoothStep(Ctx.MyPlayer.colliderInitialDimensions.y * Ctx.MyPlayer.coilInfo.minHeightRatio, Ctx.MyPlayer.colliderInitialDimensions.y, (coilTimer / (Ctx.MvmtSettings.coilTimerLength / 2)) - 1);
				}
				var h = GetCoilHeightAtTime(coilTimer);

				Ctx.MyPlayer.MyCapsule.size = new(Ctx.MyPlayer.colliderInitialDimensions.x, h);

				// TODO: Remove when using animations
				//var srb = spriteRenderer.bounds;
				//srb.size = new Vector3(colliderInitialDimensions.x, h, rendererInitialDimensions.z);//srb.size.z);
				//spriteRenderer.bounds = srb;
			}
		}
		#endregion
	}
}
