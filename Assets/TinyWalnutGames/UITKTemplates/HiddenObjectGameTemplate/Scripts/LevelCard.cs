using UnityEngine;
using UnityEngine.UIElements;
using TinyWalnutGames.UITKTemplates.Tools;

namespace TinyWalnutGames.UITKTemplates.HOGT
{
    // level card template class for scriptable level object to show in the UI
    [System.Serializable]
    [CreateAssetMenu(fileName = "LevelCard", menuName = "HOGT/Scriptable Objects/LevelCard")]
    public class LevelCard : ScriptableObject
    {
        public string levelName; // Name of the level
        public string levelNameKey; // Localization key for the level name
        public int levelIndex; // Index of the level in the build settings
        public string levelDescription; // Description of the level for the tooltips
        public string levelDescriptionKey; // Localization key for the level description for tooltips
        public Texture thumbnail; // Thumbnail image for the level
        public bool isUnlocked; // Is the level unlocked?
        public bool isCompleted; // Is the level completed?

        public string sceneName; // Name of the scene to load for this level

        // Constructor for Modular Validator
        private UIDocumentValidationConfig config;

        public UIDocumentValidationConfig Config
        {
            get
            {
                config ??= new UIDocumentValidationConfig
                {
                    RequiredElementNames = new string[] { "LevelCardTemplate" },
                    RequiredElementTypes = new System.Type[] { typeof(VisualElement) },
                    NamedElements = new System.Collections.Generic.Dictionary<string, System.Type>
                        {
                            { "level_card_thumbnail", typeof(VisualElement) },
                            { "lvl_int", typeof (Label) },
                            { "lvl_name", typeof(Label) },
                            { "LevelCardTemplate", typeof(VisualElement) } // contains the tooltip we need to display
                        },
                    ProgressBarName = "level_progress_bar"
                };
                return config;
            }
        }

        /// <summary>
        /// OnEnable is called when the scriptable object is enabled.
        /// </summary>
        void OnEnable()
        {
            // Initialize the level card properties
            if (string.IsNullOrEmpty(levelName))
                levelName = "Level " + levelIndex;            
        }
    }
    /// <Remarks>
    /// levelName and levelTooltip, or levelDescription, can be localized but we don't do that here, this data would 
    /// receive that localization from the LocalizationTableHolder and the LocalizationHelper.
    /// The level card is used to display the level in the UI, so it should contain all the necessary
    /// information to show the level in the UI, such as the name, index, tooltip, thumbnail, and
    /// whether it is unlocked or completed. The sceneName is used to load the scene for this level when
    /// the player selects it from the UI.
    /// </Remarks>
}
