using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Samples.Common.Utilities
{
    public static class JavaScriptUtils
    {
        public static string LimitLength(string text, int length) => StringUtils.LimitLength(text, length);
    }
}
