namespace Assets.Scripts
{
	public enum GameSceneObject
	{
		Main_Camera,
		VCam,
		Goalpost,
		EnemyManager,
		Player,
		Canvas,
	}
	public static class MyProjectExtensions
	{
		public static string GetName(this GameSceneObject gso)
		{
			return gso switch
			{
				GameSceneObject.Main_Camera => "Main Camera",
				GameSceneObject.VCam => "CM vcam1",
				GameSceneObject.Goalpost => "Goalpost",
				GameSceneObject.EnemyManager => "EnemyManager",
				GameSceneObject.Player => "Player",
				GameSceneObject.Canvas => "Canvas(Level)",
				_ => throw new System.ArgumentException(),
			};
		}

		//public static string GetName(this InputNames ian) => ian.ToString();
	}
}
