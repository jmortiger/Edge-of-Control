using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Utility
{
	[RequireComponent(typeof(Camera))]
	public class CameraSmoothFollow : MonoBehaviour
	{
		private new Camera camera;

		#region ScreenShake Fields
		//public static readonly Vector3 defaultScreenShakeMax = new Vector3(SCREENSHAKE_MAX_X, SCREENSHAKE_MAX_Y, SCREENSHAKE_MAX_Z);
		public const float SCREENSHAKE_MAX_X = 4f;
		public const float SCREENSHAKE_MAX_Y = 4f;
		public const float SCREENSHAKE_MAX_Z = 0f;
		public Vector3 shakeMax = new Vector3(SCREENSHAKE_MAX_X, SCREENSHAKE_MAX_Y, SCREENSHAKE_MAX_Z);
		public float screenShakeLength = .25f;
		private float screenShakeTimer = 0f;
		public bool IsScreenShaking { get => screenShakeTimer > 0; }
		#endregion

		private Vector3 currPos;
		/// <summary>
		/// The global position of this object disregarding any offset from screenshake. Overwrites <see cref="Transform.position"/>.
		/// </summary>
		/// <remarks>Use this instead of <see cref="Transform.position"/> to prevent conflict with screen shake functionality.</remarks>
		public Vector3 CurrentPosition
		{
			get => currPos;
			//set
			//{
			//	if (value.IsFinite())
			//		currPos = value;
			//	else
			//		Debug.LogWarning($"Attempt to modify failed due to invalid parameters of value ({value})");
			//}
		}

		private Vector3 tap;
		/// <summary>
		/// The <b>T</b>eleport <b>A</b>ttempt <b>P</b>osition. Overwrites <see cref="Transform.position"/> and <see cref="CurrentPosition"/>.
		/// </summary>
		/// <remarks>Use this instead of <see cref="Transform.position"/> and <see cref="CurrentPosition"/> to prevent changes being overwritten by this and to avoid conflict with screen shake functionality.</remarks>
		public Vector3 TAP
		{
			get => tap;
			set
			{
				if (value.IsFinite())
					tap = value;
				else
					Debug.LogWarning($"Attempt to modify failed due to invalid parameters of value ({value})");
			}
		}

		#region Target Following
		public Transform target = null;
		public Vector3 targetOffset = Vector3.zero;
		/// <summary>
		/// Whether or not to use nomalized cooridnates for the offset (i.e. bottom left = (-1,-1), top right = (1,1)) Z coordinate is unaffected.
		/// </summary>
		/// <remarks>Z coordinate may be used with near and far clip planes in the future.</remarks>
		public bool defineTargetOffsetNormalized = false;
		// TODO: Test nomarlized offsets
		public Vector3 NormalizedTargetOffsetConverted
		{
			get
			{
				if (targetOffset.x > 1 ||
					targetOffset.x < -1 ||
					targetOffset.y > 1 ||
					targetOffset.y < -1 ||
					!defineTargetOffsetNormalized)
					return Vector3.positiveInfinity;
				var t = targetOffset / 2f;
				t += new Vector3(.5f, .5f);
				t.x *= camera.pixelWidth;
				t.y *= camera.pixelHeight;
				t.z = camera.nearClipPlane;//transform.position.z;
				t = camera.ScreenToWorldPoint(t);
				t.z = CurrentPosition.z;
				return t;
			}
		}
		#endregion

		void Reset()
		{
			camera = GetComponent<Camera>();
			camera.name = "Main Camera";
			camera.tag = "MainCamera";
		}

		void Start()
		{
			currPos = tap = transform.position;
		}

		void Update()
		{
			// Determine target position
			if (target != null)
			{
				Vector3 targetPosition;
				// TODO: Test if assignment
				if (defineTargetOffsetNormalized && (targetPosition = NormalizedTargetOffsetConverted).IsFinite())
					targetPosition = target.position + targetPosition;
				else
					targetPosition = target.position + (targetOffset.IsFinite() ? targetOffset : Vector3.zero);
				TAP = targetPosition;
			}

			// TODO: Check if TAP is viable (e.g. doesn't cross boundaries)
			currPos = TAP;

			if (screenShakeTimer > 0f)
				ScreenShake();
			else
				transform.position = currPos;
		}

		#region Screenshake Methods
		private void ScreenShake()
		{
			screenShakeTimer -= Time.deltaTime;
			Func<float, float> getShake = (x) => Random.value * (x * 2f) - x;
			transform.position = currPos + new Vector3(getShake(shakeMax.x), getShake(shakeMax.y), 0);
		}

		/// <summary>
		/// Start shaking the <see cref="camera"/> for the given time, with the given max offset.
		/// </summary>
		/// <param name="screenShakeLength">The time to shake the screen for.</param>
		/// <param name="screenShakeMax">The maximum offset from the target position. Takes priority over <paramref name="shakeMaxX"/>, <paramref name="shakeMaxY"/>, and <paramref name="shakeMaxZ"/>.</param>
		/// <param name="shakeMaxX">The maximum X offset from the target position.</param>
		/// <param name="shakeMaxY">The maximum Y offset from the target position.</param>
		/// <param name="shakeMaxZ">The maximum Z offset from the target position.</param>
		/// <remarks>Only <paramref name="screenShakeMax"/> or <paramref name="shakeMaxX"/>, <paramref name="shakeMaxY"/>, and <paramref name="shakeMaxZ"/>. Do not use both.</remarks>
		public void StartScreenShake(
			float screenShakeLength = .25f,
			Vector3? screenShakeMax = null,
			float shakeMaxX = SCREENSHAKE_MAX_X,
			float shakeMaxY = SCREENSHAKE_MAX_Y,
			float shakeMaxZ = SCREENSHAKE_MAX_Z)
		{
			shakeMax = screenShakeMax ?? new Vector3(shakeMaxX, shakeMaxY, shakeMaxZ);
			screenShakeTimer = screenShakeLength;
		}

		//public void StartScreenShake(
		//	float screenShakeLength = .25f, 
		//	float screenShakeMaxX = SCREENSHAKE_MAX_X, 
		//	float screenShakeMaxY = SCREENSHAKE_MAX_Y, 
		//	float screenShakeMaxZ = SCREENSHAKE_MAX_Z)
		//{
		//	shakeMax = new Vector3(screenShakeMaxX, screenShakeMaxY, screenShakeMaxZ);
		//	screenShakeTimer = screenShakeLength;
		//}
		#endregion
	}

}