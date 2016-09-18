using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class TextBoxGenerator : SeleniumGenerator<TextBox>
    {
        private static readonly DotvvmProperty[] nameProperties = new [] { TextBox.TextProperty, Validator.ValueProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;



        protected override void AddDeclarationsCore(HelperDefinition helper, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.TextBoxProxy";
            helper.Members.Add(GeneratePropertyForProxy(context, type));
            helper.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }

    }
}
