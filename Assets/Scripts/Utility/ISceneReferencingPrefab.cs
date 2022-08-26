using UnityEngine;

namespace Assets.Scripts.Utility
{
	/// <summary>
	/// Creates a context menu item that attempts to automatically assgin scene references; implementing methods require <see cref="ContextMenu"/> attribute.
	/// </summary>
	public interface ISceneReferencingPrefab
	{
		/// <summary>
		/// Creates a context menu item that attempts to automatically assgin scene references; implementing methods require <see cref="ContextMenu"/> attribute.
		/// </summary>
		[ContextMenu("Assign Scene References")]
		void AssignSceneReferences();
	}
}
