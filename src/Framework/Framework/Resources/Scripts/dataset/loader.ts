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
    Items: DotvvmObservable<any[]>,
    TotalItemsCount?: DotvvmObservable<number>
};

export async function loadDataSet(dataSet: GridViewDataSet, loadData: (options: GridViewDataSetOptions) => Promise<GridViewDataSetResult>) {
    if (dataSet.IsRefreshRequired) {
        dataSet.IsRefreshRequired.setState(true);
    }

    const result = await loadData({
        FilteringOptions: dataSet.FilteringOptions,
        SortingOptions: dataSet.SortingOptions,
        PagingOptions: dataSet.PagingOptions
    });

    dataSet.Items.setState([]);
    dataSet.Items.setState(result.Items.state);

    const pagingOptions = dataSet.PagingOptions.state;
    const totalItemsCount = result.TotalItemsCount?.state; 
    if (totalItemsCount && ko.isWriteableObservable(pagingOptions.TotalItemsCount)) {
        dataSet.PagingOptions.patchState({
            TotalItemsCount: result.TotalItemsCount
        });
    }

    if (dataSet.IsRefreshRequired) {
        dataSet.IsRefreshRequired.setState(false);
    }
}

