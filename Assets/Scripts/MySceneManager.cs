using Assets.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class MySceneManager : MonoBehaviour
	{
		#region Singleton
		private static MySceneManager instance;
		public static MySceneManager Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<MySceneManager>();
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

		void Awake()
		{
			SingletonSetup();
		}
		#endregion

		/// <summary>
		/// Links the scene names to their build index.
		/// </summary>
		public enum MyScenes
		{
			SceneManagement = 0
		}

		#region Fields and Properties
		/// <summary>
		/// Determines which element of <see cref="sceneCollections"/> is loaded when launched.
		/// </summary>
		[HideInInspector]
		public int firstCollectionToLoad = 0;
		readonly List<AsyncOperation> scenesLoading = new();

		private int currentLevelNum = 1;
		public string CurrentLevel { get => $"Level{currentLevelNum}"; }

		#region Loading Screen Scene Objects
		public GameObject loadingScreen;
		public UnityEngine.UI.Slider progressBar;
		//public Camera loadingScreenCam;
		#endregion

		public List<SceneCollection> sceneCollections = new();

		#region GameSceneAdditive References
		public GameObject Main_Camera;
		public GameObject VCam;
		public GameObject Goalpost;
		public GameObject Canvas;
		public GameObject EnemyManager;
		public GameObject Player;
		#endregion
		#endregion

		public delegate void BatchLoadEvent(AsyncOperation[] operations);
		public event BatchLoadEvent AllSceneLoadsComplete;

		void Start()
		{
			if (firstCollectionToLoad == 0)
				LoadTitle();
			else
				LoadSceneCollection(sceneCollections[firstCollectionToLoad]);
		}

		#region Load Functions
		public void LoadTitle()
		{
			loadingScreen.SetActive(true);
			//loadingScreenCam.gameObject.SetActive(true);
			scenesLoading.Add(SceneManager.LoadSceneAsync("TitleScene", LoadSceneMode.Additive));
			scenesLoading[0].completed += (op) =>
			{
				var t = FindObjectsOfType<Canvas>();
				foreach (var c in t)
				{
					if (c.gameObject.name == "Canvas")
					{
						c.gameObject.SetActive(true);
						c.enabled = true;
						break;
					}
				}
			};

			StartCoroutine(GetSceneLoadProgress());
		}

		public void LoadFromTitle()
		{
			loadingScreen.SetActive(true);
			//loadingScreenCam.gameObject.SetActive(true);
			scenesLoading.Add(SceneManager.UnloadSceneAsync("TitleScene"));
			scenesLoading.Add(SceneManager.LoadSceneAsync("GameSceneAdditive", LoadSceneMode.Additive));
			scenesLoading.Add(SceneManager.LoadSceneAsync(CurrentLevel, LoadSceneMode.Additive));
			scenesLoading[1].completed += AssignGameAdditiveObjectReferences;
			AllSceneLoadsComplete += OnAllGameScenesLoaded;
			StartCoroutine(GetSceneLoadProgress());
		}

		public void LoadNextLevel()
		{
			OnUnloadLevelScene();
			loadingScreen.SetActive(true);
			//loadingScreenCam.gameObject.SetActive(true);
			scenesLoading.Add(SceneManager.UnloadSceneAsync(CurrentLevel));
			currentLevelNum++;
			scenesLoading.Add(SceneManager.LoadSceneAsync(CurrentLevel, LoadSceneMode.Additive));
			AllSceneLoadsComplete += OnAllGameScenesLoaded;
			StartCoroutine(GetSceneLoadProgress());
		}

		public void LoadSceneCollection(SceneCollection collection)
		{
			static bool DoForceReload(string scenePath, SceneCollection sceneCollection)
			{
				foreach (var s in sceneCollection.scenes)
					if (s.scenePath == scenePath && s.forceReload)
						return true;
				return false;
			}
			//var scenesLoading = new List<AsyncOperation>(5);
			for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
			{
				var curr = SceneManager.GetSceneAt(i);
				if (curr.name != "SceneManagement" && !DoForceReload(curr.path, collection))
					scenesLoading.Add(SceneManager.UnloadSceneAsync(i, UnloadSceneOptions.None));
			}
			var scenes = collection.scenes;
			for (int i = 0; i < scenes.Length; i++)
			{
				var curr = scenes[i];
				if (curr.sceneName != "SceneManagement")
				{
					// If the scene isn't currently loaded.
					if (SceneManager.GetSceneByName(curr.sceneName) == null || !SceneManager.GetSceneByName(curr.sceneName).IsValid())
					{
						scenesLoading.Add(SceneManager.LoadSceneAsync(curr.scenePath, LoadSceneMode.Additive));
						if (collection.collectionName.Contains("Level") && curr.sceneName == "GameSceneAdditive")
							scenesLoading[^1].completed += AssignGameAdditiveObjectReferences;
					}
					// If the scene should be reloaded.
					else if (curr.forceReload)
					{
						scenesLoading.Add(SceneManager.UnloadSceneAsync(i, UnloadSceneOptions.None));
						scenesLoading[^1].completed += (op) =>
						{
							scenesLoading.Add(SceneManager.LoadSceneAsync(curr.scenePath, LoadSceneMode.Additive));
							if (collection.collectionName.Contains("Level") && curr.sceneName == "GameSceneAdditive")
								scenesLoading[^1].completed += AssignGameAdditiveObjectReferences;
						};
					}
				}
			}
			if (collection.collectionName.Contains("Level"))
				AllSceneLoadsComplete += OnAllGameScenesLoaded;
			StartCoroutine(GetSceneLoadProgress());
		}
		#endregion

		#region Loading Callbacks and Coroutines
		void AssignGameAdditiveObjectReferences(AsyncOperation _)
		{
			Main_Camera		= GameObject.Find(GameSceneObject.Main_Camera	.GetName());
			VCam			= GameObject.Find(GameSceneObject.VCam			.GetName());
			Goalpost		= GameObject.Find(GameSceneObject.Goalpost		.GetName());
			Canvas			= GameObject.Find(GameSceneObject.Canvas		.GetName());
			EnemyManager	= GameObject.Find(GameSceneObject.EnemyManager	.GetName());
			Player			= GameObject.Find(GameSceneObject.Player		.GetName());
		}

		public IEnumerator GetSceneLoadProgress()
		{
			float totalSceneLoadProgress;
			for (int i = 0; i < scenesLoading.Count; i++)
			{
				while (!scenesLoading[i].isDone)
				{
					totalSceneLoadProgress = 0;
					foreach (var op in scenesLoading)
						totalSceneLoadProgress += op.progress;
					totalSceneLoadProgress /= scenesLoading.Count;

					progressBar.value = totalSceneLoadProgress;
					Debug.Log(totalSceneLoadProgress);
					yield return null;
				}
			}
			loadingScreen.SetActive(false);
			//loadingScreenCam.gameObject.SetActive(false);
			AllSceneLoadsComplete?.Invoke(scenesLoading.ToArray());
			scenesLoading.Clear();
		}

		private void OnAllGameScenesLoaded(AsyncOperation[] operations)
		{
			InitGameSceneAdditiveReferences();
			FindObjectOfType<Goalpost>().GoalReached += MySceneManager_GoalReached;
			AllSceneLoadsComplete -= OnAllGameScenesLoaded;
		}

		void InitGameSceneAdditiveReferences()
		{
			/*GameObject.Find(GameSceneObject.*/
			Main_Camera     /*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/
			VCam            /*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/
			Goalpost        /*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/
			Canvas          /*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/
			EnemyManager    /*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/
			Player          /*.GetName())*/.SetActive(true);
		}

		private void OnUnloadLevelScene()
		{
			FindObjectOfType<Goalpost>().ResetTime();
			var p = FindObjectOfType<Player>();
			p.transform.position = new(0, 7.233036f, 0);
			p.combo = 0;
			p.score = 0;

			if (Main_Camera == null || VCam == null)
				InitGameSceneAdditiveReferences();
			Main_Camera .SetActive(false);
			VCam        .SetActive(false);
			Goalpost    .SetActive(false);
			Canvas      .SetActive(false);
			EnemyManager.SetActive(false);
			Player      .SetActive(false);
		}

		private void MySceneManager_GoalReached(object sender, System.EventArgs e) => LoadNextLevel();
		#endregion

		public void MoveToScene(GameObject gameObject, string sceneName = null)
		{
			SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName(sceneName ?? CurrentLevel));
		}
	}
}
