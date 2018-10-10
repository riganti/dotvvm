using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Binding
{
    public class BindingCollectionInfo
    {
        public BindingCollectionInfo(int index)
        {
            this.Index = index;
        }

        public int Index { get; }
        public bool IsFirst => Index == 0;
        public bool IsOdd => Index % 2 == 1;
        public bool IsEven => Index % 2 == 0;

        internal static void RegisterJavascriptTranslations(JavascriptTranslatableMethodCollection methods)
        {
            IJavascriptMethodTranslator memberAccess(string name) =>
                new GenericMethodCompiler(
                    builder: a => a[0].CastTo<JsObjectExpression>().Properties.Single(p => p.Name == name).Expression.Clone(),
                    check: (_m, a, _a) => a.GetParameterAnnotation() is BindingParameterAnnotation ann && ann.ExtensionParameter is BindingCollectionInfoExtensionParameter
                );
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(Index), memberAccess(nameof(Index)));
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(IsFirst), memberAccess(nameof(IsFirst)));
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(IsOdd), memberAccess(nameof(IsOdd)));
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(IsEven), memberAccess(nameof(IsEven)));
        }
    }
}
