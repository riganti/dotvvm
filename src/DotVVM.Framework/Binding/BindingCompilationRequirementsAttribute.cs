using System;

namespace DotVVM.Framework.Binding
{
    public class BindingCompilationRequirementsAttribute : Attribute
    {
        public BindingCompilationRequirementType OriginalString { get; set; }
        public BindingCompilationRequirementType Javascript { get; set; }
        public BindingCompilationRequirementType Expression { get; set; }
        public BindingCompilationRequirementType Delegate { get; set; }
        public BindingCompilationRequirementType UpdateDelegate { get; set; }
        public BindingCompilationRequirementType RegisterInBindingRepository { get; set; }
        public BindingCompilationRequirementType ActionFilters { get; set; }
    }
}