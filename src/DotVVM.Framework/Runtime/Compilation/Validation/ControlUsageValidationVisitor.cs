using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.ControlTree.Resolved;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Validation
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
