namespace DotVVM.Framework.Controls.DynamicData.Builders
{
    public interface IFormBuilder
    {
        DotvvmControl BuildForm(DynamicDataContext dynamicDataContext, DynamicEntityBase.FieldProps fieldProps);
    }
}
