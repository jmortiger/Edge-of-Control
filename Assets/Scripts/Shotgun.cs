using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
	public class Shotgun : MonoBehaviour
	{
		#region State
		float fireRate = 0f;
		GameObject[] pellets;
		Vector3[] pelletsQueuedPositions;
		Material[] pelletTrailMats;
		#endregion

		#region Settings
		public float fireRateLength = 1f;
		public GameObject pellet;
		//public Vector2 projectileSpawnOffset = new(.5f, 0);
		public float sprayRangeDegrees = 15f;
		public int shotgunPelletNumber = 5;
		public AudioClip sfx_Shotgun;
		#endregion

		#region Component References
		public AudioSource aSource;
		public PlayerInput input;
		public Player player;
		[ContextMenu("Assign Component References")]
		void AssignComponentReferences()
		{
			aSource = GetComponent<AudioSource>();
			input = GetComponent<PlayerInput>();
			player = GetComponent<Player>();
		}
		#endregion

		#region Unity Messages
		void Start()
		{
			pelletsQueuedPositions = new Vector3[shotgunPelletNumber];
			for (int i = 0; i < pelletsQueuedPositions.Length; i++)
				pelletsQueuedPositions[i] = Vector3.positiveInfinity;
			pelletTrailMats = new Material[shotgunPelletNumber];
			pellets = new GameObject[shotgunPelletNumber];
			Debug.Log(pellets.Length);
			for (int i = 0; i < pellets.Length; i++)
			{
				pellets[i] = Instantiate(pellet, Vector3.zero, Quaternion.identity);
				if (FindObjectOfType<MySceneManager>() != null)
					FindObjectOfType<MySceneManager>().MoveToScene(pellets[i], "GameSceneAdditive");
				var tr = pellets[i].GetComponent<TrailRenderer>();
				tr.time = fireRateLength;
				tr.emitting = tr.enabled = false;
				tr.material.color = Color.white;
				pelletTrailMats[i] = tr.material;
				pellets[i].SetActive(false);
			}
		}
		void FixedUpdate()
		{
			if (fireRate > 0)
			{
				for (int i = 0; i < pellets.Length; i++)
				{
					if (pelletsQueuedPositions[i].IsFinite())
					{
						pellets[i].transform.position = pelletsQueuedPositions[i];
						pelletsQueuedPositions[i] = Vector3.positiveInfinity;
					}
					//var c = pelletTrailMats[i].color;
					var c = pellets[i].GetComponent<TrailRenderer>().material.color;
					c.a = Mathf.SmoothStep(0, 1, fireRate / fireRateLength);
					//pelletTrailMats[i].color = c;
					pellets[i].GetComponent<TrailRenderer>().material.color = c;
					pellets[i].GetComponent<SpriteRenderer>().color = c;
				}
				fireRate -= Time.fixedDeltaTime;
				if (fireRate <= 0)
					ResetPellets();
			}
		}
		void OnRenderObject()
		{
			DrawAimingIndicators(true);
		}
		#endregion
		
		public void OnFire(InputAction.CallbackContext cc)
		{
			if (fireRate <= 0)
			{
				aSource.PlayOneShot(sfx_Shotgun);
				fireRate = fireRateLength;
				//var t = projectileSpawnOffset;
				//t.x *= (Velocity.x != 0) ? (Velocity.x > 0 ? 1 : -1) : 0;
				//t.y *= (Velocity.y != 0) ? (Velocity.y > 0 ? 1 : -1) : 0;
				var aimDirection = input.GetActionValueAsJoystick("Aim", transform.position);
				if (aimDirection.magnitude == 0)
					aimDirection = Vector2.right;
				//projectile.GetComponent<TrailRenderer>().time = fireRateLength;
				for (int i = 0; i < pellets.Length; i++)
				{
					pellets[i].transform.position = (Vector3)aimDirection + transform.position;
					var tr = pellets[i].GetComponent<TrailRenderer>();
					tr.time = fireRateLength;
					tr.emitting = tr.enabled = true;
					pellets[i].SetActive(true);
					var angleOffset = (Random.value * sprayRangeDegrees) - (sprayRangeDegrees / 2f);
					Debug.Log($"angleOffset[{i}]: {angleOffset}");
					var pelletDir = (Quaternion.Euler(0, 0, angleOffset) * aimDirection).normalized;
					var raycastHit = new RaycastHit2D[2];
					var raycastHitNum = Physics2D.Raycast(pellets[i].transform.position, pelletDir, GlobalConstants.EnemyAndGround, raycastHit);
					if (raycastHitNum <= 0)
						pelletsQueuedPositions[i] = pellets[i].transform.position + pelletDir * Camera.main.OrthographicBoundsByScreen().size.x;
					else
					{
						pelletsQueuedPositions[i] = raycastHit[0].point + (Vector2)pelletDir;
						if (raycastHit[0].collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
						{
							player.OnFirearmHit(50, 5);//.UpdateScore(50);
							raycastHit[0].collider.GetComponent<Enemy>().KillEnemy();
						}
					}
				}
			}
		}

		void ResetPellets()
		{
			for (int i = 0; i < pellets.Length; i++)
			{
				var tr = pellets[i].GetComponent<TrailRenderer>();
				tr.time = fireRateLength;
				tr.emitting = tr.enabled = false;
				tr.material.color = Color.white;
				pellets[i].SetActive(false);
			}
		}

		void DrawAimingIndicators(bool showSpreadRange)
		{
#if UNITY_EDITOR
			MyGizmos.DrawLine3D(transform.position, transform.position + (Vector3)input.GetRightStickOrMouseValueAsJoystickEditor(transform.position) * 5f, Color.red);
#else
			MyGizmos.DrawLine3D(transform.position, transform.position + (Vector3)input.GetActionValueAsJoystick("Aim", transform.position));
#endif

			if (showSpreadRange)
			{
#if UNITY_EDITOR
				var aimDirection = input.GetRightStickOrMouseValueAsJoystickEditor(transform.position);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, sprayRangeDegrees / 2f) * aimDirection * 10f);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, -sprayRangeDegrees / 2f) * aimDirection * 10f);
#else
				var aimDirection = input.GetActionValueAsJoystick("Aim", transform.position);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, sprayRangeDegrees / 2f) * aimDirection * 10f);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, -sprayRangeDegrees / 2f) * aimDirection * 10f);
#endif
			}
		}

	}
}