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

type MultiCriteriaSortingOptions = {
    Criteria: SortCriterion[],
    MaxSortCriteriaCount: number
};

type SortCriterion = {
    SortExpression: string,
    SortDescending: boolean
};

export const translations = {
    PagingOptions: {
        goToFirstPage(options: PagingOptions) {
            options.PageIndex = 0;
        },
        goToLastPage(options: PagingOptions) {
            options.PageIndex = options.PagesCount - 1;
        },
        goToNextPage(options: PagingOptions) {
            if (options.PageIndex < options.PagesCount - 1) {
                options.PageIndex = options.PageIndex + 1;
            }
        },
        goToPreviousPage(options: PagingOptions) {
            if (options.PageIndex > 0) {
                options.PageIndex = options.PageIndex - 1;
            }
        },
        goToPage(options: PagingOptions, pageIndex: number) {
            if (options.PageIndex >= 0 && options.PageIndex < options.PagesCount) {
                options.PageIndex = pageIndex;
            }
        }
    },
    NextTokenPagingOptions: {
        goToFirstPage(options: NextTokenPagingOptions) {
            options.CurrentToken = null;
        },
        goToNextPage(options: NextTokenPagingOptions) {
            if (options.NextPageToken) {
                options.CurrentToken = options.NextPageToken;
            }
        }
    },
    NextTokenHistoryPagingOptions: {
        goToFirstPage(options: NextTokenHistoryPagingOptions) {
            options.PageIndex = 0;
        },
        goToNextPage(options: NextTokenHistoryPagingOptions) {
            if (options.PageIndex < options.TokenHistory.length - 1) {
                options.PageIndex = options.PageIndex + 1;
            }
        },
        goToPreviousPage(options: NextTokenHistoryPagingOptions) {
            if (options.PageIndex > 0) {
                options.PageIndex = options.PageIndex - 1;
            }
        },
        goToPage(options: NextTokenHistoryPagingOptions, pageIndex: number) {
            if (options.PageIndex >= 0 && options.PageIndex < options.TokenHistory.length) {
                options.PageIndex = pageIndex;
            }
        }
    },

    SortingOptions: {
        setSortExpression(options: SortingOptions, sortExpression: string) {
            if (sortExpression == null) {
                options.SortExpression = null;
                options.SortDescending = false;
            }
            else if (sortExpression == options.SortExpression) {
                options.SortDescending = !options.SortDescending;
            }
            else {
                options.SortExpression = sortExpression;
                options.SortDescending = false;
            }
        },
        isColumnSortedAscending(options: SortingOptions, sortExpression: string) {
            return options.SortExpression === sortExpression && !options.SortDescending;
        },
        isColumnSortedDescending(options: SortingOptions, sortExpression: string) {
            return options.SortExpression === sortExpression && options.SortDescending;
        }
    },
    MultiCriteriaSortingOptions: {
        setSortExpression(options: MultiCriteriaSortingOptions, sortExpression: string) {
            if (sortExpression == null) {
                options.Criteria = [];
                return;
            }

            const index = options.Criteria.findIndex(c => c.SortExpression == sortExpression);
            if (index === 0) {
                options.Criteria[index].SortDescending = !options.Criteria[index].SortDescending;
            }
            else if (index > 0) {
                options.Criteria.splice(index, 1);
                options.Criteria.unshift({ SortExpression: sortExpression, SortDescending: false });
            }
            else {
                options.Criteria.unshift({ SortExpression: sortExpression, SortDescending: false });
            }

            if (options.Criteria.length > options.MaxSortCriteriaCount)
            {
                options.Criteria.splice(options.MaxSortCriteriaCount, options.Criteria.length - options.MaxSortCriteriaCount);
            }
        },
        isColumnSortedAscending(options: MultiCriteriaSortingOptions, sortExpression: string) {
            return options.Criteria.some(c => c.SortExpression === sortExpression && !c.SortDescending);
        },
        isColumnSortedDescending(options: MultiCriteriaSortingOptions, sortExpression: string) {
            return options.Criteria.some(c => c.SortExpression === sortExpression && c.SortDescending);
        }
    }
};
