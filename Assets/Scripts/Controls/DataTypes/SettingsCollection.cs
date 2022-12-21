using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A common interface to be implemented by all settings-based data types
    /// </summary>
    [System.Serializable]
    public abstract class Setting
    {
        [Tooltip("The label to indicate the end-user setting the dropdown will configure.")]
        public string Label = null;
    }

    [CreateAssetMenu(menuName = "Nova Samples/Settings Collection", fileName = "Settings Collection")]
    public class SettingsCollection : ScriptableObject
    {
        [Tooltip("The settings category the list of Settings represents.")]
        public string Category;

        [Tooltip("The list of settings for the given Category.")]
        [SerializeReference, TypeSelector]
        public List<Setting> Settings = new List<Setting>();
    }

    /// <summary>
    /// An attribute that, when paired with <see cref="SerializeReference"/>, will allow us to select the type of object to serialize.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TypeSelectorAttribute : PropertyAttribute { }
}
