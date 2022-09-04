using System.Collections.Generic;
using System.Linq;
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
        /// Gets a collection of validation errors for arguments (used for static commands)
        /// </summary>
        public IReadOnlyList<StaticCommandArgumentValidationError> ArgumentErrors => ArgumentErrorsInternal.AsReadOnly();
        internal List<StaticCommandArgumentValidationError> ArgumentErrorsInternal;

        /// <summary>
        /// Gets a value indicating whether the ModelState is valid (i.e. does not contain any errors)
        /// </summary>
        public bool IsValid
        {
            get { return !Errors.Any() && !ArgumentErrors.Any(); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelState"/> class.
        /// </summary>
        public ModelState()
        {
            ErrorsInternal = new List<ViewModelValidationError>();
            ArgumentErrorsInternal = new List<StaticCommandArgumentValidationError>();
            ValidationTargetPath = "/";
        }
    }
}
