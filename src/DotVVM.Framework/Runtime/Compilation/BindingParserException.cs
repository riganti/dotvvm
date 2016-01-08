using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class BindingParserException : Exception
    {
        public Type DataContext { get; set; }
        public string BidningExpression { get; set; }
        public Type[] DataContextAncestors { get; set; }
        public Type ControlType { get; set; }

        public BindingParserException(Type dataContext, string bindingExpression, Type[] dataContextAncestors, Type controlType, Exception innerException = null)
            : base($"Failed to parse binding '{ bindingExpression }' in the context of '{ dataContext.Name }' type.", innerException)
        {
            DataContext = dataContext;
            BidningExpression = bindingExpression;
            DataContextAncestors = dataContextAncestors;
            ControlType = controlType;

        }
    }
}
