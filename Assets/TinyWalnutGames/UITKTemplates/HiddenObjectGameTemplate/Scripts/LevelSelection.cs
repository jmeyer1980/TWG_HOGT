using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TinyWalnutGames.UITKTemplates.MainMenu;

namespace TinyWalnutGames.UITKTemplates.HOGT
{
    // Manages level progression and loading
    public class LevelSelection : MonoBehaviour
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

        private bool _preloadSubscribed = false;

        // Add flag to track when cards are loaded
        private bool levelCardsLoaded = false;

        // Ensure LevelManager instantiates or enables LevelUIController in Level01 scene if not already present.
        void Awake()
        {
            if (FindFirstObjectByType<LevelSelection>() == null)
            {
                Debug.Log("[LevelManager] LevelManager not found in the scene. Attempting to find it.");
                var levelManagerObject = this.GetComponent<LevelSelection>();
            } 
            else
            {
                Debug.Log("[LevelManager] LevelManager already exists in the scene.");

                // ensure level cards are assigned as we aree not in the actual level yet. This is level selection UI
                // Todo: rename this script to LevelSelection and update the other scripts accordingly as needed.
                if (levelCards == null || levelCards.Length == 0)
                {
                    Debug.LogError("[LevelManager] No level cards assigned! Please assign them in the Inspector.");
                }
            }

            // Ensure the level card template is assigned
            if (levelCardTemplate == null)
            {
                Debug.LogError("[LevelManager] Level card template is not assigned! Please assign it in the Inspector.");
                return;
            }

            // Initialize the level card template element
            LoadLevelCards();
        }

        // Initialize the level manager
        private void Start()
        {
            if (PreloadAssets.Instance != null && !PreloadAssets.Instance.IsReady)
            {
                Debug.Log("[LevelManager] Waiting for PreloadAssets to complete before starting level logic.");
                PreloadAssets.PreloadComplete += OnPreloadComplete;
                _preloadSubscribed = true;
            }
            else
            {
                Debug.Log("[LevelManager] PreloadAssets is ready, starting level logic immediately.");

            }

            // Disable play button at start
            root = UIDocument.rootVisualElement;
            playButton = root.Q<Button>("button_play");            
            if (playButton != null)
            {
                playButton.SetEnabled(false);
                playButton.clicked += OnPlayButtonClicked;
                Debug.Log("[LevelManager] Play button initialized and disabled.");
            }
            else
            {
                Debug.LogError("[LevelManager] Play button not found in the UI. Check UXML for name='button_play'.");
            }

            // Add this for the back button
            returnToMenuButton = root.Q<Button>("button_return_to_menu");
            if (returnToMenuButton != null)
                returnToMenuButton.clicked += OnReturnToMenuClicked;
        }

        private void OnDestroy()
        {
            if (_preloadSubscribed)
                PreloadAssets.PreloadComplete -= OnPreloadComplete;
        }

        private void OnPreloadComplete()
        {
            Debug.Log("[LevelManager] PreloadAssets.PreloadComplete fired. Now starting level selection logic by spawning the card visual element template.");
            // verify we haven't lost the level card template
            if (levelCardTemplate == null)
            {
                Debug.LogError("[LevelManager] Level card template is not assigned! Please assign it in the Inspector.");
                return;
            }
        }

        // Remove public scene loading methods, or make them private if still needed internally
        // (If not used elsewhere, you can safely delete these two methods)
        private void LoadLevel(int levelIndex)
        {
            if (IsLevelUnlocked(levelIndex))
            {
                PreloadAssets.LoadSceneWhenReady(levelIndex);
            }
            else
            {
                Debug.Log("Level is locked!");
            }
        }

        private void LoadLevel(string sceneName)
        {
            PreloadAssets.LoadSceneWhenReady(sceneName);
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
            StartCoroutine(LoadLevelCardsCoroutine());
        }

        private IEnumerator LoadLevelCardsCoroutine()
        {
            root = UIDocument.rootVisualElement;
            levelCardList = root.Q<VisualElement>("level_card_list");

            if (levelCards == null || levelCards.Length == 0)
            {
                Debug.LogError("[LevelManager] No level cards assigned! Please assign them in the Inspector.");
                yield break;
            }
            if (levelCardTemplate == null)
            {
                Debug.LogError("[LevelManager] Level card template is not assigned! Please assign it in the Inspector.");
                yield break;
            }
            if (levelCardList == null)
            {
                Debug.LogError("[LevelManager] Level card list VisualElement not found in the UI.");
                yield break;
            }

            levelCardList.Clear();

            // Simulate async loading if needed (e.g., for asset loading)
            for (int i = 0; i < levelCards.Length; i++)
            {
                LevelCard cardData = levelCards[i];
                VisualElement levelCard = levelCardTemplate.CloneTree();

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

                levelCard.RegisterCallback<ClickEvent>(evt =>
                {
                    if (levelCardsLoaded && cardData.isUnlocked)
                    {
                        selectedLevelCard = cardData;
                        selectedLevelIndex = cardData.levelIndex;
                        playButton?.SetEnabled(true);

                        Debug.Log($"Level {cardData.levelIndex} selected!");
                    }
                    else if (!levelCardsLoaded)
                    {
                        Debug.LogWarning("Cards not fully loaded yet!");
                    }
                    else
                    {
                        Debug.Log("Level is locked!");
                    }
                });

                levelCardList.Add(levelCard);

                // Optional: yield return null; // Uncomment if you want to spread loading over multiple frames
            }

            // All cards are now loaded
            levelCardsLoaded = true;
            if (selectedLevelCard != null && selectedLevelCard.isUnlocked)
                playButton?.SetEnabled(true);
            else
                playButton?.SetEnabled(false);
            yield break;
        }

        private void OnPlayButtonClicked()
        {
            if (!levelCardsLoaded)
            {
                Debug.LogWarning("Cannot load scene: Level cards not finished loading.");
                return;
            }

            if (selectedLevelCard != null && selectedLevelCard.isUnlocked)
            {
                // Load by scene name for flexibility
                if (!string.IsNullOrEmpty(selectedLevelCard.sceneName))
                {
                    PreloadAssets.LoadSceneWhenReady(selectedLevelCard.sceneName);
                }
                else
                {
                    PreloadAssets.LoadSceneWhenReady(selectedLevelCard.levelIndex);
                }
            }
            else
            {
                Debug.LogWarning("No level card selected or card is locked.");
            }
        }

        private void OnReturnToMenuClicked()
        {
            PreloadAssets.LoadSceneWhenReady("MainMenu");
        }
    }
}
