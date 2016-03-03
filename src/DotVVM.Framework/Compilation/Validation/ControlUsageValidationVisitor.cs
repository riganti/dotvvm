using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Compilation.Validation
{
    public class ControlUsageValidationVisitor: ResolvedControlTreeVisitor
    {
        public List<ControlUsageError> Errors { get; set; } = new List<ControlUsageError>();
        IControlUsageValidator validator;
        public ControlUsageValidationVisitor(IControlUsageValidator validator)
        {
            this.validator = validator;
        }
        public ControlUsageValidationVisitor(DotvvmConfiguration config) : this(config.ServiceLocator.GetService<IControlUsageValidator>()) { }

        public override void VisitControl(ResolvedControl control)
        {
            var err = validator.Validate(control);
            Errors.AddRange(err);
            base.VisitControl(control);
        }
    }
}
