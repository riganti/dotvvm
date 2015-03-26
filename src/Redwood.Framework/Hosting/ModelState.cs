using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Validation;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// Keeps track of all validation errors.
    /// </summary>
    public class ModelState
    {
        /// <summary>
        /// Gets the validation target path relative to the command target.
        /// </summary>
        public string[] ValidationTargetPath { get; internal set; }

        /// <summary>
        /// Gets the command data context path
        /// </summary>
        public string[] DataContextPath { get; internal set; }


        /// <summary>
        /// Gets a collection of validation errors.
        /// </summary>
        public List<ViewModelValidationError> Errors { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ValidationTarget"/> is valid or not.
        /// </summary>
        public bool IsValid
        {
            get { return !Errors.Any(); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelState"/> class.
        /// </summary>
        public ModelState()
        {
            Errors = new List<ViewModelValidationError>();
            ValidationTargetPath = new string[0];
        }

    }
}