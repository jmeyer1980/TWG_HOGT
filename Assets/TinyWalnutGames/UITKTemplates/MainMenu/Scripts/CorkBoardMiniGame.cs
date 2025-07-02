using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyWalnutGames.UITKTemplates.Tools;
using TinyWalnutGames.UITKTemplates.MainMenu;

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    public class CorkBoardMiniGame : MonoBehaviour
    {
        public UIDocument uiDocument;
        public string playButtonName = "button_play";

        // Dictionary to define matches: key element must be matched with value element
        public Dictionary<string, string> elementMatches = new()
        {
            // Example pairs; replace with your actual element names
            { "Signup", "AnotherSignup" },
            { "SchoolBingo", "SchoolPicture" },
            { "Music2", "Music" },
            { "Notes", "SunnyDayActivities" },
            { "Schoolgirls", "ThePath" }
        };

        // Track which pairs have been matched
        private readonly HashSet<string> matchedPairs = new();
        private Button playButton;

        private const string PlayerPrefsKey = "CorkBoardMiniGame_CompletedPairs";

        // --- UI Sound Effect Helper (mirrors MainMenuController) ---
        /// <summary>
        /// Safely plays a UI sound effect by key using AudioManager.Instance, if available.
        /// </summary>
        private static void PlayUISound(string key)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(key);
        }

        void Start()
        {
            var root = uiDocument.rootVisualElement;
            Debug.Log($"[CorkBoardMiniGame] rootVisualElement child count: {root.childCount}");
            foreach (var child in root.Children())
                Debug.Log($"[CorkBoardMiniGame] Child: {child.name} ({child.GetType()})");

            playButton = root.Q<Button>(playButtonName);
            // Disable play button initially, but avoid null propogation
            playButton?.SetEnabled(false);

            // Register draggable for all elements in the dictionary (both keys and values)
            var allElements = elementMatches.Keys.Concat(elementMatches.Values).Distinct();
            foreach (var name in allElements)
            {
                var element = root.Q<VisualElement>(name);
                if (element != null)
                    MakeDraggable(element);
            }

            // Load completion state from PlayerPrefs if available
            LoadCompletionState();
        }

        void MakeDraggable(VisualElement element)
        {
            Vector2 offset = Vector2.zero;
            bool dragging = false;

            element.pickingMode = PickingMode.Position;

            element.RegisterCallback<PointerDownEvent>(evt =>
            {
                dragging = true;
                offset = evt.localPosition;
                element.CapturePointer(evt.pointerId);
                PlayUISound("grab"); // Play 'grab' sound when picking up a draggable
            });

            element.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (dragging)
                {
                    var newPos = (Vector2)evt.position - offset;
                    element.style.left = newPos.x;
                    element.style.top = newPos.y;
                }
            });

            element.RegisterCallback<PointerUpEvent>(evt =>
            {
                dragging = false;
                element.ReleasePointer(evt.pointerId);

                // On drop, check if this element is over its match
                bool matched = TryMatchElement(element);
                if (matched)
                {
                    PlayUISound("pick"); // Play 'pick' sound for a successful match
                }
                else
                {
                    PlayUISound("release"); // Play 'release' sound for a drop without match
                }
            });
        }

        // Returns true if a match was made
        bool TryMatchElement(VisualElement draggedElement)
        {
            string matchName = null;
            bool isKey = elementMatches.ContainsKey(draggedElement.name);
            bool isValue = elementMatches.ContainsValue(draggedElement.name);

            if (isKey)
                matchName = elementMatches[draggedElement.name];
            else if (isValue)
                matchName = elementMatches.FirstOrDefault(kv => kv.Value == draggedElement.name).Key;

            if (matchName == null)
                return false;

            var root = uiDocument.rootVisualElement;
            var matchElement = root.Q<VisualElement>(matchName);

            if (matchElement == null)
                return false;

            // Simple overlap check (bounding box intersection)
            var draggedRect = draggedElement.worldBound;
            var matchRect = matchElement.worldBound;

            if (draggedRect.Overlaps(matchRect))
            {
                // Mark this pair as matched (order-independent)
                var pairKey = GetPairKey(draggedElement.name, matchName);
                if (!matchedPairs.Contains(pairKey))
                {
                    matchedPairs.Add(pairKey);

                    PlayUISound("pick"); // Play 'pick' sound for a successful match

                    // Hide the matched draggable elements (direct children of "draggables")
                    draggedElement.style.display = DisplayStyle.None;
                    matchElement.style.display = DisplayStyle.None;

                    // Force style refresh to ensure UI updates immediately
                    draggedElement.MarkDirtyRepaint();
                    matchElement.MarkDirtyRepaint();

                    CheckCompletion();

                    // Save progress after a successful match
                    SaveCompletionState();

                    return true; // Indicate a match was made
                }
            }
            return false;
        }

        // Helper to create a unique key for a pair (order-independent)
        string GetPairKey(string a, string b)
        {
            return string.CompareOrdinal(a, b) < 0 ? $"{a}|{b}" : $"{b}|{a}";
        }

        void CheckCompletion()
        {
            if (matchedPairs.Count == elementMatches.Count && playButton != null)
            {
                playButton.SetEnabled(true);
                // Save completion state when finished
                SaveCompletionState();
                PlayUISound("open"); // Play 'open' sound when minigame is completed
            }
        }

        // --- PlayerPrefs Save/Load Logic ---

        void SaveCompletionState()
        {
            // Save matched pairs as a single string (pipe-separated)
            var sb = new StringBuilder();
            foreach (var pair in matchedPairs)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(pair);
            }
            PlayerPrefs.SetString(PlayerPrefsKey, sb.ToString());
            PlayerPrefs.Save();
        }

        void LoadCompletionState()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
                return;

            var saved = PlayerPrefs.GetString(PlayerPrefsKey, "");
            if (string.IsNullOrEmpty(saved))
                return;

            var root = uiDocument.rootVisualElement;
            var pairs = saved.Split(',');
            foreach (var pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair)) continue;
                matchedPairs.Add(pair);

                // Parse the pair to get element names
                var names = pair.Split('|');
                if (names.Length != 2) continue;

                var elementA = root.Q<VisualElement>(names[0]);
                var elementB = root.Q<VisualElement>(names[1]);
                if (elementA != null) elementA.style.display = DisplayStyle.None;
                if (elementB != null) elementB.style.display = DisplayStyle.None;
            }

            // If all pairs matched, enable play button
            if (matchedPairs.Count == elementMatches.Count && playButton != null)
            {
                playButton.SetEnabled(true);
            }
        }

        /// <summary>
        /// Resets the minigame state, UI, and PlayerPrefs for this minigame only.
        /// Does not affect the main game's save state.
        /// </summary>
        public void ResetMiniGame()
        {
            matchedPairs.Clear();

            // Remove PlayerPrefs entry for this minigame
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();

            var root = uiDocument.rootVisualElement;

            // Reset all draggable elements' display and position
            var allElements = elementMatches.Keys.Concat(elementMatches.Values).Distinct();
            foreach (var name in allElements)
            {
                var element = root.Q<VisualElement>(name);
                if (element != null)
                {
                    element.style.display = DisplayStyle.Flex;
                    // Optionally reset position if you want to snap back to original
                    element.style.left = StyleKeyword.Null;
                    element.style.top = StyleKeyword.Null;
                    element.MarkDirtyRepaint();
                }
            }

            // Disable play button
            playButton?.SetEnabled(false);

            PlayUISound("error"); // Play 'error' sound when resetting the minigame
        }
    }
}
