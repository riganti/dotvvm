using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumHelperVisitor : ResolvedControlTreeVisitor
    {
        public List<MemberDeclarationSyntax> ExportedDeclarations { get; } = new List<MemberDeclarationSyntax>();

        public HashSet<string> UsedNames { get; } = new HashSet<string>();


        private static Dictionary<Type, ISeleniumGenerator> generators = new Dictionary<Type, ISeleniumGenerator>()
        {
            { typeof(TextBox), new TextBoxGenerator() },
            { typeof(CheckBox), new CheckBoxGenerator() },
            { typeof(Button), new ButtonGenerator() }
        };


        public override void VisitControl(ResolvedControl control)
        {
            ISeleniumGenerator generator;
            if (generators.TryGetValue(control.Metadata.Type, out generator))
            {
                // generate the content
                var context = new SeleniumGeneratorContext()
                {
                    Control = control,
                    UsedNames = UsedNames
                };
                ExportedDeclarations.AddRange(generator.GetDeclarations(context));
            }
            else
            {
                base.VisitControl(control);
            }
        }
    }
}