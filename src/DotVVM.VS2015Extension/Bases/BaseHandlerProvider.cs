using DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.Bases
{
    public class BaseHandlerProvider
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService;

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        [Import(typeof(VisualStudioWorkspace))]
        internal VisualStudioWorkspace VsWorkspace { get; set; }
    }
}