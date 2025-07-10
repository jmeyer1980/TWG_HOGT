using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TinyWalnutGames.UITKTemplates.MainMenu;

namespace TinyWalnutGames.UITKTemplates.HOGT
{
    [ExecuteAlways]
    // ScriptableObject to hold data for hidden objects
    [CreateAssetMenu(fileName = "HiddenObjectData", menuName = "HOGT/Scriptable Objects/HiddenObjectData")]
    [Serializable]
    public class HiddenObjectData : ScriptableObject
    {
        // Name of the object (for identification)
        public string objectName;
        // localization key for the object name, used for localization purposes
        public string objectNameKey;
        public string objectID;
        // position in the UI Document
        public Vector2 position;
        // size of the clickable area (optional)
        public Vector2 size;
        // whether the object is found
        // public bool isFound = false; // This should be handled by a controller; todo
        // additional metadata (e.g., hints, description)
        [TextArea]
        // description of the object, used for hints or tooltips
        public string description;
        // localization key for the description, used for localization purposes
        public string descriptionKey;
        // List of tags for categorization
        public List<string> tags = new();
        // is the object draggable
        public bool isDraggable = false; // if true, the object can be dragged around in the UI
                                         // is secret object
        public bool isSecret = false; // I am evil. Secret objects are hidden and not shown in the UI until found but aren't required to win the level.
        // reference to the visual element representing the object in the playing field, in other words, the item in the game and not the UI
        [HideInInspector] public VisualElement visualElement;
        // the visual tree asset that represents the template for all hidden objects
        public VisualTreeAsset templateElement;
        [HideInInspector] public VisualElement playAreaElement;

        // --- Dynamic: Multiple object sprites and toast backgrounds ---
        [Serializable]
        public class ObjectSpriteVariant
        {
            public string key; // e.g. "default", "found", "en", "zh-Hans"
            public Texture2D sprite;
        }
        [Serializable]
        public class ToastBackgroundVariant
        {
            public string key; // e.g. "default", "found", "en", "zh-Hans"
            public Texture2D background;
        }

        // Use one or both of these as needed:
        public List<Texture2D> objectSprites = new(); // Simple list for variants
        public List<ObjectSpriteVariant> objectSpriteVariants = new(); // Named variants

        public List<Texture2D> toastBackgrounds = new(); // Simple list for toast backgrounds
        public List<ToastBackgroundVariant> toastBackgroundVariants = new(); // Named variants

        // --- Helpers to get sprite/background by index or key ---
        public Texture2D GetObjectSprite(int index = 0) =>
            (objectSprites != null && objectSprites.Count > index) ? objectSprites[index] : null;

        public Texture2D GetObjectSpriteByKey(string key)
        {
            if (objectSpriteVariants == null) return null;
            var found = objectSpriteVariants.Find(v => v.key == key);
            return found?.sprite;
        }

        public Texture2D GetToastBackground(int index = 0) =>
            (toastBackgrounds != null && toastBackgrounds.Count > index) ? toastBackgrounds[index] : null;

        public Texture2D GetToastBackgroundByKey(string key)
        {
            if (toastBackgroundVariants == null) return null;
            var found = toastBackgroundVariants.Find(v => v.key == key);
            return found?.background;
        }

        public void Initialize()
        {
            // There are no prefabs because of UI Toolkit. Load the hidden object templateElement are initialized properly
            if (templateElement == null)
            {
                templateElement = AssetPreloader.Instance.Get<VisualTreeAsset>("HiddenObjectTemplateKey");
                if (templateElement == null)
                {
                    Debug.LogError("HiddenObjectTemplateKey not found in preloaded assets.");
                }
            }

            // Ensure the objectID is unique
            if (string.IsNullOrEmpty(objectID))
            {
                objectID = Guid.NewGuid().ToString(); // Generate a unique ID if not set
            }
            // Ensure the objectName is set; but we will not be using the key here, see the UI Controller of the scene for that.
            if (string.IsNullOrEmpty(objectName))
            {
                objectName = "Hidden Object"; // Default name if not set
            }
            // Ensure the position and size are set to default values if not set
            if (position == Vector2.zero)
            {
                position = new Vector2(0, 0); // Default position
            }
            if (size == Vector2.zero)
            {
                size = new Vector2(100, 100); // Default size
            }
            // Ensure the tags list is initialized
            tags ??= new List<string>();
            // Ensure the description is set
            if (string.IsNullOrEmpty(description))
            {
                description = "This is a hidden object."; // Default description if not set
            }
            // Ensure the visualElement is initialized
            if (visualElement == null && templateElement != null)
            {
                visualElement = templateElement.CloneTree();
                visualElement.name = objectID; // Set the name to the objectID for easy identification
                // Optionally, set the background image or other properties of the visual element

                // Disambiguate Image type for UI Toolkit
                var image = visualElement.Q<UnityEngine.UIElements.Image>();
                Texture2D spriteToUse = GetObjectSprite(0);
                if (image != null && spriteToUse != null)
                {
                    // Try to set the image property if the texture is a Texture2D
                    image.image = spriteToUse;
                    // If not a Texture2D, fallback to setting the backgroundImage
                    if (image.image == null)
                    {
                        image.style.backgroundImage = new(spriteToUse);
                    }
                }
            }
            else if (visualElement == null)
            {
                Debug.LogError("VisualElement is null and no templateElement is provided.");
            }
        }
    }
}
