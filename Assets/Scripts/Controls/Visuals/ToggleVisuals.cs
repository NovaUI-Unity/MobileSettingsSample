using Nova;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// An <see cref="ItemVisuals"/> for a simple toggle control. Inherits from <see cref="ButtonVisuals"/>.
    /// </summary>
    [System.Serializable]
    public class ToggleVisuals : ButtonVisuals
    {
        [Tooltip("The UIBlock used to indicate the underlying \"Toggled On\" or \"Toggled Off\" state.")]
        public UIBlock IsOnIndicator;
    }
}