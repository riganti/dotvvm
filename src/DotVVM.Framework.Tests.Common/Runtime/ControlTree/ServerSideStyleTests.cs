using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .SetControlProperty<ConfirmPostBackHandler>(PostBack.HandlersProperty, s => s.SetProperty(c => c.Message, "Are you sure?"), StyleOverrideOptions.Append);

            config.Styles
                .Register<Button>(m => m.HasHtmlAttribute("data-very-dangerous"))
                .SetControlProperty<ConfirmPostBackHandler>(PostBack.HandlersProperty, s => s.SetProperty(c => c.Message, "Are you really really sure?"), StyleOverrideOptions.Append);

            config.Styles
                .Register<LinkButton>(m => m.HasHtmlAttribute("data-manyhandlers"))
                .SetControlProperty<ConfirmPostBackHandler>(PostBack.HandlersProperty, s => s.SetProperty(c => c.Message, "1"), StyleOverrideOptions.Append)
                .SetControlProperty<ConfirmPostBackHandler>(PostBack.HandlersProperty, s => s.SetProperty(c => c.Message, "2"), StyleOverrideOptions.Append)
                .SetControlProperty<ConfirmPostBackHandler>(PostBack.HandlersProperty, s => s.SetProperty(c => c.Message, "3"), StyleOverrideOptions.Append)
                .SetControlProperty<ConfirmPostBackHandler>(PostBack.HandlersProperty, s => s.SetProperty(c => c.Message, "4"), StyleOverrideOptions.Append);

            config.Styles
                .Register<Repeater>()
                .SetHtmlControlProperty(Repeater.SeparatorTemplateProperty, "hr", options: StyleOverrideOptions.Ignore);
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
        public void SetControlProperty_RepeaterTemplate()
        {
            var repeater = Parse(
@"<dot:Repeater DataSource='{value: _this}'>
    {{value: _this}}
</dot:Repeater>")
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));
            var separator = repeater.Properties[Repeater.SeparatorTemplateProperty].CastTo<ResolvedPropertyTemplate>().Content.Single();
            Assert.AreEqual(typeof(HtmlGenericControl), separator.Metadata.Type);
            Assert.AreEqual("hr", separator.ConstructorParameters.Single());
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
    }
}
