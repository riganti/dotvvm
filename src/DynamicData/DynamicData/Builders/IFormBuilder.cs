namespace DotVVM.Framework.Controls.DynamicData.Builders
{
    public interface IFormBuilder
    {
        DotvvmControl BuildForm(DynamicDataContext dynamicDataContext, DynamicEntity.FieldProps fieldProps);
    }
}
