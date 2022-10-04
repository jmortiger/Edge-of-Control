using System;

namespace Assets.Scripts.PlayerStateMachine
{
	// TODO: Implement and Integrate AerialState
	public class PlayerAerialState : PlayerBaseState
	{
		public PlayerAerialState(PlayerContext ctx, PlayerStateFactory factory) : base(ctx, factory, MovementState.Aerial)
		{
			void ExitAerial()
			{
				throw new NotImplementedException();
			}
			ExitAction = ExitAerial;
		}

		#region Abstract Method Implementations
		public override void EnterState()
		{
			throw new NotImplementedException();
		}

		public override void UpdateState()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
