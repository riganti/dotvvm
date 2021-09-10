using System;
using System.Collections.Immutable;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Compilation
{
    public interface IControlBuilder
    {
        ControlBuilderDescriptor Descriptor { get; }
        DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory, IServiceProvider services);
    }

    public interface IAbstractControlBuilderDescriptor
    {
        ITypeDescriptor DataContextType { get; }
        ITypeDescriptor ControlType { get; }
        string? FileName { get; }
        IAbstractControlBuilderDescriptor? MasterPage { get; }
        ImmutableArray<(string name, string value)> Directives { get; }
    }

    [HandleAsImmutableObjectInDotvvmProperty]
    public class ControlBuilderDescriptor: IAbstractControlBuilderDescriptor
    {
        /// <summary>
        /// Gets required data context for the control
        /// </summary>
        public Type DataContextType { get; }
        /// <summary>
        /// Gets type of result control
        /// </summary>
        public Type ControlType { get; }

        /// <summary> File where was this page or control located </summary>
        public string? FileName { get; }

        public ControlBuilderDescriptor? MasterPage { get; }

        public ImmutableArray<(string name, string value)> Directives { get; }

        public ViewModuleReferenceInfo? ViewModuleReference { get; }

        ITypeDescriptor IAbstractControlBuilderDescriptor.DataContextType => new ResolvedTypeDescriptor(this.DataContextType);

        ITypeDescriptor IAbstractControlBuilderDescriptor.ControlType => new ResolvedTypeDescriptor(this.ControlType);

        IAbstractControlBuilderDescriptor? IAbstractControlBuilderDescriptor.MasterPage => this.MasterPage;

        public ControlBuilderDescriptor(
            Type dataContextType,
            Type controlType,
            string? fileName,
            ControlBuilderDescriptor? masterPage,
            ImmutableArray<(string name, string value)> directives,
            ViewModuleReferenceInfo? viewModuleReference
        )
        {
            this.DataContextType = dataContextType;
            this.ControlType = controlType;
            this.FileName = fileName;
            this.MasterPage = masterPage;
            this.Directives = directives;
            this.ViewModuleReference = viewModuleReference;
        }
    }
}
