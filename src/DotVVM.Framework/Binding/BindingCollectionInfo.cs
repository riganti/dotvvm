using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;

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
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(Index),
                new GenericMethodCompiler(_ => new JsSymbolicParameter(JavascriptTranslator.CurrentIndexParameter)));
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(IsFirst),
                new GenericMethodCompiler(_ => new JsBinaryExpression(new JsSymbolicParameter(JavascriptTranslator.CurrentIndexParameter), BinaryOperatorType.Equal, new JsLiteral(0))));
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(IsOdd),
                    new GenericMethodCompiler(_ => new JsBinaryExpression(new JsBinaryExpression(new JsSymbolicParameter(JavascriptTranslator.CurrentIndexParameter), BinaryOperatorType.Modulo, new JsLiteral(2)), BinaryOperatorType.Equal, new JsLiteral(1))));
            methods.AddPropertyGetterTranslator(typeof(BindingCollectionInfo), nameof(IsEven),
                    new GenericMethodCompiler(_ => new JsBinaryExpression(new JsBinaryExpression(new JsSymbolicParameter(JavascriptTranslator.CurrentIndexParameter), BinaryOperatorType.Modulo, new JsLiteral(2)), BinaryOperatorType.Equal, new JsLiteral(0))));
        }
    }
}
