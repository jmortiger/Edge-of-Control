using Assets.Scripts.Utility;
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
		#endregion

		void Start()
        {
			SingletonSetup();
			LoadTitle();
        }

		readonly List<AsyncOperation> scenesLoading = new();

		private int currentLevelNum = 1;
		public string CurrentLevel { get => $"Level{currentLevelNum}"; }

		public GameObject loadingScreen;
		public UnityEngine.UI.Slider progressBar;
		//public Camera loadingScreenCam;

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
			scenesLoading[1].completed += (op) =>
			{
				Main_Camera		= GameObject.Find(GameSceneObject.Main_Camera	.GetName());
				VCam			= GameObject.Find(GameSceneObject.VCam			.GetName());
				Goalpost		= GameObject.Find(GameSceneObject.Goalpost		.GetName());
				Canvas			= GameObject.Find(GameSceneObject.Canvas		.GetName());
				EnemyManager	= GameObject.Find(GameSceneObject.EnemyManager	.GetName());
				Player			= GameObject.Find(GameSceneObject.Player		.GetName());
			};
			OnAllSceneLoadsComplete += OnAllGameScenesLoaded;
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
			OnAllSceneLoadsComplete += OnAllGameScenesLoaded;
			StartCoroutine(GetSceneLoadProgress());
		}

		float totalSceneLoadProgress = 0;

		public IEnumerator GetSceneLoadProgress()
		{
			for (int i = 0; i < scenesLoading.Count; i++)
			{
				while (!scenesLoading[i].isDone)
				{
					totalSceneLoadProgress = 0;
					foreach (var op in scenesLoading)
						totalSceneLoadProgress += op.progress;
					totalSceneLoadProgress /= scenesLoading.Count;

					progressBar.value = totalSceneLoadProgress;

					yield return null;
				}
			}
			loadingScreen.SetActive(false);
			//loadingScreenCam.gameObject.SetActive(false);
			OnAllSceneLoadsComplete?.Invoke(scenesLoading.ToArray());
			scenesLoading.Clear();
		}

		public GameObject Main_Camera;
		public GameObject VCam;
		public GameObject Goalpost;
		public GameObject Canvas;
		public GameObject EnemyManager;
		public GameObject Player;

		public delegate void BatchLoadEvent(AsyncOperation[] operations);
		public event BatchLoadEvent OnAllSceneLoadsComplete;
		private void OnAllGameScenesLoaded(AsyncOperation[] operations)
		{
			/*GameObject.Find(GameSceneObject.*/Main_Camera		/*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/VCam			/*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/Goalpost		/*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/Canvas			/*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/EnemyManager	/*.GetName())*/.SetActive(true);
			/*GameObject.Find(GameSceneObject.*/Player			/*.GetName())*/.SetActive(true);

			FindObjectOfType<Goalpost>().GoalReached += MySceneManager_GoalReached;

			OnAllSceneLoadsComplete -= OnAllGameScenesLoaded;
		}

		private void MySceneManager_GoalReached(object sender, System.EventArgs e) => LoadNextLevel();

		private void OnUnloadLevelScene()
		{
			FindObjectOfType<Goalpost>().ResetTime();
			var p = FindObjectOfType<Player>();
			p.transform.position = new(0, 7.233036f, 0);
			p.combo = 0;
			p.score = 0;

			/*GameObject.Find(GameSceneObject.*/Main_Camera	/*.GetName())*/.SetActive(false);
			/*GameObject.Find(GameSceneObject.*/VCam		/*.GetName())*/.SetActive(false);
			/*GameObject.Find(GameSceneObject.*/Goalpost	/*.GetName())*/.SetActive(false);
			/*GameObject.Find(GameSceneObject.*/Canvas		/*.GetName())*/.SetActive(false);
			/*GameObject.Find(GameSceneObject.*/EnemyManager/*.GetName())*/.SetActive(false);
			/*GameObject.Find(GameSceneObject.*/Player		/*.GetName())*/.SetActive(false);
		}

		public void MoveToScene(GameObject gameObject, string sceneName = null)
		{
			SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName((sceneName == null) ? CurrentLevel : sceneName));
		}

		#region Editor Stuff
		[HideInInspector]
		public int levelToLoad = 1;
		//[SerializeField]
		public Scene[] scenes = new Scene[1];
		public SceneWrapper[] sceneWrappers = new SceneWrapper[1];
		//public List<Scene> Scenes { get => scenes; }
		public List<string> SceneNames
		{
			get
			{
				var strings = new List<string>();
				foreach (var s in sceneWrappers)
					strings.Add(s.sceneName);
				return strings;
			}
		}
		#endregion
	}
}
