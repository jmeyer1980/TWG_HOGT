using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TinyWalnutGames.UITKTemplates.Tools
{
    /// <summary>
    /// Configuration for validating a UIDocument's required elements and custom logic.
    /// </summary>
    public class UIDocumentValidationConfig
    {
        /// <summary>
        /// Names of required elements (any type).
        /// </summary>
        public string[] RequiredElementNames { get; set; }

        /// <summary>
        /// Required element types (at least one of each type must exist).
        /// </summary>
        public Type[] RequiredElementTypes { get; set; }

        /// <summary>
        /// Required named elements with specific types (name -> type).
        /// </summary>
        public Dictionary<string, Type> NamedElements { get; set; }

        /// <summary>
        /// Optional custom validation logic.
        /// </summary>
        public Action<VisualElement> CustomValidation { get; set; }

        /// <summary>
        /// Optional name of the ProgressBar element.
        /// </summary>
        public string ProgressBarName { get; set; }
    }
}
