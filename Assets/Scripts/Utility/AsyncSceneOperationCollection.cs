using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utility
{
	// TODO: Finish
	/// <summary>
	/// A wrapper for a <see cref="List{T:AsyncOperation``1}"/> that provides similar functionality to <see cref="AsyncOperation"/>.
	/// </summary>
	public class AsyncSceneOperationCollection
	{
		List<AsyncOperation> operations;
		public delegate void AsyncSceneCollectionEvent(object sender, AsyncSceneCollectionEventArgs args);
		public event AsyncSceneCollectionEvent Completed;

		public AsyncSceneOperationCollection(uint size = 5)
		{
			if (size == 0) size = 5;
			operations = new((int)size);
		}

		public void Clear()
		{
			operations.Clear();
		}

		//public void 
	}
	public class AsyncSceneCollectionEventArgs
	{
		public string sceneName;
		public string scenePath;
		public AsyncOperation operation;
		public AsyncSceneCollectionEventArgs(string sceneName, string scenePath, AsyncOperation operation)
		{
			this.sceneName = sceneName;
			this.scenePath = scenePath;
			this.operation = operation;
		}
	}
}
