using Nova;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// An <see cref="ItemVisuals"/> class to visually represent a button in a tab-bar. Inherits from <see cref="ButtonVisuals"/>.
    /// </summary>
    [System.Serializable]
    public class TabButtonVisuals : ButtonVisuals
    {   
        [Tooltip("The UIBlock to enable while this tab is the \"selected\" tab in the tab-bar.")]
        public UIBlock2D IsSelectedIndicator;
    }
}
