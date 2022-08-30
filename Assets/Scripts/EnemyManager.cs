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
			else if (instance != this)
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
		}

		#region Used by Object Pool Directly
		private ObjectPool<Enemy> pool;
		public Queue<Vector3> positionsToCreate;
		Enemy Create()
		{
			var newEnemy = Instantiate(enemyPrefab);
			//FindObjectOfType<MySceneManager>().MoveToScene(newEnemy);
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
		Bounds enemyDimensions;
		void Start()
		{
			// Start with "defaultSize" enemies 1-2 screenlength to the right.
			var camBounds = camera.OrthographicBoundsByScreen();
			var createMinX = camBounds.min.x + camBounds.size.x;
			var createMaxX = camBounds.max.x + camBounds.size.x * 2f;
			while (positionsToCreate.Count < defaultSize)
				positionsToCreate.Enqueue(GenerateSAP(createMinX, createMaxX, enemyPrefab.transform.position.y, enemyPrefab.transform.position.z));
			while (positionsToCreate.Count > 0)
				pool.Get();

			if (player == null)
				player = FindObjectOfType<Player>();

			enemyDimensions = enemyPrefab.GetComponent<Enemy>()./*CollidersBounded*/presetBounds; // TODO: Figure this problem out
			groundLayer.SetLayerMask(LayerMask.GetMask(new string[] { "Ground" }));
			allButNonCollidingAndEnemyLayer.SetLayerMask(LayerMask.GetMask(new string[] { "Ground", "Default", "Player" }));

			FindObjectOfType<Goalpost>().GoalReached += EnemyManager_GoalReached;
		}

		void EnemyManager_GoalReached(object sender, System.EventArgs e) => ResetPool();

		ContactFilter2D groundLayer = new();
		ContactFilter2D allButNonCollidingAndEnemyLayer = new();
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
			// If enemies can't be spawned (i.e at the edge of the level), the loop guard stops an infinite loop.
			int loopGuard = 30;
			while (pool.CountInactive > positionsToCreate.Count && loopGuard > 0)
			{
				var spawnAttemptPosition = GenerateSAP(createMinX, createMaxX, player/*enemyPrefab*/.transform.position.y, enemyPrefab.transform.position.z);
				// TODO: Throughly test then trim upcoming section
				if (Physics2D.Raycast(spawnAttemptPosition, Vector2.down, groundLayer, groundHits, 200f) >= 1)
				{
					// Try to put it just over the ground.
					spawnAttemptPosition.y = groundHits[0].point.y + enemyPrefab.GetComponent<Enemy>()./*CollidersBounded*/presetBounds.extents.y + 1;
					enemyDimensions.center = spawnAttemptPosition;

					var overlaps = Physics2D.OverlapBoxAll(spawnAttemptPosition, enemyPrefab.GetComponent<Enemy>().presetBounds.size, 0, groundLayer.layerMask);
					if (overlaps.Length > 0 || groundHits[0].collider.ClosestPoint(spawnAttemptPosition) == (Vector2)spawnAttemptPosition)
					{
						if (Physics2D.Raycast(spawnAttemptPosition, Vector2.up, groundLayer, groundHits) >= 1)
						{
							spawnAttemptPosition.y = groundHits[0].point.y + enemyPrefab.GetComponent<Enemy>()./*CollidersBounded*/presetBounds.extents.y + 1;
							enemyDimensions.center = spawnAttemptPosition;
						}
					}
					if (groundHits[0].collider.ClosestPoint(spawnAttemptPosition) == (Vector2)spawnAttemptPosition)
						Debug.LogWarning($"Spawning inside ground.");
					// Check SAP didn't get pushed onscreen. If it did, spawn is cancelled.
					if (!enemyDimensions.Intersects(camBounds))
						positionsToCreate.Enqueue(spawnAttemptPosition);
					else
						loopGuard--;
				}
				else
					loopGuard--;
			}

			// While there are positions to create, create them.
			while (positionsToCreate.Count > 0)
				pool.Get();
		}

		/// <summary>
		/// Generates a random spawn attempt position.
		/// </summary>
		/// <param name="xMin"></param>
		/// <param name="xMax"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		Vector3 GenerateSAP(float xMin, float xMax, float y, float z) => new(Random.Range(xMin, xMax), y, z);
	}
}