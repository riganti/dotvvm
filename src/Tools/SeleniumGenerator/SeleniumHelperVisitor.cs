using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumHelperVisitor : ResolvedControlTreeVisitor
    {
        private Stack<HelperDefinition> HelperDefinitionsStack { get; } = new Stack<HelperDefinition>();
        
        
        private static Dictionary<Type, ISeleniumGenerator> generators = new Dictionary<Type, ISeleniumGenerator>()
        {
            { typeof(TextBox), new TextBoxGenerator() },
            { typeof(CheckBox), new CheckBoxGenerator() },
            { typeof(Button), new ButtonGenerator() },
            { typeof(Literal), new LiteralGenerator() },
            { typeof(Repeater), new RepeaterGenerator() }
        };


        public void PushScope(HelperDefinition definition)
        {
            HelperDefinitionsStack.Push(definition);
        }

        public HelperDefinition PopScope()
        {
            return HelperDefinitionsStack.Pop();
        }


        public override void VisitControl(ResolvedControl control)
        {
            ISeleniumGenerator generator;
            if (generators.TryGetValue(control.Metadata.Type, out generator))
            {
                var helperDefinition = HelperDefinitionsStack.Peek();

                // generate the content
                var context = new SeleniumGeneratorContext()
                {
                    Control = control,
                    UsedNames = helperDefinition.UsedNames,
                    Visitor = this
                };

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