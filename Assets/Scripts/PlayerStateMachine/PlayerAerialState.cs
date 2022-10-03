using System;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerAerialState : PlayerBaseState
	{
		private void AerialExitAction()
		{

		}
		public PlayerAerialState(PlayerStateMachineContext ctx, PlayerStateFactory factory) : base(ctx, factory, MovementState.Aerial) { ExitAction = AerialExitAction; }

		public override void CheckSwitchState()
		{
			throw new NotImplementedException();
		}

		public override void EnterState()
		{
			throw new NotImplementedException();
		}

		public override void InitializeSubState()
		{
			throw new NotImplementedException();
		}

		public override void UpdateState()
		{
			throw new NotImplementedException();
		}
	}
}
