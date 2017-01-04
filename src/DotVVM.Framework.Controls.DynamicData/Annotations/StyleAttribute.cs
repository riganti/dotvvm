using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StyleAttribute : Attribute
    {

        public string FormControlContainerCssClass { get; set; }

        public string FormRowCssClass { get; set; }

        public string GridCellCssClass { get; set; }

        public string GridHeaderCellCssClass { get; set; }

    }
}
