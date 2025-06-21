/* 
 * This code is part of a Unity script that adds selected scenes to the build settings. It includes a menu item for adding scenes and another for adding scenes and opening the build settings window.
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace TinyWalnutGames.Tools
{
    /// <summary>
    /// This class adds selected scenes to the build settings in Unity.
    /// </summary>
    public class AddSceneToBuildSettings
    {
        /// <summary>
        /// Menu item to add selected scenes to build settings.
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Add Scene(s) to Build Settings", true)]
        private static bool ValidateAddSceneToBuildSettings()
        {
            // Validate that at least one selected object is a scene
            return Selection.objects.Any(obj => obj is SceneAsset);
        }

        /// <summary>
        /// Adds selected scenes to the build settings.
        /// </summary>
        [MenuItem("Assets/Add Scene(s) to Build Settings", false, 100)]
        private static void AddScenesToBuildSettingsMenu()
        {
            // Backup the current build settings
            var backupScenes = EditorBuildSettings.scenes.ToArray();

            try
            {
                // Get the selected scene paths
                var scenePaths = Selection.objects
                    .Where(obj => obj is SceneAsset)
                    .Select(obj => AssetDatabase.GetAssetPath(obj))
                    .Where(path => !string.IsNullOrEmpty(path)) // Ensure the path is valid
                    .ToList();

                if (scenePaths.Count == 0)
                {
                    Debug.LogWarning("No valid scenes selected to add to build settings.");
                    return;
                }

                // Get the current build settings scenes
                var currentScenes = EditorBuildSettings.scenes.ToList();

                // Add new scenes that are not already in the build settings
                bool scenesAdded = false;
                foreach (var scenePath in scenePaths)
                {
                    if (!currentScenes.Any(scene => scene.path == scenePath))
                    {
                        currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                        scenesAdded = true;
                        Debug.Log("Added scene to build settings: " + scenePath);
                    }
                    else
                    {
                        Debug.Log("Scene already in build settings: " + scenePath);
                    }
                }

                if (scenesAdded)
                {
                    // Assign the updated list to the build settings
                    EditorBuildSettings.scenes = currentScenes.ToArray();
                }
                else
                {
                    Debug.Log("No new scenes were added to the build settings.");
                }
            }
            catch (System.Exception ex)
            {
                // Log the error
                Debug.LogError($"An error occurred while adding scenes to the build settings: {ex.Message}");

                // Restore the backup to maintain integrity
                EditorBuildSettings.scenes = backupScenes;

                Debug.LogWarning("The build settings have been restored to their previous state.");
            }
        }

        /// <summary>
        /// Menu item to add selected scenes to build settings and open the build settings window.
        /// </summary>
        [MenuItem("Assets/Add Scene(s) and Show Build Settings", false, 101)]
        private static void AddScenesToBuildSettingsAndOpenMenu()
        {
            // Add the scenes to build settings
            AddScenesToBuildSettingsMenu();

            // Check Unity version and open the appropriate menu
            if (Application.unityVersion.StartsWith("6"))
            {
                // Open the Build Profiles window for Unity 6+
                EditorApplication.ExecuteMenuItem("File/Build Profiles");
            }
            else
            {
                // Open the Build Settings window for earlier versions
                EditorApplication.ExecuteMenuItem("File/Build Settings...");
            }
        }
    }
}
#endif