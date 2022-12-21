using Nova;
using System;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// The <see cref="ItemVisuals"/> type used to display a <see cref="MultiOptionSetting"/> control and its list of selectable options.
    /// </summary>
    [Serializable]
    public class DropdownVisuals : ItemVisuals
    {
        [Header("Collapsed Visuals")]
        [Tooltip("The TextBlock to display the label of the dropdown itself.")]
        public TextBlock DropdownLabel;
        [Tooltip("The TextBlock to display the label of currently selected option.")]
        public TextBlock SelectionLabel;
        [Tooltip("The background visual element to change as the dropdown is pressed and released.")]
        public UIBlock2D Background;

        [Header("Expanded Visuals")]
        [Tooltip("The visual root of the content to display when the dropdown is expanded.")]
        public UIBlock ExpandedViewRoot;
        [Tooltip("The ListView used to display the different options in the dropdown.")]
        public ListView OptionsView;
        [Tooltip("The SortGroup used to ensure all dropdown content renders on top of other UI elements when it's expanded.")]
        public SortGroup SortGroup;

        [Header("Visual State Colors")]
        [Tooltip("The default background color of the dropdown.")]
        public Color DefaultColor;
        [Tooltip("The color to use on this dropdown and its list items when one of them is pressed.")]
        public Color PressedColor;

        [Header("Options View Row Colors")]
        [Tooltip("The default background color of the dropdown list items. Every even-row item will be this color.")]
        public Color DefaultRowColor;
        [Tooltip("The alternative background color of the dropdown list items. Every odd-row item will be this color.")]
        public Color AlternatingRowColor;

        /// <summary>
        /// The datasource used to populate this dropdown control and its list of options.
        /// </summary>
        public MultiOptionSetting DataSource { get; private set; }

        /// <summary>
        /// Is the dropdown list open?
        /// </summary>
        public bool IsExpanded => ExpandedViewRoot.gameObject.activeSelf;

        [NonSerialized, HideInInspector]
        private bool eventHandlersRegistered = false;

        /// <summary>
        /// Expand this dropdown control and populate its expanded 
        /// <see cref="OptionsView"/> with the options in the datasource.
        /// </summary>
        /// <param name="dataSource">The datasource to populate this dropdown's list of options.</param>
        public void Expand(MultiOptionSetting dataSource)
        {
            // Cache the current datasource, so we can update it in 
            // the event a new option is selected from the dropdown.
            DataSource = dataSource;

            // Verify the bind/interaction handlers
            // are registered for when we populate
            // the options view
            EnsureEventHandlersRegistered();

            // Enable the attached SortGroup, so the expanded list of
            // options always renders on top of the content it's popping
            // out over.
            SortGroup.enabled = true;

            // Enable the OptionsView and other visuals
            ExpandedViewRoot.gameObject.SetActive(true);

            // Set the datasource if it has changed
            if (OptionsView.GetDataSource<string>() != dataSource.Options)
            {
                OptionsView.SetDataSource(dataSource.Options);
            }

            // Jump the view to the current selected value
            OptionsView.JumpToIndex(dataSource.SelectedIndex);
        }

        /// <summary>
        /// Collapse the dropdown list of options.
        /// </summary>
        public void Collapse()
        {
            // Disable the SortGroup to revert to default sorting order
            SortGroup.enabled = false;

            // Disable the list of selectable options
            ExpandedViewRoot.gameObject.SetActive(false);
        }

        /// <summary>
        /// Register the desired databind/interaction event handlers if not already subscribed.
        /// </summary>
        private void EnsureEventHandlersRegistered()
        {
            if (eventHandlersRegistered)
            {
                return;
            }

            // Register data binder
            OptionsView.AddDataBinder<string, ButtonVisuals>(BindOption);

            // Register gesture handlers
            OptionsView.AddGestureHandler<Gesture.OnClick, ButtonVisuals>(HandleOptionSelected);
            OptionsView.AddGestureHandler<Gesture.OnPress, ButtonVisuals>(HandleOptionPressed);
            OptionsView.AddGestureHandler<Gesture.OnRelease, ButtonVisuals>(HandleOptionReleased);
            OptionsView.AddGestureHandler<Gesture.OnCancel, ButtonVisuals>(HandleOptionPressCanceled);

            eventHandlersRegistered = true;
        }

        /// <summary>
        /// Event handler to react to a list item in the dropdown (bound to one of the options in <see cref="MultiOptionSetting.Options"/>) being selected.
        /// </summary>
        /// <param name="evt">The click event.</param>
        /// <param name="option">The <see cref="ItemVisuals"/> object that was selected.</param>
        /// <param name="index">The index into <see cref="MultiOptionSetting.Options"/> of the object represented by <paramref name="option"/>.</param>
        public void HandleOptionSelected(Gesture.OnClick evt, ButtonVisuals option, int index)
        {
            // If a new option was selected, update the datasource. 
            if (index != DataSource.SelectedIndex)
            {
                // Get the currently selected list item if it's in view.
                if (OptionsView.TryGetItemView(DataSource.SelectedIndex, out ItemView selectedListItem))
                {
                    // Get the list item's visuals as a ButtonVisuals.
                    ButtonVisuals selectedVisuals = selectedListItem.Visuals as ButtonVisuals;

                    // Disable the border on the list item to indicate it's no longer selected.
                    selectedVisuals.Background.Border.Enabled = false;
                }

                // Set the selected index in the data source.
                DataSource.SelectedIndex = index;

                // Update this controls SelectionLabel to reflect the newly selected option.
                SelectionLabel.Text = DataSource.SelectionName;

                // Enable the border on the list item to indicate its selected state.
                option.Background.Border.Enabled = true;
            }

            // Collapse the expanded OptionsView
            Collapse();
        }

        /// <summary>
        /// If a press gesture was canceled, likely due to content being scrolled, reset  formerly pressed object's visual state.
        /// </summary>
        /// <param name="evt">The cancel event.</param>
        /// <param name="option">The <see cref="ItemVisuals"/> object targeted by the event.</param>
        /// <param name="index">The index into <see cref="MultiOptionSetting.Options"/> of the object represented by <paramref name="option"/>.</param>
        private void HandleOptionPressCanceled(Gesture.OnCancel evt, ButtonVisuals option, int index)
        {
            // Restore the background color to its default value
            option.Background.Color = index % 2 == 0 ? DefaultRowColor : AlternatingRowColor;
        }

        /// <summary>
        /// If a press gesture was released, reset the formerly pressed object's visual state.
        /// </summary>
        /// <param name="evt">The release event.</param>
        /// <param name="option">The <see cref="ItemVisuals"/> object targeted by the event.</param>
        /// <param name="index">The index into <see cref="MultiOptionSetting.Options"/> of the object represented by <paramref name="option"/>.</param>
        private void HandleOptionReleased(Gesture.OnRelease evt, ButtonVisuals option, int index)
        {
            // Restore the background color to its default value
            option.Background.Color = index % 2 == 0 ? DefaultRowColor : AlternatingRowColor;
        }

        /// <summary>
        /// If a press gesture begins, modify the pressed object's visual state to indicate the press.
        /// </summary>
        /// <param name="evt">The press event.</param>
        /// <param name="option">The <see cref="ItemVisuals"/> object targeted by the event.</param>
        /// <param name="index">The index into <see cref="MultiOptionSetting.Options"/> of the object represented by <paramref name="option"/>.</param>
        private void HandleOptionPressed(Gesture.OnPress evt, ButtonVisuals option, int index)
        {
            // Indicate the press is occuring by changing the pressed visual's background color
            option.Background.Color = PressedColor;
        }

        /// <summary>
        /// Bind the <paramref name="option"/> visual to its corresponding data object.
        /// </summary>
        /// <param name="evt">The bind event data.</param>
        /// <param name="option">The <see cref="ButtonVisuals"/> object representing the data being bound into view.</param>
        /// <param name="index">The index into <see cref="DataSource"/> of the data object being bound into view.</param>
        private void BindOption(Data.OnBind<string> evt, ButtonVisuals option, int index)
        {
            // The UserData on this bind event is the same value stored
            // at the given `index` into the list of options.
            //
            // I.e.
            // evt.UserData == DataSource.Options[index]
            option.Label.Text = evt.UserData;
           
            // Highlight the selected row to differentiate it from the rest
            option.Background.Border.Enabled = index == DataSource.SelectedIndex;

            // For aesthetics and legibility we want to alternate the background color of rows
            // in the options view. Even numbered rows will use the default color, and odd numbered
            // rows will use the alternating color.
            option.Background.Color = index % 2 == 0 ? DefaultRowColor : AlternatingRowColor;
        }

        /// <summary>
        /// A utility method to restore the visual state of <see cref="DropdownVisuals"/> 
        /// object when its active gesture (likely a press) is canceled.
        /// </summary>
        /// <param name="evt">The cancel event.</param>
        /// <param name="visuals">The <see cref="ItemVisuals"/> receiving the cancel event.</param>
        /// <param name="index">Unused here, but this is the index into the data source of list view invoking this event.</param>
        public static void HandlePressCanceled(Gesture.OnCancel evt, DropdownVisuals visuals, int index)
        {
            // Restore the background color
            visuals.Background.Color = visuals.DefaultColor;
        }

        /// <summary>
        /// A utility method to restore the visual state of <see cref="DropdownVisuals"/> 
        /// object when its active gesture (likely a press) is released.
        /// </summary>
        /// <param name="evt">The release event.</param>
        /// <param name="visuals">The <see cref="ItemVisuals"/> receiving the release event.</param>
        /// <param name="index">Unused here, but this is the index into the data source of list view invoking this event.</param>
        public static void HandleReleased(Gesture.OnRelease evt, DropdownVisuals visuals, int index)
        {
            // Restore the background color
            visuals.Background.Color = visuals.DefaultColor;
        }

        /// <summary>
        /// A utility method to indicate a pressed visual state of <see cref="DropdownVisuals"/> object.
        /// </summary>
        /// <param name="evt">The press event.</param>
        /// <param name="visuals">The <see cref="ItemVisuals"/> receiving the press event.</param>
        /// <param name="index">Unused here, but this is the index into the data source of list view invoking this event.</param>
        public static void HandlePressed(Gesture.OnPress evt, DropdownVisuals visuals, int index)
        {
            if (evt.Receiver.transform.IsChildOf(visuals.ExpandedViewRoot.transform))
            {
                // The hierarchy is pressed, which will happen if one of the objects in visuals.OptionsView
                // is pressed or the scrollbar is pressed, but we don't want to highlight the background
                // element in that context.
                return;
            }

            // Update the background color to indicate a press is occurring.
            visuals.Background.Color = visuals.PressedColor;
        }
    }
}
