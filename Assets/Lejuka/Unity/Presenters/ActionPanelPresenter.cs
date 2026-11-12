using System.Collections.Generic;
using Lejuka.Contracts.Cards;
using Lejuka.Unity.Agents;
using Lejuka.Unity.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lejuka.Unity.Presenters
{
    /// <summary>
    /// The human player's controls. Shows only the control for the decision the game is waiting on,
    /// and hides the whole panel when it is not the human's turn.
    /// </summary>
    public sealed class ActionPanelPresenter : MonoBehaviour
    {
        private const string RankLabelName = "Label";

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _prompt;
        [SerializeField] private Button _shuffleButton;
        [SerializeField] private GameObject _cutGroup;
        [SerializeField] private Slider _cutSlider;
        [SerializeField] private TMP_Text _cutValueText;
        [SerializeField] private Button _cutConfirmButton;
        [SerializeField] private RectTransform _rankGroup;
        [SerializeField] private Button _rankButtonPrefab;

        private readonly List<Button> _rankButtons = new List<Button>();
        private IHumanInputSource _input;
        private bool _isWired;
        private bool _isDestroyed;

        /// <summary>Connects the panel to the human player. Safe to call before or after Awake.</summary>
        public void Bind(IHumanInputSource input)
        {
            if (_input != null)
            {
                _input.PendingChanged -= Show;
            }

            _input = input;
            if (_input != null)
            {
                _input.PendingChanged += Show;
            }

            Show(_input?.Pending);
        }

        private void Awake()
        {
            // Not short-circuited: every missing reference should be reported, not just the first.
            _isWired = WiringGuard.IsAssigned(this, _panel, nameof(_panel))
                       & WiringGuard.IsAssigned(this, _prompt, nameof(_prompt))
                       & WiringGuard.IsAssigned(this, _shuffleButton, nameof(_shuffleButton))
                       & WiringGuard.IsAssigned(this, _cutGroup, nameof(_cutGroup))
                       & WiringGuard.IsAssigned(this, _cutSlider, nameof(_cutSlider))
                       & WiringGuard.IsAssigned(this, _cutValueText, nameof(_cutValueText))
                       & WiringGuard.IsAssigned(this, _cutConfirmButton, nameof(_cutConfirmButton))
                       & WiringGuard.IsAssigned(this, _rankGroup, nameof(_rankGroup))
                       & WiringGuard.IsAssigned(this, _rankButtonPrefab, nameof(_rankButtonPrefab));

            if (!_isWired)
            {
                return;
            }

            _cutSlider.wholeNumbers = true;
            _cutSlider.onValueChanged.AddListener(OnCutValueChanged);
            _shuffleButton.onClick.AddListener(OnShuffleClicked);
            _cutConfirmButton.onClick.AddListener(OnCutConfirmed);
            SpawnRankButtons();

            Show(_input?.Pending);
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
            if (_input != null)
            {
                _input.PendingChanged -= Show;
            }
        }

        private void SpawnRankButtons()
        {
            for (var rank = Rank.Ace; rank <= Rank.King; rank++)
            {
                var button = Instantiate(_rankButtonPrefab, _rankGroup);
                button.name = "RankButton_" + RankLabel.Short(rank);

                var labelTransform = button.transform.Find(RankLabelName);
                var label = labelTransform != null
                    ? labelTransform.GetComponent<TMP_Text>()
                    : button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = RankLabel.Short(rank);
                }

                // Copied so each button keeps its own rank rather than the loop's last value.
                var chosen = rank;
                button.onClick.AddListener(() => OnRankClicked(chosen));
                _rankButtons.Add(button);
            }
        }

        private void Show(HumanInputRequest request)
        {
            // The agent can still report a cancelled request while the scene is being torn down.
            if (!_isWired || _isDestroyed || _panel == null)
            {
                return;
            }

            _panel.SetActive(request != null);
            if (request == null)
            {
                return;
            }

            _shuffleButton.gameObject.SetActive(request.Kind == HumanInputKind.Shuffle);
            _cutGroup.SetActive(request.Kind == HumanInputKind.Cut);
            _rankGroup.gameObject.SetActive(request.Kind == HumanInputKind.Rank);

            switch (request.Kind)
            {
                case HumanInputKind.Shuffle:
                    _prompt.text = "You are the Dealer. Shuffle the deck.";
                    break;
                case HumanInputKind.Cut:
                    _prompt.text = "You are the Cutter. Choose where to cut the deck.";
                    PrepareCutSlider(request);
                    break;
                case HumanInputKind.Rank:
                    _prompt.text = "Call the target rank.";
                    break;
            }
        }

        private void PrepareCutSlider(HumanInputRequest request)
        {
            _cutSlider.minValue = request.MinCut;
            _cutSlider.maxValue = request.MaxCut;

            var middle = (request.MinCut + request.MaxCut) / 2;
            _cutSlider.SetValueWithoutNotify(middle);
            OnCutValueChanged(middle);
        }

        private void OnCutValueChanged(float value)
        {
            _cutValueText.text = Mathf.RoundToInt(value).ToString();
        }

        private void OnShuffleClicked()
        {
            _input?.SubmitShuffle();
        }

        private void OnCutConfirmed()
        {
            _input?.SubmitCut(Mathf.RoundToInt(_cutSlider.value));
        }

        private void OnRankClicked(Rank rank)
        {
            _input?.SubmitRank(rank);
        }
    }
}
