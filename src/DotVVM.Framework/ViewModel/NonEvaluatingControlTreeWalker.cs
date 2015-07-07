using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.ViewModel
{
    public class NonEvaluatingControlTreeWalker : ControlTreeWalker<object>
    {
        public NonEvaluatingControlTreeWalker(DotvvmControl root) : base(null, root)
        {
        }

        protected override IExpressionEvaluator<object> CreateEvaluator(object viewModel)
        {
            return new NullEvaluator();
        }

        protected override bool ShouldProcessChildren(object viewModel)
        {
            return true;
        }
    }
}