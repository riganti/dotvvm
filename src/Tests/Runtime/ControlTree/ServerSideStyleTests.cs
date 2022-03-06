using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class ServerSideStyleTests
    {
        DotvvmConfiguration config = DotvvmTestHelper.CreateConfiguration();
        public ServerSideStyleTests()
        {
            config.Styles
                .Register<Button>(m => m.HasHtmlAttribute("data-dangerous"))
                .AppendControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler("Are you sure?"));

            config.Styles
                .Register<Button>(m => m.HasHtmlAttribute("data-very-dangerous"))
                .AddCondition(x => x.HasHtmlAttribute("data-very-dangerous"))
                .AppendControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler("Are you really really sure?"));

            config.Styles
                .Register<LinkButton>(m => m.HasHtmlAttribute("data-manyhandlers"))
                .AppendControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler("1"))
                .AppendControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler("2"))
                .AppendControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler("3"))
                .AppendControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler("4"));
        }

        ResolvedTreeRoot Parse(string markup, string fileName = "default.dothtml") =>
            DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Collections.Generic.List<System.String>\n" + markup, fileName, config);


        [TestMethod]
        public void SetControlProperty_AddPostbackHandler()
        {
            var button = Parse(@"<dot:Button data-dangerous />")
                         .Content.SelectRecursively(c => c.Content)
                         .Single(c => c.Metadata.Type == typeof(Button));
            var handler = button.Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls.Single();
            Assert.AreEqual(typeof(ConfirmPostBackHandler), handler.Metadata.Type);
            var message = handler.Properties[ConfirmPostBackHandler.MessageProperty].CastTo<ResolvedPropertyValue>().Value.CastTo<string>();
            Assert.AreEqual("Are you sure?", message);
        }

        [TestMethod]
        public void SetControlProperty_StylesExclude()
        {
            var button = Parse(@"<dot:Button data-dangerous Styles.Exclude />")
                         .Content.SelectRecursively(c => c.Content)
                         .Single(c => c.Metadata.Type == typeof(Button));
            Assert.IsFalse(button.Properties.ContainsKey(PostBack.HandlersProperty));
        }

        [TestMethod]
        public void SetControlProperty_StylesExcludeAllButtons()
        {
            config.Styles.Register<HtmlGenericControl>(m => m.HasClass("aaa"))
                .SetProperty(c => c.Visible, false);
            config.Styles.Register<Button>()
                .SetDotvvmProperty(Styles.ExcludeProperty, true);

            var controls = Parse(@"<dot:LinkButton class=aaa /> <dot:Button class=aaa />")
                           .Content.SelectRecursively(c => c.Content).ToArray();
            var button = controls.Single(c => c.Metadata.Type == typeof(Button));
            var linkButton = controls.Single(c => c.Metadata.Type == typeof(LinkButton));
            Assert.IsFalse(button.Properties.ContainsKey(HtmlGenericControl.VisibleProperty));
            Assert.IsTrue(linkButton.Properties.ContainsKey(HtmlGenericControl.VisibleProperty));
        }



        [TestMethod]
        public void SetControlProperty_AppendPostbackHandler()
        {
            var button = Parse(
@"<dot:Button data-dangerous>
    <PostBack.Handlers>
        <dot:ConfirmPostBackHandler Message='This is dangerous!' />
    </PostBack.Handlers>
</dot:Button>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Button));
            var handlers = button.Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls;
            Assert.AreEqual(2, handlers.Count);
            var oldHandler = handlers[0];
            var appendedHandler = handlers[1];
            Assert.AreEqual(typeof(ConfirmPostBackHandler), appendedHandler.Metadata.Type);
            var message1 = oldHandler.Properties[ConfirmPostBackHandler.MessageProperty].CastTo<ResolvedPropertyValue>().Value.CastTo<string>();
            var message2 = appendedHandler.Properties[ConfirmPostBackHandler.MessageProperty].CastTo<ResolvedPropertyValue>().Value.CastTo<string>();
            Assert.AreEqual("This is dangerous!", message1);
            Assert.AreEqual("Are you sure?", message2);
        }

        [TestMethod]
        public void SetControlProperty_AppendPostbackHandlerParametrized()
        {
            config.Styles.Register<Button>(c => c.HasHtmlAttribute("data-confirm-msg"))
                .AppendControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler(), s =>
                    s.SetProperty(c => c.Message, c => c.Parent.GetHtmlAttribute("data-confirm-msg")));
            var button = Parse(
@"<dot:Button data-confirm-msg='The message'>
</dot:Button>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Button));
            var handlers = button.Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls;
            Assert.AreEqual(1, handlers.Count);
            Assert.AreEqual(typeof(ConfirmPostBackHandler), handlers[0].Metadata.Type);
            var message = handlers[0].Properties[ConfirmPostBackHandler.MessageProperty].CastTo<ResolvedPropertyValue>().Value.CastTo<string>();
            Assert.AreEqual("The message", message);
        }

        [TestMethod]
        public void SetControlProperty_AppendPostbackHandlerParametrized2()
        {
            config.Styles.Register<Button>(c => c.HasHtmlAttribute("data-confirm-msg2"))
                .SetDotvvmProperty(PostBack.HandlersProperty, c =>
                    new ConfirmPostBackHandler("Are you sure to do: " + c.Property(b => b.Text)));
            var button = Parse(
@"<dot:Button data-confirm-msg2 Text='Delete biscuits'>
</dot:Button>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Button));
            var handlers = button.Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls;
            Assert.AreEqual(1, handlers.Count);
            Assert.AreEqual(typeof(ConfirmPostBackHandler), handlers[0].Metadata.Type);
            var message = handlers[0].Properties[ConfirmPostBackHandler.MessageProperty].CastTo<ResolvedPropertyValue>().Value.CastTo<string>();
            Assert.AreEqual("Are you sure to do: Delete biscuits", message);
        }

        [TestMethod]
        public void SetControlProperty_RepeaterTemplate()
        {
            config.Styles
                .Register<Repeater>()
                .SetHtmlControlProperty(Repeater.SeparatorTemplateProperty, "hr", options: StyleOverrideOptions.Ignore);

            var repeater = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator = repeater.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(HtmlGenericControl), separator.Metadata.Type);
            Assert.AreEqual("hr", separator.ConstructorParameters.Single());
            var repeater2 = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    <SeparatorTemplate>XXX</SeparatorTemplate>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator2 = repeater2.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(RawLiteral), separator2.Metadata.Type);
        }

        [TestMethod]
        public void SetControlProperty_RepeaterTemplate2()
        {
            config.Styles
                .Register<Repeater>()
                .SetControlProperty(r => r.SeparatorTemplate, new HtmlGenericControl("hr"), options: StyleOverrideOptions.Ignore);

            var repeater = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator = repeater.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(HtmlGenericControl), separator.Metadata.Type);
            Assert.AreEqual("hr", separator.ConstructorParameters.Single());
            var repeater2 = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    <SeparatorTemplate>XXX</SeparatorTemplate>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator2 = repeater2.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(RawLiteral), separator2.Metadata.Type);
        }

        [TestMethod]
        public void AppendControlProperty_RepeaterTemplate()
        {
            config.Styles
                .Register<Repeater>()
                .AppendControlProperty(r => r.SeparatorTemplate, new HtmlGenericControl("hr"));

            var repeater = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator = repeater.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(HtmlGenericControl), separator.Metadata.Type);
            Assert.AreEqual("hr", separator.ConstructorParameters.Single());
            var repeater2 = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    <SeparatorTemplate>XXX</SeparatorTemplate>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator2 = repeater2.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.ToArray();
            Assert.AreEqual(2, separator2.Length);
            Assert.AreEqual(typeof(RawLiteral), separator2[0].Metadata.Type);
            Assert.AreEqual(typeof(HtmlGenericControl), separator2[1].Metadata.Type);
        }

        [TestMethod]
        public void PrependControlProperty_RepeaterTemplate()
        {
            config.Styles
                .Register<Repeater>()
                .SetControlProperty(r => r.SeparatorTemplate, new HtmlGenericControl("hr"), options: StyleOverrideOptions.Prepend);

            var repeater = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator = repeater.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(HtmlGenericControl), separator.Metadata.Type);
            Assert.AreEqual("hr", separator.ConstructorParameters.Single());
            var repeater2 = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    <SeparatorTemplate>XXX</SeparatorTemplate>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator2 = repeater2.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.ToArray();
            Assert.AreEqual(2, separator2.Length);
            Assert.AreEqual(typeof(HtmlGenericControl), separator2[0].Metadata.Type);
            Assert.AreEqual(typeof(RawLiteral), separator2[1].Metadata.Type);
        }

        [TestMethod]
        public void SetControlProperty_IgnoredTemplate()
        {
            var repeater = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    <SeparatorTemplate><div class='sep'/></SeparatorTemplate>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator = repeater.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(HtmlGenericControl), separator.Metadata.Type);
            Assert.AreEqual("div", separator.ConstructorParameters.Single());
        }

        [TestMethod]
        public void SetControlProperty_TwoAppendedStyles()
        {
                        var button = Parse(
@"<dot:Button data-dangerous data-very-dangerous>
    <PostBack.Handlers>
        <dot:ConfirmPostBackHandler Message='This is dangerous!' />
    </PostBack.Handlers>
</dot:Button>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Button));
            var handlers = button.Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls;
            Assert.AreEqual(3, handlers.Count);
            var messages = handlers.Select(h => h.Properties[ConfirmPostBackHandler.MessageProperty].CastTo<ResolvedPropertyValue>().Value.CastTo<string>()).ToArray();
            Assert.AreEqual("This is dangerous!", messages[0]);
            Assert.AreEqual("Are you sure?", messages[1]);
            Assert.AreEqual("Are you really really sure?", messages[2]);
        }

        [TestMethod]
        public void SetControlProperty_MoreAppendedStyles()
        {
            var button = Parse(
@"<dot:LinkButton data-manyhandlers />")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(LinkButton));
            var handlers = button.Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls;
            Assert.AreEqual(4, handlers.Count);
            var messages = handlers.Select(h => h.Properties[ConfirmPostBackHandler.MessageProperty].CastTo<ResolvedPropertyValue>().Value.CastTo<string>()).ToArray();
            Assert.AreEqual("1", messages[0]);
            Assert.AreEqual("2", messages[1]);
            Assert.AreEqual("3", messages[2]);
            Assert.AreEqual("4", messages[3]);
        }

        [TestMethod]
        public void AddRequiredResourceControl()
        {
            config.Styles.RegisterRoot()
                .PrependContent(new RequiredResource("my-resource"));
            var rr = Parse("<div my-attr> <span> some text </span> <br> </div>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(RequiredResource));
            var resourceName = rr.Properties[RequiredResource.NameProperty].CastTo<ResolvedPropertyValue>().Value;
            Assert.AreEqual("my-resource", resourceName);
        }

        [TestMethod]
        public void AddParametrizedResourceControl()
        {
            config.Styles.Register<HtmlGenericControl>(m => m.HasHtmlAttribute("data-requireresource"))
                .PrependContent(new RequiredResource(), s =>
                    s.SetDotvvmProperty(RequiredResource.NameProperty,
                        c => c.Parent.GetHtmlAttribute("data-requireresource"))
                );
            var rr = Parse("<div my-attr> <span data-requireresource=resourceX> some text </span> <br data-requireresource=resourceY> </div>")
                .Content.SelectRecursively(c => c.Content)
                .Where(c => c.Metadata.Type == typeof(RequiredResource))
                .ToArray();
            Assert.AreEqual(2, rr.Length);
            var resourceName = rr[0].Properties[RequiredResource.NameProperty].CastTo<ResolvedPropertyValue>().Value;
            Assert.AreEqual("resourceX", resourceName);
            resourceName = rr[1].Properties[RequiredResource.NameProperty].CastTo<ResolvedPropertyValue>().Value;
            Assert.AreEqual("resourceY", resourceName);
        }

        [TestMethod]
        public void SetButtonDefaultContent()
        {
            config.Styles.Register<Button>()
                .AddCondition(m => !m.HasProperty(b => b.Text))
                .SetContent(
                    new HtmlGenericControl("span") { Children = { RawLiteral.Create("test") } },
                    options: StyleOverrideOptions.Ignore
                );
            var buttons = Parse("<dot:Button /> <dot:Button> <!-- nothing --> </dot:Button> <dot:Button Text='abc' /> <dot:Button> xx </dot:Button> ")
                .Content.SelectRecursively(c => c.Content)
                .Where(c => c.Metadata.Type == typeof(Button))
                .ToArray();
            Assert.AreEqual(4, buttons.Length);
            Assert.AreEqual("span", buttons[0].Content.Single().ConstructorParameters.Single());
            Assert.AreEqual("test", buttons[0].Content.Single().Content.Single().ConstructorParameters[0]);
            Assert.AreEqual("span", buttons[1].Content.Single().ConstructorParameters.Single());
            Assert.AreEqual("test", buttons[1].Content.Single().Content.Single().ConstructorParameters[0]);
            Assert.AreEqual("abc", buttons[2].Properties[Button.TextProperty].CastTo<ResolvedPropertyValue>().Value);
            Assert.IsTrue(buttons[2].HasOnlyWhiteSpaceContent());
            Assert.IsFalse(buttons[3].HasOnlyWhiteSpaceContent());
            Assert.AreEqual(typeof(RawLiteral), buttons[3].Content.Single().Metadata.Type);
        }

        [TestMethod]
        public void SetBinding()
        {
            config.Styles.Register<TextBox>()
                .AddCondition(m => m.HasDataContext<string>())
                .SetPropertyBinding(r => r.Text, "_this", StyleOverrideOptions.Ignore)
                .AddClassBinding("isempty", "_this == ''", StyleOverrideOptions.Ignore);

            var repeater = Parse("<dot:Repeater DataSource={value: _this}> <dot:TextBox /> </dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));

            var r = repeater.Properties[Repeater.DataSourceProperty].CastTo<ResolvedPropertyBinding>().Binding;
            Assert.AreEqual("$rawData", r.Binding.CastTo<IValueBinding>().KnockoutExpression.ToDefaultString());

            var textbox = repeater.Properties[Repeater.ItemTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single(c => c.Metadata.Type == typeof(TextBox));
            var tb = textbox.Properties[TextBox.TextProperty].CastTo<ResolvedPropertyBinding>().Binding;
            Assert.AreEqual("$rawData", tb.Binding.CastTo<IValueBinding>().KnockoutExpression.ToDefaultString());
            var tbc = textbox.Properties[HtmlGenericControl.CssClassesGroupDescriptor.GetDotvvmProperty("isempty")].CastTo<ResolvedPropertyBinding>().Binding;
            Assert.AreEqual("$data==\"\"", tbc.Binding.CastTo<IValueBinding>().KnockoutExpression.ToDefaultString());
        }

        [TestMethod]
        public void SetBinding_ConcatInHtmlAttributes()
        {
            config.Styles.Register("div")
                .SetAttributeBinding("class", "'class1-' + _this[0]", options: StyleOverrideOptions.Append)
                .SetAttributeBinding("class", "'class2-' + _this[1]", options: StyleOverrideOptions.Append);

            var div = Parse("<div class='a' />")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(HtmlGenericControl));

            var r = div.Properties.Values.OfType<ResolvedPropertyBinding>().Single().Binding;
            Assert.AreEqual("\"a class1-\"+($data[0]()??\"\")+\" class2-\"+($data[1]()??\"\")", r.Binding.CastTo<IValueBinding>().KnockoutExpression.ToDefaultString());
        }

        [TestMethod]
        public void SetBinding_ConcatInHtmlAttributes_BindingTypeAdjust()
        {
            config.Styles.Register("div")
                .SetAttributeBinding("class", "'class1-' + _this[0]", options: StyleOverrideOptions.Append);

            var div = Parse("<div class={resource: 'a'} />")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(HtmlGenericControl));

            var r = div.Properties.Values.OfType<ResolvedPropertyBinding>().Single().Binding;
            Assert.IsInstanceOfType(r.Binding, typeof(ResourceBindingExpression));
        }

        [TestMethod]
        public void WrapWithHtmlElement()
        {
            config.Styles.Register<DotvvmControl>(x => x.HasTag("wrap-div"))
                .WrapWith(new HtmlGenericControl("div"));

            var spanDivWrapper = new HtmlGenericControl("span");
            spanDivWrapper.properties.Set(Styles.TagProperty, "wrap-div");
            config.Styles.Register<DotvvmControl>(x => x.HasTag("wrap-span-div"))
                .WrapWith(spanDivWrapper);

            // var wrapped = Parse("<a Styles.Tag=wrap-div class=x />")
            //     .Content.SelectRecursively(c => c.Content)
            //     .Single(c => c.Metadata.Type == typeof(HtmlGenericControl) && "a".Equals(c.ConstructorParameters[0]));

            // Assert.AreEqual(((ResolvedControl)wrapped.Parent).Metadata.Type, typeof(HtmlGenericControl));
            // Assert.AreEqual(((ResolvedControl)((ResolvedControl)wrapped.Parent).Parent).Metadata.Type, typeof(DotvvmView));

            var wrapped2 = Parse("<a Styles.Tag=wrap-span-div class=x />")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(HtmlGenericControl) && "a".Equals(c.ConstructorParameters[0]));

            Assert.AreEqual(((ResolvedControl)wrapped2.Parent).Metadata.Type, typeof(HtmlGenericControl));
            Assert.AreEqual(((ResolvedControl)((ResolvedControl)wrapped2.Parent).Parent).Metadata.Type, typeof(HtmlGenericControl));
        }

        [TestMethod]
        public void RequireResource()
        {
            config.Resources.Register("my_resource", new InlineScriptResource("alert(1)"));
            config.Styles.Register<DotvvmControl>(x => x.HasTag("resource"))
                .AddRequiredResource("my_resource");

            var resource = Parse("<a Styles.Tag=resource /> <span Styles.Tag=resource /> <div Styles.Tag=resource /> <dot:Button Styles.Tag=resource />")
                .Content.SelectRecursively(c => c.Content)
                .Where(c => c.Metadata.Type == typeof(RequiredResource))
                .ToArray();

            Assert.AreEqual(1, resource.Length);

            var resource2 = Parse("<dot:Repeater DataSource={value: _this}> <span Styles.Tag=resource /> <div Styles.Tag=resource /> <dot:Button Styles.Tag=resource /> </dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Where(c => c.Metadata.Type == typeof(RequiredResource))
                .ToArray();

            Assert.AreEqual(1, resource.Length, 1);
        }
        [TestMethod]
        public void InfiniteWrapper()
        {
            config.Styles.Register("infinite-wrap")
                .WrapWith(new HtmlGenericControl("infinite-wrap"));


            var e = Assert.ThrowsException<Exception>(() =>
                Parse("<infinite-wrap />"));

            Assert.IsTrue(e.Message.Contains("there is probably an infinite cycle in server-side styles"));
        }
    }
}
