using Assets.Scripts.Utility;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
	public class MyButtonInfo
	{
		private readonly bool[] onPressedBuffer;
		private bool inputPressedLastFrame = false;
		private bool inputPressedThisFrame = false;
		public InputNames InputToMonitor { get; private set; }
		public bool InputPressedOnThisFrame { get => inputPressedThisFrame && (!inputPressedLastFrame); }
		public bool InputPressedOnBufferedFrames
		{
			get
			{
				foreach (var item in onPressedBuffer)
					if (item)
						return true;
				return false;
			}
		}

		public MyButtonInfo(InputNames inputToMonitor, uint bufferLength = 10)
		{
			InputToMonitor = inputToMonitor;
			onPressedBuffer = new bool[bufferLength];
		}

		public void Update(PlayerInput input) => inputPressedThisFrame = inputPressedThisFrame || input.IsPressed(InputToMonitor);

		public void AdvanceToNextFrame()
		{
			// Buffer update
			if (onPressedBuffer.Length > 0)
				onPressedBuffer.SlideElementsDown(inputPressedThisFrame);

			inputPressedLastFrame = inputPressedThisFrame;
			inputPressedThisFrame = false;
		}
#if UNITY_EDITOR
		#region Debug Methods
		private uint updates = 0;
		public void Update_DEBUG(PlayerInput input)
		{
			updates++;
			UnityEngine.Debug.Log($"[{InputToMonitor}] #{updates}: {inputPressedThisFrame} || {input.IsPressed(InputToMonitor)} = {inputPressedThisFrame || input.IsPressed(InputToMonitor)}");
			Update(input);
		}

		public void AdvanceToNextFrame_DEBUG()
		{
			updates = 0;
			UnityEngine.Debug.Log($"[{InputToMonitor}] FINAL: {inputPressedThisFrame} && {!inputPressedLastFrame} = {inputPressedThisFrame && (!inputPressedLastFrame)} = {InputPressedOnThisFrame}");
			AdvanceToNextFrame();
		}
		#endregion
#endif
	}
}
