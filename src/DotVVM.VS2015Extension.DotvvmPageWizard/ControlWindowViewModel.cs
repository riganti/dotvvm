using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotVVM.VS2015Extension.DotvvmPageWizard.Annotations;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class ControlWindowViewModel : INotifyPropertyChanged
    {
        private string controlName;
        private string controlLocation;
        private bool createCodeBehind;
        private string codeBehindClassName;
        private string codeBehindClassLocation;
        private string codeBehindClassNamespace;
        private string codeBehindClassRootNamespace;
        private List<string> folders;

        public string ControlName
        {
            get { return controlName; }
            set
            {
                if (value == controlName) return;
                controlName = value;
                OnPropertyChanged();
            }
        }

        public string ControlLocation
        {
            get { return controlLocation; }
            set
            {
                if (value == controlLocation) return;
                controlLocation = value;
                OnPropertyChanged();
            }
        }

        public bool CreateCodeBehind
        {
            get { return createCodeBehind; }
            set
            {
                if (value == createCodeBehind) return;
                createCodeBehind = value;
                OnPropertyChanged();
            }
        }

        public string CodeBehindClassName
        {
            get { return codeBehindClassName; }
            set
            {
                if (value == codeBehindClassName) return;
                codeBehindClassName = value;
                OnPropertyChanged();
            }
        }

        public string CodeBehindClassLocation
        {
            get { return codeBehindClassLocation; }
            set
            {
                if (value == codeBehindClassLocation) return;
                codeBehindClassLocation = value;
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
        
        public string CodeBehindClassNamespace
        {
            get { return codeBehindClassNamespace; }
            set
            {
                if (value == codeBehindClassNamespace) return;
                codeBehindClassNamespace = value;
                OnPropertyChanged();
            }
        }

        public string CodeBehindClassRootNamespace
        {
            get { return codeBehindClassRootNamespace; }
            set
            {
                if (value == codeBehindClassRootNamespace) return;
                codeBehindClassRootNamespace = value;
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