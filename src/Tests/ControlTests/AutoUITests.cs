using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DotVVM.AutoUI;
using DotVVM.AutoUI.Annotations;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class AutoUITests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            _ = Repeater.RenderAsNamedTemplateProperty;
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
        }, services: s => {
            s.AddAutoUI();
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task BasicForm()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <auto:Form DataContext={value: Entity} ExcludeProperties='Id'
                            Changed-Name={command: 0}>
                        <EditorTemplate-Sometime>
                            Nah, I'm lazy and we don't support date times
                        </EditorTemplate-Sometime>
                    </auto:Form>
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task BasicEditor()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <auto:Editor Property={value: Integer} />
                    <auto:Editor Property={value: Boolean} />
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task EnumEditor()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <auto:Editor Property={value: Enum} />
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }
        
        [TestMethod]
        public async Task BasicColumn()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dot:GridView DataSource={value: List}>
                        <auto:GridViewColumn Property={value: Name} />
                    </dot:GridView>
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [DataTestMethod]
        [DataRow("Default", "", false)]
        [DataRow("Insert", "", false)]
        [DataRow("Insert", "Admin", true)]
        [DataRow("Edit", "Admin", true)]
        public async Task FormWithVisibleEnabledFields(string viewName, string userRoleName, bool isAuthenticated)
        {
            var user = isAuthenticated
                ? new ClaimsIdentity("myAuth")
                : new ClaimsIdentity();
            if (!string.IsNullOrEmpty(userRoleName))
            {
                user.AddClaim(new Claim(ClaimTypes.Role, userRoleName));
            }

            var r = await cth.RunPage(typeof(VisibleEnabledTestViewModel), $@"
                    <auto:Form ViewName={viewName} />
                ",
                user: new ClaimsPrincipal(user),
                fileName: $"{nameof(FormWithVisibleEnabledFields)}-{viewName}-{userRoleName}-{isAuthenticated}");
            check.CheckString(r.FormattedHtml, $"{viewName}-{userRoleName}-{isAuthenticated}", fileExtension: "html");
        }
        [TestMethod]
        public async Task BasicGrid()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dot:GridView DataSource={value: List} InlineEditing>
                        <auto:GridViewColumns />
                    </dot:GridView>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task FormInRepeater()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dot:Repeater DataSource={value: List}>
                        <auto:Form IncludeProperties='Email, Name' />
                    </dot:Repeater>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class SimpleEntity
        {
            [Editable(false)]
            public int Id { get; set; }
            public string Name { get; set; }
            [EmailAddress]
            [Required]
            public string Email { get; set; }
            public DateTime Sometime { get; set; }
            [Range(0, 150)]
            public int Age { get; set; }
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            [DisplayFormat(DataFormatString = "0000000000")]
            public int Integer { get; set; } = 17;
            public bool Boolean { get; set; }
            public SimpleEntity Entity { get; set; }
            public bool AfterPreRender { get; set; }
            public TestEnum Enum { get; set; } = TestEnum.Case2;

            public GridViewDataSet<SimpleEntity> List { get; set; } = new GridViewDataSet<SimpleEntity> {
                RowEditOptions = { PrimaryKeyPropertyName = "Id" },
                Items = 
                    new [] {"list-item1", "list-item2" }
                    .Select((s, i) => new SimpleEntity { Id = i, Name = s }).ToList()
            };

            public override Task PreRender()
            {
                AfterPreRender = true;
                return base.PreRender();
            }
        }

        public class VisibleEnabledTestViewModel
        {
            [Display(AutoGenerateField = false)]
            public bool CompletelyHidden { get; set; }

            [Visible(ViewNames = "Insert")]
            public bool InsertOnly { get; set; }

            [Visible(Roles = "Admin | Moderator")]
            public bool AdminOrModeratorOnly { get; set; }

            [Editable(false)]
            public bool AlwaysReadOnly { get; set; }

            [Enabled(ViewNames = "Edit", IsAuthenticated = AuthenticationMode.Authenticated)]
            public bool AuthenticatedEditOnly { get; set; }

            [Enabled(IsAuthenticated = AuthenticationMode.NonAuthenticated)]
            public bool AnonymousOnly { get; set; }
        }

        public enum TestEnum
        {
            [Display(Name = "First Case")]
            Case1,
            [Display(Name = "Second Case")]
            Case2,
            [Display(Name = "Third Case")]
            Case3
        }
    }

}
