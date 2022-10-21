using Assets.Scripts.Utility;
using Assets.Scripts.PlayerStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using JMor.Utility;

namespace Assets.Scripts
{
	public enum GameSceneObject
	{
		Main_Camera,
		VCam,
		Goalpost,
		EnemyManager,
		Player,
		Canvas,
	}
	/// <summary>
	/// The names of InputActions.
	/// </summary>
	public enum InputNames
	{
		Move,
		Look,
		Fire,
		DebugReset,
		Aim,
		Jump,//UpAction,
		Boost,
		DownAction,
	}
	public static class MyProjectExtensions
	{
		public static string GetName(this GameSceneObject gso) => gso switch
		{
			GameSceneObject.Main_Camera => "Main Camera",
			GameSceneObject.VCam		=> "CM vcam1",
			GameSceneObject.Goalpost	=> "Goalpost",
			GameSceneObject.EnemyManager=> "EnemyManager",
			GameSceneObject.Player		=> "Player",
			GameSceneObject.Canvas		=> "Canvas(Level)",
			_ => throw new System.ArgumentException(),
		};

		public static string GetName(this InputNames ian) => ian.ToString();

		#region PlayerInput using InputNames
		public static bool WasPressedThisFrame(this PlayerInput input, InputNames actionName) => input.WasPressedThisFrame(actionName.ToString());
		public static bool IsPressed(this PlayerInput input, InputNames actionName) => input.IsPressed(actionName.ToString());
		public static InputAction FindAction(this PlayerInput input, InputNames actionName) => input.FindAction(actionName.ToString());
		public static Vector2 GetActionValueAsJoystick(this PlayerInput input, InputNames actionName, Vector2 relativeTo) => input.GetActionValueAsJoystick(actionName.ToString(), relativeTo);
		#endregion

		public static bool IsInJumpableState(this MovementState movementState)
		{
			return movementState.HasFlag(MovementState.Grounded) && !(movementState.HasFlag(MovementState.Rolling) || movementState.HasFlag(MovementState.Stumbling));
		}
		public static bool IsInCoilableState(this MovementState movementState)
		{
			return (movementState.HasFlag(MovementState.Jumping) || movementState.HasFlag(MovementState.Falling)) && !movementState.HasFlag(MovementState.Coiling);
		}
		public static bool IsInAerialState(this MovementState movementState)
		{
			return movementState.HasFlag(MovementState.Jumping) || movementState.HasFlag(MovementState.Falling);
		}
	}
}
