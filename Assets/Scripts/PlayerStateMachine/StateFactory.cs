using System;
using System.Collections;

namespace Assets.Scripts.PlayerStateMachine
{
	public class StateFactory : IEnumerable
	{
		private readonly PlayerContext _ctx;
		public StateFactory(PlayerContext ctx) { _ctx = ctx; }
		public static Tuple<PlayerContext, StateFactory> CreateStateMachine(Player player) => PlayerContext.CreateStateMachine(player);
		public void InitializeStateMachine()
		{
			// Initialize States
			foreach (BaseState state in this)
				state.ReinitializeState();

			// Start up state machine
			_ctx.CurrentBaseState = FallState;
			_ctx.CurrentBaseState.EnterState();
		}

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator() => new FactoryEnumerator(this);
		private class FactoryEnumerator : IEnumerator
		{
			public object Current { get; private set; }
			private readonly StateFactory _factory;
			private readonly BaseState _start;

			public FactoryEnumerator(StateFactory factory)
			{
				_factory = factory;
				_start = GetNextInstantiated(null);
			}

			private BaseState GetNextInstantiated(BaseState current) => current switch
			{
				null => _start ??
						(BaseState)_factory._aerialState ??
						(BaseState)_factory._coilState ??
						(BaseState)_factory._fallState ??
						(BaseState)_factory._groundState ??
						(BaseState)_factory._invincibleState ??
						(BaseState)_factory._jumpState ??
						(BaseState)_factory._rollState ??
						(BaseState)_factory._stumbleState ??
						(BaseState)_factory._wallrunState ??
						null,
				//not PlayerBaseState => throw new System.Exception(),
				PlayerStateMachine.AerialState => (BaseState)_factory._coilState ??
										(BaseState)_factory._fallState ??
										(BaseState)_factory._groundState ??
										(BaseState)_factory._invincibleState ??
										(BaseState)_factory._jumpState ??
										(BaseState)_factory._rollState ??
										(BaseState)_factory._stumbleState ??
										(BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.CoilState => (BaseState)_factory._fallState ??
										(BaseState)_factory._groundState ??
										(BaseState)_factory._invincibleState ??
										(BaseState)_factory._jumpState ??
										(BaseState)_factory._rollState ??
										(BaseState)_factory._stumbleState ??
										(BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.FallState => (BaseState)_factory._groundState ??
										(BaseState)_factory._invincibleState ??
										(BaseState)_factory._jumpState ??
										(BaseState)_factory._rollState ??
										(BaseState)_factory._stumbleState ??
										(BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.GroundState => (BaseState)_factory._invincibleState ??
										(BaseState)_factory._jumpState ??
										(BaseState)_factory._rollState ??
										(BaseState)_factory._stumbleState ??
										(BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.InvincibleState => (BaseState)_factory._jumpState ??
										(BaseState)_factory._rollState ??
										(BaseState)_factory._stumbleState ??
										(BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.JumpState => (BaseState)_factory._rollState ??
										(BaseState)_factory._stumbleState ??
										(BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.RollState => (BaseState)_factory._stumbleState ??
										(BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.StumbleState => (BaseState)_factory._wallrunState ??
										null,
				PlayerStateMachine.WallrunState => null,
				_ => throw new System.NotImplementedException("A class that derives from PlayerBaseState hasn't been covered by the enumerator."),
			};

			public bool MoveNext()
			{
				Current = GetNextInstantiated((BaseState)Current);
				return Current != null;
			}

			public void Reset() => Current = null;
		}
		#endregion

		#region Ground States
		#region GroundState
		private GroundState _groundState;
		public GroundState GroundState
		{
			get
			{
				if (_groundState == null)
					_groundState = new GroundState(_ctx, this);
				return _groundState;
			}
		}
		#endregion
		#region RollState
		private RollState _rollState;
		public RollState RollState
		{
			get
			{
				if (_rollState == null)
					_rollState = new RollState(_ctx, this);
				return _rollState;
			}
		}
		#endregion
		#endregion
		#region Aerial States
		#region AerialState
		private AerialState _aerialState;
		public AerialState AerialState
		{
			get
			{
				if (_aerialState == null)
					_aerialState = new AerialState(_ctx, this);
				return _aerialState;
			}
		}
		#endregion
		#region JumpState
		private JumpState _jumpState;
		public JumpState JumpState
		{
			get
			{
				if (_jumpState == null)
					_jumpState = new JumpState(_ctx, this);
				return _jumpState;
			}
		}
		#endregion
		#region FallState
		private FallState _fallState;
		public FallState FallState
		{
			get
			{
				if (_fallState == null)
					_fallState = new FallState(_ctx, this);
				return _fallState;
			}
		}
		#endregion
		#region CoilState
		private CoilState _coilState;
		public CoilState CoilState
		{
			get
			{
				if (_coilState == null)
					_coilState = new CoilState(_ctx, this);
				return _coilState;
			}
		}
		#endregion
		#region WallrunState
		private WallrunState _wallrunState;
		public WallrunState WallrunState
		{
			get
			{
				if (_wallrunState == null)
					_wallrunState = new WallrunState(_ctx, this);
				return _wallrunState;
			}
		}
		#endregion
		#endregion
		#region Disjoint States
		#region InvincibleState
		private InvincibleState _invincibleState;
		public InvincibleState InvincibleState
		{
			get
			{
				if (_invincibleState == null)
					_invincibleState = new InvincibleState(_ctx, this);
				return _invincibleState;
			}
		}
		#endregion
		#endregion
		#region Other States
		#region StumbleState
		private StumbleState _stumbleState;
		public StumbleState StumbleState
		{
			get
			{
				if (_stumbleState == null)
					_stumbleState = new StumbleState(_ctx, this);
				return _stumbleState;
			}
		}
		#endregion
		#endregion
	}
}
