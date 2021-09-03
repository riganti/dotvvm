using System;
using System.Collections.Generic;
using System.Text;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;

namespace DotVVM.Samples.Tests
{
    public static class DotVVMAssertModified
    {
        public static void UploadFile(IElementWrapper element, string fullFileName)
        {
            if (element.BrowserWrapper.IsDotvvmPage())
            {
                element.BrowserWrapper.LogVerbose("Selenium.DotVVM : Uploading file");
                var name = element.GetTagName();
                if (name == "a" && element.HasAttribute("onclick") && (element.GetAttribute("onclick")?.Contains("showUploadDialog") ?? false))
                {
                    element = element.ParentElement.ParentElement;
                }

                if (element.GetTagName() == "div")
                {
                    var fileInput = element.Single("input[type=file]");
                    fileInput.SendKeys(fullFileName);

                    element.Wait(element.ActionWaitTime);
                    return;
                }

                element.BrowserWrapper.LogVerbose("Selenium.DotVVM : Cannot identify DotVVM scenario. Uploading over standard procedure.");
            }

            element.BrowserWrapper.OpenInputFileDialog(element, fullFileName);
        }
        
    }
}
