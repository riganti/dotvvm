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
using DotVVM.Framework.Controls.DynamicData;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DotVVM.Framework.Controls.DynamicData.Annotations;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class DynamicDataTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            _ = Repeater.RenderAsNamedTemplateProperty;
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
        }, services: s => {
            s.AddDynamicData();
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task BasicDynamicEntity()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dd:DynamicEntity DataContext={value: Entity} ExcludeProperties='Id'
                            Changed-Name={command: 0} />
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task BasicDynamicEditor()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dd:DynamicEditor Property={value: Integer} />
                    <dd:DynamicEditor Property={value: Boolean} />
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }
        
        [TestMethod]
        public async Task BasicDynamicGridColumn()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dot:GridView DataSource={value: List}>
                        <dd:DynamicGridColumn Property={value: Name} />
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
        public async Task DynamicEntityWithVisibleEnabledFields(string viewName, string userRoleName, bool isAuthenticated)
        {
            var user = isAuthenticated
                ? new ClaimsIdentity("myAuth")
                : new ClaimsIdentity();
            if (!string.IsNullOrEmpty(userRoleName))
            {
                user.AddClaim(new Claim(ClaimTypes.Role, userRoleName));
            }

            var r = await cth.RunPage(typeof(VisibleEnabledTestViewModel), $@"
                    <dd:DynamicEntity ViewName={viewName} />
                ",
                user: new ClaimsPrincipal(user),
                fileName: $"{nameof(DynamicEntityWithVisibleEnabledFields)}-{viewName}-{userRoleName}-{isAuthenticated}");
            check.CheckString(r.FormattedHtml, $"{viewName}-{userRoleName}-{isAuthenticated}", fileExtension: "html");
        }
        [TestMethod]
        public async Task BasicDynamicGrid()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dot:GridView DataSource={value: List} InlineEditing>
                        <dd:DynamicColumns />
                    </dot:GridView>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task RepeateredEditor()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dot:Repeater DataSource={value: List}>
                        <dd:DynamicEntity IncludeProperties='Email, Name' />
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
    }

}
