using System.Collections;
using UnityEngine;

using Assets.Scripts.Utility;
using JMor.Utility;

namespace Assets.Scripts
{
	public class Enemy : MonoBehaviour
	{
		public float timeBetweenMoves = 5f;
		private float waitTimer;

		public float moveTime = 3f;
		private float moveTimer;

		public float moveSpeed = 5f;

		private bool waiting = true;
		public bool IsWaiting { get => waiting; }
		public bool IsWalking { get => !waiting; }

		Bounds collidersBounded;
		public Bounds CollidersBounded
		{
			get
			{
				if (collidersBounded == new Bounds())
				{
					collidersBounded = colliders[0].bounds;
					collidersBounded.Encapsulate(colliders[1].bounds);
				}
				return collidersBounded;
			}
		}
		public Bounds presetBounds;
		public AudioSource aSource;
		//public AudioClip sfx_EnemyDeath;
		public Animator animator;

		//private void Reset()
		//{
		//	gameObject.layer = LayerMask.NameToLayer("Enemy");
		//	animator = GetComponent<Animator>();
		//}

		//void Start()
		//{
		//	// Should be set in the prefab.
		//	//gameObject.layer = LayerMask.NameToLayer("Enemy");

		//	// Relying on EnemyManager/ObjectPool to initialize.
		//	//Init();
		//}

		public void Init()
		{
			waitTimer = timeBetweenMoves;
			moveTimer = moveTime;
			waiting = Random.Range(-1f, 1f) < 0;
			GetComponent<SpriteRenderer>().color = Color.white;
			gameObject.SetActive(true);
			enabled = true;
			foreach (var collider in colliders)
				collider.enabled = true;
			GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
		}

		void Update()
		{
			if (isDying)
			{
				deathTimer -= Time.deltaTime;
				if (deathTimer <= 0)
				{
					FindObjectOfType<EnemyManager>().QueueRemoval(this);
					deathTimer = float.MaxValue;
					enabled = false;
					return;
				}
			}
			else if (IsWaiting)
			{
				waitTimer -= Time.deltaTime;
				animator.SetInteger("Direction", 0);
				if (waitTimer <= 0f)
				{
					waiting = false;
					moveTimer = moveTime;
					moveSpeed *= (Random.Range(-1, 1) > 0) ? 1 : -1;
				}
			}
			else if (IsWalking)
			{
				moveTimer -= Time.deltaTime;
				transform.position += new Vector3(moveSpeed * Time.deltaTime, 0);
				animator.SetInteger("Direction", moveSpeed > 0 ? Mathf.CeilToInt(moveSpeed) : Mathf.FloorToInt(moveSpeed));
				if (moveTimer <= 0f)
				{
					waiting = true;
					waitTimer = timeBetweenMoves;
				}
			}
		}
		// TODO: Finish Player Stomp
		public void OnPlayerStomp()
		{
			KillEnemy(); // TEMP
		}

		float deathTimer;
		public float deathTimerLength = 1f;
		bool isDying = false;
		// TODO: Figure out enemy death
		public void KillEnemy()
		{
			isDying = true;
			deathTimer = deathTimerLength;
			GetComponent<SpriteRenderer>().color = new(1, 1, 1, .5f);
			GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
			// Stop collisions with this object (i.e. player, bullets)
			foreach (var collider in colliders)
				collider.enabled = false;
			//aSource.Play();
			// Prevent multikills (i.e. shotgun) from playing 20 at a time.
			aSource.PlayDelayed(Random.value / 4f);
		}

		public Collider2D[] colliders = new Collider2D[0];
		private void OnRenderObject()
		{
			var camera = FindObjectOfType<Camera>();
			var screenPos = camera.WorldToScreenPoint(transform.position);
			var pos = transform.position;
			var screenOrigin = camera.ScreenToWorldPoint(new(0, 0, screenPos.z));
			var ob = camera.OrthographicBoundsByScreen();
			var indicatorHeight = ob.size.y / 5f;
			pos.y = screenOrigin.y;
			MyGizmos.DrawLine3D(pos, pos + new Vector3(0, indicatorHeight, 0), Color.red);
			var bounds = colliders?[0].bounds ?? new Bounds();
			for (int i = 1; i < colliders.Length; i++)
				bounds.Encapsulate(colliders[i].bounds);
			bounds.size = new Vector3(bounds.size.x, indicatorHeight, bounds.size.z);
			pos.y += indicatorHeight / 2f;
			bounds.center = pos;
			var c = Color.red;
			c.a = .3f;
			MyGizmos.DrawFilledBox2D(bounds, color: c);
		}
	}

}