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
        public List<(ResolvedControl control, ControlUsageError err)> Errors { get; set; } = new List<(ResolvedControl, ControlUsageError)>();
        readonly IControlUsageValidator validator;
        public ControlUsageValidationVisitor(IControlUsageValidator validator)
        {
            this.validator = validator;
        }

        public override void VisitControl(ResolvedControl control)
        {
            var err = validator.Validate(control);
            foreach (var e in err)
            {
                Errors.Add((control, e));
                foreach (var node in e.Nodes)
                    node.AddError(e.ErrorMessage);
            }
            base.VisitControl(control);
        }


        public void VisitAndAssert(ResolvedTreeRoot view)
        {
            if (this.Errors.Any()) throw new Exception("The ControlUsageValidationVisitor has already collected some errors.");
            VisitView(view);
            if (this.Errors.Any())
            {
                var controlUsageError = this.Errors.First();
                var lineNumber =
                    controlUsageError.control.GetAncestors()
                        .Select(c => c.DothtmlNode)
                        .FirstOrDefault(n => n != null)
                        ?.Tokens.FirstOrDefault()?.LineNumber;
                var message = $"Validation error in {controlUsageError.control.Metadata.Type.Name} at line {lineNumber}: {controlUsageError.err.ErrorMessage}";
                throw new DotvvmCompilationException(message, controlUsageError.err.Nodes.SelectMany(n => n.Tokens));
            }
        }
    }
}
