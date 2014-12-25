using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework.ViewModel
{
    public class NonEvaluatingControlTreeWalker : ControlTreeWalker<object>
    {
        public NonEvaluatingControlTreeWalker(RedwoodControl root) : base(null, root)
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