#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A custom property drawer for the <see cref="TypeSelectorAttribute"/>, which will allow us to select the type of object to serialized for a given <see cref="SerializeReference"/> field.
    /// </summary>
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    public class TypeSelectorPropertyDrawer : PropertyDrawer
    {
        private const string Unassigned = "None (Unassigned)";

        /// <summary>
        /// A <see cref="SerializedProperty"/> for a managed reference object, and the system Type to create for that managed reference.
        /// </summary>
        private struct TypedProperty
        {
            public Type Type;
            public SerializedProperty Property;
        }

        /// <summary>
        /// Draw the type-selector dropdown for the given <paramref name="property"/>.
        /// </summary>
        /// <param name="position">The position the property's controls will be drawn.</param>
        /// <param name="property">The serialized property backing this custom UI control.</param>
        /// <param name="label">The property label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TypeSelectorDropdown(property, position);
            EditorGUI.PropertyField(position, property, label, true);
        }

        /// <summary>
        /// Get the height of the expanded property.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        /// <summary>
        /// Create a dropdown button to select a object type to create for the <paramref name="property"/>'s <see cref="SerializeReference"/> field.
        /// </summary>
        /// <param name="property">The property for this dropdown control</param>
        /// <param name="position">The size/position to draw the property and this dropdown control.</param>
        public static void TypeSelectorDropdown(SerializedProperty property, Rect position)
        {
            Rect typeSelectionField = position;

            // Figure out how wide to make the dropdown control
            float propertyLabelWidth = EditorStyles.label.CalcSize(new GUIContent(property.displayName)).x;
            float labelWidth = Mathf.Max(propertyLabelWidth, EditorGUIUtility.labelWidth);

            // Adjust the size/position so that the control will be drawn
            // to the right of the property label
            typeSelectionField.x += labelWidth;
            typeSelectionField.width = position.width - labelWidth;
            typeSelectionField.height = EditorGUIUtility.singleLineHeight;

            // Convert the currently selected full type name into its class name and its assembly name
            (string assemblyName, string className) = GetSplitNamesFromTypename(property.managedReferenceFullTypename);

            bool assigned = !string.IsNullOrWhiteSpace(className);
            className = !assigned ? Unassigned : className;

            string label = className.Substring(className.LastIndexOf(".") + 1);
            string tooltip = className + "  ( " + assemblyName + " )";

            // Show the type-selector dropdown button and listen for click events
            bool showContextMenu = GUI.Button(typeSelectionField, new GUIContent(label, tooltip), EditorStyles.popup);

            if (showContextMenu)
            {
                if (GetContextMenu(property, out GenericMenu context))
                {
                    // Display the list of derived types to choose from for the given property
                    context.ShowAsContext();
                }
            }
        }

        /// <summary>
        /// Populated a <see cref="GenericMenu"/> with a list of discovered Types, which inherit from or implement the managed reference Type on the given <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property holding the managed reference base Type, which acts as a filter for the types the context menu will display.</param>
        /// <param name="context">The menu to create and populate with disovered object Types.</param>
        /// <returns>True if a menu was successfull created and populated, otherwise false.</returns>
        private static bool GetContextMenu(SerializedProperty property, out GenericMenu context)
        {
            context = new GenericMenu();

            // Add a default "unassigned" option to the context menu.
            context.AddItem(new GUIContent(Unassigned), string.IsNullOrWhiteSpace(property.managedReferenceFullTypename), ClearType, property);

            Type serializeReferenceType = GetSerializeReferenceType(property.managedReferenceFieldTypename);

            if (serializeReferenceType == null)
            {
                Debug.LogError($"SerializeReference type, [{property.managedReferenceFieldTypename}], not found.");
                return false;
            }

            // Grab the derived types from the TypeCache
            TypeCache.TypeCollection derivedTypes = TypeCache.GetTypesDerivedFrom(serializeReferenceType);

            // Add the discovered types to the menu
            for (int i = 0; i < derivedTypes.Count; ++i)
            {
                AddTypeToMenu(context, derivedTypes[i], property);
            }

            return true;
        }

        /// <summary>
        /// Null-out the currently assigned managed reference value and apply the changes to the <paramref name="propertyToClear"/>.
        /// </summary>
        /// <param name="propertyToClear">The <see cref="SerializedProperty"/> to clear out.</param>
        private static void ClearType(object propertyToClear)
        {
            SerializedProperty property = propertyToClear as SerializedProperty;
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Insert the given <paramref name="type"/> into the <paramref name="context"/> menu.
        /// </summary>
        /// <param name="context">The context menu to add the given type to.</param>
        /// <param name="type">The type to add to the context menu.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> the context menu is for.</param>
        /// <param name="useFullAssemblyName">Should the full assembly name be used when displaying this specific type?</param>
        private static void AddTypeToMenu(GenericMenu context, Type type, SerializedProperty property)
        {
            TypedProperty typedProperty = new TypedProperty() { Type = type, Property = property };
            bool isCurrentlySelectedType = type == GetSerializeReferenceType(property.managedReferenceFullTypename);

            context.AddItem(new GUIContent(type.Name), isCurrentlySelectedType, CreateInstanceOfType, typedProperty);
        }

        /// <summary>
        /// Create an instance of the provided <see cref="TypedProperty"/> (boxed as an object), and apply the changes to its <see cref="TypedProperty.Property"/>.
        /// </summary>
        /// <param name="typeAsObject">A<see cref="TypedProperty"/> boxed as an object.</param>
        private static void CreateInstanceOfType(object typeAsObject)
        {
            TypedProperty propertyType = (TypedProperty)typeAsObject;
            propertyType.Property.managedReferenceValue = Activator.CreateInstance(propertyType.Type);
            propertyType.Property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Get the object Type from the given type-string.
        /// </summary>
        /// <param name="typename">The managed reference full type name -- <see cref="SerializedProperty.managedReferenceFullTypename"/>.</param>
        /// <returns>The object Type matching the type-string.</returns>
        private static Type GetSerializeReferenceType(string stringType)
        {
            (string assemblyName, string className) = GetSplitNamesFromTypename(stringType);

            return Type.GetType($"{className}, {assemblyName}");
        }

        /// <summary>
        /// Split the given managed reference type name into its class name and its assembly name
        /// </summary>
        /// <param name="typename">The managed reference full type name -- <see cref="SerializedProperty.managedReferenceFullTypename"/>.</param>
        /// <returns>The name split into the type's assembly name and class name, respectively.</returns>
        private static (string AssemblyName, string ClassName) GetSplitNamesFromTypename(string typename)
        {
            if (string.IsNullOrEmpty(typename))
            {
                return (string.Empty, string.Empty);
            }

            string[] typeSplitString = typename.Split(char.Parse(" "));

            return (typeSplitString[0], typeSplitString[1]);
        }
    }
}
#endif
