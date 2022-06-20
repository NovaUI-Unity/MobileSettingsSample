# Nova Settings Menu Sample

A colorful, multi-page settings menu that includes drop-downs, toggles, and sliders.

https://user-images.githubusercontent.com/8591310/176090767-6879d371-b148-4133-84c2-9d9f8261030b.mp4

## Setup

This sample does not include Nova, which must be imported before the sample can be used. After cloning the repo:

1. Open the project in Unity. The project will have errors due to the missing Nova assets, so when prompted by Unity either open the project in Safe Mode or select `Ignore`.
1. Import the Nova asset into the project via the Package Manager.
    - When selecting the files to import, be sure to deselect the Nova settings (`Nova/Resources/NovaSettings.asset`) as they are already included and configured for the sample.

## Script Highlights

- [`SettingsMenu`](Assets/Scripts/SettingsMenu.cs): The root-level UI controller responsible for displaying a list of [`SettingsCollections`](Assets/Scripts/Controls/DataTypes/SettingsCollection.cs), where each collection is represented by a tab button in a tab bar. When one of the tabs in the tab bar is selected, its corresponding list of settings will be provided to [`SettingControls`](Assets/Scripts/SettingControls.cs) to populate a set of UI controls.
- [`SettingControls`](Assets/Scripts/SettingControls.cs): A UI Component resposible for binding a variety of UI controls (Toggles, Sliders, Dropdowns, etc.) to a [ListView](https://www.novaui.io/manual/ListView.html) and updating the underlying data source as each control is manipulated by the user.
- [`SettingsCollection`](Assets/Scripts/Controls/DataTypes/SettingsCollection.cs): A ScriptableObject containing a serialized list of [Toggle](Assets/Scripts/Controls/DataTypes/ToggleSetting.cs), [Slider](Assets/Scripts/Controls/DataTypes/SliderSetting.cs), and/or [Dropdown](Assets/Scripts/Controls/DataTypes/DropdownSetting.cs) settings which are provided to the [`SettingsMenu`](Assets/Scripts/SettingsMenu.cs) as the data source.

## Scenes

- `Scenes/SettingsMenu`: PC (Mouse), Mobile (Touch)

## Attributions

- Bangers Font: https://fonts.google.com/specimen/Bangers