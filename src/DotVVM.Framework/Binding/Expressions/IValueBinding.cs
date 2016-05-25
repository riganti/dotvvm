namespace DotVVM.Framework.Binding.Expressions
{
    public interface IValueBinding: IStaticValueBinding
    {
        string GetKnockoutBindingExpression();
    }
}
