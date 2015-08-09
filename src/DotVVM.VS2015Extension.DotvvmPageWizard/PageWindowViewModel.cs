using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using DotVVM.VS2015Extension.DotvvmPageWizard.Annotations;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class PageWindowViewModel : INotifyPropertyChanged
    {
        private string viewName;
        private string viewLocation;
        private bool createViewModel;
        private string viewModelName;
        private string viewModelLocation;
        private bool embedInMasterPage;
        private string masterPageLocation;
        private string viewModelNamespace;
        private string viewModelRootNamespace;
        private List<string> contentPlaceHolderIds;
        private List<string> masterPages;
        private List<string> folders;

        public string ViewName
        {
            get { return viewName; }
            set
            {
                if (value == viewName) return;
                viewName = value;
                OnPropertyChanged();
            }
        }

        public string ViewLocation
        {
            get { return viewLocation; }
            set
            {
                if (value == viewLocation) return;
                viewLocation = value;
                OnPropertyChanged();
            }
        }

        public bool CreateViewModel
        {
            get { return createViewModel; }
            set
            {
                if (value == createViewModel) return;
                createViewModel = value;
                OnPropertyChanged();
            }
        }

        public string ViewModelName
        {
            get { return viewModelName; }
            set
            {
                if (value == viewModelName) return;
                viewModelName = value;
                OnPropertyChanged();
            }
        }

        public string ViewModelLocation
        {
            get { return viewModelLocation; }
            set
            {
                if (value == viewModelLocation) return;
                viewModelLocation = value;
                OnPropertyChanged();
            }
        }

        public bool EmbedInMasterPage
        {
            get { return embedInMasterPage; }
            set
            {
                if (value == embedInMasterPage) return;
                embedInMasterPage = value;
                OnPropertyChanged();
            }
        }

        public string MasterPageLocation
        {
            get { return masterPageLocation; }
            set
            {
                if (value == masterPageLocation) return;
                masterPageLocation = value;
                OnPropertyChanged();
            }
        }


        public List<string> Folders
        {
            get { return folders; }
            set
            {
                if (Equals(value, folders)) return;
                folders = value;
                OnPropertyChanged();
            }
        }

        public List<string> MasterPages
        {
            get { return masterPages; }
            set
            {
                if (Equals(value, masterPages)) return;
                masterPages = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProjectContainsMasterPages));
            }
        }

        public bool ProjectContainsMasterPages
        {
            get { return MasterPages.Any(); }
        }

        public List<string> ContentPlaceHolderIds
        {
            get { return contentPlaceHolderIds; }
            set
            {
                if (Equals(value, contentPlaceHolderIds)) return;
                contentPlaceHolderIds = value;
                OnPropertyChanged();
            }
        }

        public string ViewModelNamespace
        {
            get { return viewModelNamespace; }
            set
            {
                if (value == viewModelNamespace) return;
                viewModelNamespace = value;
                OnPropertyChanged();
            }
        }

        public string ViewModelRootNamespace
        {
            get { return viewModelRootNamespace; }
            set
            {
                if (value == viewModelRootNamespace) return;
                viewModelRootNamespace = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}