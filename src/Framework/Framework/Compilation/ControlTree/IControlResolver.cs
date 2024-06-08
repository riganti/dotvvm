using System.Collections.Generic;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IControlResolver
    {

        /// <summary>
        /// Resolves the metadata for specified element.
        /// </summary>
        IControlResolverMetadata? ResolveControl(string? tagPrefix, string tagName, out object[]? activationParameters);

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

        /// <summary> Returns a list of possible DotVVM controls. </summary>
        /// <remark>Used only for smart error handling, the list isn't necessarily complete, but doesn't contain false positives.</remark>
        IEnumerable<(string tagPrefix, string? tagName, IControlType type)> EnumerateControlTypes();

        /// <summary>
        /// Resolves the binding type.
        /// </summary>
        BindingParserOptions? ResolveBinding(string bindingType);

        /// <summary>
        /// Finds the property in the control metadata.
        /// </summary>
        IPropertyDescriptor? FindProperty(IControlResolverMetadata controlMetadata, string name, MappingMode requiredMode);
    }
}
