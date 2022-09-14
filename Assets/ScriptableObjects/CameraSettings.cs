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
		public float defaultOrthographicSize = 5;
		public bool isOrthographicSizeFunctionActive = false;
		[InspectorName("(i)nitial Orthographic Size")]
		[Range(1, 50)]
		public float initialCameraOrthographicSize = 5f;
		[InspectorName("(s)cale Distance from Ground")]
		//[Tooltip("Scales orthograpic size as speed increases.")]
		[Range(0, 1)]
		public float scaleDistance = 1;
		[InspectorName("(b)ase for Orthographic Size Function")]
		[Range(1, 1.5f)]
		public float baseForCamSizeFunction = 1.25f;
		public float OrthographicSizeFunction(float x)
		{
			return initialCameraOrthographicSize - 1 + Mathf.Pow(baseForCamSizeFunction, x * scaleDistance);
		}
		public float GetNewOrthographicSize(Vector2 velocity, Vector3 position, ContactFilter2D groundLayer)
		{
			if (velocity.magnitude > 1)
			{
				var rcResults = new RaycastHit2D[1];
				var numResults = Physics2D.Raycast(position, Vector2.down, groundLayer, rcResults);
				float dist = 0;
				if (numResults > 0/* && rcResults[0].distance > 1*/)
					dist = rcResults[0].distance;
				// f(x) = initialCameraOrthographicSize - 1 + orthographicSizeFunctionBase ^ (dist * scaleDistance)
				return initialCameraOrthographicSize - 1 + Mathf.Pow(baseForCamSizeFunction, dist * scaleDistance);
			}
			else
				return defaultOrthographicSize;//myCam.m_Lens.OrthographicSize;
		}
		#endregion
	}
}