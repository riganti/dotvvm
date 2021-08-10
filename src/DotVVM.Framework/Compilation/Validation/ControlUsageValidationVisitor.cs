using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.Validation
{
    public class ControlUsageValidationVisitor: ResolvedControlTreeVisitor
    {
        public List<ControlUsageError> Errors { get; set; } = new List<ControlUsageError>();
        readonly IControlUsageValidator validator;
        public ControlUsageValidationVisitor(IControlUsageValidator validator)
        {
            this.validator = validator;
        }

        public override void VisitControl(ResolvedControl control)
        {
            var err = validator.Validate(control);
            Errors.AddRange(err);
            foreach (var e in err)
                foreach (var node in e.Nodes)
                    node.AddError(e.ErrorMessage);
            base.VisitControl(control);
        }


        public void VisitAndAssert(ResolvedTreeRoot view)
        {
            if (this.Errors.Any()) throw new Exception("The ControlUsageValidationVisitor has already collected some errors.");
            VisitView(view);
            if (this.Errors.Any())
            {
                var controlUsageError = this.Errors.First();
                throw new DotvvmCompilationException(controlUsageError.ErrorMessage, controlUsageError.Nodes.SelectMany(n => n.Tokens));
            }
        }
    }
}
