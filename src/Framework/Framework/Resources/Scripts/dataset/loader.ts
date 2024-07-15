import { StateManager } from "../state-manager";

type GridViewDataSet = {
    PagingOptions: object,
    SortingOptions: object,
    FilteringOptions: object,
    Items: any[],
    IsRefreshRequired?: boolean
};
type GridViewDataSetOptions = {
    PagingOptions: object,
    SortingOptions: object,
    FilteringOptions: object
};
type GridViewDataSetResult = {
    Items: any[],
    PagingOptions: object,
    SortingOptions: object,
    FilteringOptions: object
};

export function getOptions(dataSetObservable: DotvvmObservable<GridViewDataSet>): GridViewDataSetOptions {
    const dataSet = dataSetObservable.state
    return structuredClone({
        FilteringOptions: dataSet.FilteringOptions,
        SortingOptions: dataSet.SortingOptions,
        PagingOptions: dataSet.PagingOptions
    })
}

export async function loadDataSet(
    dataSetObservable: DotvvmObservable<GridViewDataSet>,
    transformOptions: (options: GridViewDataSetOptions) => void,
    loadData: (options: GridViewDataSetOptions) => Promise<DotvvmAfterPostBackEventArgs>,
    postProcessor: (dataSet: DotvvmObservable<GridViewDataSet>, result: GridViewDataSetResult) => void = postProcessors.replace
) {
    const options = getOptions(dataSetObservable);
    transformOptions(options);
        
    const result = await loadData(options);
    const commandResult = result.commandResult as GridViewDataSetResult;

    postProcessor(dataSetObservable, commandResult);
}

export const postProcessors = {

    replace(dataSet: DotvvmObservable<GridViewDataSet>, result: GridViewDataSetResult) {
        dataSet.updateState(ds => ({...ds, ...result}));
    },

    append(dataSet: DotvvmObservable<GridViewDataSet>, result: GridViewDataSetResult) {
        dataSet.updateState(ds => ({
            ...ds,
            ...result,
            Items: [...ds.Items, ...result.Items]
        }));
    }

};
