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
            let enabled = false
            let commandRunning = false

            const observable = ko.isObservable(prop.enabled) ? prop.enabled : ko.pureComputed(() => ko.unwrap(valueAccessor().enabled));
            const subscription = observable.subscribe(newValue => createOrDestroyTimer(newValue));
            createOrDestroyTimer(ko.unwrap(prop.enabled));

            function createOrDestroyTimer(newEnabled: boolean) {
                enabled = newEnabled
                if (timer != null) {
                    window.clearTimeout(timer)
                    timer = null
                }

                // if commandRunning: it will schedule next run by itself
                if (enabled && !commandRunning) {
                    timer = window.setTimeout(callback, prop.interval);
                }
            };

            async function callback() {
                timer = null
                commandRunning = true
                try {
                    await prop.command.bind(element)();
                } catch (err) {
                    dotvvm.log.logError("postback", err);
                } finally {
                    commandRunning = false
                    createOrDestroyTimer(enabled)
                }
            }

            ko.utils.domNodeDisposal.addDisposeCallback(element, () => {
                subscription.dispose();
                createOrDestroyTimer(false);
            });
        }
    }
};
