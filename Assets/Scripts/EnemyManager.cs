using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

using Assets.Scripts.Utility;

namespace Assets.Scripts
{
	public class EnemyManager : MonoBehaviour
	{
		#region Singleton
		private static EnemyManager instance;
		public static EnemyManager Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<EnemyManager>();
				return instance;
			}
		}
		void SingletonSetup()
		{
			DontDestroyOnLoad(this);
			if (instance == null)
				instance = this;
			if (instance != this)
			{
				Destroy(gameObject);
				return;
			}
		}
		#endregion

		#region Pool Handling
		public const int defaultSize = 30;
		void Awake()
		{
			SingletonSetup();
			if (enemyPrefab == null)
				Debug.LogError("EnemyPrefab not set.");
			// Pool Setup
			pool = new ObjectPool<Enemy>(Create, OnGet, OnRelease, OnDestroyEnemy, true, defaultSize, 500);
			positionsToCreate = new Queue<Vector3>();
			activeEnemies = new List<Enemy>(defaultSize);
			removalQueue = new List<Enemy>(7);
			groundLayer.SetLayerMask(LayerMask.GetMask(new string[] { "Ground" }));
		}

		#region Used by Object Pool Directly
		private ObjectPool<Enemy> pool;
		public Queue<Vector3> positionsToCreate;
		Enemy Create()
		{
			var newEnemy = Instantiate(enemyPrefab);
			newEnemy.name = $"Enemy {activeEnemies.Count}";
			var newEnemyComponent = newEnemy.GetComponent<Enemy>();
			return newEnemyComponent;
		}

		void OnGet(Enemy enemy)
		{
			enemy.transform.position = positionsToCreate.Dequeue();
			enemy.Init();
			activeEnemies.Add(enemy);
			#region Probably should be removed - rely on Enemy.Init().
			enemy.gameObject.SetActive(true);
			enemy.enabled = true;
			enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
			foreach (var collider in enemy.colliders)
				collider.enabled = true;
			#endregion
			Debug.Log($"Active: {pool.CountActive} | {activeEnemies.Count} Inactive:{pool.CountInactive}");
		}

		void OnRelease(Enemy enemy)
		{
			activeEnemies.Remove(enemy);
			#region Probably should be removed - rely on Enemy.Init().
			enemy.gameObject.SetActive(false);
			enemy.enabled = false;
			enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
			foreach (var collider in enemy.colliders)
				collider.enabled = false;
			#endregion
			Debug.Log($"Active: {pool.CountActive} | {activeEnemies.Count} Inactive:{pool.CountInactive}");
		}

		void OnDestroyEnemy(Enemy enemy)
		{
			activeEnemies.Remove(enemy);
			Destroy(enemy.gameObject);
			Debug.Log($"Active: {pool.CountActive} | {activeEnemies.Count} Inactive:{pool.CountInactive}");
		}
		#endregion

		void ResetPool()
		{
			for (int i = activeEnemies.Count - 1; i >= 0; i--)
				pool.Release(activeEnemies[i]);
			Debug.Assert(activeEnemies.Count == 0);
			positionsToCreate.Clear();
		}

		List<Enemy> removalQueue;
		public void QueueRemoval(Enemy enemy) => removalQueue.Add(enemy);
		#endregion

		#region Inspector Fields
		public GameObject enemyPrefab;
		public new Camera camera;
		public Player player;
		[ContextMenu("Assign Scene References")]
		void AssignSceneReferences()
		{
			camera = FindObjectOfType<Camera>();
			player = FindObjectOfType<Player>();
		}
		#endregion
		
		List<Enemy> activeEnemies;

		void Start()
		{
			// Start with "defaultSize" enemies 1-2 screenlength to the right.
			var camBounds = camera.OrthographicBoundsByScreen();
			var createMinX = camBounds.min.x + camBounds.size.x;
			var createMaxX = camBounds.max.x + camBounds.size.x * 2f;
			while (positionsToCreate.Count < defaultSize)
				positionsToCreate.Enqueue(new Vector3(Random.Range(createMinX, createMaxX), enemyPrefab.transform.position.y, enemyPrefab.transform.position.z));

			if (player == null)
				player = FindObjectOfType<Player>();

			// While there are positions to create, create them.
			while (positionsToCreate.Count > 0)
				pool.Get();
		}

		ContactFilter2D groundLayer = new();
		RaycastHit2D[] groundHits = new RaycastHit2D[2];
		void Update()
		{
			for (int i = 0; i < removalQueue.Count; i++)
			{
				pool.Release(removalQueue[i]);
				removalQueue.RemoveAt(i);
			}

			// If enemies go 1 screenlengths behind, kill them.
			var camBounds = camera.OrthographicBoundsByScreen();
			var limit = camBounds.min.x - camBounds.size.x * 1f;
			var toRemove = new List<int>();
			for (int i = 0; i < activeEnemies.Count; i++)
				if (activeEnemies[i].transform.position.x < limit)
					toRemove.Add(i);
			for (int i = toRemove.Count - 1; i >= 0; i--)
				pool.Release(activeEnemies[toRemove[i]]);

			// If there is space, try to create new enemies .5 screenlengths ahead.
			var createMinX = camBounds.max.x + camBounds.extents.x;
			var createMaxX = createMinX + camBounds.size.x;
			while (pool.CountInactive > positionsToCreate.Count)
			{
				//var spawnPosition = new Vector3(Random.Range(createMinX, createMaxX), player.transform.position.y/*enemyPrefab.transform.position.y*/, enemyPrefab.transform.position.z)/*player.transform.position.y*/;
				//if (Physics2D.(spawnPosition, enemyPrefab.GetComponent<Enemy>().CollidersBounded.size), )
				// TODO: Take height into account.
				var spawnAttemptPosition = new Vector3(Random.Range(createMinX, createMaxX), enemyPrefab.transform.position.y, enemyPrefab.transform.position.z);
				if (Physics2D.Raycast(spawnAttemptPosition, Vector2.down, groundLayer, groundHits) >= 1)
					positionsToCreate.Enqueue(spawnAttemptPosition);
			}

			// While there are positions to create, create them.
			while (positionsToCreate.Count > 0)
				pool.Get();
		}
	}

}