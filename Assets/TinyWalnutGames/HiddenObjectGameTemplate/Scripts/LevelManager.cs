using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using TinyWalnutGames.UI;

namespace TinyWalnutGames.HOGT
{
    // Manages level progression and loading
    public class LevelManager : MonoBehaviour
    {
        public UIDocument UIDocument; // Reference to the UI Document
        public VisualTreeAsset levelCardTemplate; // Reference to the level card template. Is this correct? The template is saved as a UI Document
        public int currentLevelIndex = 0; // Highest unlocked level
        public LevelCard[] levelCards; // Assign these in the Inspector


        // Reference to the root visual element
        private VisualElement root;
        // Reference to the level card list
        private VisualElement levelCardList;
        // Reference to the level card template
        private readonly VisualElement levelCardTemplateElement;

        private LevelCard selectedLevelCard = null;
        private int selectedLevelIndex = -1;

        private Button playButton;
        private Button returnToMenuButton;

        // Initialize the level manager
        void Start()
        {
            root = UIDocument.rootVisualElement;
            levelCardList = root.Q<GroupBox>("level_list");

            playButton = root.Q<Button>("button_play");
            returnToMenuButton = root.Q<Button>("button_return_to_menu");

            if (playButton != null)
            {
                playButton.SetEnabled(false);
                playButton.clicked += OnPlayButtonClicked;
            }
            if (returnToMenuButton != null)
            {
                returnToMenuButton.clicked += OnReturnToMenuClicked;
            }

            LoadLevelCards();
        }

        // Call this method to attempt to load a level by its build index
        public void LoadLevel(int levelIndex)
        {
            if (IsLevelUnlocked(levelIndex))
            {
                SceneManager.LoadScene(levelIndex);
            }
            else
            {
                Debug.Log("Level is locked!");
            }
        }

        // Determine if the given level is unlocked
        private bool IsLevelUnlocked(int levelIndex)
        {
            // TODO: Create your own unlocking logic (e.g., based on previous level completions)
            return levelIndex <= currentLevelIndex + 1;
        }

        // Load the level cards into the UI
        private void LoadLevelCards()
        {
            foreach (var cardData in levelCards)
            {
                VisualElement levelCard = levelCardTemplate.CloneTree();
                SetLevelCardData(levelCard, cardData);

                levelCard.RegisterCallback<ClickEvent>(evt =>
                {
                    if (cardData.isUnlocked)
                    {
                        selectedLevelCard = cardData;
                        selectedLevelIndex = cardData.levelIndex;
                        if (playButton != null)
                            playButton.SetEnabled(true);

                        Debug.Log($"Level {cardData.levelIndex} selected!");
                        // Optionally, add visual feedback for selection here
                    }
                    else
                    {
                        Debug.Log("Level is locked!");
                    }
                });

                levelCardList.Add(levelCard);
            }
        }

        // Set the level card data to the UI elements
        private void SetLevelCardData(VisualElement levelCard, LevelCard cardData)
        {
            // Set the level index
            var levelIndexLabel = levelCard.Q<Label>("lvl_int");
            if (levelIndexLabel != null)
                levelIndexLabel.text = cardData.levelIndex.ToString();

            // Set the level name
            var levelNameLabel = levelCard.Q<Label>("lvl_name");
            if (levelNameLabel != null)
                levelNameLabel.text = cardData.levelName;

            // Set the thumbnail image as background
            var spriteElement = levelCard.Q<VisualElement>("level_sprite");
            if (spriteElement != null && cardData.thumbnail != null)
                spriteElement.style.backgroundImage = new StyleBackground((Texture2D)cardData.thumbnail);

            // Set lock overlay visibility
            var lockElement = levelCard.Q<VisualElement>("LevelLock");
            if (lockElement != null)
                lockElement.style.display = cardData.isUnlocked ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnPlayButtonClicked()
        {
            if (selectedLevelCard != null && selectedLevelCard.isUnlocked)
            {
                // Load by scene name for flexibility
                if (!string.IsNullOrEmpty(selectedLevelCard.sceneName))
                    SceneManager.LoadScene(selectedLevelCard.sceneName);
                else
                    SceneManager.LoadScene(selectedLevelCard.levelIndex);
            }
        }

        private void OnReturnToMenuClicked()
        {
            // Replace "MainMenu" with your actual main menu scene name
            SceneManager.LoadScene("MainMenu");
        }

    }
}
