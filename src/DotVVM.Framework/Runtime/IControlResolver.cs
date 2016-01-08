using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.ControlTree;

namespace DotVVM.Framework.Runtime
{
    public interface IControlResolver
    {

        /// <summary>
        /// Resolves the metadata for specified element.
        /// </summary>
        IControlResolverMetadata ResolveControl(string tagPrefix, string tagName, out object[] activationParameters);

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        IControlResolverMetadata ResolveControl(IControlType type);

        /// <summary>
        /// Builds the control metadata.
        /// </summary>
        IControlResolverMetadata BuildControlMetadata(IControlType type);

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        IControlResolverMetadata ResolveControl(ITypeDescriptor controlType);

        /// <summary>
        /// Resolves the binding type.
        /// </summary>
        BindingParserOptions ResolveBinding(string bindingType);

    }
}
