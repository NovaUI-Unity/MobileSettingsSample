using Nova;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("ListView")]
        public ListView SettingsList = null;
        [Tooltip("The Scroller responsible for scrolling the settings ListView.")]
        [FormerlySerializedAs("Scroller")]
        public Scroller SettingsScroller = null;

        [Header("Navigation")]
        [Tooltip("The UIBlock of a \"Back\" button to simulate a navigable page. Just logs to the console in this example.")]
        public UIBlock BackButton = null;

        /// <summary>
        /// The currently selected tab index.
        /// </summary>
        [NonSerialized]
        private int selectedIndex = -1;
        /// <summary>
        /// The dropdown that's currently open -- may be null if none are expanded.
        /// </summary>
        [NonSerialized, HideInInspector]
        private DropdownVisuals currentlyExpandedDropdown = null;

        private List<Setting> CurrentSettings => SettingsTabs[selectedIndex].Settings;

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
            TabBar.AddGestureHandler<Gesture.OnHover, TabButtonVisuals>(TabButtonVisuals.HandleHover);
            TabBar.AddGestureHandler<Gesture.OnPress, TabButtonVisuals>(TabButtonVisuals.HandlePress);
            TabBar.AddGestureHandler<Gesture.OnRelease, TabButtonVisuals>(TabButtonVisuals.HandleRelease);
            TabBar.AddGestureHandler<Gesture.OnUnhover, TabButtonVisuals>(TabButtonVisuals.HandleUnhover);
            TabBar.AddGestureHandler<Gesture.OnCancel, TabButtonVisuals>(TabButtonVisuals.HandleCancel);

            // Subscribe to databind events for the different UI controls
            SettingsList.AddDataBinder<FloatSetting, SliderVisuals>(BindSlider);
            SettingsList.AddDataBinder<BoolSetting, ToggleVisuals>(BindToggle);
            SettingsList.AddDataBinder<MultiOptionSetting, DropdownVisuals>(BindDropDown);
            SettingsList.AddDataUnbinder<MultiOptionSetting, DropdownVisuals>(UnbindDropDown);

            // Subscribe to gesture events for the dropdown control type
            SettingsList.AddGestureHandler<Gesture.OnClick, DropdownVisuals>(HandleDropdownClicked);
            SettingsList.AddGestureHandler<Gesture.OnCancel, DropdownVisuals>(DropdownVisuals.HandlePressCanceled);
            SettingsList.AddGestureHandler<Gesture.OnPress, DropdownVisuals>(DropdownVisuals.HandlePressed);
            SettingsList.AddGestureHandler<Gesture.OnRelease, DropdownVisuals>(DropdownVisuals.HandleReleased);

            // Subscribe to gesture events for the toggle control type
            SettingsList.AddGestureHandler<Gesture.OnClick, ToggleVisuals>(HandleToggleClicked);
            SettingsList.AddGestureHandler<Gesture.OnCancel, ToggleVisuals>(ToggleVisuals.HandlePressCanceled);
            SettingsList.AddGestureHandler<Gesture.OnPress, ToggleVisuals>(ToggleVisuals.HandlePressed);
            SettingsList.AddGestureHandler<Gesture.OnRelease, ToggleVisuals>(ToggleVisuals.HandleReleased);

            // Subscribe to gesture events for the slider control type
            SettingsList.AddGestureHandler<Gesture.OnDrag, SliderVisuals>(HandleSliderDragged);

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
            HandleFocusChanged(focusReceiver: null);

            // Unsubscribe from the "focus"-tracking gesture events
            UIRoot.RemoveGestureHandler<Gesture.OnPress>(HandleSomethingPressed);
            UIRoot.RemoveGestureHandler<Gesture.OnScroll>(HandleSomethingScrolled);

            // Remove the tab view's databinder
            TabBar.RemoveDataBinder<SettingsCollection, TabButtonVisuals>(BindSettingsTab);

            // Remove the tab view's gesture handlers
            TabBar.RemoveGestureHandler<Gesture.OnClick, TabButtonVisuals>(HandleSettingsTabClicked);
            TabBar.RemoveGestureHandler<Gesture.OnHover, TabButtonVisuals>(TabButtonVisuals.HandleHover);
            TabBar.RemoveGestureHandler<Gesture.OnPress, TabButtonVisuals>(TabButtonVisuals.HandlePress);
            TabBar.RemoveGestureHandler<Gesture.OnRelease, TabButtonVisuals>(TabButtonVisuals.HandleRelease);
            TabBar.RemoveGestureHandler<Gesture.OnUnhover, TabButtonVisuals>(TabButtonVisuals.HandleUnhover);
            TabBar.RemoveGestureHandler<Gesture.OnCancel, TabButtonVisuals>(TabButtonVisuals.HandleCancel);

            BackButton.RemoveGestureHandler<Gesture.OnClick, ButtonVisuals>(HandleBackButtonClicked);
            BackButton.RemoveGestureHandler<Gesture.OnPress, ButtonVisuals>(ButtonVisuals.HandlePressed);
            BackButton.RemoveGestureHandler<Gesture.OnRelease, ButtonVisuals>(ButtonVisuals.HandleReleased);
            BackButton.RemoveGestureHandler<Gesture.OnCancel, ButtonVisuals>(ButtonVisuals.HandlePressCanceled);

            // Remove bind event handlers
            SettingsList.RemoveDataBinder<FloatSetting, SliderVisuals>(BindSlider);
            SettingsList.RemoveDataBinder<BoolSetting, ToggleVisuals>(BindToggle);
            SettingsList.RemoveDataBinder<MultiOptionSetting, DropdownVisuals>(BindDropDown);
            SettingsList.RemoveDataUnbinder<MultiOptionSetting, DropdownVisuals>(UnbindDropDown);

            // Remove dropdown control gesture handlers
            SettingsList.RemoveGestureHandler<Gesture.OnClick, DropdownVisuals>(HandleDropdownClicked);
            SettingsList.RemoveGestureHandler<Gesture.OnCancel, DropdownVisuals>(DropdownVisuals.HandlePressCanceled);
            SettingsList.RemoveGestureHandler<Gesture.OnPress, DropdownVisuals>(DropdownVisuals.HandlePressed);
            SettingsList.RemoveGestureHandler<Gesture.OnRelease, DropdownVisuals>(DropdownVisuals.HandleReleased);

            // Remove toggle control gesture handlers
            SettingsList.RemoveGestureHandler<Gesture.OnClick, ToggleVisuals>(HandleToggleClicked);
            SettingsList.RemoveGestureHandler<Gesture.OnCancel, ToggleVisuals>(ToggleVisuals.HandlePressCanceled);
            SettingsList.RemoveGestureHandler<Gesture.OnPress, ToggleVisuals>(ToggleVisuals.HandlePressed);
            SettingsList.RemoveGestureHandler<Gesture.OnRelease, ToggleVisuals>(ToggleVisuals.HandleReleased);

            // Remove slider control gesture handlers
            SettingsList.RemoveGestureHandler<Gesture.OnDrag, SliderVisuals>(HandleSliderDragged);
        }

        /// <summary>
        /// Handle all Press events to track "focus" changes. Press events cover all "pointer down" events.
        /// </summary>
        /// <param name="evt">The data associated with the press event.</param>
        private void HandleSomethingPressed(Gesture.OnPress evt)
        {
            // Tell the ControlsPanel to handle something new being pressed
            HandleFocusChanged(evt.Receiver);
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
            HandleFocusChanged(evt.Receiver);
        }

        /// <summary>
        /// Tell this control panel to handle a focus change event, which may involve
        /// removing its concept of "focus" from the actively "focused" object. 
        /// 
        /// i.e.
        /// This will collapse any expanded dropdown if <paramref name="focusReceiver"/>
        /// is outside the expanded dropdown's child hierarchy.
        /// </summary>
        /// <param name="focusReceiver">The newly "focused" object. May be null.</param>
        public void HandleFocusChanged(UIBlock focusReceiver)
        {
            if (currentlyExpandedDropdown == null)
            {
                // Not currently tracking a focused dropdown,
                // so we don't need to do anything here.
                return;
            }

            // null may be provided if nothing new is "focused"
            if (focusReceiver != null)
            {
                if (focusReceiver.transform.IsChildOf(currentlyExpandedDropdown.View.transform))
                {
                    // The newly focused object is a child (inclusive) of the
                    // currently expanded dropdown, so we want to leave
                    // the dropdown expanded.
                    return;
                }
            }

            // Something outside the currentlyExpandedDropdown's hierarchy
            // was "focused", so collapse the dropdown and clear it out.
            currentlyExpandedDropdown.Collapse();
            currentlyExpandedDropdown = null;
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
                    selected.IsSelected = false;
                }
            }

            // Update our tracked selectedIndex
            selectedIndex = index;

            // Update the visuals of the newly selected tab to indicate it's now selected.
            button.IsSelected = true;

            // Populate the ControlsPanel with the list of Settings
            // backing the selected tab's category of settings.
            SettingsScroller.CancelScroll();
            SettingsList.SetDataSource(SettingsTabs[index].Settings);
        }

        /// <summary>
        /// Bind a <see cref="FloatSetting"/> data object to a <see cref="SliderVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The bind event holding the relevant <see cref="FloatSetting"/> data.</param>
        /// <param name="sliderControl">The interactive <see cref="ItemVisuals"/> object to display the relevant slider data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="FloatSetting"/>being bound to the view.</param>
        private void BindSlider(Data.OnBind<FloatSetting> evt, SliderVisuals sliderControl, int index)
        {
            // The UserData on this bind event is the same value stored
            // at the given `index` into the DataSource.
            //
            // I.e.
            // evt.UserData == DataSource[index]
            FloatSetting slider = evt.UserData;

            // Update the control's label
            sliderControl.Label.Text = slider.Label;

            // Update the fill bar to reflect the slider's value between it's min/max range
            sliderControl.FillBar.Size.X.Percent = (slider.Value - slider.Min) / (slider.Max - slider.Min);

            // Update the units displayed
            sliderControl.Units.Text = slider.ValueLabel;
        }

        /// <summary>
        /// Bind a <see cref="BoolSetting"/> data object to a <see cref="ToggleVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The bind event holding the relevant <see cref="BoolSetting"/> data.</param>
        /// <param name="toggleControl">The interactive <see cref="ItemVisuals"/> object to display the relevant toggle data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="BoolSetting"/>being bound to the view.</param>
        private void BindToggle(Data.OnBind<BoolSetting> evt, ToggleVisuals toggleControl, int index)
        {
            // The UserData on this bind event is the same value stored
            // at the given `index` into the DataSource.
            //
            // I.e.
            // evt.UserData == DataSource[index]
            BoolSetting toggle = evt.UserData;

            // Update the control's label
            toggleControl.Label.Text = toggle.Label;

            // Update the controls toggle indicator
            toggleControl.IsOnIndicator.gameObject.SetActive(toggle.IsOn);
        }

        /// <summary>
        /// Bind a <see cref="MultiOptionSetting"/> data object to a <see cref="DropdownVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The bind event holding the relevant <see cref="MultiOptionSetting"/> data.</param>
        /// <param name="dropdownControl">The interactive <see cref="ItemVisuals"/> object to display the relevant dropdown data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="MultiOptionSetting"/>being bound to the view.</param>
        private void BindDropDown(Data.OnBind<MultiOptionSetting> evt, DropdownVisuals dropdownControl, int index)
        {
            // Ensure the control starts in a collapsed state
            // when it's newly bound into view.
            dropdownControl.Collapse();

            // The UserData on this bind event is the same value stored
            // at the given `index` into the DataSource.
            //
            // I.e.
            // evt.UserData == DataSource[index]
            MultiOptionSetting dropdown = evt.UserData;

            // Update the control's label
            dropdownControl.DropdownLabel.Text = dropdown.Label;

            // Update the selection field to indicate the currently selected option in the dropdown
            dropdownControl.SelectionLabel.Text = dropdown.SelectionName;
        }

        /// <summary>
        /// Unbind a <see cref="MultiOptionSetting"/> data object from a <see cref="DropdownVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The unbind event holding the relevant <see cref="MultiOptionSetting"/> data.</param>
        /// <param name="dropdownControl">The interactive <see cref="ItemVisuals"/> object displaying the relevant dropdown data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="MultiOptionSetting"/> being unbound from the view.</param>
        private void UnbindDropDown(Data.OnUnbind<MultiOptionSetting> evt, DropdownVisuals dropdownControl, int index)
        {
            if (dropdownControl == currentlyExpandedDropdown)
            {
                // if this dropdown is tracked as the currentlyExpandedObject
                // clear this field to indicate no dropdown is currently expanded
                currentlyExpandedDropdown = null;
            }

            // Ensure the dropdown control is collapsed
            dropdownControl.Collapse();
        }

        /// <summary>
        /// Handle a <see cref="DropdownVisuals"/> object in the <see cref="SettingsList">
        /// being clicked, and either expand or collapse it accordingly.
        /// </summary>
        /// <param name="evt">The click event data.</param>
        /// <param name="dropdownControl">The <see cref="ItemVisuals"/> object which was clicked.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="MultiOptionSetting"/> represented by <paramref name="dropdownControl"/>.</param>
        private void HandleDropdownClicked(Gesture.OnClick evt, DropdownVisuals dropdownControl, int index)
        {
            if (evt.Receiver.transform.IsChildOf(dropdownControl.OptionsView.transform))
            {
                // The clicked object was not the dropdown itself but rather a list item within the dropdown.
                // The dropdownControl itself will handle this event, so we don't need to do anything here.
                return;
            }

            if (dropdownControl.IsExpanded)
            {
                // Collapse the dropdown and stop tracking it
                // as the expanded focused object.
                dropdownControl.Collapse();

                // Because this event will always be called each time a dropdown is
                // clicked, we can safely assume the currentlyExpandedDropdown == dropdownControl,
                // since we assign it below and nowhere else.
                currentlyExpandedDropdown = null;
            }
            else
            {
                // Get the dropdown's underlying data source
                // so we can expand the dropdownControl with
                // its set of selectable options.
                MultiOptionSetting dropdown = CurrentSettings[index] as MultiOptionSetting;

                // Tell the dropdown to expand, showing a list of
                // selectable options.
                dropdownControl.Expand(dropdown);

                // Start tracking this dropdownControl as the
                // currentlyExpandedDropdown.
                currentlyExpandedDropdown = dropdownControl;
            }
        }

        /// <summary>
        /// Handle a <see cref="ToggleVisuals"/> object in the <see cref="SettingsList">
        /// being clicked, and toggle its visual state and data-backed state accordingly.
        /// </summary>
        /// <param name="evt">The click event data.</param>
        /// <param name="toggleControl">The <see cref="ItemVisuals"/> object which was clicked.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="BoolSetting"/> represented by <paramref name="toggleControl"/>.</param>
        private void HandleToggleClicked(Gesture.OnClick evt, ToggleVisuals toggleControl, int index)
        {
            // Get the underlying toggle data object represented by toggleControl
            BoolSetting toggle = CurrentSettings[index] as BoolSetting;

            // Toggle the data object's state
            toggle.IsOn = !toggle.IsOn;

            // Update the toggleControl's visual state to reflect the new IsOn state
            toggleControl.IsOnIndicator.gameObject.SetActive(toggle.IsOn);
        }

        /// <summary>
        /// Handle a <see cref="SliderVisuals"/> object in the <see cref="SettingsList">
        /// being dragged, and adjust its visual state and data-backed state accordingly.
        /// </summary>
        /// <param name="evt">The drag event data.</param>
        /// <param name="sliderControl">The <see cref="ItemVisuals"/> object which was dragged.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="FloatSetting"/> represented by <paramref name="sliderControl"/>.</param>
        private void HandleSliderDragged(Gesture.OnDrag evt, SliderVisuals sliderControl, int index)
        {
            // Get the underlying slider data object represented by sliderControl
            FloatSetting slider = CurrentSettings[index] as FloatSetting;

            // Convert the current drag position into slider local space
            Vector3 pointerLocalPosition = sliderControl.SliderBounds.transform.InverseTransformPoint(evt.PointerPositions.Current);

            // Get the pointer's distance from the left edge of the control
            float sliderPositionFromLeft = pointerLocalPosition.x + 0.5f * sliderControl.SliderBounds.CalculatedSize.X.Value;

            // Convert the distance from the left edge into a percent from the left edge
            float sliderPercent = Mathf.Clamp01(sliderPositionFromLeft / sliderControl.SliderBounds.CalculatedSize.X.Value);

            // Update the slider control to the new value within its min/max range
            slider.Value = Mathf.Lerp(slider.Min, slider.Max, sliderPercent);

            // Update the control's fillbar to reflect its new slider value
            sliderControl.FillBar.Size.X.Percent = sliderPercent;

            // Update the control's unit text to display the numeric
            // value associated with the slider control
            sliderControl.Units.Text = slider.ValueLabel;
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
