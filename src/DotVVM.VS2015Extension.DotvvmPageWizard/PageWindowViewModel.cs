using System.Collections.Generic;
using System.Windows.Documents;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class PageWindowViewModel
    {
        public string ViewName { get; set; }

        public string ViewLocation { get; set; }

        public bool CreateViewModel { get; set; }

        public string ViewModelName { get; set; }

        public string ViewModelLocation { get; set; }

        public bool EmbedInMasterPage { get; set; }

        public string MasterPageLocation { get; set; }


        public List<string> Folders { get; set; }

        public List<string> MasterPages { get; set; }
    }
}