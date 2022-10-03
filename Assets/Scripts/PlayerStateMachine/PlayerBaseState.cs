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
		protected PlayerStateMachineContext Ctx { get; private set; }
		protected Action ExitAction { get; set; }
		public PlayerBaseState SuperState { get; protected set; }
		public PlayerBaseState SubState { get; protected set; }
		#endregion

		public PlayerBaseState(PlayerStateMachineContext ctx, PlayerStateFactory factory, MovementState stateFlag, Action exitAction = null)
		{
			Ctx = ctx;
			Factory = factory;
			_stateFlag = stateFlag;
			ExitAction = exitAction;
		}

		public abstract void EnterState();
		public virtual void ExitState(StateSwitchBehaviour behaviour = StateSwitchBehaviour.Self)
		{
			// TODO: Disjoint states currently don't support sub/super state.
			if ((PlayerBaseState)Ctx.CurrentDisjointState == this && !behaviour.HasFlag(StateSwitchBehaviour.None))
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
					SubState = null;
					break;
				}
				case StateSwitchBehaviour.SelfAndAllDownstream:
				{
					ExitAction?.Invoke();
					Ctx.movementState ^= _stateFlag;
					SuperState = null;
					goto case StateSwitchBehaviour.AllDownstream;
				}
				case StateSwitchBehaviour.AllDownstream:
				{
					SubState?.ExitState(StateSwitchBehaviour.SelfAndAllDownstream);
					SubState = null;
					break;
				}
				case StateSwitchBehaviour.AllButBase:
				{
					if (SuperState == null) // If this is the root
						goto case StateSwitchBehaviour.AllDownstream;
					else // If this isn't the root
						SuperState.ExitState(behaviour); // Pass up the chain, so they can tell substates to kill downstream
					break;
				}
				case StateSwitchBehaviour.All:
				{
					if (SuperState == null) // If this is the root
						goto case StateSwitchBehaviour.SelfAndAllDownstream;
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
		public abstract void UpdateState();
		public abstract void CheckSwitchState();
		public abstract void InitializeSubState();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newState">The state to switch to.</param>
		/// <param name="behaviour"></param>
		protected void SwitchState(PlayerBaseState newState, StateSwitchBehaviour behaviour = StateSwitchBehaviour.SelfAndAllDownstream)
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
					goto case StateSwitchBehaviour.SelfAndAllDownstream;
				}
				case StateSwitchBehaviour.SelfAndAllDownstream:
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
				case StateSwitchBehaviour.AllDownstream:
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
						goto case StateSwitchBehaviour.SelfAndAllDownstream;
				}
				case StateSwitchBehaviour.AllButBase:
				{
					if (SuperState != null)
					{
						SuperState.SwitchState(newState, behaviour);
						return;
					}
					else
						goto case StateSwitchBehaviour.AllDownstream;
				}
				case StateSwitchBehaviour.None:
				default:
				{
					Debug.LogWarning("What are you doing?");
					break;
				}
			}
			ExitState(behaviour); // ExitState clears sub & super state references, must go after reconfig above.
			if (behaviour == StateSwitchBehaviour.AllDownstream) // Needs to be here to avoid disconnection and an ExitState call in ExitState.
				SubState = newState;
			newState?.EnterState();
		}
		protected void SwitchState(PlayerBaseState[] newStates, StateSwitchBehaviour behaviour = StateSwitchBehaviour.SelfAndAllDownstream)
		{
			if (newStates != null && newStates.Length > 0)
			{
				// Get the index of the first element that isn't null.
				int lastGoodIndex = 0;
				while (newStates[lastGoodIndex] == null && lastGoodIndex < newStates.Length)
					lastGoodIndex++;
				// Switch to it if found.
				if (lastGoodIndex < newStates.Length)
					SwitchState(newStates[0], behaviour);
				else
					throw new ArgumentNullException();
				// Do the rest.
				for (int i = lastGoodIndex + 1; i < newStates.Length; i++)
				{
					if (newStates[i] != null)
					{
						newStates[lastGoodIndex].SwitchState(newStates[i], StateSwitchBehaviour.AllDownstream);
						lastGoodIndex = i;
					}
				}
			}
			else
				throw new ArgumentNullException();
		}

		protected bool CanBoostJump()
		{
			return	Ctx.JumpButton.InputPressedOnThisFrame &&
					Ctx.Input.IsPressed(InputNames.Boost) &&
					Ctx.boostConsumable &&
					Ctx.MyPlayer.boostMeter >= Ctx.MvmtSettings.boostJumpCost;
		}

		protected virtual bool TryBoostJumping()
		{
			if (CanBoostJump())
			{
				// If falling, zero out vertical velocity so the jump isn't fighting the downward momentum.
				if (Ctx.Velocity.y < 0)
				{
					var vel = Ctx.Velocity;
					vel.y = 0;
					Ctx.Rb.velocity = vel;
				}
				//aSource.Stop();// Should I do this?
				//particleSystem.Play();// Should I do this?
				//EnterJumping(moveVector);// Pass this responsibility off to the caller.
				Ctx.MyPlayer.UpdateBoostMeter(-Ctx.MvmtSettings.boostJumpCost);
				Ctx.boostConsumable = false;
				return true;
			}
			return false;
		}

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator()
		{
			return new PlayerBaseStateEnumerator(this);
		}
		class PlayerBaseStateEnumerator : IEnumerator
		{
			private readonly PlayerBaseState original;
			public object Current { get; private set; }

			public PlayerBaseStateEnumerator(PlayerBaseState original)
			{
				this.original = original;
				Current = null;
			}

			public bool MoveNext()
			{
				Current = (Current == null) ? Current = original : ((PlayerBaseState)Current).SubState;
				return Current == null;
			}

			public void Reset() => Current = null;
		}
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
		AllDownstream,
		/// <summary>
		/// Attempts to exit itself, and have all substates exit.
		/// </summary>
		SelfAndAllDownstream,
		/// <summary>
		/// Attempts to have all sub and super states in the chain exit, with the exception of the base state.
		/// </summary>
		/// <remarks>May cause desyncs if <see cref="PlayerBaseState.SuperState"/> == null and <see cref="PlayerBaseState.this"/> != <see cref="PlayerBaseState._ctx.CurrentBaseState"/>.</remarks>
		AllButBase,
		/// <summary>
		/// Attempts to have all sub and super states in the chain exit, with no exceptions.
		/// </summary>
		/// <remarks>Must be used with either <see cref="PlayerBaseState.SwitchState(PlayerBaseState, StateSwitchBehaviour)"/> or by manually setting the new <see cref="PlayerStateMachineContext.CurrentBaseState"/> after usage.</remarks>
		All,
	}
}
