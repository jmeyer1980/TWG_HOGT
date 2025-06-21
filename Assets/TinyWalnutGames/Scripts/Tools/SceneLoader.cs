/*
 * This code is part of a Unity script that manages scene loading and unloading.
*/
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TinyWalnutGames.Tools
{
    /// <summary>
    /// This class is responsible for loading and unloading scenes in Unity.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// The name of the scene to be loaded.
        /// </summary>
        public string sceneName;

        /// <summary>
        /// Loads the next scene via scene index.
        /// Scenes must be added to the build settings for this to work.
        /// </summary>
        public void LoadNextScene()
        {
            // Get the current scene index and calculate the next scene index
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // Check if the next scene index is within the range of the build settings
            if (nextSceneIndex >= 0)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("No more scenes to load!");
            }
        }

        /// <summary>
        /// Loads the previous scene via scene index.
        /// </summary>
        public void LoadPreviousScene()
        {
            // Get the current scene index and calculate the previous scene index
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex - 1;

            // Check if the next scene index is within the range of the build settings
            if (nextSceneIndex >= 0)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("No more scenes to load!");
            }
        }

        /// <summary>
        /// Loads a scene by its name.
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadSceneByName(string sceneName)
        {
            // Check if the scene name is valid
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.Log("Scene " + sceneName + " cannot be loaded. Please check the scene name.");
            }
        }
    }
}
