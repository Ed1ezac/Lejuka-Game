using Lejuka.Bootstrap;
using Lejuka.Unity.Camera;
using Lejuka.Unity.Config;
using Lejuka.Unity.Presenters;
using Lejuka.Unity.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lejuka.Unity.Editor
{
    /// <summary>
    /// Wires the hand-built Mockup scene: finds its objects by name, adds the Lejuka components
    /// and assigns every serialized reference. Anything it cannot find goes into a checklist in the Console.
    /// Running it again only repeats the same assignments.
    /// </summary>
    public static class MockupSceneWirer
    {
        private const string ExpectedSceneName = "Mockup";
        private const string CardPrefabPath = "Assets/Lejuka/Prefabs/Card.prefab";
        private const string RankButtonPrefabPath = "Assets/Lejuka/Prefabs/RankButton.prefab";
        private const string RankButtonLabelName = "Label";
        private const string InputSystemModuleTypeName = "InputSystemUIInputModule";

        private static readonly string[] RequiredSortingLayers = { "Background", "Table", "Cards" };

        /// <summary>Wires the scene that is open and active in the Editor.</summary>
        [MenuItem("Lejuka/Wire Mockup Scene", priority = 1)]
        public static void WireMockupScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Lejuka] Leave Play mode before wiring the scene.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            var report = new WiringReport($"Wire Mockup Scene ('{scene.name}')");
            if (scene.name != ExpectedSceneName)
            {
                report.Problem($"The active scene is '{scene.name}', not '{ExpectedSceneName}'. Open the Mockup scene first.");
            }

            var finder = new SceneObjectFinder(scene, report);

            CheckSortingLayers(report);
            var art = LoadConfig<CardArtConfig>(DefaultConfigCreator.CardArtPath, report);
            CheckCardArt(art, report);
            var cardPrefab = PrepareCardPrefab(report);

            var tableView = WireBoard(finder, report, cardPrefab, art);
            WireCamera(finder, report);
            CheckEventSystem(finder, report);

            var hud = finder.Find("HUD");
            var hudPresenter = WireHud(finder, report, hud);
            var actionPanel = WireActionPanel(finder, report, hud);
            var ceremony = WireCeremony(finder, report, hud);

            WireComposition(finder, report, tableView, hudPresenter, actionPanel, ceremony);

            EditorSceneManager.MarkSceneDirty(scene);
            report.Log();
        }

        private static TableView WireBoard(
            SceneObjectFinder finder,
            WiringReport report,
            CardView cardPrefab,
            CardArtConfig art)
        {
            var board = finder.Find("Board");
            var deckSpot = finder.FindUnder(board, "Board", "DeckSpot");
            var seatAPile = finder.FindUnder(board, "Board", "SeatA_Pile");
            var seatBPile = finder.FindUnder(board, "Board", "SeatB_Pile");

            var pool = Ensure<CardPool>(board, report);
            Assign(pool, "_cardPrefab", cardPrefab, report);
            Assign(pool, "_art", art, report);

            var deck = Ensure<DeckView>(deckSpot, report);
            Assign(deck, "_cardPrefab", cardPrefab, report);
            Assign(deck, "_art", art, report);

            var tableView = Ensure<TableView>(board, report);
            Assign(tableView, "_deck", deck, report);
            Assign(tableView, "_seatAPile", Ensure<SeatPileView>(seatAPile, report), report);
            Assign(tableView, "_seatBPile", Ensure<SeatPileView>(seatBPile, report), report);
            Assign(tableView, "_pool", pool, report);
            return tableView;
        }

        private static void WireCamera(SceneObjectFinder finder, WiringReport report)
        {
            var cameraObject = finder.Find("Main Camera");
            if (cameraObject != null && finder.Require<UnityEngine.Camera>(cameraObject) == null)
            {
                return;
            }

            var table = finder.Find("Table");
            var background = finder.FindUnder(table, "Table", "Background_Room");
            finder.FindUnder(table, "Table", "Table_Felt");

            var cameraFit = Ensure<CameraFit>(cameraObject, report);
            Assign(cameraFit, "_background", finder.Require<SpriteRenderer>(background), report);
        }

        private static HudPresenter WireHud(SceneObjectFinder finder, WiringReport report, GameObject hud)
        {
            var presenter = Ensure<HudPresenter>(hud, report);

            WireSeatPanel(finder, report, presenter, "_seatAPanel", finder.FindUnder(hud, "HUD", "SeatA_Panel"), "SeatA_Panel");
            WireSeatPanel(finder, report, presenter, "_seatBPanel", finder.FindUnder(hud, "HUD", "SeatB_Panel"), "SeatB_Panel");

            Assign(presenter, "_scoreText", FindText(finder, hud, "HUD", "ScoreText"), report);
            Assign(presenter, "_targetText", FindText(finder, hud, "HUD", "TargetText"), report);
            Assign(presenter, "_stageBanner", FindText(finder, hud, "HUD", "StageBanner"), report);
            return presenter;
        }

        private static void WireSeatPanel(
            SceneObjectFinder finder,
            WiringReport report,
            HudPresenter presenter,
            string panelField,
            GameObject panel,
            string panelName)
        {
            Assign(presenter, panelField + "._nameText", FindText(finder, panel, panelName, "NameText"), report);
            Assign(presenter, panelField + "._creditsText", FindText(finder, panel, panelName, "CreditsText"), report);
            Assign(presenter, panelField + "._roleText", FindText(finder, panel, panelName, "RoleText"), report);
            Assign(presenter, panelField + "._winsText", FindText(finder, panel, panelName, "WinsText"), report);
        }

        private static ActionPanelPresenter WireActionPanel(SceneObjectFinder finder, WiringReport report, GameObject hud)
        {
            const string PanelName = "ActionPanel";
            const string CutGroupName = "CutGroup";

            var panel = finder.FindUnder(hud, "HUD", PanelName);
            var cutGroup = finder.FindUnder(panel, PanelName, CutGroupName);
            var rankGroup = finder.FindUnder(panel, PanelName, "RankGroup");
            if (rankGroup != null && rankGroup.GetComponent<GridLayoutGroup>() == null)
            {
                report.Problem("'RankGroup' has no Grid Layout Group, so the 13 rank buttons will sit on top of each other.");
            }

            // The presenter sits on the HUD, not on the panel: it has to keep running while the panel is hidden.
            var presenter = Ensure<ActionPanelPresenter>(hud, report);
            Assign(presenter, "_panel", panel, report);
            Assign(presenter, "_prompt", FindText(finder, panel, PanelName, "ActionPrompt"), report);
            Assign(presenter, "_shuffleButton", finder.Require<Button>(finder.FindUnder(panel, PanelName, "ShuffleButton")), report);
            Assign(presenter, "_cutGroup", cutGroup, report);
            Assign(presenter, "_cutSlider", finder.Require<Slider>(finder.FindUnder(cutGroup, CutGroupName, "CutSlider")), report);
            Assign(presenter, "_cutValueText", FindText(finder, cutGroup, CutGroupName, "CutValueText"), report);
            Assign(presenter, "_cutConfirmButton", finder.Require<Button>(finder.FindUnder(cutGroup, CutGroupName, "CutConfirmButton")), report);
            Assign(presenter, "_rankGroup", finder.Require<RectTransform>(rankGroup), report);
            Assign(presenter, "_rankButtonPrefab", LoadRankButtonPrefab(report), report);
            return presenter;
        }

        private static CeremonyPresenter WireCeremony(SceneObjectFinder finder, WiringReport report, GameObject hud)
        {
            const string PanelName = "CeremonyPanel";

            var panel = finder.FindUnder(hud, "HUD", PanelName);

            // On the HUD for the same reason as the action panel: the ceremony panel starts inactive.
            var presenter = Ensure<CeremonyPresenter>(hud, report);
            Assign(presenter, "_panel", panel, report);
            Assign(presenter, "_resultText", FindText(finder, panel, PanelName, "ResultText"), report);
            Assign(presenter, "_playAgainButton", finder.Require<Button>(finder.FindUnder(panel, PanelName, "PlayAgainButton")), report);
            return presenter;
        }

        private static void WireComposition(
            SceneObjectFinder finder,
            WiringReport report,
            TableView tableView,
            HudPresenter hud,
            ActionPanelPresenter actionPanel,
            CeremonyPresenter ceremony)
        {
            var scope = Ensure<GameLifetimeScope>(finder.Find("Composition"), report);
            Assign(scope, "_gameConfig", LoadConfig<LejukaGameConfig>(DefaultConfigCreator.GameConfigPath, report), report);
            Assign(scope, "_tableCatalog", LoadConfig<TableCatalogConfig>(DefaultConfigCreator.TableCatalogPath, report), report);
            Assign(scope, "_tableView", tableView, report);
            Assign(scope, "_hud", hud, report);
            Assign(scope, "_actionPanel", actionPanel, report);
            Assign(scope, "_ceremony", ceremony, report);
        }

        private static void CheckEventSystem(SceneObjectFinder finder, WiringReport report)
        {
            var eventSystem = finder.Find("EventSystem");
            if (eventSystem == null)
            {
                return;
            }

            finder.Require<EventSystem>(eventSystem);

            // Looked up by name so this assembly does not need a reference to the Input System package.
            if (eventSystem.GetComponent(InputSystemModuleTypeName) == null)
            {
                report.Problem("'EventSystem' has no Input System UI Input Module, so the buttons will not react.");
            }

            if (eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                report.Problem("'EventSystem' still has a legacy Standalone Input Module. Remove it: the project uses the new Input System only.");
            }
        }

        private static void CheckSortingLayers(WiringReport report)
        {
            foreach (var required in RequiredSortingLayers)
            {
                var exists = false;
                foreach (var layer in SortingLayer.layers)
                {
                    exists |= layer.name == required;
                }

                if (!exists)
                {
                    report.Problem($"Sorting layer '{required}' does not exist (Project Settings > Tags and Layers).");
                }
            }
        }

        private static void CheckCardArt(CardArtConfig art, WiringReport report)
        {
            if (art == null)
            {
                return;
            }

            if (art.Back == null)
            {
                report.Problem($"CardArtConfig has no back sprite. Check '{CardArtAutoFill.SpriteFolder}' and run Lejuka > Create Default Configs.");
            }

            var missing = art.MissingFaceCount;
            if (missing > 0)
            {
                report.Problem(
                    $"CardArtConfig is missing {missing} of {CardArtConfig.FaceCount} face sprites. " +
                    $"Check '{CardArtAutoFill.SpriteFolder}' and run Lejuka > Create Default Configs.");
            }
        }

        private static CardView PrepareCardPrefab(WiringReport report)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            if (prefab == null)
            {
                report.Problem($"Prefab '{CardPrefabPath}' not found.");
                return null;
            }

            if (prefab.GetComponent<SpriteRenderer>() == null)
            {
                report.Problem("'Card.prefab' has no SpriteRenderer on its root.");
                return null;
            }

            if (prefab.GetComponent<CardView>() == null)
            {
                var contents = PrefabUtility.LoadPrefabContents(CardPrefabPath);
                try
                {
                    contents.AddComponent<CardView>();
                    PrefabUtility.SaveAsPrefabAsset(contents, CardPrefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }

                report.Done("Added CardView to 'Card.prefab'.");
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            }

            return prefab.GetComponent<CardView>();
        }

        private static Button LoadRankButtonPrefab(WiringReport report)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RankButtonPrefabPath);
            if (prefab == null)
            {
                report.Problem($"Prefab '{RankButtonPrefabPath}' not found.");
                return null;
            }

            var button = prefab.GetComponent<Button>();
            if (button == null)
            {
                report.Problem("'RankButton.prefab' has no Button on its root.");
                return null;
            }

            var label = prefab.transform.Find(RankButtonLabelName);
            if (label == null || label.GetComponent<TMP_Text>() == null)
            {
                report.Problem($"'RankButton.prefab' has no TMP child named '{RankButtonLabelName}', so the buttons will have no text.");
            }

            return button;
        }

        private static T LoadConfig<T>(string path, WiringReport report)
            where T : ScriptableObject
        {
            var config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (config == null)
            {
                report.Problem($"Config asset '{path}' not found. Run Lejuka > Create Default Configs first.");
            }

            return config;
        }

        private static TMP_Text FindText(SceneObjectFinder finder, GameObject parent, string parentName, string objectName)
        {
            return finder.Require<TMP_Text>(finder.FindUnder(parent, parentName, objectName));
        }

        private static T Ensure<T>(GameObject target, WiringReport report)
            where T : Component
        {
            if (target == null)
            {
                return null;
            }

            if (target.TryGetComponent<T>(out var existing))
            {
                return existing;
            }

            report.Done($"Added {typeof(T).Name} to '{target.name}'.");
            return Undo.AddComponent<T>(target);
        }

        private static void Assign(Object owner, string propertyPath, Object value, WiringReport report)
        {
            // A missing owner has been reported already, and there is nothing to assign to.
            if (owner == null)
            {
                return;
            }

            var ownerName = owner.GetType().Name;
            var serializedOwner = new SerializedObject(owner);
            var property = serializedOwner.FindProperty(propertyPath);
            if (property == null)
            {
                report.Problem($"{ownerName} has no serialized field '{propertyPath}'. The wiring tool and the script are out of step.");
                return;
            }

            if (value == null)
            {
                report.Unassigned($"{ownerName}.{propertyPath}");
                return;
            }

            property.objectReferenceValue = value;
            serializedOwner.ApplyModifiedProperties();
            report.Assigned();
        }
    }
}
