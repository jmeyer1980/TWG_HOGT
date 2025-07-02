/*
 * This code is part of a Unity script that prevents a GameObject from being destroyed when loading a new scene.
 */
using UnityEngine;

namespace TinyWalnutGames.UITKTemplates.Tools
{
    /// <summary>
    /// This class prevents a GameObject from being destroyed when loading a new scene.
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour
	{
        /// <summary>
        /// The instance of the DontDestroyOnLoad class.
        /// </summary>
        private static DontDestroyOnLoad Instance { get; set; }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
		{
            // Ensure this is the only instance
            if (Instance == null)
            {
                Instance = this;
                // deparent the GameObject to avoid issues with scene loading
                transform.SetParent(null, true);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
	}
}