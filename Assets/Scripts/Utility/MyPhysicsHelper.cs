using UnityEngine;

namespace Assets.Scripts.Utility
{
	// TODO: Flesh out, reformat.
	public static class MyPhysicsHelper
	{
		/// <summary>
		/// Solves vFinal² = vInitial² + 2aΔd for Δd.
		/// </summary>
		/// <param name="vIComp">Initial Velocity Component</param>
		/// <param name="aComp">Acceleration Component</param>
		/// <param name="vFComp">Final Velocity Component; default = 0</param>
		/// <returns>The component change in location.</returns>
		/// <remarks>Uses formula <c>vFinal² = vInitial² + 2aΔd</c></remarks>
		public static float GetDisplacementComponent(float vIComp, float aComp, float vFComp = 0f)
		{
			// vFinal ^ 2 = vInitial ^ 2 + 2 * a * Δd
			// vFinal ^ 2 - vInitial ^ 2 = 2 * a * Δd
			// (vFinal ^ 2 - vInitial ^ 2) / (2 * a) = Δd
			// Δd = (vFinal ^ 2 - vInitial ^ 2) / (2 * a)
			return (vFComp * vFComp - vIComp * vIComp) / (2f * aComp);
		}
		/// <summary>
		/// Solves vFinal² = vInitial² + 2aΔd for Δd.
		/// </summary>
		/// <param name="vIComp">Initial Velocity Component</param>
		/// <param name="aComp">Acceleration Component</param>
		/// <param name="vFComp">Final Velocity Component; default = 0</param>
		/// <returns>The component change in location.</returns>
		/// <remarks>Uses formula <c>vFinal² = vInitial² + 2aΔd</c></remarks>
		public static float GetΔd(float vI, float a, float vF = 0f) => GetDisplacementComponent(vI, a, vF);

		/// <summary>
		/// Solves 𝑣̅ = Δd/Δt for Δt.
		/// Get the time to travel a given distance at given constant speed.
		/// </summary>
		/// <param name="ΔdComp">Displacement (component)</param>
		/// <param name="v̄Comp">Constant Velocity (component)</param>
		/// <returns>Delta Time to travel distance <paramref name="ΔdComp"/>.</returns>
		public static float GetTravelTime(float ΔdComp, float v̄Comp)
		{
			return ΔdComp / v̄Comp;
		}

		/// <summary>
		/// Solves Δd = 𝑣I * Δt + 1/2 * a * Δt² for 𝑣I.
		/// Get velocity to travel given distance in given time with given acceleration.
		/// </summary>
		/// <param name="ΔdComp">The component distance to travel.</param>
		/// <param name="Δt">The travel time.</param>
		/// <param name="aComp">The component acceleration.</param>
		/// <returns>The component velocity.</returns>
		public static float GetDesiredVelocityFromDistance(float ΔdComp, float Δt, float aComp)
		{
			// d = vI * deltaT + 1/2 * a * deltaT ^ 2
			// -1/2 * a * deltaT ^ 2 + d = vI * deltaT
			// vI = (d / deltaT) - (1/2 * a * deltaT)
			return (float)((ΔdComp / Δt) - ((1.0 / 2.0) * aComp * Δt));
		}

		/// <summary>
		/// Solves vF = vI + aΔt for vI.
		/// </summary>
		/// <param name="accelComp"></param>
		/// <param name="Δt"></param>
		/// <param name="vFComp"></param>
		/// <returns></returns>
		public static float GetDesiredVelocity(float accelComp, float Δt, float vFComp = 0f)
		{
			// vF = vI + at
			// vI = vF - at
			return vFComp - accelComp * Δt;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vIComp"></param>
		/// <param name="Δt"></param>
		/// <param name="accelComp"></param>
		/// <returns></returns>
		public static float GetDistanceTravelled(float vIComp, float Δt, float accelComp)
		{
			// d = vI * deltaT + 1/2 * a * deltaT ^ 2
			return (float)(vIComp * Δt + (1.0 / 2.0) * accelComp * Δt * Δt);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vIComp"></param>
		/// <param name="accelComp"></param>
		/// <param name="Δd"></param>
		/// <returns></returns>
		public static float GetVelocityFinalComponent(float vIComp, float accelComp, float Δd)
		{
			// vF^2 = vI^2 + 2ad
			return Mathf.Sqrt(vIComp * vIComp + 2 * accelComp * Δd);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vIComp"></param>
		/// <param name="accelComp"></param>
		/// <param name="Δt"></param>
		/// <returns></returns>
		public static float GetVelocityFinalComponentByTime(float vIComp, float accelComp, float Δt)
		{
			// vF = vI + at
			return vIComp + accelComp * Δt;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accelComp"></param>
		/// <param name="Δd"></param>
		/// <param name="vIComp"></param>
		/// <returns></returns>
		public static float GetInitialVelocityComponentFromDistance(float accelComp, float Δd, float vFComp = 0f)
		{
			// vF^2 = vI^2 + 2ad
			// vI^2 = vF^2 - 2ad
			return vFComp * vFComp - 2 * accelComp * Δd;
		}

		//public static float GetInitialVelocityTrial(float deltaDComp, float deltaT, float vFComp = 0f)
		//{
		//	return ((2f * deltaDComp) / deltaT) + vFComp;
		//}
	}

}