using UnityEngine;

// TODO: Refactor to ScriptableObject
namespace Assets.Scripts
{
    public class LevelData : MonoBehaviour
    {
        public float emptyClearTime = 26.68f;
        public float timeLimitMultiplier = 1.5f;
        public float PlayerTimeLimit { get => emptyClearTime * timeLimitMultiplier; }
        #region Unity Messages
        public TMPro.TMP_Text timerText;
        void Start()
        {
            timerText = timerText != null ? timerText : (GameObject.Find("TimerDisplay") ?? GameObject.Find("Timer")).GetComponent<TMPro.TMP_Text>();
        }

        bool cleared = false;
        bool clearedInTime = false;

        void Update()
        {
            timerText.text = ("" + (PlayerTimeLimit - Time.timeSinceLevelLoad)).Substring(0, 4) + "s";
            if (cleared && Time.timeSinceLevelLoad <= PlayerTimeLimit)
                clearedInTime = true;
            if (!clearedInTime && Time.timeSinceLevelLoad > PlayerTimeLimit)
			{
                Debug.LogWarning($"Failed to clear level in expected {PlayerTimeLimit} seconds.");
                timerText.color = Color.red;
			}
            else if (clearedInTime)
                timerText.color = Color.green;
        }

		void OnTriggerEnter2D(Collider2D collision)
		{
            var p = collision.gameObject.GetComponent<Player>();
            if (p != null)
			{
                cleared = true;
                p.AddUIMessage("Goalpost Reached!");
                p.UpdateScore(500);
			}
		}
		#endregion
	}
}
