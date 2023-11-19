import { getStateManager } from "../dotvvm-base";

type AppendableDataPagerBinding = {
    autoLoadWhenInViewport: boolean,
    loadNextPage: () => Promise<any>,
    dataSet: any
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
                    while (entry?.isIntersecting) {
                        const dataSet = valueAccessor().dataSet;
                        if (dataSet.PagingOptions().IsLastPage()) {
                            return;
                        }

                        isLoading = true;
                        try {
                            await binding.loadNextPage();

                            // getStateManager().doUpdateNow();

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
