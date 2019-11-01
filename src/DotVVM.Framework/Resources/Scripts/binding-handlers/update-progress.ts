export default {
    "dotvvm-UpdateProgress-Visible": {
        init(element: HTMLElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            element.style.display = "none";
            var delay = element.getAttribute("data-delay");

            let includedQueues = (element.getAttribute("data-included-queues") || "").split(",").filter(i => i.length > 0);
            let excludedQueues = (element.getAttribute("data-excluded-queues") || "").split(",").filter(i => i.length > 0);

            var timeout: any;
            var running = false;

            var show = () => {
                running = true;
                if (delay == null) {
                    element.style.display = "";
                } else {
                    timeout = setTimeout(e => {
                        element.style.display = "";
                    }, +delay);
                }
            }

            var hide = () => {
                running = false;
                clearTimeout(timeout);
                element.style.display = "none";
            }

            dotvvm.updateProgressChangeCounter.subscribe(e => {
                let shouldRun = false;

                if (includedQueues.length === 0) {
                    for (const queue of Object.keys(dotvvm.postbackQueues)) {
                        if (excludedQueues.indexOf(queue) < 0 && dotvvm.postbackQueues[queue].noRunning > 0) {
                            shouldRun = true;
                            break;
                        }
                    }
                } else {
                    shouldRun = includedQueues.some(q => dotvvm.postbackQueues[q] && dotvvm.postbackQueues[q].noRunning > 0);
                }

                if (shouldRun) {
                    if (!running) {
                        show();
                    }
                } else {
                    hide();
                }
            });

        }
    }
}
