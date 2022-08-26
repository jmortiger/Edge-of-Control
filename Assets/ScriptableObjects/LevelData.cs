using System;
using TMPro;
using UnityEngine;

// TODO: Refactor to ScriptableObject
namespace Assets.Scripts
{
	public class LevelData : MonoBehaviour
	{
		public EventHandler GoalReached;

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
				GoalReached?.Invoke(this, null);
			}
		}
		#endregion
	}
}
