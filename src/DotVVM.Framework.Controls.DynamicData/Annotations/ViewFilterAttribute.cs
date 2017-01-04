using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ViewFilterAttribute : Attribute
    {

        public string[] ViewNames { get; set; }


        public ViewFilterAttribute(string[] viewNames)
        {
            ViewNames = viewNames;
        }

    }
}
