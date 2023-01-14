type PagingOptions = {
    PageIndex: number,
    PagesCount: number
};

type NextTokenPagingOptions = {
    CurrentToken: string | null,
    NextPageToken: string | null
};

type NextTokenHistoryPagingOptions = {
    PageIndex: number,
    TokenHistory: (string | null)[]
};

type SortingOptions = {
    SortExpression: string | null,
    SortDescending: boolean
};

export const translations = {
    PagingOptions: {
        goToFirstPage(options: DotvvmObservable<PagingOptions>) {
            options.patchState({ PageIndex: 0 });
        },
        goToLastPage(options: DotvvmObservable<PagingOptions>) {
            options.patchState({ PageIndex: options.state.PagesCount - 1 });
        },
        goToNextPage(options: DotvvmObservable<PagingOptions>) {
            if (options.state.PageIndex < options.state.PagesCount - 1) {
                options.patchState({ PageIndex: options.state.PageIndex + 1 });
            }
        },
        goToPreviousPage(options: DotvvmObservable<PagingOptions>) {
            if (options.state.PageIndex > 0) {
                options.patchState({ PageIndex: options.state.PageIndex - 1 });
            }
        },
        goToPage(options: DotvvmObservable<PagingOptions>, pageIndex: number) {
            if (options.state.PageIndex >= 0 && options.state.PageIndex < options.state.PagesCount) {
                options.patchState({ PageIndex: pageIndex });
            }
        }
    },
    NextTokenPagingOptions: {
        goToFirstPage(options: DotvvmObservable<NextTokenPagingOptions>) {
            options.patchState({ CurrentToken: null });
        },
        goToNextPage(options: DotvvmObservable<NextTokenPagingOptions>) {
            if (options.state.NextPageToken) {
                options.patchState({ CurrentToken: options.state.NextPageToken });
            }
        }
    },
    NextTokenHistoryPagingOptions: {
        goToFirstPage(options: DotvvmObservable<NextTokenHistoryPagingOptions>) {
            options.patchState({ PageIndex: 0 });
        },
        goToLastPage(options: DotvvmObservable<NextTokenHistoryPagingOptions>) {
            options.patchState({ PageIndex: options.state.TokenHistory.length - 1 });
        },
        goToNextPage(options: DotvvmObservable<NextTokenHistoryPagingOptions>) {
            if (options.state.PageIndex < options.state.TokenHistory.length - 1) {
                options.patchState({ PageIndex: options.state.PageIndex + 1 });
            }
        },
        goToPreviousPage(options: DotvvmObservable<NextTokenHistoryPagingOptions>) {
            if (options.state.PageIndex > 0) {
                options.patchState({ PageIndex: options.state.PageIndex - 1 });
            }
        },
        goToPage(options: DotvvmObservable<NextTokenHistoryPagingOptions>, pageIndex: number) {
            if (options.state.PageIndex >= 0 && options.state.PageIndex < options.state.TokenHistory.length) {
                options.patchState({ PageIndex: pageIndex });
            }
        }
    },

    SortingOptions: {
        setSortExpression(options: DotvvmObservable<SortingOptions>, sortExpression: string) {
            if (sortExpression == null) {
                options.patchState({
                    SortExpression: null,
                    SortDescending: false
                });
            }
            else if (sortExpression == options.state.SortExpression) {
                options.patchState({
                    SortDescending: !options.state.SortDescending
                });
            }
            else {
                options.patchState({
                    SortExpression: sortExpression,
                    SortDescending: false
                });
            }
        }
    }
};
