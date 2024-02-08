using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using FastExpressionCompiler;

namespace DotVVM.Framework.Compilation.Validation
{
    public class ControlUsageValidationVisitor: ResolvedControlTreeVisitor
    {
        public List<DotvvmCompilationDiagnostic> Errors { get; set; } = new();
        public bool WriteErrorsToNodes { get; set; } = true;
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
                var location = new DotvvmCompilationSourceLocation(control, e.Nodes.FirstOrDefault(), e.Nodes.SelectMany(n => n.Tokens));
                var msgPrefix = $"{control.Metadata.Type.ToCode(stripNamespace: true)} validation";
                if (location.LineNumber is {})
                {
                    msgPrefix += $" at line {location.LineNumber}";
                }
                Errors.Add(new DotvvmCompilationDiagnostic(
                    msgPrefix + ": " + e.ErrorMessage,
                    e.Severity,
                    location
                ) { Priority = -1 });
                if (this.WriteErrorsToNodes)
                {
                    foreach (var node in e.Nodes)
                    {
                        switch (e.Severity)
                        {
                            case DiagnosticSeverity.Error:
                                node.AddError(e.ErrorMessage);
                                break;
                            case DiagnosticSeverity.Warning:
                                node.AddWarning(e.ErrorMessage);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            base.VisitControl(control);
        }


        public void VisitAndAssert(ResolvedTreeRoot view)
        {
            if (this.Errors.Any()) throw new Exception("The ControlUsageValidationVisitor has already collected some errors.");
            VisitView(view);
            if (this.Errors.FirstOrDefault(e => e.Severity == DiagnosticSeverity.Error) is { } controlUsageError)
            {
                throw new DotvvmCompilationException(controlUsageError, this.Errors);
            }
        }
    }
}
