using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using TinyWalnutGames.UITKTemplates.HOGT;

namespace TinyWalnutGames.UITKTemplates.HOGT.Editor
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

                    data.objectSprite = newTex;
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
