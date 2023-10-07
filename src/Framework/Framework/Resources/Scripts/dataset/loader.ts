type GridViewDataSet = {
    PagingOptions: DotvvmObservable<any>,
    SortingOptions: DotvvmObservable<any>,
    FilteringOptions: DotvvmObservable<any>,
    Items: DotvvmObservable<any[]>,
    IsRefreshRequired?: DotvvmObservable<boolean>
};
type GridViewDataSetOptions = {
    PagingOptions: DotvvmObservable<any>,
    SortingOptions: DotvvmObservable<any>,
    FilteringOptions: DotvvmObservable<any>
};
type GridViewDataSetResult = {
    Items: any[],
    PagingOptions: any,
    SortingOptions: any,
    FilteringOptions: any
};

export async function loadDataSet(dataSetObservable: KnockoutObservable<GridViewDataSet>, loadData: (options: GridViewDataSetOptions) => Promise<DotvvmAfterPostBackEventArgs>) {
    const dataSet = ko.unwrap(dataSetObservable);
    if (dataSet.IsRefreshRequired) {
        dataSet.IsRefreshRequired.setState(true);
    }

    const result = await loadData({
        FilteringOptions: dataSet.FilteringOptions.state,
        SortingOptions: dataSet.SortingOptions.state,
        PagingOptions: dataSet.PagingOptions.state
    });
    const commandResult = result.commandResult as GridViewDataSetResult;

    dataSet.Items.setState([]);
    dataSet.Items.setState(commandResult.Items);

    if (commandResult.FilteringOptions && ko.isWriteableObservable(dataSet.FilteringOptions)) {
        dataSet.FilteringOptions.setState(commandResult.FilteringOptions);
    }
    if (commandResult.SortingOptions && ko.isWriteableObservable(dataSet.SortingOptions)) {
        dataSet.SortingOptions.setState(commandResult.SortingOptions);
    }
    if (commandResult.PagingOptions && ko.isWriteableObservable(dataSet.PagingOptions)) {
        dataSet.PagingOptions.setState(commandResult.PagingOptions);
    }

    if (dataSet.IsRefreshRequired) {
        dataSet.IsRefreshRequired.setState(false);
    }
}

