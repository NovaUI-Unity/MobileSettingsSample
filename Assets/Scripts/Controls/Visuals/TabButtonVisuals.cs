using Nova;
using System;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// An <see cref="ItemVisuals"/> class to visually represent a button in a tab-bar. Inherits from <see cref="ButtonVisuals"/>.
    /// </summary>
    [System.Serializable]
    public class TabButtonVisuals : ItemVisuals
    {
        [Tooltip("The button's background UIBlock.")]
        public UIBlock2D Background = null;
        [Tooltip("The TextBlock to display the button's label.")]
        public TextBlock Label = null;
        [Tooltip("The UIBlock to enable while this tab is the \"selected\" tab in the tab-bar.")]
        public UIBlock2D IsSelectedIndicator;

        [Tooltip("The default background color.")]
        public Color DefaultColor;
        [Tooltip("The background color to use when the tab is actively selected.")]
        public Color SelectedColor;

        [Tooltip("The default gradient color.")]
        public Color DefaultGradientColor;
        [Tooltip("The gradient color to use when the tab is hovered.")]
        public Color HoveredGradientColor;
        [Tooltip("The gradient color to use when the tab is pressed.")]
        public Color PressedGradientColor;

        public bool IsSelected
        {
            set
            {
                IsSelectedIndicator.gameObject.SetActive(value);
                Background.Color = value ? SelectedColor : DefaultColor;
            }
        }

        internal static void HandleCancel(Gesture.OnCancel evt, TabButtonVisuals target, int index)
        {
            target.Background.Gradient.Color = target.DefaultGradientColor;
        }

        internal static void HandleHover(Gesture.OnHover evt, TabButtonVisuals target, int index)
        {
            target.Background.Gradient.Color = target.HoveredGradientColor;
        }

        internal static void HandlePress(Gesture.OnPress evt, TabButtonVisuals target, int index)
        {
            target.Background.Gradient.Color = target.PressedGradientColor;
        }

        internal static void HandleRelease(Gesture.OnRelease evt, TabButtonVisuals target, int index)
        {
            target.Background.Gradient.Color = target.HoveredGradientColor;
        }

        internal static void HandleUnhover(Gesture.OnUnhover evt, TabButtonVisuals target, int index)
        {
            target.Background.Gradient.Color = target.DefaultGradientColor;
        }
    }
}
