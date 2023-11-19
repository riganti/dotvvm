import { StateManager } from "../state-manager";

type GridViewDataSet = {
    PagingOptions: DotvvmObservable<any>,
    SortingOptions: DotvvmObservable<any>,
    FilteringOptions: DotvvmObservable<any>,
    Items: DotvvmObservable<any[]>,
    IsRefreshRequired?: DotvvmObservable<boolean>
};
type GridViewDataSetOptions = {
    PagingOptions: any,
    SortingOptions: any,
    FilteringOptions: any
};
type GridViewDataSetResult = {
    Items: any[],
    PagingOptions: any,
    SortingOptions: any,
    FilteringOptions: any
};

export async function loadDataSet(
    dataSetObservable: DotvvmObservable<GridViewDataSet>,
    transformOptions: (options: GridViewDataSetOptions) => void,
    loadData: (options: GridViewDataSetOptions) => Promise<DotvvmAfterPostBackEventArgs>,
    postProcessor: (dataSet: DotvvmObservable<GridViewDataSet>, result: GridViewDataSetResult) => void = postProcessors.replace
) {
    const dataSet = dataSetObservable.state;

    const options: GridViewDataSetOptions = {
        FilteringOptions: structuredClone(dataSet.FilteringOptions),
        SortingOptions: structuredClone(dataSet.SortingOptions),
        PagingOptions: structuredClone(dataSet.PagingOptions)
    };
    transformOptions(options);
        
    const result = await loadData(options);
    const commandResult = result.commandResult as GridViewDataSetResult;

    postProcessor(dataSetObservable, commandResult);
}

export const postProcessors = {

    replace(dataSet: DotvvmObservable<GridViewDataSet>, result: GridViewDataSetResult) {
        dataSet.patchState(result);
    },

    append(dataSet: DotvvmObservable<GridViewDataSet>, result: GridViewDataSetResult) {
        const currentItems = (dataSet.state as any).Items as any[];
        dataSet.patchState({
            FilteringOptions: result.FilteringOptions,
            SortingOptions: result.SortingOptions,
            PagingOptions: result.PagingOptions,
            Items: [...currentItems, ...result.Items]
        });
    }

};
