using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


namespace TinyWalnutGames.HOGT
{
    // ScriptableObject to hold data for hidden objects
    [CreateAssetMenu(fileName = "HiddenObjectData", menuName = "Scriptable Objects/HiddenObjectData")]
    [Serializable]
    public class HiddenObjectData : ScriptableObject
    {
        // Name of the object (for identification)
        public string objectName;
        // sprite or texture for the object
        public Sprite objectSprite;
        // unique identifier for the object
        public string objectID;
        // position in the scene (optional, can be set dynamically)
        [HideInInspector] public Vector2 position;
        // size of the clickable area (optional)
        [HideInInspector] public Vector2 size;
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
        // Reference to a prefab asset (not a scene object). Safe to assign prefab from Assets folder.
        public GameObject objectPrefab;
        // the visual tree asset that represents the template for all hidden objects
        public VisualTreeAsset templateElement;
    }
}
