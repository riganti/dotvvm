type AppendableDataPagerBinding = {
    autoLoadWhenInViewport: boolean,
    loadNextPage: () => Promise<any>
};

export default {
    'dotvvm-appendable-data-pager': {
        init: (element: HTMLInputElement, valueAccessor: () => AppendableDataPagerBinding, allBindingsAccessor: KnockoutAllBindingsAccessor) => {
            const binding = valueAccessor();
            if (binding.autoLoadWhenInViewport) {
                let isLoading = false;

                // track the scroll position and load the next page when the element is in the viewport
                const observer = new IntersectionObserver(async (entries) => {
                    if (isLoading) return;

                    let entry = entries[0];
                    while (entry.isIntersecting) {
                        const dataSet = allBindingsAccessor.get("dotvvm-gridviewdataset").dataSet as DotvvmObservable<any>;
                        if (dataSet.state.PagingOptions.IsLastPage) {
                            return;
                        }

                        isLoading = true;
                        try {
                            await binding.loadNextPage();

                            // when the loading was finished, check whether we need to load another page
                            entry = observer.takeRecords()[0];
                        }
                        finally {
                            isLoading = false;
                        }
                    }
                });
                observer.observe(element);
                ko.utils.domNodeDisposal.addDisposeCallback(element, () => observer.disconnect());
            }
        }
    }
}
