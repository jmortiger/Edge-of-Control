using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utility
{
	public class MiscEvents : MonoBehaviour
	{
		public static void SwitchFromTitle()
		{
#if UNITY_EDITOR
			SceneManager.LoadScene("SampleScene");
#else
			SceneManager.LoadScene(1);
#endif
		}

	}
}
