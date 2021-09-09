using System;

namespace DotVVM.Framework.Compilation
{
    public class BindingParserException : Exception
    {
        public Type DataContext { get; set; }
        public string BindingExpression { get; set; }
        public Type[] DataContextAncestors { get; set; }
        public Type ControlType { get; set; }

        public BindingParserException(Type dataContext, string bindingExpression, Type[] dataContextAncestors, Type controlType, Exception? innerException = null)
            : base($"Failed to parse binding '{ bindingExpression }' in the context of '{ dataContext.Name }' type.", innerException)
        {
            DataContext = dataContext;
            BindingExpression = bindingExpression;
            DataContextAncestors = dataContextAncestors;
            ControlType = controlType;

        }
    }
}
