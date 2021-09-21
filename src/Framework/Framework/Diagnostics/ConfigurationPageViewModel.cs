using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Diagnostics
{
    public class ConfigurationPageViewModel : DotvvmViewModelBase
    {
        public int ActiveTab { get; set; } = 0;
    }
}
