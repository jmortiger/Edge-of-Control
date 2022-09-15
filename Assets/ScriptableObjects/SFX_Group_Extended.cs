using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Utility;

// TODO: Auto-selecting SFX_Groups
namespace Assets.ScriptableObjects
{
	public class SFX_Group_Extended : SFX_Group
	{
		public SFX_Group_Extended[] sfx_Groups;
		//public new string label;
		public override int Length => sfx_Groups.Length;

		//public override AudioClip GetRandomClip(string labels)
		//{
		//	var labelArr = labels.Split("_", StringSplitOptions.RemoveEmptyEntries);
		//	if (labelArr.Length == 0)
		//		return base.GetRandomClip();
		//	if (labelArr[0] == label)
		//		labelArr.SlideElementsUp("");
		//	for (int i = 0; i < sfx_Groups.Length; i++)
		//	{
		//		if ()
		//	}
		//}
	}
}
