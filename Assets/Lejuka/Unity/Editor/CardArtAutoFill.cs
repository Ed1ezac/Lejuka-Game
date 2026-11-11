using System.Collections.Generic;
using Lejuka.Unity.Config;
using UnityEditor;
using UnityEngine;

namespace Lejuka.Unity.Editor
{
    /// <summary>Fills a <see cref="CardArtConfig"/> from the sprites under <c>Assets/Graphics/Cards</c>.</summary>
    public static class CardArtAutoFill
    {
        /// <summary>The folder searched for card sprites, including its subfolders.</summary>
        public const string SpriteFolder = "Assets/Graphics/Cards";

        /// <summary>
        /// Fills the config and returns the number of faces still missing,
        /// or -1 when the sprite folder does not exist.
        /// </summary>
        public static int Fill(CardArtConfig art)
        {
            if (!AssetDatabase.IsValidFolder(SpriteFolder))
            {
                Debug.LogWarning($"[Lejuka] Card art not filled: the folder '{SpriteFolder}' does not exist.", art);
                return -1;
            }

            var missing = art.AutoFill(LoadSprites());
            EditorUtility.SetDirty(art);

            if (missing > 0 || art.Back == null)
            {
                Debug.LogWarning(
                    $"[Lejuka] Card art filled from '{SpriteFolder}', but {missing} of {CardArtConfig.FaceCount} faces " +
                    $"are missing and the back is {(art.Back == null ? "missing" : "set")}. " +
                    "Expected names: Card_{Suit}_{Rank} (Rank A, 2-10, J, Q, K) and Back_{Colour}_{n}.",
                    art);
            }
            else
            {
                Debug.Log($"[Lejuka] Card art filled: all {CardArtConfig.FaceCount} faces and back '{art.Back.name}'.", art);
            }

            return missing;
        }

        [MenuItem("CONTEXT/CardArtConfig/Auto-fill from Graphics Cards folder")]
        private static void FillFromContextMenu(MenuCommand command)
        {
            if (command.context is CardArtConfig art)
            {
                Fill(art);
                AssetDatabase.SaveAssets();
            }
        }

        private static List<Sprite> LoadSprites()
        {
            var sprites = new List<Sprite>();
            var seenPaths = new HashSet<string>();

            // A texture can hold several sprites, so every sub-asset of every match is collected.
            foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { SpriteFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!seenPaths.Add(path))
                {
                    continue;
                }

                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is Sprite sprite)
                    {
                        sprites.Add(sprite);
                    }
                }
            }

            return sprites;
        }
    }
}
