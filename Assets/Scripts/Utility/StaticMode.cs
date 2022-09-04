using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Utility
{
	[CreateAssetMenu(fileName = "dtm_StaticMode0", menuName = "ScriptableObjects/Dynamic Texture Mode/Static Mode")]
	public class StaticMode : DynamicTextureMode
	{
		[Tooltip("How many screen pixels make up 1 pixel of static.")]
		[Range(1, 100)]
		public int staticPixelSize = 1;

		public override void Initializer(Texture2D texture) => base.Initializer(texture);

		public override void Updater(Texture2D texture)
		{
			GenerateStatic(texture, staticPixelSize);
			base.Updater(texture);
		}
	}
}