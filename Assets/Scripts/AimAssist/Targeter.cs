using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Assets.Scripts.Utility;

namespace Assets.Scripts.AimAssist
{
	[RequireComponent(typeof(PlayerInput))]
	public class Targeter : MonoBehaviour
	{
		public float TargetAngle { get; private set; }
		public Vector2 TargetDirection { get; private set; }
		public Quaternion TargetOrientation { get; private set; }
		public PlayerInput input;
		void Reset()
		{
			input = GetComponent<PlayerInput>();
		}
		private Func<float, float> currAngleInterpolater;
		void Update()
		{
			var targets = FindObjectsOfType<Targetable>();
			var targetRanges = new Vector2[targets.Length];
			for (int i = 0; i < targets.Length; i++)
				targetRanges[i] = targets[i].GetInputRange(transform.position, Vector2.right);
			var interpolater = GetAngleInterpolater(targetRanges);
			currAngleInterpolater = interpolater;
			var aimAction = input.actions.FindAction("Aim");
			var rawInputDirection = aimAction.ReadValue<Vector2>();
			if (aimAction?.activeControl?.device is Mouse)// Unity doesn't support actions in Edit mode
				rawInputDirection = (Vector2)Camera.main.ScreenToWorldPoint(rawInputDirection) - (Vector2)transform.position;
			if (rawInputDirection == Vector2.zero)
			{
				//return;
				TargetAngle = 0f;
				TargetOrientation/*transform.rotation*/ = Quaternion.identity;
				TargetDirection = Vector2.zero;
			}
			var angle = MyMath.SignedAngle(Vector2.right, rawInputDirection);
			TargetAngle = interpolater(angle);
			TargetOrientation/*transform.rotation*/ = Quaternion.Euler(0, 0, TargetAngle);
			TargetDirection = TargetOrientation * Vector2.right;
		}

		public Func<float, float> GetAngleInterpolater(Vector2[] targetRanges)
		{
			float[] inputs, outputs;
			MutateInputRanges(targetRanges, out inputs, out outputs);
			#region Handle looping range
			// To handle angles < 0 and > 360, duplicate angles with offsets of +/-360
			var xsList = new List<float>(inputs.Length * 2);
			var ysList = new List<float>(outputs.Length * 2);
			xsList.AddRange(inputs);
			ysList.AddRange(outputs);
			for (int i = 0; i < inputs.Length; i++)
			{
				xsList.Add(xsList[i] - 360f);
				ysList.Add(ysList[i] - 360f);
				xsList.Add(xsList[i] + 360f);
				ysList.Add(ysList[i] + 360f);
			}
			inputs = xsList.ToArray();
			outputs = ysList.ToArray();
			#endregion
			return MyMath.ConstructInterpolaterFunction(inputs, outputs);
		}

		public void MutateInputRanges(Vector2[] xMinMaxes, out float[] inputs, out float[] outputs)
		{
			var inputsList = new List<float>();
			var outputsList = new List<float>();
			#region Old: Didn't handle range overlap
			//for (int i = 0; i < xMinMaxes.Length; i++)
			//{
			//	//Debug.Log($"targetRange:{xMinMaxes[i]}");
			//	//Debug.Log($"Mutate: {xMinMaxes[i].x}, {xMinMaxes[i].y}");
			//	var mutated = MutateInputRange(xMinMaxes[i].x, xMinMaxes[i].y);
			//	for (int j = 0; j < mutated.Length; j++)
			//	{
			//		//Debug.Log($"Mutated[{j}]:{mutated[j]}");
			//		inputsList.Add(mutated[j].x);
			//		outputsList.Add(mutated[j].y);
			//	}
			//}
			#endregion
			#region New: Handles domain overlap
			//// TODO: Handle overlapping domain; Make more robust
			Array.Sort(xMinMaxes, new CompareVector2ByX());
			for (int i = 0; i < xMinMaxes.Length; i++)
			{
				var mutated = MutateInputRange(xMinMaxes[i].x, xMinMaxes[i].y);
				for (int j = 0; j < mutated.Length; j++)
				{
					inputsList.Add(mutated[j].x);
					outputsList.Add(mutated[j].y);
				}
			}

			#region Shrink by set
			// TODO: Finish
			//var numInputSets = xMinMaxes.Length;
			//var valsPerSetFloat = (float)inputsList.Count / (float)xMinMaxes.Length;
			//var valsPerSet = inputsList.Count / xMinMaxes.Length;
			//valsPerSet = (valsPerSet == valsPerSetFloat) ? valsPerSet : -1;
			//for (int i = 0; i < numInputSets; i++)
			//{
			//	//var max
			//	for (int j = 0; j < valsPerSet; j++)
			//	{

			//	}
			//}
			#endregion
			//var flag = false;
			for (int i = 1; i < inputsList.Count; i++)
			{
				if (inputsList[i - 1] == inputsList[i])
				{
					outputsList[i - 1] = (outputsList[i - 1] + outputsList[i]) / 2f;
					inputsList.RemoveAt(i);
					outputsList.RemoveAt(i);
					i--;
				}
				else if (inputsList[i - 1] > inputsList[i]/* && outputsList[i - 1] > outputsList[i]*/)
				{
					var inputMid = (inputsList[i - 1] + inputsList[i]) / 2f;
					//outputsList[i - 1] = (outputsList[i - 1] + outputsList[i]) / 2f;
					var t = inputsList[i - 1];
					inputsList[i - 1] = (inputMid + inputsList[i]) / 2f;
					inputsList[i] = (inputMid + t) / 2f;
					//i--;// Keep on or off?
					//outputsList[i - 1] = (outputsList[i - 1] + outputsList[i]) / 2f;
					//inputsList[i - 1] = (inputsList[i - 1] + inputsList[i]) / 2f;
					//inputsList.RemoveAt(i);
					//outputsList.RemoveAt(i);
					//i--;
				}
			}
			//for (int i = 1; flag && i < inputsList.Count; i++)
			//{
			//	if (inputsList[i - 1] == inputsList[i])
			//	{
			//		outputsList[i - 1] = (outputsList[i - 1] + outputsList[i]) / 2f;
			//		inputsList.RemoveAt(i);
			//		outputsList.RemoveAt(i);
			//		i--;
			//	}
			//	else if (inputsList[i - 1] > inputsList[i]/* && outputsList[i - 1] > outputsList[i]*/)
			//	{
			//		var inputMid = (inputsList[i - 1] + inputsList[i]) / 2f;
			//		//outputsList[i - 1] = (outputsList[i - 1] + outputsList[i]) / 2f;
			//		var t = inputsList[i - 1];
			//		inputsList[i - 1] = (inputMid + inputsList[i]) / 2f;
			//		inputsList[i] = (inputMid + t) / 2f;
			//		//i--;// Keep on or off?
			//		//outputsList[i - 1] = (outputsList[i - 1] + outputsList[i]) / 2f;
			//		//inputsList[i - 1] = (inputsList[i - 1] + inputsList[i]) / 2f;
			//		//inputsList.RemoveAt(i);
			//		//outputsList.RemoveAt(i);
			//		//i--;
			//	}
			//}
			#endregion

			// If there's only 1 target range, add a counter point to force 
			// the output towards the input when farthest from the target range.
			if (xMinMaxes.Length == 1)
			{
				var mid = (xMinMaxes[0].x + xMinMaxes[0].y) / 2f;
				mid += 180f;
				//if (mid >= 360)
				//	mid -= 360f;
				if (mid >= 360 || mid < 0)
					mid += (mid >= 360) ? -360f : 360f;
				inputsList.Add(mid);
				outputsList.Add(mid);
			}
			//else
			//{
			//	var min = new float[xMinMaxes.Length];
			//	var max = new float[xMinMaxes.Length];
			//	for (int i = 0; i < xMinMaxes.Length; i++)
			//	{
			//		min[i] = xMinMaxes[i].x;
			//		max[i] = xMinMaxes[i].y;
			//	}
			//	Array.Sort(min, max);
			//	var mid = (min[0] + max[^1]) / 2f;
			//	mid += 180f;
			//	if (mid > 360)
			//		mid -= 360f;
			//	inputsList.Add(mid);
			//	outputsList.Add(mid);
			//	mid += 360f;
			//	inputsList.Add(mid);
			//	outputsList.Add(mid);
			//}

			#region Migrated to MyMath
			//// To handle angles < 0 and > 360, duplicate angles with offsets of +/-360
			//var length = inputsList.Count;
			//for (int i = 0; i < length; i++)
			//{
			//	inputsList.Add(inputsList[i] - 360f);
			//	outputsList.Add(outputsList[i] - 360f);
			//	inputsList.Add(inputsList[i] + 360f);
			//	outputsList.Add(outputsList[i] + 360f);
			//}
			#endregion

			inputs = inputsList.ToArray();
			outputs = outputsList.ToArray();
		}
		[Tooltip("The amount of INPUT_CORRECTION_RANGE to use for strong input correction.")]
		public /*const */float INPUT_RANGE_AMOUNT = .75f;
		[Tooltip("The total range of input correction.")]
		public /*const */float INPUT_CORRECTION_RANGE = 2.5f;
		/// <summary>
		/// Constructs an array of inputs and outputs for the interpolator by mutating the given range by its set cofiguration.
		/// </summary>
		/// <param name="xInputMin"></param>
		/// <param name="xInputMax"></param>
		/// <returns></returns>
		/// <remarks>
		/// This is where the core behaviour of the algorithm is defined. 
		/// Cannot mutate the output (Vector2.y), as <see cref="MutateInputRanges(Vector2[], out float[], out float[])"/> 
		/// uses this before mutation to prevent overlap.
		/// </remarks>
		public Vector2[] MutateInputRange(float xInputMin, float xInputMax)
		{
			// Edge case: Values are identical
			if (xInputMax == xInputMin)
				return new Vector2[] { new Vector2(xInputMin, xInputMax) };
			var inputOutputArray = new Vector2[/*3*/7];
			var mid = (xInputMax + xInputMin) / 2f;
			var step = (xInputMax - xInputMin) / 2f;
			inputOutputArray[0] = new Vector2(mid - step * (INPUT_CORRECTION_RANGE + .01f), mid - step * (INPUT_CORRECTION_RANGE + .01f));
			inputOutputArray[1 + 0] = new Vector2(mid - step * INPUT_CORRECTION_RANGE, mid - step * INPUT_CORRECTION_RANGE);
			inputOutputArray[1 + 0 + 1] = new Vector2(mid - step * INPUT_RANGE_AMOUNT * INPUT_CORRECTION_RANGE, xInputMin);
			inputOutputArray[1 + 1 + 1] = new Vector2(mid, mid);
			inputOutputArray[1 + 2 + 1] = new Vector2(mid + step * INPUT_RANGE_AMOUNT * INPUT_CORRECTION_RANGE, xInputMax);
			inputOutputArray[1 + 4] = new Vector2(mid + step * INPUT_CORRECTION_RANGE, mid + step * INPUT_CORRECTION_RANGE);
			inputOutputArray[^1] = new Vector2(mid + step * (INPUT_CORRECTION_RANGE + .01f), mid + step * (INPUT_CORRECTION_RANGE + .01f));
			return inputOutputArray;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawRay(transform.position, Vector2.right * 3f);
			//var aimAction = input.actions.FindAction("Aim");
			//var rawInputDirection = aimAction.ReadValue<Vector2>();// Unity doesn't support actions in Edit mode
			//Debug.Log($"output: {aimAction.ReadValue<Vector2>()}");
			var rawInputDirection = (Gamepad.current != null) ? Gamepad.current.leftStick.ReadValue() : MouseHelper.ScreenPosition;
			var inputWorldMagnitude = 4f;
			//if (aimAction?.activeControl?.device is Mouse)// Unity doesn't support actions in Edit mode
			if (Gamepad.current == null)
			{
				Debug.Log("Gamepad.current = null; using mouse input.");
				rawInputDirection = (Vector2)Camera.main.ScreenToWorldPoint(rawInputDirection) - (Vector2)transform.position;
				inputWorldMagnitude = rawInputDirection.magnitude;
			}
			Debug.Log($"AimInput:{rawInputDirection}");
			rawInputDirection.Normalize();
			Debug.Log($"AimInputNormalized:{rawInputDirection}");
			// If there's no direction (i.e. Joystick neutral), avoid errors by setting to a neutral value?
			//if (rawInputDirection == Vector2.zero)
			//	rawInputDirection = Vector2.right;
			// If there's no direction (i.e. Joystick neutral), exit early
			if (rawInputDirection == Vector2.zero)
				return;
			var angle = Vector2.SignedAngle(Vector2.right, rawInputDirection);
			var calculatedInputDir = (Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.right);
			angle = (angle < 0) ? angle + 360 : angle;
			Debug.Log($"InputAngle: {angle}");
			if (calculatedInputDir == Vector2.right && rawInputDirection == Vector2.zero)
				calculatedInputDir = Vector2.zero;
			// TODO: Work on buggy asserts
			if (Vector2.Distance(calculatedInputDir, rawInputDirection) > Vector2.kEpsilon * 10)
				if (!(calculatedInputDir == Vector2.right && rawInputDirection == Vector2.zero))
					Debug.LogWarning($"Failed Assert: {calculatedInputDir} != {rawInputDirection}, it should");

			Gizmos.DrawSphere(rawInputDirection * inputWorldMagnitude + (Vector2)transform.position, 1);
			Gizmos.DrawLine(transform.position, transform.position + (Vector3)calculatedInputDir * inputWorldMagnitude);
			// TODO: Work on buggy asserts
			if (Vector2.Distance((Vector2)transform.position + calculatedInputDir * inputWorldMagnitude, rawInputDirection * inputWorldMagnitude + (Vector2)transform.position) > Vector2.kEpsilon * 10)
				Debug.LogWarning($"Failed Assert: {(Vector2)transform.position + calculatedInputDir * inputWorldMagnitude} != {rawInputDirection * inputWorldMagnitude + (Vector2)transform.position}, it should");

			var targets = FindObjectsOfType<Targetable>();
			var targetRanges = new Vector2[targets.Length];
			for (int i = 0; i < targets.Length; i++)
				targetRanges[i] = targets[i].GetInputRange(transform.position, Vector2.right);

			MutateInputRanges(targetRanges, out float[] inputs, out float[] outputs);
			for (int i = 0; i < inputs.Length; i++)
			{
				var positInput = (Vector2)(Quaternion.Euler(0, 0, inputs[i]) * Vector2.right);
				positInput = transform.position + (Vector3)positInput * /*inputWorldMagnitude*/4f;
				Gizmos.color = new Color(0, 1, 0, .5f);
				Gizmos.DrawSphere(new Vector3(positInput.x, positInput.y, transform.position.z), .1f);
				var positOutput = (Vector2)(Quaternion.Euler(0, 0, outputs[i]) * Vector2.right);
				positOutput = transform.position + (Vector3)positOutput * /*inputWorldMagnitude*/4f;
				Gizmos.color = new Color(0, 0, 1, .5f);
				Gizmos.DrawSphere(new Vector3(positOutput.x, positOutput.y, transform.position.z), .1f);
			}
			var interpolater = GetAngleInterpolater(targetRanges);
			var mutatedAngle = interpolater(angle);
			Debug.Log($"Mutated Angle: {mutatedAngle}");
			var mutatedDirection = (Vector2)(Quaternion.Euler(0, 0, interpolater(mutatedAngle)) * Vector2.right);
			Debug.Log($"Mutated Direction: {mutatedDirection}");
			Gizmos.color = Color.yellow * 2;
			Gizmos.DrawLine(transform.position, transform.position + (Vector3)mutatedDirection * inputWorldMagnitude);
		}
	}

}