using System;
using System.Linq;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class WizardHelpers
    {
        private const string ViewModelClassNameSuffix = "ViewModel";
        private const string ViewsFolderName = "Views";
        private const string ViewModelsFolderName = "ViewModels";


        public static string GenerateViewModelName(string name)
        {
            // make first letter upper case
            if (name.Length > 0 && char.IsLower(name[0]))
            {
                name = name.Substring(0, 1).ToUpper() + name.Substring(1);
            }

            // append viewmodel if it is not there
            if (!name.EndsWith(ViewModelClassNameSuffix, StringComparison.CurrentCultureIgnoreCase))
            {
                name += ViewModelClassNameSuffix;
            }
            else
            {
                name = name.Substring(0, ViewModelClassNameSuffix.Length) + ViewModelClassNameSuffix;
            }
            return name;
        }

        public static string GenerateViewModelLocation(string viewLocation)
        {
            // if the view location contains a folder named Views, change it to ViewModels, otherwise do nothing
            var parts = viewLocation.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i], ViewsFolderName, StringComparison.CurrentCultureIgnoreCase))
                {
                    parts[i] = ViewModelsFolderName;
                }
            }
            return string.Join("/", parts);
        }

        public static string GenerateViewModelNamespace(string rootNamespace, string viewModelLocation)
        {
            var parts = new[] { rootNamespace }.Concat(viewModelLocation.Split('/')).Where(n => !string.IsNullOrEmpty(n));
            return string.Join(".", parts);
        }
    }
}