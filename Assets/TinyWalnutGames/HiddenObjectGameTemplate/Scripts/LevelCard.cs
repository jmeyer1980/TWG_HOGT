using UnityEngine;

namespace TinyWalnutGames.HOGT
{
    // level card template class for scriptable level object to show in the UI
    [System.Serializable]
    [CreateAssetMenu(fileName = "LevelCard", menuName = "HOGT/LevelCard")]
    public class LevelCard : ScriptableObject
    {
        public string levelName; // Name of the level
        public int levelIndex; // Index of the level in the build settings
        public string levelTooltip; // Description of the level for the tooltips
        public Texture thumbnail; // Thumbnail image for the level
        public bool isUnlocked; // Is the level unlocked?
        public bool isCompleted; // Is the level completed?

        void OnEnable()
        {
            // Initialize the level card properties
            levelName = "Level " + levelIndex;
            levelTooltip = "Description for " + levelName;
            isUnlocked = false;
            isCompleted = false;
        }
    }
}
