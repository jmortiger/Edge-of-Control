using System;
using System.Collections;

namespace Assets.Scripts.PlayerStateMachine
{
	public class PlayerStateFactory : IEnumerable
	{
		private readonly PlayerContext _ctx;
		public PlayerStateFactory(PlayerContext ctx) { _ctx = ctx; }
		public static Tuple<PlayerContext, PlayerStateFactory> CreateStateMachine(Player player) => PlayerContext.CreateStateMachine(player);
		public void InitializeStateMachine()
		{
			#region Initialize States
			//_aerialState	?.ReinitializeState();
			//_coilState		?.ReinitializeState();
			//_fallState		?.ReinitializeState();
			//_groundState	?.ReinitializeState();
			//_invincibleState?.ReinitializeState();
			//_jumpState		?.ReinitializeState();
			//_rollState		?.ReinitializeState();
			//_stumbleState	?.ReinitializeState();
			//_wallrunState	?.ReinitializeState();
			var e = GetEnumerator();
			while (e.MoveNext())
				((PlayerBaseState)e.Current).ReinitializeState();
			foreach (PlayerBaseState state in this)
				state.ReinitializeState();
			#endregion

			// Start up state machine
			_ctx.CurrentBaseState = GroundState;
		}

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator() => new FactoryEnumerator(this);
		private class FactoryEnumerator : IEnumerator
		{
			public object Current { get; private set; }
			private readonly PlayerStateFactory _factory;
			private readonly PlayerBaseState _start;

			public FactoryEnumerator(PlayerStateFactory factory)
			{
				_factory = factory;
				_start = GetNextInstantiated(null);
			}

			private PlayerBaseState GetNextInstantiated(PlayerBaseState current) => current switch
			{
				null => _start ??
						(PlayerBaseState)_factory._aerialState ??
						(PlayerBaseState)_factory._coilState ??
						(PlayerBaseState)_factory._fallState ??
						(PlayerBaseState)_factory._groundState ??
						(PlayerBaseState)_factory._invincibleState ??
						(PlayerBaseState)_factory._jumpState ??
						(PlayerBaseState)_factory._rollState ??
						(PlayerBaseState)_factory._stumbleState ??
						(PlayerBaseState)_factory._wallrunState ??
						null,
				//not PlayerBaseState => throw new System.Exception(),
				PlayerAerialState => (PlayerBaseState)_factory._coilState ??
										(PlayerBaseState)_factory._fallState ??
										(PlayerBaseState)_factory._groundState ??
										(PlayerBaseState)_factory._invincibleState ??
										(PlayerBaseState)_factory._jumpState ??
										(PlayerBaseState)_factory._rollState ??
										(PlayerBaseState)_factory._stumbleState ??
										(PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerCoilState => (PlayerBaseState)_factory._fallState ??
										(PlayerBaseState)_factory._groundState ??
										(PlayerBaseState)_factory._invincibleState ??
										(PlayerBaseState)_factory._jumpState ??
										(PlayerBaseState)_factory._rollState ??
										(PlayerBaseState)_factory._stumbleState ??
										(PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerFallState => (PlayerBaseState)_factory._groundState ??
										(PlayerBaseState)_factory._invincibleState ??
										(PlayerBaseState)_factory._jumpState ??
										(PlayerBaseState)_factory._rollState ??
										(PlayerBaseState)_factory._stumbleState ??
										(PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerGroundState => (PlayerBaseState)_factory._invincibleState ??
										(PlayerBaseState)_factory._jumpState ??
										(PlayerBaseState)_factory._rollState ??
										(PlayerBaseState)_factory._stumbleState ??
										(PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerInvincibleState => (PlayerBaseState)_factory._jumpState ??
										(PlayerBaseState)_factory._rollState ??
										(PlayerBaseState)_factory._stumbleState ??
										(PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerJumpState => (PlayerBaseState)_factory._rollState ??
										(PlayerBaseState)_factory._stumbleState ??
										(PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerRollState => (PlayerBaseState)_factory._stumbleState ??
										(PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerStumbleState => (PlayerBaseState)_factory._wallrunState ??
										null,
				PlayerWallrunState => null,
				_ => throw new System.NotImplementedException("A class that derives from PlayerBaseState hasn't been covered by the enumerator."),
			};

			public bool MoveNext()
			{
				Current = GetNextInstantiated((PlayerBaseState)Current);
				return Current == null;
			}

			public void Reset() => Current = null;
		}
		#endregion

		#region Ground States
		#region GroundState
		private PlayerGroundState _groundState;
		public PlayerGroundState GroundState
		{
			get
			{
				if (_groundState == null)
					_groundState = new PlayerGroundState(_ctx, this);
				return _groundState;
			}
		}
		#endregion
		#region RollState
		private PlayerRollState _rollState;
		public PlayerRollState RollState
		{
			get
			{
				if (_rollState == null)
					_rollState = new PlayerRollState(_ctx, this);
				return _rollState;
			}
		}
		#endregion
		#endregion
		#region Aerial States
		#region AerialState
		private PlayerAerialState _aerialState;
		public PlayerAerialState AerialState
		{
			get
			{
				if (_aerialState == null)
					_aerialState = new PlayerAerialState(_ctx, this);
				return _aerialState;
			}
		}
		#endregion
		#region JumpState
		private PlayerJumpState _jumpState;
		public PlayerJumpState JumpState
		{
			get
			{
				if (_jumpState == null)
					_jumpState = new PlayerJumpState(_ctx, this);
				return _jumpState;
			}
		}
		#endregion
		#region FallState
		private PlayerFallState _fallState;
		public PlayerFallState FallState
		{
			get
			{
				if (_fallState == null)
					_fallState = new PlayerFallState(_ctx, this);
				return _fallState;
			}
		}
		#endregion
		#region CoilState
		private PlayerCoilState _coilState;
		public PlayerCoilState CoilState
		{
			get
			{
				if (_coilState == null)
					_coilState = new PlayerCoilState(_ctx, this);
				return _coilState;
			}
		}
		#endregion
		#region WallrunState
		private PlayerWallrunState _wallrunState;
		public PlayerWallrunState WallrunState
		{
			get
			{
				if (_wallrunState == null)
					_wallrunState = new PlayerWallrunState(_ctx, this);
				return _wallrunState;
			}
		}
		#endregion
		#endregion
		#region Disjoint States
		#region InvincibleState
		private PlayerInvincibleState _invincibleState;
		public PlayerInvincibleState InvincibleState
		{
			get
			{
				if (_invincibleState == null)
					_invincibleState = new PlayerInvincibleState(_ctx, this);
				return _invincibleState;
			}
		}
		#endregion
		#endregion
		#region Other States
		#region StumbleState
		private PlayerStumbleState _stumbleState;
		public PlayerStumbleState StumbleState
		{
			get
			{
				if (_stumbleState == null)
					_stumbleState = new PlayerStumbleState(_ctx, this);
				return _stumbleState;
			}
		}
		#endregion
		#endregion
	}
}
