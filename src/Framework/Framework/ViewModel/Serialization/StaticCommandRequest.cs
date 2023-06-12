using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class StaticCommandRequest
    {

        public StaticCommandInvocationPlan ExecutionPlan { get; }
        public IEnumerable<Func<Type, object>> ArgumentAccessors { get; }

        public StaticCommandRequest(StaticCommandInvocationPlan executionPlan, IEnumerable<Func<Type, object>> argumentAccessors)
        {
            ExecutionPlan = executionPlan;
            ArgumentAccessors = argumentAccessors;
        }
    }
}
