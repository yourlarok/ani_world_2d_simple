#if UNITY_EDITOR
using AniWorld.Board.Generation;
using UnityEditor;
using UnityEngine;

namespace AniWorld.Board.Editor
{
    public static class BoardGeneratedMapAssetUtility
    {
        public static BoardGeneratedMap SaveGeneratedMap(BoardGenerationResult result, string folder, string assetName)
        {
            if (result == null)
            {
                return null;
            }

            string safeFolder = string.IsNullOrWhiteSpace(folder) ? "Assets/GeneratedMaps" : folder;
            string safeAssetName = string.IsNullOrWhiteSpace(assetName) ? "GeneratedBoardMap" : assetName;

            if (!safeFolder.StartsWith("Assets"))
            {
                safeFolder = $"Assets/{safeFolder}";
            }

            EnsureFolderExists(safeFolder);

            BoardGeneratedMap asset = ScriptableObject.CreateInstance<BoardGeneratedMap>();
            asset.SetFromResult(result);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{safeFolder}/{safeAssetName}.asset");
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Saved generated board map asset: {path}");
            return asset;
        }

        private static void EnsureFolderExists(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
