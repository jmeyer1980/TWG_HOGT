using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using UnityEngine.UIElements;
using TinyWalnutGames.UITKTemplates.MainMenu;

namespace TinyWalnutGames.UITKTemplates.HOGT
{
    /// <summary>
    /// ScriptableObject to hold level data for the Hidden Object Game Template.
    /// This game uses the UI Toolkit, so this scriptable object can be used to store level-specific data such as:
    /// The level background image, the level name, the list of objects to find, and any other level-specific settings.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "HOGT/Scriptable Objects/LevelData")]
    [Serializable]
    public class LevelData : ScriptableObject
    {
        public string levelName; // The name of the level
        public string levelNameKey; // The localization key for the level name, used for localization purposes
        public string levelDescription; // The description of the level, can be used for UI display or tooltips
        public string levelDescriptionKey; // The localization key for the level description, used for localization purposes
        public int levelNumber; // The non-zero index of the level, used for sorting and identification
        public bool isUnlocked; // Whether the level is unlocked or not, used for UI display and logic
        public Texture2D levelBackground; // The background texture for the level
        public Texture2D levelForeground; // The foreground texture for the level, could be considerd the overlay where we show the 

        // level rules, like time limit, number of objects to find, etc. can be added here as well
        public bool hasTimeLimit; // Whether the level has a time limit or not
        public float timeLimit; // The time limit for the level, in seconds
        private int numberOfObjectsToFind; // total number of objects in objectsToFind, used for UI display and logic
        public List<HiddenObjectData> objectsToFind = new();
        public List<HiddenObjectData> objectsFound = new();

        // what if the level requires a 3d scene to be loaded?
        public string sceneName; // The name of the scene to load for this level, if applicable

        // Ensure we use the correct UI Document for this level
        public VisualTreeAsset uiDocument; // The UI Document for this level, used to load the UI elements specific to this level

        // If you use addressable assets for levelBackground, levelForeground, or uiDocument,
        // ensure they are labeled "Preload" and loaded/cached via PreloadAssets if needed.

        public void Initialize()
        {
            // Ensure the level data is initialized properly
            objectsToFind ??= new();
            // Calculate the number of objects to find
            numberOfObjectsToFind = objectsToFind.Count;
            // Optionally, you can preload assets here if they are addressables
            if (levelBackground == null)
                levelBackground = AssetPreloader.Instance.Get<Texture2D>("LevelBackgroundKey");
            if (levelForeground == null)
                levelForeground = AssetPreloader.Instance.Get<Texture2D>("LevelForegroundKey");
            if (uiDocument == null)
                uiDocument = AssetPreloader.Instance.Get<VisualTreeAsset>("LevelUIDocumentKey");
        }

        /// <Remarks>
        /// While all textures and UI documents can be addressable assets, this scriptable object
        /// is designed to be used as a data holder for the level. It can be used to store
        /// level-specific data such as the level name, description, and background image.
        /// While the keys are used for localization, the actual localization logic is within
        /// The UI Controller and LocalizationHelper. The UI Controller requests the key
        /// gives it to LocalizationHelper, which provides the localized text.
        /// </Remarks>
    }
}
