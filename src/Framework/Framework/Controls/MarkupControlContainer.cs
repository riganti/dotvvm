using System;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls
{
    public class MarkupControlContainer: DotvvmControl
    {
        public string? MarkupVirtualPath { get; set; }
        public string? TagPrefix { get; set; }
        public string? TagName { get; set; }
        public DotvvmMarkupControl? CreatedControl { get; private set; }
        public Type ExpectedControlType { get; }
        public Action<DotvvmMarkupControl>? SetProperties { get; set; }

        public MarkupControlContainer(string pathOrTagName, Action<DotvvmMarkupControl>? setProperties = null): this(pathOrTagName, setProperties, typeof(DotvvmMarkupControl)) { }

        internal MarkupControlContainer(string pathOrTagName, Action<DotvvmMarkupControl>? setProperties, Type expectedControlType)
        {
            ExpectedControlType = expectedControlType;
            SetProperties = setProperties;
            var tagSplit = pathOrTagName.Split(':');
            if (tagSplit.Length == 2 && !pathOrTagName.Contains(".") && !pathOrTagName.Contains("/"))
            {
                TagPrefix = tagSplit[0];
                TagName = tagSplit[1];
            }
            else
            {
                MarkupVirtualPath = pathOrTagName;
            }
        }

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var path = GetMarkupPath(context);
            var controlBuilderFactory = context.Services.GetRequiredService<IControlBuilderFactory>();
            var b = controlBuilderFactory.GetControlBuilder(path);
            var controlType = b.descriptor.ControlType;
            if (typeof(DotvvmMarkupControl).IsAssignableFrom(controlType))
                throw new DotvvmControlException(this, $"'{path}' is not a markup control.");
            if (ExpectedControlType.IsAssignableFrom(controlType))
                throw new DotvvmControlException(this, $"'{path}' is not of expected type {ExpectedControlType.Name}.");
            var control = (DotvvmMarkupControl)b.builder.Value.BuildControl(controlBuilderFactory, context.Services);

            CreatedControl = control;
            Children.Add(control);
        }

        internal string GetMarkupPath(IDotvvmRequestContext context)
        {
            if (MarkupVirtualPath is null)
            {
                if (TagPrefix is null || TagName is null)
                    throw new DotvvmControlException(this, "TagPrefix and TagName must not be null when no MarkupVirtualPath is specified");
                var c = context.Configuration.Markup.Controls.FirstOrDefault(c => c.TagPrefix == TagPrefix && c.TagName == TagName);
                return (c?.Src).NotNull($"Markup control <{TagPrefix}:{TagName}> is not registered.");
            }
            else
            {
                if (TagPrefix is object || TagName is object)
                    throw new DotvvmControlException(this, "TagPrefix and TagName must be null when MarkupVirtualPath is specified.");
                return MarkupVirtualPath;
            }
        }
    }

    public class MarkupControlContainer<TMarkupControl>: MarkupControlContainer
        where TMarkupControl: DotvvmMarkupControl
    {
        public new TMarkupControl? CreatedControl => (TMarkupControl?)base.CreatedControl;
        public MarkupControlContainer(string pathOrTagName, Action<TMarkupControl>? setProperties = null): base(pathOrTagName, setProperties is null ? null : c => setProperties((TMarkupControl)c), typeof(TMarkupControl)) { }

    }
}
