using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
 public static  class Extensions
    {
        public static bool Contains(this string text, string value, StringComparison comparison)
        {

            return text.IndexOf(value, comparison) > -1;
        }
    }
}
