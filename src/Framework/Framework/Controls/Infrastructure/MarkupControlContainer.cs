using System;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls.Infrastructure
{
    /// <summary> Allows using markup controls from code controls or from server-side styles. Use like this <code>new MarkupControlContainer("cc:MyControl", c => c.SetValue(MyControl.NameProperty, "X"))</code> </summary>
    /// <seealso cref="MarkupControlContainer{TMarkupControl}"/>
    public class MarkupControlContainer: DotvvmControl
    {
        public string? MarkupVirtualPath { get; set; }
        public string? TagPrefix { get; set; }
        public string? TagName { get; set; }
        /// <summary> After OnInit is invoked, this property contains the initialized markup control. </summary>
        public DotvvmMarkupControl? CreatedControl { get; private set; }
        public Type ExpectedControlType { get; }
        /// <summary> Action which is called on newly created markup control to assign it its dotvvm properties. </summary>
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
            var path = GetMarkupPath(context.Configuration);
            var controlBuilderFactory = context.Services.GetRequiredService<IControlBuilderFactory>();
            var b = controlBuilderFactory.GetControlBuilder(path);
            var controlType = b.descriptor.ControlType;
            if (!typeof(DotvvmMarkupControl).IsAssignableFrom(controlType))
                throw new DotvvmControlException(this, $"'{path}' is not a markup control.");
            if (!ExpectedControlType.IsAssignableFrom(controlType))
                throw new DotvvmControlException(this, $"'{path}' is not of expected type {ExpectedControlType.Name}.");
            var control = (DotvvmMarkupControl)b.builder.Value.BuildControl(controlBuilderFactory, context.Services);
            // The control has a "unique" ID assigned by the builder, but it's unique inside the markup control, not the current page
            // Since MarkupControlContainer doesn't need the ID, we can assign the markup control the same ID
            control.SetValue(Internal.UniqueIDProperty, this.GetValue(Internal.UniqueIDProperty));
            SetProperties?.Invoke(control);

            CreatedControl = control;
            Children.Add(control);
        }

        internal string GetMarkupPath(DotvvmConfiguration config)
        {
            if (MarkupVirtualPath is null)
            {
                if (TagPrefix is null || TagName is null)
                    throw new DotvvmControlException(this, "TagPrefix and TagName must not be null when no MarkupVirtualPath is specified");
                var c = config.Markup.Controls.FirstOrDefault(c => c.TagPrefix == TagPrefix && c.TagName == TagName);
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

    /// <summary> Allows using markup controls from code controls or from server-side styles. Use like this <code>new <see cref="MarkupControlContainer{TMarkupControl}"/>("cc:MyControl", c => c.Name = "X")</code> </summary>
    public class MarkupControlContainer<TMarkupControl>: MarkupControlContainer
        where TMarkupControl: DotvvmMarkupControl
    {
        /// <summary> After OnInit is invoked, this property contains the initialized markup control. </summary>
        public new TMarkupControl? CreatedControl => (TMarkupControl?)base.CreatedControl;
        public MarkupControlContainer(string pathOrTagName, Action<TMarkupControl>? setProperties = null): base(pathOrTagName, setProperties is null ? null : c => setProperties((TMarkupControl)c), typeof(TMarkupControl)) { }

    }
}
