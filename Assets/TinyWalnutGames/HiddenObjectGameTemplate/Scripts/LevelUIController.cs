using TinyWalnutGames.HOGT;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// Controls the population of the LevelOverlay UI with data from a LevelData ScriptableObject.
/// Refactored for testability: UI logic is separated into methods, dependencies can be injected, and fields are accessible for testing.
/// </summary>
namespace TinyWalnutGames.HOGT
{
    public class LevelUIController : MonoBehaviour
    {
        public LevelData levelData; // Assign in Inspector or via code
        public UIDocument uiDocument; // Reference to the UI Document that contains the UI elements for this level

        // UI element references for testability and Inspector assignment
        [FormerlySerializedAs("levelNameLabel")] [SerializeField] private Label levelNameLabel;
        [FormerlySerializedAs("backgroundImage")] [SerializeField] private VisualElement backgroundImage;
        [FormerlySerializedAs("objectsList")] [SerializeField] private VisualElement objectsList;

        /// <summary>
        /// Allows injection of dependencies for testing.
        /// </summary>
        public void InjectDependencies(LevelData data, UIDocument document)
        {
            levelData = data;
            uiDocument = document;
        }

        private void Awake()
        {
            // Ensure levelData and uiDocument are assigned
            if (levelData == null)
            {
                Debug.LogError("LevelData is not assigned in LevelUIController.");
                levelData = Resources.Load<LevelData>("LD01");
            }
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument is not assigned in LevelUIController.");
                uiDocument = Resources.Load<UIDocument>("UIDocument");
            }
        }

        private void Start()
        {
            InitializeUIReferences();
            PopulateUI();
        }

        /// <summary>
        /// Initializes UI element references from the UIDocument.
        /// </summary>
        public void InitializeUIReferences()
        {
            if (uiDocument == null) return;
            var root = uiDocument.rootVisualElement;
            levelNameLabel = root.Q<Label>("levelNameLabel");
            backgroundImage = root.Q<VisualElement>("backgroundImage");
            objectsList = root.Q<VisualElement>("objectsList");
        }

        /// <summary>
        /// Populates all UI elements with data from LevelData.
        /// </summary>
        public void PopulateUI()
        {
            SetLevelName();
            SetBackgroundImage();
            PopulateObjectsList();
        }

        /// <summary>
        /// Sets the level name label.
        /// </summary>
        public void SetLevelName()
        {
            if (levelNameLabel != null && levelData != null)
                levelNameLabel.text = levelData.levelName;
        }

        /// <summary>
        /// Sets the background image.
        /// </summary>
        public void SetBackgroundImage()
        {
            if (backgroundImage != null && levelData != null && levelData.levelBackground != null)
                backgroundImage.style.backgroundImage = new StyleBackground(levelData.levelBackground);
        }

        /// <summary>
        /// Populates the list of objects to find.
        /// </summary>
        public void PopulateObjectsList()
        {
            if (objectsList != null && levelData != null && levelData.objectsToFind != null)
            {
                objectsList.Clear();
                foreach (var obj in levelData.objectsToFind)
                {
                    var objLabel = new Label(obj.objectName);
                    objectsList.Add(objLabel);
                }
            }
        }
    }
}
