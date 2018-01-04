using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Owin;

namespace DotVVM.Compiler
{
    internal class CompilerAppBuilder : IAppBuilder
    {
        internal object[] args;


        public IAppBuilder Use(object middleware, params object[] args)
        {
            if (middleware.GetType().Name == "DotvvmMiddleware")
            {
                this.args = args;
            }
            return this;
        }

        public object Build(Type returnType)
        {
            return null;
        }

        public IAppBuilder New()
        {
            return new CompilerAppBuilder();
        }

        public IDictionary<string, object> Properties { get; } = new ConcurrentDictionary<string, object>();
    }
}
