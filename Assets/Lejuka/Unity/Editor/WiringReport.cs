using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lejuka.Unity.Editor
{
    /// <summary>Collects what a wiring run did and what it could not do, then logs it as one checklist.</summary>
    public sealed class WiringReport
    {
        private readonly List<string> _problems = new List<string>();
        private readonly List<string> _unassigned = new List<string>();
        private readonly List<string> _done = new List<string>();
        private readonly string _title;
        private int _assignedCount;

        /// <summary>Creates a report with the heading it is logged under.</summary>
        public WiringReport(string title)
        {
            _title = title;
        }

        /// <summary>True when nothing was missing.</summary>
        public bool IsClean => _problems.Count == 0 && _unassigned.Count == 0;

        /// <summary>Records something that is missing or wrong and needs the user's attention.</summary>
        public void Problem(string message)
        {
            _problems.Add(message);
        }

        /// <summary>Records a serialized field that was left alone because its value could not be found.</summary>
        public void Unassigned(string field)
        {
            _unassigned.Add(field);
        }

        /// <summary>Records a change that was made.</summary>
        public void Done(string message)
        {
            _done.Add(message);
        }

        /// <summary>Counts one serialized reference that was assigned.</summary>
        public void Assigned()
        {
            _assignedCount++;
        }

        /// <summary>Writes the checklist to the Console: a warning when there are problems, otherwise a plain log.</summary>
        public void Log()
        {
            var text = new StringBuilder();
            text.Append("[Lejuka] ").Append(_title).Append(": ");
            text.Append(IsClean ? "everything found." : _problems.Count + " item(s) need attention.");

            if (!IsClean)
            {
                text.Append("\nMISSING OR WRONG");
                foreach (var problem in _problems)
                {
                    text.Append("\n  [ ] ").Append(problem);
                }
            }

            if (_unassigned.Count > 0)
            {
                text.Append("\nFIELDS LEFT UNASSIGNED (because of the items above)");
                foreach (var field in _unassigned)
                {
                    text.Append("\n  [ ] ").Append(field);
                }
            }

            text.Append("\nDONE");
            foreach (var done in _done)
            {
                text.Append("\n  [x] ").Append(done);
            }

            text.Append("\n  [x] Assigned ").Append(_assignedCount).Append(" serialized reference(s).");
            text.Append(IsClean
                ? "\nNext: save the scene (Ctrl+S) and press Play."
                : "\nNext: fix the items above, run this menu item again, then save the scene.");

            if (IsClean)
            {
                Debug.Log(text.ToString());
            }
            else
            {
                Debug.LogWarning(text.ToString());
            }
        }
    }
}
