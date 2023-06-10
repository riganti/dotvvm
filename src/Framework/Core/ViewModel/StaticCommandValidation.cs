namespace DotVVM.Framework.ViewModel
{
    public enum StaticCommandValidation
    {
        /// <summary>
        /// Default value - no validation should be performed.
        /// This setting is backwards compatible with DotVVM before 4.2
        /// </summary>
        None,

        /// <summary>
        /// Attach only manually added errors and clean any previous errors on target observables.
        /// Use the "StaticCommandModelState" class to add the errors.
        /// </summary>
        Manual,

        /// <summary>
        /// Validates all arguments - any child objects implementing IValidatableObject and checks validation attributes.
        /// It is also possible to manually add errors.
        /// </summary>
        Automatic
    }
}
