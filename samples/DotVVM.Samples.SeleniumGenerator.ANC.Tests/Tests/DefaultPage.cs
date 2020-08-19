using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Authentication;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.CRUD;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests.Tests
{
    public class DefaultPage : AppSeleniumTest
    {
        [Fact]
        public void DefaultPageLoadTest() => RunInAllBrowsers<defaultPageObject>((browser, defaultpage) => {

            defaultpage.Header_SignIn.Click();
            
            var signIn = browser.InitRootPageObject<SignInPageObject>();
            signIn.TextsLabel_Register.Click();

            var register = browser.InitRootPageObject<RegisterPageObject>();
            register.UserName.SetText("User4");
            register.Password.SetText("Pa$$word1");
            register.ConfirmPassword.SetText("Pa$$word1");
            register.TextsLabel_Register.Click();

            browser.Wait(1000);
            defaultpage.TextsLabel_NewStudent.Click();

            var newStudent = browser.InitRootPageObject<CreatePageObject>();
            newStudent.StudentFirstName.SetText("FirstName2");
            newStudent.StudentLastName.SetText("LastName2");
            newStudent.StudentAbout.SetText("This is first student created by UI test generated from GUT");
            newStudent.TextsLabel_Add.Click();

            browser.Wait(1000);
            Assert.Equal(3, defaultpage.Students.GetVisibleRowsCount());
        });
    }
}
