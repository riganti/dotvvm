namespace DotVVM.Framework.Controls.DynamicData.Builders
{
    public interface IFormBuilder
    {
        void BuildForm(DotvvmControl hostControl, DynamicDataContext dynamicDataContext);
    }
}