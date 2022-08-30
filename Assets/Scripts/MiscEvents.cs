using UnityEngine;

namespace Assets.Scripts
{
	public class MiscEvents : MonoBehaviour
	{
		public static void SwitchFromTitle()
		{
			FindObjectOfType<MySceneManager>().LoadFromTitle();
		}
	}
}
