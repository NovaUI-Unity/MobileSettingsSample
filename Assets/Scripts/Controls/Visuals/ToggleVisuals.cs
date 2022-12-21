using Nova;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// An <see cref="ItemVisuals"/> for a simple toggle control.
    /// </summary>
    [System.Serializable]
    public class ToggleVisuals : ItemVisuals
    {
        [Tooltip("The button's background UIBlock.")]
        public UIBlock2D Background = null;
        [Tooltip("The TextBlock to display the button's label.")]
        public TextBlock Label = null;
        [Tooltip("The UIBlock used to indicate the underlying \"Toggled On\" or \"Toggled Off\" state.")]
        public UIBlock IsOnIndicator;

        [Header("Styles")]
        [Tooltip("The color to apply to the Background when this button is pressed.")]
        public Color PressedColor;
        [Tooltip("The default Background color.")]
        public Color DefaultColor;

        /// <summary>
        /// A utility method to indicate a pressed visual state of <see cref="ToggleVisuals"/> object.
        /// </summary>
        /// <param name="evt">The press event.</param>
        /// <param name="button">The <see cref="ItemVisuals"/> receiving the press event.</param>
        /// <param name="index">Unused here, but this is the index into the data source of list view invoking this event.</param>
        public static void HandlePressed(Gesture.OnPress evt, ToggleVisuals button, int index)
        {
            button.Background.Color = button.PressedColor;
        }

        /// <summary>
        /// A utility method to restore the visual state of <see cref="ToggleVisuals"/> object when its active gesture is released.
        /// </summary>
        /// <param name="evt">The release event.</param>
        /// <param name="button">The <see cref="ItemVisuals"/> receiving the release event.</param>
        /// <param name="index">Unused here, but this is the index into the data source of list view invoking this event.</param>
        public static void HandleReleased(Gesture.OnRelease evt, ToggleVisuals button, int index)
        {
            button.Background.Color = button.DefaultColor;
        }

        /// <summary>
        /// A utility method to restore the visual state of <see cref="ButtonVisuals"/> object when its active gesture is canceled.
        /// </summary>
        /// <param name="evt">The cancel event.</param>
        /// <param name="button">The <see cref="ItemVisuals"/> receiving the cancel gesture event.</param>
        /// <param name="index">Unused here, but this is the index into the data source of list view invoking this event.</param>
        public static void HandlePressCanceled(Gesture.OnCancel evt, ToggleVisuals button, int index)
        {
            button.Background.Color = button.DefaultColor;
        }
    }
}