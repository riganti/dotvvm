import { getStateManager } from "../dotvvm-base";

type AppendableDataPagerBinding = {
    autoLoadWhenInViewport: boolean,
    loadNextPage: () => Promise<any>,
    dataSet: any
};

export default {
    'dotvvm-appendable-data-pager': {
        init: (element: HTMLInputElement, valueAccessor: () => AppendableDataPagerBinding, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            const binding = valueAccessor();

            const isLoading = ko.observable(false);
            const canLoadNextPage = ko.computed(() => !isLoading() && valueAccessor().dataSet?.PagingOptions()?.IsLastPage() === false);

            // prepare the context with $appendableDataPager object
            const state = {
                loadNextPage: async () => {
                    try {
                        isLoading(true);

                        await binding.loadNextPage();
                    }
                    finally {
                        isLoading(false);
                    }
                },
                canLoadNextPage,
                isLoading: ko.computed(() => isLoading())
            };

            // set up intersection observer
            if (binding.autoLoadWhenInViewport) {

                // track the scroll position and load the next page when the element is in the viewport
                const observer = new IntersectionObserver(async (entries) => {
                    let entry = entries[0];
                    if (entry?.isIntersecting) {
                        if (!canLoadNextPage()) return;

                        // load the next page
                        await state.loadNextPage();

                        // when the loading was finished, check whether we need to load another page
                        await new Promise(r => window.setTimeout(r, 500));
                        observer.unobserve(element);
                        observer.observe(element);
                    }
                }, {
                    rootMargin: "20px"
                });
                observer.observe(element);
                ko.utils.domNodeDisposal.addDisposeCallback(element, () => observer.disconnect());
            }

            // extend the context
            const innerBindingContext = bindingContext!.extend({
                $appendableDataPager: state
            });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    }
}
