using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Runtime
{
    public interface IControlResolver
    {

        /// <summary>
        /// Resolves the type of a control.
        /// </summary>
        ControlResolverMetadata ResolveControl(string tagPrefix, string tagName, out object[] activationParameters);

        /// <summary>
        /// Resolves the binding type.
        /// </summary>
        Type ResolveBinding(string bindingType);
    }
}
