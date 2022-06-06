using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public enum MappingMode
    {
        /// <summary> It's not possible to use this property from dothtml markup. </summary>
        Exclude = 0,
        /// <summary> Property is used as an attribute. For example `&lt;div Visible={value: ...}` </summary>
        Attribute = 1,
        /// <summary> Property is used as a child element. For example `&lt;dot:GridView ...&gt; &lt;Columns&gt; ... property content &lt;Columns&gt;` </summary>
        InnerElement = 2,
        /// <summary> It is allowed to use this property either as an attribute or as a child element. </summary>
        Both = 3
    }
}
