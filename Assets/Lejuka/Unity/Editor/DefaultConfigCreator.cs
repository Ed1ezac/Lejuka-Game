using Lejuka.Unity.Config;
using UnityEditor;
using UnityEngine;

namespace Lejuka.Unity.Editor
{
    /// <summary>Creates the three config assets the mockup needs, with default values.</summary>
    public static class DefaultConfigCreator
    {
        /// <summary>The folder the config assets live in.</summary>
        public const string ConfigFolder = "Assets/Lejuka/Config";

        /// <summary>Path of the game config asset.</summary>
        public const string GameConfigPath = ConfigFolder + "/LejukaGameConfig.asset";

        /// <summary>Path of the card art asset.</summary>
        public const string CardArtPath = ConfigFolder + "/CardArtConfig.asset";

        /// <summary>Path of the table catalog asset.</summary>
        public const string TableCatalogPath = ConfigFolder + "/TableCatalogConfig.asset";

        /// <summary>
        /// Creates any config asset that does not exist yet and refills the card art.
        /// Existing assets keep their values, so this is safe to run again.
        /// </summary>
        [MenuItem("Lejuka/Create Default Configs", priority = 0)]
        public static void CreateDefaultConfigs()
        {
            EnsureFolder(ConfigFolder);

            LoadOrCreate<LejukaGameConfig>(GameConfigPath);
            LoadOrCreate<TableCatalogConfig>(TableCatalogPath);
            CardArtAutoFill.Fill(LoadOrCreate<CardArtConfig>(CardArtPath));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static T LoadOrCreate<T>(string path)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                Debug.Log($"[Lejuka] Kept existing {typeof(T).Name} at '{path}'.", asset);
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[Lejuka] Created {typeof(T).Name} at '{path}'.", asset);
            return asset;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var separator = folder.LastIndexOf('/');
            var parent = folder.Substring(0, separator);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder.Substring(separator + 1));
        }
    }
}
