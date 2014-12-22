using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Runtime
{
    public interface IControlResolver
    {

        /// <summary>
        /// Resolves the metadata for specified element.
        /// </summary>
        ControlResolverMetadata ResolveControl(string tagPrefix, string tagName, out object[] activationParameters);

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        ControlResolverMetadata ResolveControl(Type type, Type controlBuilderType = null);

        /// <summary>
        /// Resolves the binding type.
        /// </summary>
        Type ResolveBinding(string bindingType);
    }
}
