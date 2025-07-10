using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using TinyWalnutGames.UITKTemplates.HOGT;

namespace TinyWalnutGames.UITKTemplates.HOGT.Editor
{
    public static class HiddenObjectDataCreator
    {
        [MenuItem("Assets/Create/HOGT/Scriptable Objects/Hidden Object Data from Sprites", false, 1000)]
        public static void CreateHiddenObjectDataFromSprites()
        {
            // Prompt for folder
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder to Save HiddenObjectData", "Assets", "");
            if (string.IsNullOrEmpty(folderPath))
                return;

            // Convert absolute path to relative path
            if (folderPath.StartsWith(Application.dataPath))
                folderPath = "Assets" + folderPath[Application.dataPath.Length..];

            // Prompt user to select a LevelData asset to add the new HiddenObjectData to
            string levelDataPath = EditorUtility.OpenFilePanel("Select LevelData asset to add objects to", "Assets", "asset");
            if (string.IsNullOrEmpty(levelDataPath))
                return;
            if (levelDataPath.StartsWith(Application.dataPath))
                levelDataPath = "Assets" + levelDataPath[Application.dataPath.Length..];
            LevelData selectedLevelData = AssetDatabase.LoadAssetAtPath<LevelData>(levelDataPath);
            if (selectedLevelData == null)
            {
                EditorUtility.DisplayDialog("Error", "No valid LevelData asset selected.", "OK");
                return;
            }

            int addedCount = 0;
            foreach (Object obj in Selection.objects)
            {
                if (obj is Sprite sprite)
                {
                    // Create new asset
                    var data = ScriptableObject.CreateInstance<HiddenObjectData>();
                    data.objectName = sprite.name;

                    // Extract the sprite's region into a new Texture2D asset
                    Texture2D sourceTex = sprite.texture;
                    Rect rect = sprite.rect;
                    Texture2D newTex = new((int)rect.width, (int)rect.height, sourceTex.format, false);
                    Color[] pixels = sourceTex.GetPixels(
                        (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                    // Convert Color[] to Color32[] for SetPixels32
                    Color32[] pixels32 = new Color32[pixels.Length];
                    for (int i = 0; i < pixels.Length; i++)
                        pixels32[i] = pixels[i];
                    newTex.SetPixels32(pixels32);
                    newTex.Apply();

                    // Save the new texture as an asset
                    string texAssetPath = Path.Combine(folderPath, $"{sprite.name}_HiddenObjectTexture.asset");
                    AssetDatabase.CreateAsset(newTex, texAssetPath);

                    // Instead of assigning to objectSprite (which no longer exists), add to objectSprites list
                    data.objectSprites = new List<Texture2D> { newTex };

                    data.objectID = System.Guid.NewGuid().ToString();
                    data.size = new Vector2(sprite.rect.width, sprite.rect.height);

                    // Optionally, set default flags (customize as needed)
                    data.isDraggable = false;
                    data.isSecret = false;
                    // You can add more logic here to set these flags per object if desired

                    string assetPath = Path.Combine(folderPath, $"{sprite.name}_HiddenObjectData.asset");
                    AssetDatabase.CreateAsset(data, assetPath);

                    // Ensure uniqueness in the LevelData's objectsToFind list
                    if (!selectedLevelData.objectsToFind.Contains(data))
                    {
                        selectedLevelData.objectsToFind.Add(data);
                        EditorUtility.SetDirty(selectedLevelData);
                        addedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"HiddenObjectData {data.objectName} already exists in LevelData {selectedLevelData.levelName}. Skipping addition.");
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Hidden Object Data",
                $"HiddenObjectData assets created and added to LevelData '{selectedLevelData.levelName}'.\nAdded: {addedCount}",
                "OK"
            );
        }

        // Example: Batch-create and assign multiple sprites/backgrounds based on naming convention
        public static void AssignSpritesAndBackgrounds(HiddenObjectData data, string baseName, string spritesFolder, string toastsFolder)
        {
            // Assign objectSprites
            data.objectSprites = new List<Texture2D>();
            string[] spriteGuids = AssetDatabase.FindAssets($"{baseName}_HiddenObjectTexture_", new[] { spritesFolder });
            foreach (string guid in spriteGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                    data.objectSprites.Add(tex);
            }

            // Assign toastBackgrounds
            data.toastBackgrounds = new List<Texture2D>();
            string[] toastGuids = AssetDatabase.FindAssets($"{baseName}_ToastBackground_", new[] { toastsFolder });
            foreach (string guid in toastGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                    data.toastBackgrounds.Add(tex);
            }

            // Optionally, assign named variants if you use ObjectSpriteVariant/ToastBackgroundVariant
            data.objectSpriteVariants = new List<HiddenObjectData.ObjectSpriteVariant>();
            foreach (string guid in spriteGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    // Extract variant key from filename, e.g. "item_HiddenObjectTexture_found"
                    string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                    string[] parts = filename.Split('_');
                    string key = parts.Length > 2 ? parts[2] : "default";
                    data.objectSpriteVariants.Add(new HiddenObjectData.ObjectSpriteVariant { key = key, sprite = tex });
                }
            }

            data.toastBackgroundVariants = new List<HiddenObjectData.ToastBackgroundVariant>();
            foreach (string guid in toastGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                    string[] parts = filename.Split('_');
                    string key = parts.Length > 2 ? parts[2] : "default";
                    data.toastBackgroundVariants.Add(new HiddenObjectData.ToastBackgroundVariant { key = key, background = tex });
                }
            }
        }
    }
}
