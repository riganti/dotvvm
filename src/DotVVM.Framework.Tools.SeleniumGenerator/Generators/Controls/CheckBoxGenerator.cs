using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class CheckBoxGenerator : SeleniumGenerator<CheckBox>
    {
        private static readonly DotvvmProperty[] nameProperties = new [] { CheckBox.CheckedProperty, CheckBox.CheckedItemsProperty, Validator.ValueProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;


        protected override IEnumerable<MemberDeclarationSyntax> GetDeclarationsCore(SeleniumGeneratorContext context)
        {
            yield return GeneratePropertyForProxy(context, "DotVVM.Framework.Testing.SeleniumHelpers.CheckBoxProxy");
        }

    }
}
