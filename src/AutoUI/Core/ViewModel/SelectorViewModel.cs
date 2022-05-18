using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.AutoUI.Annotations;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.AutoUI.ViewModel;

public class SelectorViewModel<TItem> : DotvvmViewModelBase, ISelectorViewModel<TItem>
    where TItem : Annotations.SelectorItem
{

    public List<TItem>? Items { get; set; }

    public override async Task PreRender()
    {
        if (Items == null)
        {
            await LoadItems();
        }
        await base.PreRender();
    }

    protected virtual async Task LoadItems()
    {
        var selectorDataProvider = Context.Services.GetService<ISelectorDataProvider<TItem>>();
        if (selectorDataProvider != null)
        {
            Items = await selectorDataProvider.GetSelectorItems();
        }
        else
        {
            throw new DotvvmControlException($"Cannot resolve ISelectorDataProvider<{typeof(TItem).FullName}> service! Either load data into {GetType()}.Items collection manually, or register a service which can provide the selector items.");
        }
    }
}

public class SelectorViewModel<TItem, TParam> : SelectorViewModel<TItem>
    where TItem : Annotations.SelectorItem
{
    private readonly Func<TParam> parameterProvider;

    public SelectorViewModel(Func<TParam> parameterProvider)
    {
        this.parameterProvider = parameterProvider;
    }

    protected override async Task LoadItems()
    {
        var selectorDataProvider = ServiceProviderServiceExtensions.GetService<ISelectorDataProvider<TItem, TParam>>(Context.Services);
        if (selectorDataProvider != null)
        {
            var parameter = parameterProvider();
            Items = await selectorDataProvider.GetSelectorItems(parameter);
        }
        else
        {
            throw new DotvvmControlException($"Cannot resolve ISelectorDataProvider<{typeof(TItem).FullName}> service! Either load data into {GetType()}.Items collection manually, or register a service which can provide the selector items.");
        }
    }
}
