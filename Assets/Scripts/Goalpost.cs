using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
	public class Goalpost : MonoBehaviour
	{
		public event EventHandler GoalReached;
		[JMor.Utility.Expandable] public LevelData levelData;

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
			// Automatically move goalpost to the end of the ground.
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

		public void ResetTime() => myTime = 0;

		float myTime = 0;
		bool cleared = false;
		bool clearedInTime = false;
		[Range(2, 10)]
		public int timerCharLength = 6;
		void Update()
		{
			myTime += Time.deltaTime;
			timerText.text = ((levelData.PlayerTimeLimit - myTime).ToString().Length < timerCharLength - 1) ?
				levelData.PlayerTimeLimit - myTime + "s" :
				("" + (levelData.PlayerTimeLimit - myTime)).Substring(0, timerCharLength - 1) + "s";
			if (cleared && myTime <= levelData.PlayerTimeLimit)
				clearedInTime = true;
			if (!clearedInTime && myTime > levelData.PlayerTimeLimit)
				timerText.color = Color.red;//Debug.LogWarning($"Failed to clear level in expected {levelData.PlayerTimeLimit} seconds.");
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
