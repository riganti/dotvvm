using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime
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
        ControlResolverMetadata ResolveControl(ControlType type);

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        ControlResolverMetadata ResolveControl(Type controlType);


        /// <summary>
        /// Resolves the binding type.
        /// </summary>
        Type ResolveBinding(string bindingType);
    }
}
