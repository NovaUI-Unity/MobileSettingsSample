using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A data structure used to store the state of a dropdown UI control.
    /// </summary>
    [Serializable]
    public class DropdownSetting : ISetting
    {
        public const string NothingSelected = "None";

        [SerializeField]
        [Tooltip("The label to indicate the end-user setting the dropdown will configure.")]
        private string label;

        public string Label { get => label; set => label = value; }

        /// <summary>
        /// The name of the dropdown's selected option, i.e. the value
        /// at the <see cref="SelectedIndex"/> into <see cref="Options"/>.
        /// </summary>
        public string SelectionName
        {
            get
            {
                if (Options == null || SelectedIndex < 0 || SelectedIndex >= Options.Count)
                {
                    // If the dropdown doesn't have any options or the
                    // SelectedIndex is out of range, indicate nothing selected.
                    return NothingSelected;
                }

                // Get the 
                return Options[SelectedIndex];
            }
        }

        /// <summary>
        /// The index into the list of <see cref="Options"/> of the selected value.
        /// </summary>
        public int SelectedIndex;

        /// <summary>
        /// The list of selectable options the end-user can choose from for this given dropdown.
        /// </summary>
        public List<string> Options = new List<string>();
    }
}

