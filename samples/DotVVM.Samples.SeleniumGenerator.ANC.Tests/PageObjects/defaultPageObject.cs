using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Controls;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects
{
    public class defaultPageObject : SeleniumHelperBase
    {
        public RouteLinkProxy TextsLabel_NewStudent
        {
            get;
        }

        public GridViewProxy<StudentsGridViewPageObject> Students
        {
            get;
        }

        public RouteLinkProxy Home
        {
            get;
        }

        public LinkButtonProxy Header_SignOut
        {
            get;
        }

        public RouteLinkProxy Header_SignIn
        {
            get;
        }

        public NestedUserControlPageObject NestedUserControl
        {
            get;
        }

        public defaultPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            TextsLabel_NewStudent = new RouteLinkProxy(this, new PathSelector{UiName = "TextsLabel_NewStudent", Parent = parentSelector});
            Students = new GridViewProxy<StudentsGridViewPageObject>(this, new PathSelector{UiName = "Students", Parent = parentSelector});
            Home = new RouteLinkProxy(this, new PathSelector{UiName = "Home", Parent = parentSelector});
            Header_SignOut = new LinkButtonProxy(this, new PathSelector{UiName = "Header_SignOut", Parent = parentSelector});
            Header_SignIn = new RouteLinkProxy(this, new PathSelector{UiName = "Header_SignIn", Parent = parentSelector});
            NestedUserControl = new NestedUserControlPageObject(webDriver, this, new PathSelector{UiName = "NestedUserControl", Parent = parentSelector});
        }

        public class StudentsGridViewPageObject : SeleniumHelperBase
        {
            public GridViewTextColumnProxy FirstName
            {
                get;
            }

            public GridViewTextColumnProxy LastName
            {
                get;
            }

            public GridViewTemplateColumnGridViewTemplateColumn GridViewTemplateColumn
            {
                get;
            }

            public GridViewTemplateColumn1GridViewTemplateColumn GridViewTemplateColumn1
            {
                get;
            }

            public StudentsGridViewPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
            {
                FirstName = new GridViewTextColumnProxy(this, new PathSelector{UiName = "FirstName", Parent = parentSelector});
                LastName = new GridViewTextColumnProxy(this, new PathSelector{UiName = "LastName", Parent = parentSelector});
                GridViewTemplateColumn = new GridViewTemplateColumnGridViewTemplateColumn(webDriver, this, parentSelector);
                GridViewTemplateColumn1 = new GridViewTemplateColumn1GridViewTemplateColumn(webDriver, this, parentSelector);
            }

            public class GridViewTemplateColumnGridViewTemplateColumn : SeleniumHelperBase
            {
                public RouteLinkProxy TextsLabel_Detail
                {
                    get;
                }

                public GridViewTemplateColumnGridViewTemplateColumn(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
                {
                    TextsLabel_Detail = new RouteLinkProxy(this, new PathSelector{UiName = "TextsLabel_Detail", Parent = parentSelector});
                }
            }

            public class GridViewTemplateColumn1GridViewTemplateColumn : SeleniumHelperBase
            {
                public RouteLinkProxy TextsLabel_Edit
                {
                    get;
                }

                public GridViewTemplateColumn1GridViewTemplateColumn(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
                {
                    TextsLabel_Edit = new RouteLinkProxy(this, new PathSelector{UiName = "TextsLabel_Edit", Parent = parentSelector});
                }
            }
        }
    }
}