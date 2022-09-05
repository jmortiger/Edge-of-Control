using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace Assets.Scripts
{
	// TODO: Finish
	public class Meter : MonoBehaviour
	{
		public UnityEvent<float> ValueChange = new();
		[SerializeField]
		[Range(0, 1)]
		float value;

		public float Value
		{
			get { return value; }
			set
			{
				if (value >= 0)
				{
					if (value <= 1)
					{
						this.value = value;
						UpdateMeter();
						ValueChange?.Invoke(value);
					}
					else
						//this.value = 1;
						throw new ArgumentException("Value must satisfy condition: 0 <= value <= 1");
				}
				else
					//this.value = 0;
					throw new ArgumentException("Value must satisfy condition: 0 <= value <= 1");
			}
		}

		//private float trueValue;

		//public float TrueValue
		//{
		//	get { return trueValue; }
		//	set { trueValue = value; }
		//}


		private void Awake()
		{
			//if (ValueChange == null)
		}


		private void UpdateMeter()
		{

		}
	}
}