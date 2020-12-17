#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls
{
    public static class GridViewDataSetHelper
    {
        public static IGridViewDataSetHandler GetHandler(IBaseGridViewDataSet<object>? baseGridViewDataSet, IDotvvmRequestContext context)
        {
            // TODO: find handler


            // use this as fallback even if dataSet is null
            return new GridViewDataSetCommandHandler(context.Services.GetRequiredService<BindingCompilationService>().WithoutInitialization());
        }


        public class PagerCommands
        {

            public static readonly string GoToFirstPage = nameof(GoToFirstPage);
            public static readonly string GoToPrevPage = nameof(GoToPrevPage);
            public static readonly string GoToNextPage = nameof(GoToNextPage);
            public static readonly string GoToLastPage = nameof(GoToLastPage);
            public static readonly string GoToThisPage = nameof(GoToThisPage);

        }

        public class SorterCommands
        {

            public static readonly string SortByColumn = nameof(SortByColumn);

        }
    }
}
