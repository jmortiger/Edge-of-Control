using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class InputSystemHeldTester : MonoBehaviour
	{
		public PlayerInput input;

		bool jumpPressedLastFrame = false;
		bool jumpPressedThisFrame = false;
		bool JumpedThisFrame { get => jumpPressedThisFrame && (!jumpPressedLastFrame); }
		void Update()
		{
			jumpPressedThisFrame = jumpPressedThisFrame || /*Keyboard.current.spaceKey.isPressed*/IsPressed(input, "Jump");
			Debug.Log($"jumpPressedThisFrame: {jumpPressedThisFrame}, jumpPressedLastFrame: {jumpPressedLastFrame}");
			// Column aligned
			//Debug.Log($"jumpPressedThisFrame: {(jumpPressedThisFrame ? 1 : 0)}, jumpPressedLastFrame: {(jumpPressedLastFrame ? 1 : 0)}");
		}

		public uint jumpPresses = 0;
		void FixedUpdate()
		{
			// Debug check for buggy inputs
			if (JumpedThisFrame)
			{
				jumpPresses++;
				Debug.Log($"JumpPressedOnThisFrame");
				Debug.Log($"{jumpPresses} jump presses");
			}

			// Do other important stuff...
			if (JumpedThisFrame)
			{
				GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 7f), ForceMode2D.Impulse);
			}
			// Do other important stuff...

			// After we're done using them, reset jump inputs
			jumpPressedLastFrame = jumpPressedThisFrame;
			jumpPressedThisFrame = false;
		}
		static bool IsPressed(PlayerInput input, string actionNameOrId)
		{
			return input.actions.FindAction(actionNameOrId)?./*activeControl?.*/IsPressed() ?? false;
		}
	}
}