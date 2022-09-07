using System;
using UnityEngine;

namespace Assets.Scripts
{
	public static class ProjectSpecificExtensionMethods
	{
		public static string GetNames(this InputActionNames ian) => ian.ToString();
	}
}
