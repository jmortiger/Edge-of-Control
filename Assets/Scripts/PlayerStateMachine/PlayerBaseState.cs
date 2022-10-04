using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerStateMachine
{
	public abstract class PlayerBaseState : IEnumerable
	{
		#region Fields
		private readonly MovementState _stateFlag;
		protected PlayerStateFactory Factory { get; private set; }
		protected PlayerContext Ctx { get; private set; }
		protected Action ExitAction { get; init; }
		public PlayerBaseState SuperState { get; protected set; }
		public PlayerBaseState SubState { get; protected set; }
		#endregion

		public PlayerBaseState(PlayerContext ctx, PlayerStateFactory factory, MovementState stateFlag, Action exitAction = null)
		{
			Ctx = ctx;
			Factory = factory;
			_stateFlag = stateFlag;
			ExitAction = exitAction;
		}

		public abstract void EnterState();
		public abstract void UpdateState();
		public virtual void ReinitializeState()
		{
			SuperState = null;
			SubState = null;
		}
		public void ExitState(StateSwitchBehaviour behaviour = StateSwitchBehaviour.Self)
		{
			if (behaviour != StateSwitchBehaviour.None && (
					behaviour.HasFlag(StateSwitchBehaviour.Self) || 
					behaviour.HasFlag(StateSwitchBehaviour.SelfAndDownstream) || 
					behaviour.HasFlag(StateSwitchBehaviour.All) || (
						behaviour.HasFlag(StateSwitchBehaviour.AllButBase) && 
						this != Ctx.CurrentBaseState && 
						this != Ctx.CurrentDisjointState)))
				Debug.Assert(Ctx.movementState.HasFlag(_stateFlag), $"Exiting a state that's not active.");
			// TODO: Disjoint states currently don't support sub/super states.
			if (Ctx.CurrentDisjointState == this &&
				behaviour != StateSwitchBehaviour.None &&
				(behaviour.HasFlag(StateSwitchBehaviour.Self) || behaviour.HasFlag(StateSwitchBehaviour.SelfAndDownstream) || behaviour.HasFlag(StateSwitchBehaviour.All)))
			{
				ExitAction?.Invoke();
				Ctx.movementState ^= _stateFlag;
				SuperState = null;
				SubState = null;
				Ctx.CurrentDisjointState = null;
				return;
			}
			switch (behaviour)
			{
				case StateSwitchBehaviour.Self:
				{
					ExitAction?.Invoke();
					Ctx.movementState ^= _stateFlag;
					SuperState = null;
					// TODO: Have SelfAndAllDownstream fallthrough here.
					/*if (behaviour.HasFlag())*/ SubState = null;
					break;
				}
				case StateSwitchBehaviour.SelfAndDownstream:
				{
					ExitAction?.Invoke();
					Ctx.movementState ^= _stateFlag;
					SuperState = null;
					goto case StateSwitchBehaviour.Downstream;
				}
				case StateSwitchBehaviour.Downstream:
				{
					SubState?.ExitState(StateSwitchBehaviour.SelfAndDownstream);
					SubState = null;
					break;
				}
				case StateSwitchBehaviour.AllButBase:
				{
					if (SuperState == null) // If this is the root
						goto case StateSwitchBehaviour.Downstream;
					else // If this isn't the root
						SuperState.ExitState(behaviour); // Pass up the chain, so they can tell substates to kill downstream
					break;
				}
				case StateSwitchBehaviour.All:
				{
					if (SuperState == null) // If this is the root
						goto case StateSwitchBehaviour.SelfAndDownstream;
					else // If this isn't the root
						SuperState.ExitState(behaviour); // Pass up the chain, so they can tell substates to kill downstream
					break;
				}
				case StateSwitchBehaviour.None:
				default:
				{
					//goto case StateSwitchBehaviour.Self;
					Debug.Log("StateSwitchBehaviour.None");
					break;
				}
			}
		}

		#region SwitchState
		/// <summary>
		/// 
		/// </summary>
		/// <param name="newState">The state to switch to. Can pass in null to correctly change the pointers to this state in the super/sub states, which <see cref="ExitState(StateSwitchBehaviour)"/> doesn't do.</param>
		/// <param name="behaviour"></param>
		protected void SwitchState(PlayerBaseState newState, StateSwitchBehaviour behaviour = StateSwitchBehaviour.SelfAndDownstream)
		{
			// Reconfigure sub/super states
			switch (behaviour)
			{
				case StateSwitchBehaviour.Self:
				{
					if (SubState != null)
					{
						SubState.SuperState = newState;
						if (newState != null)
							newState.SubState = SubState;
					}
					goto case StateSwitchBehaviour.SelfAndDownstream;
				}
				case StateSwitchBehaviour.SelfAndDownstream:
				{
					if (SuperState != null)
					{
						SuperState.SubState = newState;
						if (newState != null)
							newState.SuperState = SuperState;
					}
					if (Ctx.CurrentBaseState == this)
						Ctx.CurrentBaseState = newState;
					break;
				}
				case StateSwitchBehaviour.Downstream:
				{
					if (newState != null)
						newState.SuperState = this;
					break;
				}
				case StateSwitchBehaviour.All:
				{
					if (SuperState != null)
					{
						SuperState.SwitchState(newState, behaviour);
						return;
					}
					else
						goto case StateSwitchBehaviour.SelfAndDownstream;
				}
				case StateSwitchBehaviour.AllButBase:
				{
					if (SuperState != null)
					{
						SuperState.SwitchState(newState, behaviour);
						return;
					}
					else
						goto case StateSwitchBehaviour.Downstream;
				}
				case StateSwitchBehaviour.None:
				default:
				{
					Debug.LogWarning("What are you doing?");
					break;
				}
			}
			ExitState(behaviour); // ExitState clears sub & super state references, must go after reconfig above.
			if (behaviour == StateSwitchBehaviour.Downstream) // Needs to be here to avoid disconnection and an Sub/SuperState.ExitState call in ExitState.
				SubState = newState;
			newState?.EnterState();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="newStates">The states to switch to. Must include at least 1 non-null value.</param>
		/// <param name="behaviour"></param>
		/// <exception cref="ArgumentNullException"> if <paramref name="newStates"/> is null or all values in <paramref name="newStates"/> are null.</exception>
		protected void SwitchState(PlayerBaseState[] newStates, StateSwitchBehaviour behaviour = StateSwitchBehaviour.SelfAndDownstream)
		{
			if (newStates != null && newStates.Length > 0)
			{
				// Get the index of the first element that isn't null.
				int lastValidIndex = 0;
				while (newStates[lastValidIndex] == null && lastValidIndex < newStates.Length)
					lastValidIndex++;
				// Switch to it if found.
				if (lastValidIndex < newStates.Length)
					SwitchState(newStates[0], behaviour);
				else
					throw new ArgumentNullException();
				// Do the rest.
				for (int i = lastValidIndex + 1; i < newStates.Length; i++)
				{
					if (newStates[i] != null)
					{
						newStates[lastValidIndex].SwitchState(newStates[i], StateSwitchBehaviour.Downstream);
						lastValidIndex = i;
					}
				}
			}
			else
				throw new ArgumentNullException();
		}
		#endregion

		#region Non-Core Stuff
		protected bool CanBoostJump()
		{
			return Ctx.JumpButton.InputPressedOnThisFrame &&
					Ctx.Input.IsPressed(InputNames.Boost) &&
					Ctx.boostConsumable &&
					Ctx.MyPlayer.boostMeter >= Ctx.MvmtSettings.boostJumpCost;
		}

		//protected virtual bool TryBoostJumping()
		//{
		//	if (CanBoostJump())
		//	{
		//		// If falling, zero out vertical velocity so the jump isn't fighting the downward momentum.
		//		if (Ctx.Velocity.y < 0)
		//		{
		//			var vel = Ctx.Velocity;
		//			vel.y = 0;
		//			Ctx.Rb.velocity = vel;
		//		}
		//		//aSource.Stop();// Should I do this?
		//		//particleSystem.Play();// Should I do this?
		//		//EnterJumping(moveVector);// Pass this responsibility off to the caller.
		//		Ctx.MyPlayer.UpdateBoostMeter(-Ctx.MvmtSettings.boostJumpCost);
		//		Ctx.boostConsumable = false;
		//		return true;
		//	}
		//	return false;
		//}

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator() => new PlayerBaseStateEnumerator(this);
		class PlayerBaseStateEnumerator : IEnumerator
		{
			private readonly PlayerBaseState original;
			public object Current { get; private set; }

			public PlayerBaseStateEnumerator(PlayerBaseState original)
			{
				this.original = original ?? throw new ArgumentNullException();
				Current = null;
			}

			public bool MoveNext()
			{
				Current = (Current == null) ? original : ((PlayerBaseState)Current).SubState;
				return Current != null;
			}

			public void Reset() => Current = null;
		}
		#endregion
		#endregion
	}

	public enum StateSwitchBehaviour
	{
		None,
		/// <summary>
		/// Attempts to exit itself, and remove itself from the chain of states, connecting its super and sub state directly.
		/// </summary>
		Self,
		/// <summary>
		/// Attempts to have all substates exit, while not exiting itself.
		/// </summary>
		Downstream,
		/// <summary>
		/// Attempts to exit itself, and have all substates exit.
		/// </summary>
		SelfAndDownstream,
		/// <summary>
		/// Attempts to have all sub and super states in the chain exit, with the exception of the base state.
		/// </summary>
		/// <remarks>May cause desyncs if <see cref="PlayerBaseState.SuperState"/> == null and <see cref="PlayerBaseState.this"/> != <see cref="PlayerBaseState._ctx.CurrentBaseState"/>.</remarks>
		AllButBase,
		/// <summary>
		/// Attempts to have all sub and super states in the chain exit, with no exceptions.
		/// </summary>
		/// <remarks>Must be used with either <see cref="PlayerBaseState.SwitchState(PlayerBaseState, StateSwitchBehaviour)"/> or by manually setting the new <see cref="PlayerContext.CurrentBaseState"/> after usage.</remarks>
		All,
	}
}
