type TimerProps = {
    interval: number,
    enabled: KnockoutObservable<boolean>,
    command: () => Promise<DotvvmAfterPostBackEventArgs>
}

ko.virtualElements.allowedBindings["dotvvm-timer"] = true;

export default {
    "dotvvm-timer": {
        init: (element: HTMLElement, valueAccessor: () => TimerProps) => {
            const prop = valueAccessor();
            let timer: number | null = null;

            const observable = ko.isObservable(prop.enabled) ? prop.enabled : ko.pureComputed(() => ko.unwrap(valueAccessor().enabled));
            const subscription = observable.subscribe(newValue => createOrDestroyTimer(newValue));
            createOrDestroyTimer(ko.unwrap(prop.enabled));

            function createOrDestroyTimer(enabled: boolean) {
                if (enabled) {
                    if (timer) {
                        window.clearTimeout(timer);
                    }

                    const callback = async () => {
                        try {
                            await prop.command.bind(element)();
                        } catch (err) {
                            dotvvm.log.logError("postback", err);
                        }
                        timer = window.setTimeout(callback, prop.interval);
                    };
                    timer = window.setTimeout(callback, prop.interval);

                } else if (timer) {
                    window.clearTimeout(timer);
                }
            };

            ko.utils.domNodeDisposal.addDisposeCallback(element, () => {
                subscription.dispose();
                createOrDestroyTimer(false);
            });
        }
    }
};
