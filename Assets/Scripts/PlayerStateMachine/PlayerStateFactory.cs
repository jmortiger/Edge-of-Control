namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerStateFactory
	{
		private readonly PlayerStateMachineContext _ctx;
		#region GroundState
		private PlayerGroundState groundState;
		public PlayerGroundState GroundState
		{
			get
			{
				if (groundState == null)
					groundState = new PlayerGroundState(_ctx, this);
				return groundState;
			}
		}
		#endregion
		#region FallState
		private PlayerFallState fallState;
		public PlayerFallState FallState
		{
			get
			{
				if (fallState == null)
					fallState = new PlayerFallState(_ctx, this);
				return fallState;
			}
		}
		#endregion
		#region CoilState
		private PlayerCoilState coilState;
		public PlayerCoilState CoilState
		{
			get
			{
				if (coilState == null)
					coilState = new PlayerCoilState(_ctx, this);
				return coilState;
			}
		}
		#endregion
		#region StumbleState
		private PlayerStumbleState stumbleState;
		public PlayerStumbleState StumbleState
		{
			get
			{
				if (stumbleState == null)
					stumbleState = new PlayerStumbleState(_ctx, this);
				return stumbleState;
			}
		}
		#endregion
		#region JumpState
		private PlayerJumpState jumpState;
		public PlayerJumpState JumpState
		{
			get
			{
				if (jumpState == null)
					jumpState = new PlayerJumpState(_ctx, this);
				return jumpState;
			}
		}
		#endregion
		#region RollState
		private PlayerRollState rollState;
		public PlayerRollState RollState
		{
			get
			{
				if (rollState == null)
					rollState = new PlayerRollState(_ctx, this);
				return rollState;
			}
		}
		#endregion
		#region WallrunState
		private PlayerWallrunState wallrunState;
		public PlayerWallrunState WallrunState
		{
			get
			{
				if (wallrunState == null)
					wallrunState = new PlayerWallrunState(_ctx, this);
				return wallrunState;
			}
		}
		#endregion
		#region InvincibleState
		private PlayerInvincibleState invincibleState;
		public PlayerInvincibleState InvincibleState
		{
			get
			{
				if (invincibleState == null)
					invincibleState = new PlayerInvincibleState(_ctx, this);
				return invincibleState;
			}
		}
		#endregion

		public PlayerStateFactory(PlayerStateMachineContext ctx) { _ctx = ctx; }
	}
}
