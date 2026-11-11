using System;
using Lejuka.Unity.Presentation;
using TMPro;
using UnityEngine;

namespace Lejuka.Unity.Presenters
{
    /// <summary>The four texts of one seat panel on the HUD.</summary>
    [Serializable]
    public sealed class SeatPanel
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _creditsText;
        [SerializeField] private TMP_Text _roleText;
        [SerializeField] private TMP_Text _winsText;

        /// <summary>Reports any text that is not assigned. Returns true when all four are.</summary>
        public bool IsWired(UnityEngine.Object owner, string panelName)
        {
            // Not short-circuited: every missing reference should be reported, not just the first.
            return WiringGuard.IsAssigned(owner, _nameText, panelName + "." + nameof(_nameText))
                   & WiringGuard.IsAssigned(owner, _creditsText, panelName + "." + nameof(_creditsText))
                   & WiringGuard.IsAssigned(owner, _roleText, panelName + "." + nameof(_roleText))
                   & WiringGuard.IsAssigned(owner, _winsText, panelName + "." + nameof(_winsText));
        }

        /// <summary>Shows a seat's name, credits as "available (reserved)", role and games won.</summary>
        public void Render(SeatViewState seat)
        {
            _nameText.text = seat.Name;
            _creditsText.text = $"Credits: {seat.Available} ({seat.Reserved})";
            _roleText.text = seat.Role == SeatRole.None ? string.Empty : seat.Role.ToString();
            _winsText.text = $"Games won: {seat.GamesWon}";
        }
    }
}
