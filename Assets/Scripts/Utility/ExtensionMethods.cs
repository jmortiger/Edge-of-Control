using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Assets.Scripts.Utility
{
	public static class ExtensionMethods
	{
		#region Vector Helpers
		public static bool IsFinite(this Vector2 v) => float.IsFinite(v.x) && float.IsFinite(v.y);
		public static bool IsFinite(this Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);

		public static Vector2 RotateTowards(this Vector2 from, Vector2 to, float maxRadiansDelta, float maxMagnitudeDelta = 0f)
		{
			return Vector3.RotateTowards(from, to, maxRadiansDelta, maxMagnitudeDelta);
		}
		#endregion

		#region PlayerInput Helpers
		public static bool WasPressedThisFrame(this PlayerInput input, string actionNameOrId)
		{
			return ((ButtonControl)input.actions.FindAction(actionNameOrId)?.activeControl)?.wasPressedThisFrame ?? false;
		}

		public static bool IsPressed(this PlayerInput input, string actionNameOrId)
		{
			//if (input.actions.FindAction(actionNameOrId)?.IsPressed() == null)
			//	Debug.Log($"Bad val of input");
			return input.actions.FindAction(actionNameOrId)?.IsPressed()/*activeControl?.IsPressed()*/ ?? false;
		}

		public static InputAction FindAction(this PlayerInput input, string actionNameOrId) => input.actions.FindAction(actionNameOrId);

		public static Vector2 GetActionValueAsJoystick(this PlayerInput input, string actionNameOrId, Vector2 relativeTo)
		{
			var control = input.actions.FindAction(actionNameOrId);
			var val = control.ReadValue<Vector2>();
			return ((control?.activeControl?.device is Mouse) ? 
				((Vector2)Camera.main.ScreenToWorldPoint(val) - relativeTo) : 
				val).normalized;
		}
		public static Vector2 GetRightStickOrMouseValueAsJoystickEditor(this PlayerInput input, Vector2 relativeTo)
		{
			return ((Gamepad.current != null) ? 
				Gamepad.current.rightStick.ReadValue() : 
				(Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - relativeTo).normalized;
		}
		#endregion

		public static Bounds OrthographicBoundsByScreen(this Camera camera)
		{
			float screenAspect = (float)Screen.width / (float)Screen.height;
			float cameraHeight = camera.orthographicSize * 2;
			Bounds bounds = new(
				camera.transform.position,
				new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
			return bounds;
		}
		public static Bounds OrthographicBoundsByCamera(this Camera camera)
		{
			//float screenAspect = (float)Screen.width / (float)Screen.height;
			float cameraHeight = camera.orthographicSize * 2;
			Bounds bounds = new(
				camera.transform.position,
				new Vector3(cameraHeight * camera.aspect/*screenAspect*/, cameraHeight, 0));
			return bounds;
		}

		public static void LogQualifiedName(this Type type)
		{
			Debug.Log($"{type.AssemblyQualifiedName}, {type.Assembly}");
		}

		#region Array manip
		#region SlideDown
		public static void SlideElementsDown<T>(this T[] source, T defaultValue, out T[] output, uint indexesToSlideDown = 1)
		{
			if (indexesToSlideDown > source.Length)
				throw new ArgumentException("indexesToSlideDown is not <= source.Length");
			output = new T[source.Length];
			for (int i = source.Length - 1; i >= 0; i--)
				output[i] = (i - indexesToSlideDown < 0) ? defaultValue : source[i - indexesToSlideDown];
		}
		public static void SlideElementsDown<T>(this T[] source, T defaultValue, uint indexesToSlideDown = 1)
		{
			if (indexesToSlideDown > source.Length)
				throw new ArgumentException("indexesToSlideDown is not <= source.Length");
			for (int i = source.Length - 1; i >= 0; i--)
				source[i] = (i - indexesToSlideDown < 0) ? defaultValue : source[i - indexesToSlideDown];
		}
		#endregion
		#region SlideUp
		public static void SlideElementsUp<T>(this T[] source, T defaultValue, out T[] output, uint indexesToSlideUp = 1)
		{
			if (indexesToSlideUp > source.Length)
				throw new ArgumentException("indexesToSlideUp is not <= source.Length");
			output = new T[source.Length];
			for (int i = 0; i < source.Length; i++)
				output[i] = (i + indexesToSlideUp > source.Length) ? defaultValue : source[i + indexesToSlideUp];
		}
		public static void SlideElementsUp<T>(this T[] source, T defaultValue, uint indexesToSlideUp = 1)
		{
			if (indexesToSlideUp > source.Length)
				throw new ArgumentException("indexesToSlideUp is not <= source.Length");
			for (int i = 0; i < source.Length; i++)
				source[i] = (i + indexesToSlideUp > source.Length) ? defaultValue : source[i + indexesToSlideUp];
		}
		#endregion
		#endregion
	}
}