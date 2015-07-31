using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public interface IValueBinding: IStaticValueBinding
    {
        string GetKnockoutBindingExpression();
    }
}
