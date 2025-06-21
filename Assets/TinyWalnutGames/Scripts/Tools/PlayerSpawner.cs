/*
 * This code is part of a Unity script that spawns a player character at a specified spawn point.
 */
using UnityEngine;

namespace TinyWalnutGames.Tools
{
    /// <summary>
    /// This class is responsible for spawning a player character at a specified spawn point.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
	{
        /// <summary>
        /// The prefab of the character to be spawned.
        /// </summary>
        public GameObject characterPrefab;

        /// <summary>
        /// The default spawn point for the character.
        /// </summary>
        public Transform defaultSpawnpoint;

        /// <summary>
        /// The key used to store the player's position in PlayerPrefs.
        /// </summary>
        private const string PlayerPositionKey = "PlayerPosition";

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
		{
			SpawnCharacter();
		}

        /// <summary>
        /// Saves the player's position to PlayerPrefs.
        /// </summary>
        /// <param name="position"></param>
        public void SavePlayerPosition(Vector3 position)
		{
			PlayerPrefs.SetFloat(PlayerPositionKey + "X", position.x);
			PlayerPrefs.SetFloat(PlayerPositionKey + "Y", position.y);
			PlayerPrefs.SetFloat(PlayerPositionKey + "Z", position.z);
			PlayerPrefs.Save();
		}

        /// <summary>
        /// Loads the player's position from PlayerPrefs.
        /// </summary>
        /// <returns>
		/// defaultSpawnpoint.position if no saved position is found.
		/// </returns>
        public Vector3 LoadPlayerPosition()
		{
            // Check if the PlayerPrefs contains the player's position
            if (PlayerPrefs.HasKey(PlayerPositionKey + "X"))
			{
                // Load the player's position from PlayerPrefs
                float x = PlayerPrefs.GetFloat(PlayerPositionKey + "X");
				float y = PlayerPrefs.GetFloat(PlayerPositionKey + "Y");
				float z = PlayerPrefs.GetFloat(PlayerPositionKey + "Z");

                // Create a new Vector3 with the loaded position
                return new Vector3(x, y, z);
			}

            // If no saved position is found, return the default spawn point's position
            return defaultSpawnpoint.position;
		}

        /// <summary>
        /// Spawns the character at the saved position or at the default spawn point if no saved position is found.
        /// </summary>
        public void SpawnCharacter()
		{
            // Check if the character prefab is assigned
            if (characterPrefab != null)
			{
                // Load the player's position
                Vector3 spawnPosition = LoadPlayerPosition();

                // Check if the spawn position is valid
                GameObject newCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

                // Set the parent of the new character to the default spawn point
                newCharacter.transform.SetParent(defaultSpawnpoint, false);

                // Set the local position of the new character to zero
                newCharacter.transform.localPosition = Vector3.zero;
			}
			else
			{
				Debug.LogWarning("CharacterPrefab is not assigned.");
			}
		}
	}
}