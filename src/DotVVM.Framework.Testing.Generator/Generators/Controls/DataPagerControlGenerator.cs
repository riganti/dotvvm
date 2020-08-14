using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class DataPagerControlGenerator : SeleniumGenerator<DataPager>
    {
        private static readonly DotvvmProperty[] nameProperties = { DataPager.DataSetProperty };
        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "DataPagerProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
