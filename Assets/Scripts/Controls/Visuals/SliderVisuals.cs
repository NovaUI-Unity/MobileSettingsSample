using Nova;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// The <see cref="ItemVisuals"/> type used to display a draggable <see cref="FloatSetting"/> control.
    /// </summary>
    [System.Serializable]
    public class SliderVisuals : ItemVisuals
    {
        [Tooltip("The TextBlock used to display the label associated with the slider.")]
        public TextBlock Label;
        [Tooltip("The draggable bounds of the slider control.")]
        public UIBlock2D SliderBounds;
        [Tooltip("The UIBlock2D within the SliderBounds to indicate the selected slider value.")]
        public UIBlock2D FillBar;
        [Tooltip("The Textblock used to display the numerical value and units of the slider's selected value.")]
        public TextBlock Units;
    }
}
