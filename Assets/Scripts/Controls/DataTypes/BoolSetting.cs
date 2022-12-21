using System;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A data structure used to store the state of a toggle UI control.
    /// </summary>
    [Serializable]
    public class BoolSetting : Setting
    {
        /// <summary>
        /// The toggle state of this togglable setting.
        /// </summary>
        public bool IsOn;
    }
}

