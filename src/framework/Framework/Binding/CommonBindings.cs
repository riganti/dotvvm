using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Binding
{
    public class CommonBindings
    {
        public CommonBindings(BindingCompilationService service)
        {
            this.BindingCompilationService = service;
        }

        public BindingCompilationService BindingCompilationService { get; }
    }
}
