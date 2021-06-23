using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser.Annotations
{
    public sealed class WriteAccessAnnotation : IBindingParserAnnotation
    {
        private WriteAccessAnnotation()
        {

        }

        private static WriteAccessAnnotation instance;
        public static WriteAccessAnnotation Instance
        {
            get
            {
                if (instance == null)
                    instance = new WriteAccessAnnotation();

                return instance;
            }
        }
    }
}
