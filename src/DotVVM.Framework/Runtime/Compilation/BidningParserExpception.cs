using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class BidningParserExpception : Exception
    {
        public Type DataContext { get; set; }
        public string BidningExpression { get; set; }
        public Type[] DataContextAncestors { get; set; }
        public Type ControlType { get; set; }

        public BidningParserExpception(Type dataContext, string bindingExpression, Type[] dataContextAncestors, Type controlType, Exception innerException = null)
            : base($"failed to parse binding '{ bindingExpression }' at context { dataContext.Name }", innerException)
        {
            DataContext = dataContext;
            BidningExpression = bindingExpression;
            DataContextAncestors = dataContextAncestors;
            ControlType = controlType;

        }
    }
}
