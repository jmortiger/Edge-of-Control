using UnityEngine;

namespace Assets.Scripts
{
	/// <summary>
	/// A storage class for universal, unchanging data that cannot be defined as a constant nor at compile-time (i.e. <see cref="ContactFilter2D"/>s).
	/// Data stored here is (generally) lazy-loaded, and SHOULD BE UNCHANGEABLE outside this class.
	/// The only state this class should hold are checks whether certain variables have been initialized.
	/// </summary>
	public static class GlobalConstants
	{
		#region ContactFilter2Ds
		#region EnemyAndGround
		private static bool enemyAndGround_Initialized = false;
		private static ContactFilter2D enemyAndGround;
		public static ContactFilter2D EnemyAndGround
		{
			get
			{
				if (!enemyAndGround_Initialized)
				{
					enemyAndGround = new ContactFilter2D() { useLayerMask = true, layerMask = LayerMask.GetMask("Ground", "Enemy") };
					enemyAndGround_Initialized = true;
				}
				return enemyAndGround;
			}
		}
		#endregion
		#region EnemyLayer
		private static bool enemyLayer_Initialized = false;
		private static ContactFilter2D enemyLayer;
		public static ContactFilter2D EnemyLayer
		{
			get
			{
				if (!enemyLayer_Initialized)
				{
					enemyLayer = new ContactFilter2D() { useLayerMask = true, layerMask = LayerMask.GetMask("Enemy") };
					enemyLayer_Initialized = true;
				}
				return enemyLayer;
			}
		}
		#endregion
		#region GroundLayer
		private static bool groundLayer_Initialized = false;
		private static ContactFilter2D groundLayer;
		public static ContactFilter2D GroundLayer
		{
			get
			{
				if (!groundLayer_Initialized)
				{
					groundLayer = new ContactFilter2D() { useLayerMask = true, layerMask = LayerMask.GetMask("Ground") };
					groundLayer_Initialized = true;
				}
				return groundLayer;
			}
		}
		#endregion
		#region AllButNonCollidingAndEnemyLayer
		private static bool allButNonCollidingAndEnemyLayer_Initialized = false;
		private static ContactFilter2D allButNonCollidingAndEnemyLayer;
		public static ContactFilter2D AllButNonCollidingAndEnemyLayer
		{
			get
			{
				if (!allButNonCollidingAndEnemyLayer_Initialized)
				{
					allButNonCollidingAndEnemyLayer = new ContactFilter2D() { useLayerMask = true, layerMask = LayerMask.GetMask("Ground", "Default", "Player") };
					allButNonCollidingAndEnemyLayer_Initialized = true;
				}
				return allButNonCollidingAndEnemyLayer;
			}
		}
		#endregion
		#endregion
	}
}
