using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "sfxs_NewGroup", menuName = "ScriptableObjects/SFX_Group")]
    public class SFX_Group : ScriptableObject
    {
        public AudioClip[] sfxs;
        public string label;
        public int Length { get => sfxs.Length; }

        /// <summary>
        /// Gets a random <see cref="AudioClip"/> from <see cref="sfxs"/>.
        /// </summary>
        /// <returns>A random <see cref="AudioClip"/> from <see cref="sfxs"/>.</returns>
        public AudioClip GetClip()
		{
            if (sfxs.Length == 0)
			{
                Debug.LogWarning("Warning: No sfx in collection.");
                return null;
			}
            return sfxs[Random.Range(0, sfxs.Length)];
		}
    }
}
