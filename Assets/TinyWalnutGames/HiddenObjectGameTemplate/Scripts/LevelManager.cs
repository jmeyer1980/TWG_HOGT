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

        // Initialize the level manager
        void Start()
        {
            // Get the root visual element from the UIDocument
            root = UIDocument.rootVisualElement;
            // Get the level card list element
            levelCardList = root.Q<VisualElement>("LevelCardList");
            // Load the level cards
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
            levelCardList.Clear();
            foreach (var cardData in levelCards)
            {
                VisualElement levelCard = levelCardTemplate.CloneTree();
                SetLevelCardData(levelCard, cardData);
                levelCardList.Add(levelCard);
            }
        }


        // Set the level card data to the UI elements
        private void SetLevelCardData(VisualElement levelCard, LevelCard cardData)
        {
            // Set the level name
            Label levelNameLabel = levelCard.Q<Label>("LevelName");
            levelNameLabel.text = cardData.levelName;
            // Set the thumbnail image
            Image thumbnailImage = levelCard.Q<Image>("Thumbnail");
            thumbnailImage.image = cardData.thumbnail;
            // Set the tooltip
            Tooltip tooltip = levelCard.Q<Tooltip>();
            // the description for the tooltip is rendered as a label so we need that to access the text
            tooltip.Q<Label>("tooltip_description").text = cardData.levelTooltip;
            // Set the unlocked state
            Button unlockButton = levelCard.Q<Button>("UnlockButton");
            unlockButton.SetEnabled(!cardData.isUnlocked);
        }
    }
}
