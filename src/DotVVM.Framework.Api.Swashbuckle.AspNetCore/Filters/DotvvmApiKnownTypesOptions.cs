using System;
using System.Collections.Generic;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class DotvvmApiKnownTypesOptions
    {
        public List<Type> KnownTypes { get; }
            = new List<Type> { typeof(GridViewDataSet<>), typeof(IPagingOptions), typeof(ISortingOptions), typeof(IRowEditOptions) };
    }
}
