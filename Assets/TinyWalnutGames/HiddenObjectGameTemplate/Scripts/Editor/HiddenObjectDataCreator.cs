using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using TinyWalnutGames.HOGT;

namespace TinyWalnutGames.HOGT.Editor
{
    public class HiddenObjectDataCreator
    {
        [MenuItem("Assets/Create/Hidden Object Data from Sprites", false, 1000)]
        public static void CreateHiddenObjectDataFromSprites()
        {
            // Prompt for folder
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder to Save HiddenObjectData", "Assets", "");
            if (string.IsNullOrEmpty(folderPath))
                return;

            // Convert absolute path to relative path
            if (folderPath.StartsWith(Application.dataPath))
                folderPath = "Assets" + folderPath[Application.dataPath.Length..];

            foreach (Object obj in Selection.objects)
            {
                if (obj is Sprite sprite)
                {
                    // Create new asset
                    var data = ScriptableObject.CreateInstance<HiddenObjectData>();
                    data.objectName = sprite.name;
                    data.objectSprite = sprite;
                    data.objectID = System.Guid.NewGuid().ToString();
                    data.size = new Vector2(sprite.rect.width, sprite.rect.height);

                    string assetPath = Path.Combine(folderPath, $"{sprite.name}_HiddenObjectData.asset");
                    AssetDatabase.CreateAsset(data, assetPath);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Hidden Object Data", "HiddenObjectData assets created!", "OK");
        }
    }

}
