using System;
using TMPro;
using UnityEngine;

// TODO: Refactor to ScriptableObject
namespace Assets.Scripts
{
	public class Goalpost : MonoBehaviour
	{
		public event EventHandler GoalReached;
		[Expandable]
		public LevelData levelData;
		public float emptyClearTime = 26.68f;
		public float timeLimitMultiplier = 1.75f;
		public float PlayerTimeLimit { get => emptyClearTime * timeLimitMultiplier; }

		#region Scene References
		public TMP_Text timerText;
		[ContextMenu("Assign Scene References")]
		void AssignSceneReferences()
		{
			timerText = (GameObject.Find("TimerDisplay") ?? GameObject.Find("Timer")).GetComponent<TMP_Text>();
		}
		#endregion

		#region Unity Messages
		void Reset()
		{
			AssignSceneReferences();
			var groundObjects = GameObject.FindGameObjectsWithTag("Ground");
			if (groundObjects.Length <= 0)
				Debug.LogWarning("No GOs tagged 'Ground'; cannot autoplace Goalpost");
			else
			{
				float maxX = float.MinValue;
				foreach (var ground in groundObjects)
				{
					var x = ground.GetComponent<CompositeCollider2D>().bounds.max.x;
					if (x > maxX)
						maxX = x;
				}
				transform.position = new(maxX, transform.position.y, transform.position.z);
			}
		}

		//void Start()
		//{
		//	timerText = timerText != null ? timerText : (GameObject.Find("TimerDisplay") ?? GameObject.Find("Timer")).GetComponent<TMP_Text>();
		//}

		public void ResetTime() => myTime = 0;

		float myTime = 0;
		bool cleared = false;
		bool clearedInTime = false;
		[Range(2, 10)]
		public int timerCharLength = 6;
		void Update()
		{
			myTime += Time.deltaTime;
			timerText.text = ((levelData.PlayerTimeLimit - /*Time.timeSinceLevelLoad*/myTime).ToString().Length < timerCharLength - 1) ?
				levelData.PlayerTimeLimit - /*Time.timeSinceLevelLoad*/myTime + "s" :
				("" + (levelData.PlayerTimeLimit - /*Time.timeSinceLevelLoad*/myTime)).Substring(0, timerCharLength - 1) + "s";
			if (cleared && /*Time.timeSinceLevelLoad*/myTime <= levelData.PlayerTimeLimit)
				clearedInTime = true;
			if (!clearedInTime && /*Time.timeSinceLevelLoad*/myTime > levelData.PlayerTimeLimit)
			{
				Debug.LogWarning($"Failed to clear level in expected {levelData.PlayerTimeLimit} seconds.");
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
				GoalReached?.Invoke(this, null);
			}
		}
		#endregion
	}
}
