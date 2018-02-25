using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using DotVVM.Framework;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using System.Globalization;
using System.IO;
using System.Threading;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Runtime.Tracing;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Blazor
{
    public class DotvvmBlazorBridgedControl : IComponent
    {
        private RenderHandle renderHandle;
        private DotvvmControl control;
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            IDotvvmRequestContext context = null; //new BlazorDotvvmRequestContext();
            var writer = new BlazorHtmlWriter(builder, context);
            // control.OnPreRender(context);
            this.control.Render(writer, context);
        }
        public virtual void Init(RenderHandle handle)
        {
            this.renderHandle = handle;
        }
        public virtual void SetParameters(ParameterCollection parameters)
        {
            var p = parameters.ToDictionary();
            var dirty = false;

            if (this.control == null)
            {
                this.control = ((DotvvmControlInitializer)p[nameof(DotvvmControlInitializer)]).Invoke();
                dirty = true;
            }

            if (p.TryGetValue(nameof(DotvvmControlPropertyAssigner), out var assigner))
                dirty |= ((DotvvmControlPropertyAssigner)assigner).Invoke(this.control);

            if (dirty)
                this.renderHandle.Render(BuildRenderTree);
        }
    }

    /// takes some data from it's closure, assigns parameters and return if something has changed.
    public delegate bool DotvvmControlPropertyAssigner(DotvvmBindableObject control);
    public delegate DotvvmControl DotvvmControlInitializer();

    public class BlazorDotvvmRequestContext : IDotvvmRequestContext
    {
        public IHttpContext HttpContext { get; set; }
        public string CsrfToken { get; set; }
        public JObject ReceivedViewModelJson { get { return null; } set {} }
        public object ViewModel { get; set; }
        public JObject ViewModelJson { get => null; set {} }
        public DotvvmConfiguration Configuration { get; set; }
        public IDotvvmPresenter Presenter { get; set; }
        public RouteBase Route { get; set; }
        public bool IsPostBack { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public ResourceManager ResourceManager { get; set; }
        public ModelState ModelState { get; set; }
        public IQueryCollection Query { get; set; }
        public bool IsCommandExceptionHandled { get; set; }
        public bool IsPageExceptionHandled { get; set; }
        public Exception CommandException { get; set; }
        public bool IsSpaRequest { get; set; }
        public bool IsInPartialRenderingMode { get; set; }
        public string ApplicationHostPath { get; set; }
        public string ResultIdFragment { get; set; }
        public DotvvmView View { get => null; set {} }

        private IServiceProvider _services;
        public IServiceProvider Services
        {
            get => _services ?? Configuration?.ServiceProvider ?? throw new NotSupportedException();
            set => _services = value;
        }
    }

    public class BlazorHtmlWriter : IHtmlWriter
    {
        private readonly RenderTreeBuilder builder;
        public BlazorHtmlWriter(RenderTreeBuilder builder, IDotvvmRequestContext requestContext = null)
        {
            this.builder = builder;
            this.requestContext = requestContext;
        }

        private readonly IDotvvmRequestContext requestContext;
        private readonly Dictionary<HtmlTagAttributePair, HtmlAttributeTransformConfiguration> htmlAttributeTransforms;

        private readonly OrderedDictionary attributes = new OrderedDictionary();
        private readonly OrderedDictionary dataBindAttributes = new OrderedDictionary();

        private int fakeSeq = 0;

        private int NextSeq(RenderTreeFrameType fragmentType)
        {
            fakeSeq += 5;
            return fakeSeq - (int)fragmentType;
        }

        public void AddAttribute(string name, string value, bool append = false, string appendSeparator = null)
        {
            if (append)
            {
                if (attributes.Contains(name))
                {
                    var currentValue = attributes[name] as string;
                    attributes[name] = HtmlWriter.JoinAttributeValues(name, currentValue, value, appendSeparator);
                    return;
                }
            }

            // set the value
            attributes[name] = value;
        }

        public void AddStyleAttribute(string name, string value)
        {
            AddAttribute("style", name + ":" + value, true, ";");
        }

        public void AddKnockoutDataBind(string name, string expression)
        {
            if (dataBindAttributes.Contains(name) && dataBindAttributes[name] is KnockoutBindingGroup)
            {
                throw new InvalidOperationException($"The binding handler '{name}' already contains a KnockoutBindingGroup. The expression could not be added. Please call AddKnockoutDataBind(string, KnockoutBindingGroup) overload!");
            }

            dataBindAttributes.Add(name, expression);
        }

        public void AddKnockoutDataBind(string name, KnockoutBindingGroup bindingGroup)
        {
            if (dataBindAttributes.Contains(name) && !(dataBindAttributes[name] is KnockoutBindingGroup))
            {
                throw new InvalidOperationException($"The value of binding handler '{name}' cannot be combined with a KnockoutBindingGroup!");
            }

            if (dataBindAttributes.Contains(name))
            {
                var currentGroup = (KnockoutBindingGroup)dataBindAttributes[name];
                currentGroup.AddFrom(bindingGroup);
            }
            else
            {
                dataBindAttributes[name] = bindingGroup;
            }
        }

        public void RenderBeginTag(string name)
        {
            builder.OpenElement(NextSeq(RenderTreeFrameType.Element), name);
            SpitOutAttributes(elementName: name);
        }

        public void RenderSelfClosingTag(string name)
        {
            RenderBeginTag(name); RenderEndTag();
        }

        private void SpitOutAttributes(string elementName)
        {
            foreach (DictionaryEntry attr in dataBindAttributes)
            {
                AddAttribute("data-bind", attr.Key + ": " + ConvertHtmlAttributeValue(attr.Value), true, ", ");
            }
            dataBindAttributes.Clear();

            foreach (DictionaryEntry attr in attributes)
            {
                var attributeName = (string)attr.Key;
                var attributeValue = ConvertHtmlAttributeValue(attr.Value);

                // allow to use the attribute transformer
                var pair = new HtmlTagAttributePair() { TagName = elementName, AttributeName = attributeName };
                if (htmlAttributeTransforms != null && htmlAttributeTransforms.TryGetValue(pair, out var transformConfiguration))
                {
                    // use the transformer
                    var transformer = transformConfiguration.GetInstance();
                    transformer.RenderHtmlAttribute(this, requestContext, attributeName, attributeValue);
                }
                else
                {
                    WriteHtmlAttribute(attributeName, attributeValue);
                }
            }

            attributes.Clear();
        }

        private string ConvertHtmlAttributeValue(object value)
        {
            if (value is KnockoutBindingGroup)
            {
                return value.ToString();
            }

            return (string) value;
        }

        public void WriteHtmlAttribute(string attributeName, string attributeValue)
        {
            builder.AddAttribute(NextSeq(RenderTreeFrameType.Attribute), attributeName, attributeValue);
        }

        public void RenderEndTag()
        {
            builder.CloseElement();
        }

        public void WriteText(string text)
        {
            builder.AddContent(NextSeq(RenderTreeFrameType.Text), text);
        }

        public void WriteUnencodedText(string text)
        {
            if (!text.Contains("<"))
            {
                WriteText(WebUtility.HtmlDecode(text));
            }
            else
            {
                throw new NotSupportedException($"Sry, can't write unencoded text (with '<', ...) in blazor - \'{text}\'");
            }
        }
    }
}
