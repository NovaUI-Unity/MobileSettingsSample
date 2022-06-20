using System;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A data structure used to store the state of a toggle UI control.
    /// </summary>
    [Serializable]
    public class ToggleSetting : ISetting
    {
        [SerializeField]
        [Tooltip("The label to indicate the end-user setting the slider will configure.")]
        private string label;
        public string Label { get => label; set => label = value; }

        /// <summary>
        /// The toggle state of this togglable setting.
        /// </summary>
        public bool IsOn;
    }
}

