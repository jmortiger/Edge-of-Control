using System;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerStumbleState : PlayerBaseState
	{
		#region Ctor
		void ExitStumbling()
		{
			Ctx.MyPlayer.AddUIMessage("Back up!");
			stumbleTimer = -1;
		}
		public PlayerStumbleState(PlayerStateMachineContext ctx, PlayerStateFactory factory)
			: base(ctx, factory, MovementState.Stumbling) { ExitAction = ExitStumbling; }
		#endregion

		#region State
		float stumbleTimer = 0;
		#endregion

		public override void EnterState()
		{
			Ctx.movementState |= MovementState.Stumbling;
			/*Ctx.*/stumbleTimer = Ctx.MvmtSettings.stumbleTimerLength;
			var knockback = new Vector2(-1 * Ctx./*Velocity*/CachedVelocity.x * .5f, 2);
			Ctx.Rb.velocity = Vector2.zero;
			//_ctx.rb.velocity = knockback;
			Ctx.Rb.AddForce(knockback, ForceMode2D.Impulse);
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), true);
			Ctx.MyPlayer.damageIndicator.Show(Ctx.MvmtSettings.stumbleTimerLength);
			Ctx.MyPlayer.ImpulseSource?.GenerateImpulseAt(Ctx.PlayerTransform.position, Ctx./*Velocity*/CachedVelocity);
			Ctx.MyPlayer.AddUIMessage("Stumbled...");
			Ctx.MyPlayer.ResetBoostMeter();
			Ctx.MyPlayer.UpdateScore(-100);
			var c = Ctx.MyPlayer.SRenderer.color;
			c.a = .5f;
			Ctx.MyPlayer.SRenderer.color = c;
			Ctx.ASource.PlayOneShot(Ctx.SFX_Hurt);
			// TODO: Figure out start direction for blood sprays.
			var m = Ctx.particleSystem.main;
			m.startColor = Color.red;

			var startSpeed = m.startSpeed;
			var origStartSpeedConstant = startSpeed.constant;
			startSpeed.constant = 3f;//knockback.x * 2f;
			m.startSpeed = startSpeed;

			var startLifetime = m.startLifetime;
			var origStartLifetimeConstant = startLifetime.constant;
			startLifetime.constant = Ctx.MvmtSettings.stumbleTimerLength;
			m.startLifetime = startLifetime;

			Ctx.particleSystem.Emit(20);
			Ctx.particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

			m.startColor = Color.white;

			startSpeed.constant = origStartSpeedConstant;
			m.startSpeed = startSpeed;

			startLifetime.constant = origStartLifetimeConstant;
			m.startLifetime = startLifetime;
		}

		public override void UpdateState()
		{
			/*Ctx.*/stumbleTimer -= Time.fixedDeltaTime;
			if (/*Ctx.*/stumbleTimer <= 0f)
			{
				// Enter Invincible
				Ctx.CurrentDisjointState = Factory.InvincibleState;
				Ctx.CurrentDisjointState.EnterState();

				// Switch to Normal States
				if (Ctx.collisionState.HasFlag(CollisionState.Ground))
					SwitchState(Factory.GroundState, StateSwitchBehaviour.All);
				else
					SwitchState(Factory.FallState, StateSwitchBehaviour.All);
			}
		}
	}
}
