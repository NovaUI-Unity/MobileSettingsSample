using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovaSamples.SettingsMenu
{
    /// <summary>
    /// A common interface to be implemented by all settings-based data types
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// The label to indicate the end-user setting this will configure.
        /// </summary>
        string Label { get; set; }
    }

    [CreateAssetMenu(menuName = "Nova Samples/Settings Collection", fileName = "Settings Collection")]
    public class SettingsCollection : ScriptableObject
    {
        [Tooltip("The settings category the list of Settings represents.")]
        public string Category;
        
        [Tooltip("The list of settings for the given Category.")]
        [SerializeReference, TypeSelector]
        public List<ISetting> Settings = new List<ISetting>();
    }

    /// <summary>
    /// An attribute that, when paired with <see cref="SerializeReference"/>, will allow us to select the type of object to serialize.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TypeSelectorAttribute : PropertyAttribute { }
}
