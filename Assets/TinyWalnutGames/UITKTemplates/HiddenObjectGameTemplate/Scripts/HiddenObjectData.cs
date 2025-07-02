using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TinyWalnutGames.UITKTemplates.MainMenu;

namespace TinyWalnutGames.UITKTemplates.HOGT
{
    [ExecuteAlways]
    // ScriptableObject to hold data for hidden objects
    [CreateAssetMenu(fileName = "HiddenObjectData", menuName = "Scriptable Objects/HiddenObjectData")]
    [Serializable]
    public class HiddenObjectData : ScriptableObject
    {
        // Name of the object (for identification)
        public string objectName;
        // sprite or texture for the object
        public Texture2D objectSprite;
        // unique identifier for the object
        public string objectID;
        // position in the UI Document
        public Vector2 position;
        // size of the clickable area (optional)
        public Vector2 size;
        // whether the object is found
        // public bool isFound = false; // This should be handled by a controller; todo
        // additional metadata (e.g., hints, description)
        [TextArea]
        public string description;
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

        // Add a reference for the found-toast sprite
        public Texture2D foundToastSprite;

        // Optionally, cache the playarea VisualElement
        [HideInInspector] public VisualElement playAreaElement;

        // If objectPrefab or templateElement are addressables, ensure they are labeled "Preload".
        // You can retrieve them via PreloadAssets.Instance.Get<GameObject>("objectPrefabKey") or PreloadAssets.Instance.Get<VisualTreeAsset>("templateElementKey").
        // I am using the UI Toolkit to create the UI for the hidden object game, so I will be using a VisualTreeAsset to create the UI elements for each hidden object.
        // This allows for easy instantiation and management of hidden objects in the UI.
        public void Initialize()
        {
            // There are no prefabs because of UI Toolkit. Load the hidden object templateElement are initialized properly
            if (templateElement == null)
            {
                templateElement = PreloadAssets.Instance.Get<VisualTreeAsset>("HiddenObjectTemplateKey");
                if (templateElement == null)
                {
                    Debug.LogError("HiddenObjectTemplateKey not found in preloaded assets.");
                }
            }
            // There are no pre-existing prefabs for the UI Toolkit to use because it uses VisualTreeAssets instead.
            // use visual elements for the game objects, so no need to initialize a prefab here.
            if (objectSprite == null)
            {
                objectSprite = PreloadAssets.Instance.Get<Texture2D>("HiddenObjectSpriteKey");
                if (objectSprite == null)
                {
                    Debug.LogError("HiddenObjectSpriteKey not found in preloaded assets.");
                }
            }
            // Ensure the objectID is unique
            if (string.IsNullOrEmpty(objectID))
            {
                objectID = Guid.NewGuid().ToString(); // Generate a unique ID if not set
            }
            // Ensure the objectName is set
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
                if (image != null && objectSprite != null)
                {
                    // Try to set the image property if the texture is a Texture2D
                    image.image = objectSprite as Texture2D;
                    // If not a Texture2D, fallback to setting the backgroundImage
                    if (image.image == null)
                    {
                        image.style.backgroundImage = new(objectSprite);
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
