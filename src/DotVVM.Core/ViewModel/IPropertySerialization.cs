using System.Reflection;

namespace DotVVM.Framework.ViewModel
{
    public interface IPropertySerialization
    {
        string ResolveName(PropertyInfo propertyInfo);
    }
}
