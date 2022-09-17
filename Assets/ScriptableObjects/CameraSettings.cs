using Cinemachine;
using UnityEngine;

namespace Assets.ScriptableObjects
{
	[CreateAssetMenu(fileName = "camSettings_New", menuName = "Scriptable Object/Camera Settings")]
	public class CameraSettings : ScriptableObject
	{
		#region Camera Orthographic Size
		[Header("Camera Orthographic Size Settings")]
		[Header("OrthoSize = i - 1 + b^(deltaD * s)")]
		//public CinemachineVirtualCamera myCam;
		[Range(1, 50)]
		[InspectorName("Default Orthographic Size - Used when velocity.magnitude < 1")]
		[Tooltip("Default Orthographic Size - Used when velocity.magnitude < 1 (CURRENTLY UNUSED)")]
		public float defaultOrthographicSize = 5;
		[Tooltip("Is the orthographic size function active?")]
		public bool isOrthographicSizeFunctionActive = false;
		[InspectorName("(i)nitial Orthographic Size")]
		[Range(1, 50)]
		public float initialCameraOrthographicSize = 5f;
		[InspectorName("(s)cale Distance from Ground")]
		[Range(0, 1)]
		public float scaleDistance = 1;
		[InspectorName("(b)ase for Orthographic Size Function")]
		[Range(1, 1.5f)]
		public float baseForCamSizeFunction = 1.25f;

		public float OrthographicSizeFunction(float x) => initialCameraOrthographicSize - 1 + Mathf.Pow(baseForCamSizeFunction, x * scaleDistance);
		public float GetTargetOrthographicSize(/*Vector2 velocity, */Vector3 position, ContactFilter2D groundLayer)
		{
			//if (velocity.magnitude > 1)
			//{
				//var rcResults = new RaycastHit2D[1];
				//var numResults = Physics2D.Raycast(position, Vector2.down, groundLayer, rcResults);
				//float dist = 0;
				//if (numResults > 0/* && rcResults[0].distance > 1*/)
				//	dist = rcResults[0].distance;
				var dist = GetDistanceFromGround(position, groundLayer);
				dist = dist < 0 ? 0 : dist;
				// f(x) = initialCameraOrthographicSize - 1 + orthographicSizeFunctionBase ^ (dist * scaleDistance)
				return OrthographicSizeFunction(dist);
			//}
			//else
			//	return defaultOrthographicSize;//myCam.m_Lens.OrthographicSize;
		}
		public float GetTargetOrthographicSize(/*Vector2 velocity, */Vector3 position, ContactFilter2D groundLayer, out float distance)
		{
			distance = -1;
			//if (velocity.magnitude > 1)
			//{
				//var rcResults = new RaycastHit2D[1];
				//var numResults = Physics2D.Raycast(position, Vector2.down, groundLayer, rcResults);
				//float dist = 0;
				//if (numResults > 0/* && rcResults[0].distance > 1*/)
				//	dist = distance = rcResults[0].distance;
				var dist = distance = GetDistanceFromGround(position, groundLayer);
				dist = dist < 0 ? 0 : dist;
				// f(x) = initialCameraOrthographicSize - 1 + orthographicSizeFunctionBase ^ (dist * scaleDistance)
				return OrthographicSizeFunction(dist);
			//}
			//else
			//	return defaultOrthographicSize;//myCam.m_Lens.OrthographicSize;
		}
		[Header("Easing")]
		public bool easeOrthographicSizeChange = false;
		[Range(0, 1)]
		public float easeIntensity = .5f;
		#endregion

		//[Header("Camera Offset")]
		//public Vector3 trackedObjectOffset = new(5, -2.25f, 0);
		// TODO: Use Player.PlayerDistanceFromGround
		// TODO: Ease trackedObjectOffset changes (https://www.youtube.com/watch?v=Nc9x0LfvJhI)
		public void UpdateCamera(CinemachineVirtualCamera vCam, /*Vector2 velocity, */Vector3 position, ContactFilter2D groundLayer)
		{
			float groundDistance = -1;
			float newOrtho = isOrthographicSizeFunctionActive ?
				GetTargetOrthographicSize(/*velocity, */position, groundLayer, out groundDistance) :
				defaultOrthographicSize;
			if (easeOrthographicSizeChange && isOrthographicSizeFunctionActive)
				newOrtho = Mathf.Lerp(vCam.m_Lens.OrthographicSize, newOrtho, easeIntensity);
			vCam.m_Lens.OrthographicSize = newOrtho;

			var ft = vCam.GetComponentInChildren<CinemachineFramingTransposer>();
			if (/*velocity.magnitude <= 1 || */!isOrthographicSizeFunctionActive)
				groundDistance = GetDistanceFromGround(position, groundLayer);
			if (groundDistance < Mathf.Abs(ft.m_TrackedObjectOffset.y))
				ft.m_TrackedObjectOffset = new(ft.m_TrackedObjectOffset.x, Mathf.Abs(ft.m_TrackedObjectOffset.y), ft.m_TrackedObjectOffset.z);
			else
				ft.m_TrackedObjectOffset = new(ft.m_TrackedObjectOffset.x, -Mathf.Abs(ft.m_TrackedObjectOffset.y), ft.m_TrackedObjectOffset.z);
		}

		private float GetDistanceFromGround(Vector2 position, ContactFilter2D groundLayer)
		{
			var rcResults = new RaycastHit2D[1];
			var numResults = Physics2D.Raycast(position, Vector2.down, groundLayer, rcResults);
			return (numResults > 0) ? rcResults[0].distance : -1;
		}
		private float GetDistanceFromGround(Vector2 position, ContactFilter2D groundLayer, ref RaycastHit2D[] rcResults)
		{
			var numResults = Physics2D.Raycast(position, Vector2.down, groundLayer, rcResults);
			return (numResults > 0) ? rcResults[0].distance : -1;
		}
	}
}