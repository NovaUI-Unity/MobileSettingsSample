using System;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A data structure used to store the state of a slider UI control.
    /// </summary>
    [Serializable]
    public class FloatSetting : Setting
    {
        [SerializeField]
        [Tooltip("The numeric value associated with the slider.")]
        private float value;

        /// <summary>
        /// The numeric value associated with the slider.
        /// </summary>
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                // Clamp within range before assigning
                this.value = Mathf.Clamp(value, Min, Max);
            }
        }

        [Tooltip("The mininum value for the given slider.")]
        public float Min = 0;
        [Tooltip("The maximum value for the given slider.")]
        public float Max = 100;

        [SerializeField]
        [Tooltip("The string format to use when displaying the slider value to the end user.")]
        private string unitsFormat = "{0:0.00}";

        /// <summary>
        /// The formatted text to display based on the slider's <see cref="Value"/>.
        /// </summary>
        public string ValueLabel => string.Format(unitsFormat, Value);
    }
}