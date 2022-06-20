using Nova;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A UI Component resposible for binding a variety of UI controls (Toggles, Sliders, Dropdowns, etc.) to a <see cref="ListView"/>
    /// and updating the underlying data source as each control is manipulated by the user.
    /// </summary>
    public class SettingControls : MonoBehaviour
    {
        [Tooltip("The ListView to populate with a list of UI controls.")]
        public ListView ListView = null;

        [Tooltip("The Scroller responsible for scrolling the ListView.")]
        public Scroller Scroller = null;

        /// <summary>
        /// Track our event registration state, so we only
        /// try to register event handles once
        /// </summary>
        [NonSerialized, HideInInspector]
        private bool eventHandlersRegistered = false;
        
        /// <summary>
        /// The dropdown that's currently open -- may be null if none are expanded.
        /// </summary>
        [NonSerialized, HideInInspector]
        private DropdownVisuals currentlyExpandedDropdown = null;

        /// <summary>
        /// The list of ISettings (<see cref="ToggleSetting"/>, <see cref="SliderSetting"/>, and <see cref="DropdownSetting"/>)
        /// acting as a data source for a corresponding visual/interactive list of controls 
        /// (<see cref="ToggleVisuals"/>, <see cref="SliderVisuals"/>, and <see cref="DropdownVisuals"/>).
        /// </summary>
        public IList<ISetting> DataSource 
        {
            get
            {
                // return the data source from the ListView.
                // May be null if the data source was never set.
                return ListView.GetDataSource<ISetting>();
            }
            set
            {
                // Ensure bind and gesture event callbacks are registered
                RegisterEventHandlers();

                // Ensure the content isn't scrolling
                // when we populate the list view with
                // a new set of controls.
                Scroller.CancelScroll();

                // Populate the ListView with a variety of
                // UI controls backed by the provided data source.
                ListView.SetDataSource(value);
            }
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
        /// Handle panel enabled
        /// </summary>
        private void OnEnable()
        {
            // Ensure bind and gesture event callbacks are registered
            RegisterEventHandlers();
        }

        /// <summary>
        /// Handle panel disabled
        /// </summary>
        private void OnDisable()
        {
            // Collapse the "focused" dropdown
            HandleFocusChanged(focusReceiver: null);

            // Unregister bind and gesture event callbacks
            UnregisterEventHandlers();
        }

        /// <summary>
        /// Subscribe to desired bind/gesture events
        /// </summary>
        private void RegisterEventHandlers()
        {
            if (eventHandlersRegistered)
            { 
                // Already subscribed
                return;
            }

            eventHandlersRegistered = true;

            // Subscribe to databind events for the different UI controls
            ListView.AddDataBinder<SliderSetting, SliderVisuals>(BindSlider);
            ListView.AddDataBinder<ToggleSetting, ToggleVisuals>(BindToggle);
            ListView.AddDataBinder<DropdownSetting, DropdownVisuals>(BindDropDown);
            ListView.AddDataUnbinder<DropdownSetting, DropdownVisuals>(UnbindDropDown);

            // Subscribe to gesture events for the dropdown control type
            ListView.AddGestureHandler<Gesture.OnClick, DropdownVisuals>(HandleDropdownClicked);
            ListView.AddGestureHandler<Gesture.OnCancel, DropdownVisuals>(DropdownVisuals.HandlePressCanceled);
            ListView.AddGestureHandler<Gesture.OnPress, DropdownVisuals>(DropdownVisuals.HandlePressed);
            ListView.AddGestureHandler<Gesture.OnRelease, DropdownVisuals>(DropdownVisuals.HandleReleased);

            // Subscribe to gesture events for the toggle control type
            ListView.AddGestureHandler<Gesture.OnClick, ToggleVisuals>(HandleToggleClicked);
            ListView.AddGestureHandler<Gesture.OnCancel, ToggleVisuals>(ToggleVisuals.HandlePressCanceled);
            ListView.AddGestureHandler<Gesture.OnPress, ToggleVisuals>(ToggleVisuals.HandlePressed);
            ListView.AddGestureHandler<Gesture.OnRelease, ToggleVisuals>(ToggleVisuals.HandleReleased);

            // Subscribe to gesture events for the slider control type
            ListView.AddGestureHandler<Gesture.OnDrag, SliderVisuals>(HandleSliderDragged);
        }

        /// <summary>
        /// Unregister bind/gesture event handlers
        /// </summary>
        private void UnregisterEventHandlers()
        {
            if (!eventHandlersRegistered)
            {
                // Not currently subscribed, nothing to unsubscribe.
                return;
            }

            eventHandlersRegistered = false;

            // Remove bind event handlers
            ListView.RemoveDataBinder<SliderSetting, SliderVisuals>(BindSlider);
            ListView.RemoveDataBinder<ToggleSetting, ToggleVisuals>(BindToggle);
            ListView.RemoveDataBinder<DropdownSetting, DropdownVisuals>(BindDropDown);
            ListView.RemoveDataUnbinder<DropdownSetting, DropdownVisuals>(UnbindDropDown);

            // Remove dropdown control gesture handlers
            ListView.RemoveGestureHandler<Gesture.OnClick, DropdownVisuals>(HandleDropdownClicked);
            ListView.RemoveGestureHandler<Gesture.OnCancel, DropdownVisuals>(DropdownVisuals.HandlePressCanceled);
            ListView.RemoveGestureHandler<Gesture.OnPress, DropdownVisuals>(DropdownVisuals.HandlePressed);
            ListView.RemoveGestureHandler<Gesture.OnRelease, DropdownVisuals>(DropdownVisuals.HandleReleased);

            // Remove toggle control gesture handlers
            ListView.RemoveGestureHandler<Gesture.OnClick, ToggleVisuals>(HandleToggleClicked);
            ListView.RemoveGestureHandler<Gesture.OnCancel, ToggleVisuals>(ToggleVisuals.HandlePressCanceled);
            ListView.RemoveGestureHandler<Gesture.OnPress, ToggleVisuals>(ToggleVisuals.HandlePressed);
            ListView.RemoveGestureHandler<Gesture.OnRelease, ToggleVisuals>(ToggleVisuals.HandleReleased);

            // Remove slider control gesture handlers
            ListView.RemoveGestureHandler<Gesture.OnDrag, SliderVisuals>(HandleSliderDragged);
        }

        /// <summary>
        /// Bind a <see cref="SliderSetting"/> data object to a <see cref="SliderVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The bind event holding the relevant <see cref="SliderSetting"/> data.</param>
        /// <param name="sliderControl">The interactive <see cref="ItemVisuals"/> object to display the relevant slider data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="SliderSetting"/>being bound to the view.</param>
        private void BindSlider(Data.OnBind<SliderSetting> evt, SliderVisuals sliderControl, int index)
        {
            // The UserData on this bind event is the same value stored
            // at the given `index` into the DataSource.
            //
            // I.e.
            // evt.UserData == DataSource[index]
            SliderSetting slider = evt.UserData;

            // Update the control's label
            sliderControl.Label.Text = slider.Label;

            // Update the fill bar to reflect the slider's value between it's min/max range
            sliderControl.FillBar.Size.X.Percent = (slider.Value - slider.Min) / (slider.Max - slider.Min);

            // Update the units displayed
            sliderControl.Units.Text = slider.ValueLabel;
        }

        /// <summary>
        /// Bind a <see cref="ToggleSetting"/> data object to a <see cref="ToggleVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The bind event holding the relevant <see cref="ToggleSetting"/> data.</param>
        /// <param name="toggleControl">The interactive <see cref="ItemVisuals"/> object to display the relevant toggle data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="ToggleSetting"/>being bound to the view.</param>
        private void BindToggle(Data.OnBind<ToggleSetting> evt, ToggleVisuals toggleControl, int index)
        {
            // The UserData on this bind event is the same value stored
            // at the given `index` into the DataSource.
            //
            // I.e.
            // evt.UserData == DataSource[index]
            ToggleSetting toggle = evt.UserData;

            // Update the control's label
            toggleControl.Label.Text = toggle.Label;

            // Update the controls toggle indicator
            toggleControl.IsOnIndicator.gameObject.SetActive(toggle.IsOn);
        }

        /// <summary>
        /// Bind a <see cref="DropdownSetting"/> data object to a <see cref="DropdownVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The bind event holding the relevant <see cref="DropdownSetting"/> data.</param>
        /// <param name="dropdownControl">The interactive <see cref="ItemVisuals"/> object to display the relevant dropdown data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="DropdownSetting"/>being bound to the view.</param>
        private void BindDropDown(Data.OnBind<DropdownSetting> evt, DropdownVisuals dropdownControl, int index)
        { 
            // Ensure the control starts in a collapsed state
            // when it's newly bound into view.
            dropdownControl.Collapse();

            // The UserData on this bind event is the same value stored
            // at the given `index` into the DataSource.
            //
            // I.e.
            // evt.UserData == DataSource[index]
            DropdownSetting dropdown = evt.UserData;

            // Update the control's label
            dropdownControl.DropdownLabel.Text = dropdown.Label;

            // Update the selection field to indicate the currently selected option in the dropdown
            dropdownControl.SelectionLabel.Text = dropdown.SelectionName;
        }

        /// <summary>
        /// Unbind a <see cref="DropdownSetting"/> data object from a <see cref="DropdownVisuals"/> UI control.
        /// </summary>
        /// <param name="evt">The unbind event holding the relevant <see cref="DropdownSetting"/> data.</param>
        /// <param name="dropdownControl">The interactive <see cref="ItemVisuals"/> object displaying the relevant dropdown data.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="DropdownSetting"/> being unbound from the view.</param>
        private void UnbindDropDown(Data.OnUnbind<DropdownSetting> evt, DropdownVisuals dropdownControl, int index)
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
        /// Handle a <see cref="DropdownVisuals"/> object in the <see cref="ListView">
        /// being clicked, and either expand or collapse it accordingly.
        /// </summary>
        /// <param name="evt">The click event data.</param>
        /// <param name="dropdownControl">The <see cref="ItemVisuals"/> object which was clicked.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="DropdownSetting"/> represented by <paramref name="dropdownControl"/>.</param>
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
                DropdownSetting dropdown = DataSource[index] as DropdownSetting;

                // Tell the dropdown to expand, showing a list of
                // selectable options.
                dropdownControl.Expand(dropdown);

                // Start tracking this dropdownControl as the
                // currentlyExpandedDropdown.
                currentlyExpandedDropdown = dropdownControl;
            }
        }

        /// <summary>
        /// Handle a <see cref="ToggleVisuals"/> object in the <see cref="ListView">
        /// being clicked, and toggle its visual state and data-backed state accordingly.
        /// </summary>
        /// <param name="evt">The click event data.</param>
        /// <param name="toggleControl">The <see cref="ItemVisuals"/> object which was clicked.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="ToggleSetting"/> represented by <paramref name="toggleControl"/>.</param>
        private void HandleToggleClicked(Gesture.OnClick evt, ToggleVisuals toggleControl, int index)
        {
            // Get the underlying toggle data object represented by toggleControl
            ToggleSetting toggle = DataSource[index] as ToggleSetting;

            // Toggle the data object's state
            toggle.IsOn = !toggle.IsOn;

            // Update the toggleControl's visual state to reflect the new IsOn state
            toggleControl.IsOnIndicator.gameObject.SetActive(toggle.IsOn);
        }

        /// <summary>
        /// Handle a <see cref="SliderVisuals"/> object in the <see cref="ListView">
        /// being dragged, and adjust its visual state and data-backed state accordingly.
        /// </summary>
        /// <param name="evt">The drag event data.</param>
        /// <param name="sliderControl">The <see cref="ItemVisuals"/> object which was dragged.</param>
        /// <param name="index">The index into the <see cref="DataSource"/> of the <see cref="SliderSetting"/> represented by <paramref name="sliderControl"/>.</param>
        private void HandleSliderDragged(Gesture.OnDrag evt, SliderVisuals sliderControl, int index)
        {
            // Get the underlying slider data object represented by sliderControl
            SliderSetting slider = DataSource[index] as SliderSetting;

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
    }
}
