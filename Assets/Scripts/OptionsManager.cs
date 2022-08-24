using UnityEngine;

namespace Assets.Scripts
{
	// TODO: Expand
	public class OptionsManager : MonoBehaviour
	{
		#region Singleton
		private static OptionsManager instance;
		public static OptionsManager Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<OptionsManager>();
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

		private string showMyGizmosString = nameof(showMyGizmos);
		private bool showMyGizmos = false;
		public bool ShowMyGizmos
		{
			get => showMyGizmos;
			set
			{
				if (showMyGizmos != value)
				{
					showMyGizmos = value;
					PlayerPrefs.SetString(showMyGizmosString, showMyGizmos.ToString());
				}
			}
		}

		void Awake()
		{
			SingletonSetup();
			Initialize();
		}

		void Initialize()
		{
			var t = PlayerPrefs.GetString(showMyGizmosString, "");
			if (t == "")
				PlayerPrefs.SetString(showMyGizmosString, showMyGizmos.ToString());
			else
				showMyGizmos = bool.Parse(t);
		}

		//void InitializeObject(ScriptableObject obj)
		//{

		//}
	}
}
