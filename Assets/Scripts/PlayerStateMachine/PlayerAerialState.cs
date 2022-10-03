using System;

namespace Assets.Scripts.PlayerStateMachine
{
	// TODO: Implement and Integrate AerialState
	public class PlayerAerialState : PlayerBaseState
	{
		private void AerialExitAction()
		{

		}
		public PlayerAerialState(PlayerStateMachineContext ctx, PlayerStateFactory factory) : base(ctx, factory, MovementState.Aerial) { ExitAction = AerialExitAction; }

		//#region Likely Unnecessary
		//public override void CheckSwitchState()
		//{
		//	throw new NotImplementedException();
		//}

		//public override void InitializeSubState()
		//{
		//	throw new NotImplementedException();
		//}
		//#endregion

		public override void EnterState()
		{
			throw new NotImplementedException();
		}

		public override void UpdateState()
		{
			throw new NotImplementedException();
		}
	}
}
