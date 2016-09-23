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
        public Stack<HelperDefinition> HelperDefinitionsStack { get; private set; } = new Stack<HelperDefinition>();

        public HashSet<string> UsedNames { get; } = new HashSet<string>();



        private static Dictionary<Type, ISeleniumGenerator> generators = new Dictionary<Type, ISeleniumGenerator>()
        {
            { typeof(TextBox), new TextBoxGenerator() },
            { typeof(CheckBox), new CheckBoxGenerator() },
            { typeof(Button), new ButtonGenerator() },
            { typeof(Literal), new LiteralGenerator() }
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

                var helperDefinition = HelperDefinitionsStack.Peek();
                if (generator.CanAddDeclarations(helperDefinition, context))
                {
                    generator.AddDeclarations(helperDefinition, context);
                    return;
                }
            }

            base.VisitControl(control);
        }
    }
}