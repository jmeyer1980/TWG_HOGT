/*
 * This code is part of a Unity script that prevents a GameObject from being destroyed when loading a new scene.
 */
using UnityEngine;

namespace TinyWalnutGames.Tools
{
    /// <summary>
    /// This class prevents a GameObject from being destroyed when loading a new scene.
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour
	{
        /// <summary>
        /// The instance of the DontDestroyOnLoad class.
        /// </summary>
        private static DontDestroyOnLoad instance;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}