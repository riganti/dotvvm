using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.AutoUI.Annotations;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.AutoUI.ViewModel;

public class SelectionViewModel<TItem> : DotvvmViewModelBase, ISelectorViewModel<TItem>
    where TItem : Annotations.Selection
{

    private bool isRefreshRequested = false;

    public List<TItem>? Items { get; set; }

    public void RequestRefresh()
    {
        isRefreshRequested = true;
    }

    public override async Task PreRender()
    {
        if (Items == null || isRefreshRequested)
        {
            await LoadItems();
        }
        await base.PreRender();
    }

    protected virtual async Task LoadItems()
    {
        var selectorDataProvider = Context.Services.GetService<ISelectionProvider<TItem>>();
        if (selectorDataProvider != null)
        {
            Items = await selectorDataProvider.GetSelectorItems();
        }
        else
        {
            throw new DotvvmControlException($"Cannot resolve ISelectionProvider<{typeof(TItem).FullName}> service! Either load data into {GetType()}.Items collection manually, or register a service which can provide the selector items.");
        }
    }
}

public class SelectionViewModel<TItem, TParam> : SelectionViewModel<TItem>
    where TItem : Annotations.Selection
{
    private readonly Func<TParam> parameterProvider;

    public SelectionViewModel(Func<TParam> parameterProvider)
    {
        this.parameterProvider = parameterProvider;
    }

    protected override async Task LoadItems()
    {
        var selectorDataProvider = Context.Services.GetService<ISelectionProvider<TItem, TParam>>();
        if (selectorDataProvider != null)
        {
            var parameter = parameterProvider();
            Items = await selectorDataProvider.GetSelectorItems(parameter);
        }
        else
        {
            throw new DotvvmControlException($"Cannot resolve ISelectionProvider<{typeof(TItem).FullName}> service! Either load data into {GetType()}.Items collection manually, or register a service which can provide the selector items.");
        }
    }
}
