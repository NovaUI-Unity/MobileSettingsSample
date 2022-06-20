using Nova;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// The root-level UI controller responsible for displaying a list of <see cref="SettingsCollection"/>s, where each collection
    /// is represented by a tab button in a tab bar. When one of the tabs in the tab bar is selected, its corresponding 
    /// list of <see cref="SettingsCollection.Settings"/> will be used to populate another list with a set of UI controls.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Root")]
        [Tooltip("The root UIBlock for the entire settings menu.")]
        public UIBlock UIRoot = null;
        
        [Header("Tabs")]
        [Tooltip("The ListView to populate with a set of tab buttons.")]
        public ListView TabBar = null;
        [Tooltip("The datasource for the TabBar.")]
        public List<SettingsCollection> SettingsTabs = null;

        [Header("Controls")]
        [Tooltip("The ControlPanel managing the active set of UI controls in the menu.")]
        public SettingControls ControlsPanel = null;

        [Header("Navigation")]
        [Tooltip("The UIBlock of a \"Back\" button to simulate a navigable page. Just logs to the console in this example.")]
        public UIBlock BackButton = null;

        [Header("Tab Styles")]
        [Tooltip("The background color to use when a tab is actively selected.")]
        public Color SelectedColor;
        [Tooltip("The TabBar's tabs' default background color.")]
        public Color DefaultColor;
        [Tooltip("The accent color to use while a tab button is being pressed.")]
        public Color PressedAccentColor;
        [Tooltip("The TabBar's tabs' default accent color.")]
        public Color DefaultAccentColor;

        /// <summary>
        /// The currently selected tab index.
        /// </summary>
        [NonSerialized]
        private int selectedIndex = -1;

        private void OnEnable()
        {
            // Subscribe to press and scroll gesture events on the UIRoot to track "focus"
            // changes when anything in the entire UI is pressed or scrolled
            UIRoot.AddGestureHandler<Gesture.OnPress>(HandleSomethingPressed);
            UIRoot.AddGestureHandler<Gesture.OnScroll>(HandleSomethingScrolled);

            // Register a databinder on the tab view
            TabBar.AddDataBinder<SettingsCollection, TabButtonVisuals>(BindSettingsTab);

            // Register gesture events on tab view
            TabBar.AddGestureHandler<Gesture.OnClick, TabButtonVisuals>(HandleSettingsTabClicked);
            TabBar.AddGestureHandler<Gesture.OnPress, TabButtonVisuals>(HandleSettingsTabPressed);
            TabBar.AddGestureHandler<Gesture.OnRelease, TabButtonVisuals>(HandleSettingsTabReleased);
            TabBar.AddGestureHandler<Gesture.OnCancel, TabButtonVisuals>(HandleSettingsTabPressCanceled);

            // Set the tab view's data source
            TabBar.SetDataSource(SettingsTabs);

            // Grab the ItemView for the first tab
            if (TabBar.TryGetItemView(0, out ItemView tabView))
            {
                // Default to selecting the first tab in the list on enable
                SelectTab(tabView.Visuals as TabButtonVisuals, 0);
            }

            // Register gesture events on the back button
            BackButton.AddGestureHandler<Gesture.OnClick, ButtonVisuals>(HandleBackButtonClicked);
            BackButton.AddGestureHandler<Gesture.OnPress, ButtonVisuals>(ButtonVisuals.HandlePressed);
            BackButton.AddGestureHandler<Gesture.OnRelease, ButtonVisuals>(ButtonVisuals.HandleReleased);
            BackButton.AddGestureHandler<Gesture.OnCancel, ButtonVisuals>(ButtonVisuals.HandlePressCanceled);
        }

        private void OnDisable()
        {
            // Unsubscribe from the "focus"-tracking gesture events
            UIRoot.RemoveGestureHandler<Gesture.OnPress>(HandleSomethingPressed);
            UIRoot.RemoveGestureHandler<Gesture.OnScroll>(HandleSomethingScrolled);

            // Remove the tab view's databinder
            TabBar.RemoveDataBinder<SettingsCollection, TabButtonVisuals>(BindSettingsTab);

            // Remove the tab view's gesture handlers
            TabBar.RemoveGestureHandler<Gesture.OnClick, TabButtonVisuals>(HandleSettingsTabClicked);
            TabBar.RemoveGestureHandler<Gesture.OnPress, TabButtonVisuals>(HandleSettingsTabPressed);
            TabBar.RemoveGestureHandler<Gesture.OnRelease, TabButtonVisuals>(HandleSettingsTabReleased);
            TabBar.RemoveGestureHandler<Gesture.OnCancel, TabButtonVisuals>(HandleSettingsTabPressCanceled);

            BackButton.RemoveGestureHandler<Gesture.OnClick, ButtonVisuals>(HandleBackButtonClicked);
            BackButton.RemoveGestureHandler<Gesture.OnPress, ButtonVisuals>(ButtonVisuals.HandlePressed);
            BackButton.RemoveGestureHandler<Gesture.OnRelease, ButtonVisuals>(ButtonVisuals.HandleReleased);
            BackButton.RemoveGestureHandler<Gesture.OnCancel, ButtonVisuals>(ButtonVisuals.HandlePressCanceled);
        }

        /// <summary>
        /// Handle all Press events to track "focus" changes. Press events cover all "pointer down" events.
        /// </summary>
        /// <param name="evt">The data associated with the press event.</param>
        private void HandleSomethingPressed(Gesture.OnPress evt)
        {
            // Tell the ControlsPanel to handle something new being pressed
            ControlsPanel.HandleFocusChanged(evt.Receiver);
        }

        /// <summary>
        /// Handle all Scroll events to track "focus" changes. Scroll events handle all mouse-wheel events (and pointer scroll events).
        /// </summary>
        /// <param name="evt">The data associated with the scroll event.</param>
        private void HandleSomethingScrolled(Gesture.OnScroll evt)
        {
            if (evt.ScrollType == ScrollType.Inertial)
            {
                // Not a manual scroll event, so we can ignore it.
                return;
            }

            // Tell the ControlsPanel to handle something new being scrolled
            ControlsPanel.HandleFocusChanged(evt.Receiver);
        }

        /// <summary>
        /// Bind the <paramref name="button"/> visual to its corresponding data object.
        /// </summary>
        /// <param name="evt">The bind event data.</param>
        /// <param name="button">The <see cref="TabButtonVisuals"/> object representing the data being bound into view.</param>
        /// <param name="index">The index into <see cref="SettingsTabs"/> of the data object being bound into view.</param>
        private void BindSettingsTab(Data.OnBind<SettingsCollection> evt, TabButtonVisuals button, int index)
        {
            // The UserData on this bind event is the same value stored
            // at the given `index` into the list of SettingsTabs.
            //
            // I.e.
            // evt.UserData == SettingsTabs[index]
            SettingsCollection settings = evt.UserData;

            // Update the label text to reflect the settings
            // category the button represents
            button.Label.Text = settings.Category;
        }

        /// <summary>
        /// If a press gesture begins, modify the pressed <paramref name="button"/>'s visual state to indicate the press.
        /// </summary>
        /// <param name="evt">The press event.</param>
        /// <param name="button">The <see cref="ItemVisuals"/> object targeted by the event.</param>
        /// <param name="index">The index into <see cref="SettingsTabs"/> of the object represented by <paramref name="button"/>.</param>
        private void HandleSettingsTabPressed(Gesture.OnPress evt, TabButtonVisuals button, int index)
        {
            // Update the button's background color to the pressed color
            button.Background.Gradient.Color = PressedAccentColor;
        }

        /// <summary>
        /// If a release gesture occurs, restore the <paramref name="button"/>'s visual state back to the default state.
        /// </summary>
        /// <param name="evt">The release event.</param>
        /// <param name="button">The <see cref="ItemVisuals"/> object targeted by the event.</param>
        /// <param name="index">The index into <see cref="SettingsTabs"/> of the object represented by <paramref name="button"/>.</param>
        private void HandleSettingsTabReleased(Gesture.OnRelease evt, TabButtonVisuals button, int index)
        {
            // Restore the button's background color to the default color
            button.Background.Gradient.Color = DefaultAccentColor;
        }

        /// <summary>
        /// If a cancel gesture event occurs, restore the <paramref name="button"/>'s visual state back to the default state.
        /// </summary>
        /// <param name="evt">The cancel event.</param>
        /// <param name="button">The <see cref="ItemVisuals"/> object targeted by the event.</param>
        /// <param name="index">The index into <see cref="SettingsTabs"/> of the object represented by <paramref name="button"/>.</param>
        private void HandleSettingsTabPressCanceled(Gesture.OnCancel evt, TabButtonVisuals button, int index)
        {
            // Restore the button's background color to the default color
            button.Background.Gradient.Color = DefaultAccentColor;
        }

        /// <summary>
        /// When a tab is clicked, update the <see cref="ControlsPanel"/> to display the list of settings associated the selected tab category.
        /// </summary>
        /// <param name="evt">The press event.</param>
        /// <param name="button">The <see cref="TabButtonVisuals"/> object which was clicked.</param>
        /// <param name="index">The index into <see cref="SettingsTabs"/> of the object represented by <paramref name="button"/>.</param>
        private void HandleSettingsTabClicked(Gesture.OnClick evt, TabButtonVisuals button, int index)
        {
            SelectTab(button, index);
        }

        /// <summary>
        /// Set the currently selected tab, and populate <see cref="ControlsPanel"/> with a list of UI controls to configure the underlying user settings.
        /// </summary>
        /// <param name="button">The visuals object of the selected tab.</param>
        /// <param name="index">The index into <see cref="SettingsTabs"/> of the object represented by <paramref name="button"/>.</param>
        private void SelectTab(TabButtonVisuals button, int index)
        {
            if (index == selectedIndex)
            {
                // Trying to select the already selected tab index.
                // Don't need to change anything.
                return;
            }

            // If the previously selected tab was valid (which it might not be on first initialization).
            if (selectedIndex >= 0)
            {
                if (TabBar.TryGetItemView(selectedIndex, out ItemView selectedTab))
                {
                    // Update the visuals of the previously selected tab to indicate
                    // it's no longer selected.
                    TabButtonVisuals selected = selectedTab.Visuals as TabButtonVisuals;
                    selected.IsSelectedIndicator.gameObject.SetActive(false);
                    selected.Background.Color = DefaultColor;
                }
            }

            // Update our tracked selectedIndex
            selectedIndex = index;

            // Update the visuals of the newly selected tab to indicate it's now selected.
            button.IsSelectedIndicator.gameObject.SetActive(true);
            button.Background.Color = SelectedColor;

            // Populate the ControlsPanel with the list of Settings
            // backing the selected tab's category of settings.
            ControlsPanel.DataSource = SettingsTabs[index].Settings;
        }

        /// <summary>
        /// Logs to the console whenever the back button is clicked. Just here as a stub in this example.
        /// </summary>
        /// <param name="evt">The click event data.</param>
        /// <param name="button">The button receiving the event.</param>
        private void HandleBackButtonClicked(Gesture.OnClick evt, ButtonVisuals button)
        {
            Debug.Log("Back!");
        }
    }
}
