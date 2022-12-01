using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// Keeps track of all validation errors.
    /// </summary>
    public class ModelState
    {

        /// <summary>
        /// Gets the validation target path relative to the command target.
        /// </summary>
        internal string ValidationTargetPath { get; set; }

        /// <summary>
        /// Gets the object that was validated.
        /// </summary>
        public object? ValidationTarget { get; internal set; }


        /// <summary>
        /// Gets a collection of validation errors.
        /// </summary>
        public IReadOnlyList<ViewModelValidationError> Errors => ErrorsInternal.AsReadOnly();
        internal List<ViewModelValidationError> ErrorsInternal;

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
            ErrorsInternal = new List<ViewModelValidationError>();
            ValidationTargetPath = "/";
        }
    }
}
