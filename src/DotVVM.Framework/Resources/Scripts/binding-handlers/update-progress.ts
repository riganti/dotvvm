import { postbackQueues, updateProgressChangeCounter } from "../postback/queue"

export default {
    "dotvvm-UpdateProgress-Visible": {
        init(element: HTMLElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            element.style.display = "none";
            const delay = element.getAttribute("data-delay");

            const includedQueues = (element.getAttribute("data-included-queues") || "").split(",").filter(i => i.length > 0);
            const excludedQueues = (element.getAttribute("data-excluded-queues") || "").split(",").filter(i => i.length > 0);

            let timeout: any;
            let running = false;

            const show = () => {
                running = true;
                if (delay == null) {
                    element.style.display = "";
                } else {
                    timeout = setTimeout(() => {
                        element.style.display = "";
                    }, +delay);
                }
            }

            const hide = () => {
                running = false;
                clearTimeout(timeout);
                element.style.display = "none";
            }

            updateProgressChangeCounter.subscribe(() => {
                let shouldRun = false;

                if (includedQueues.length === 0) {
                    for (const queue of Object.keys(postbackQueues)) {
                        if (excludedQueues.indexOf(queue) < 0 && postbackQueues[queue].runningPostbacksCount > 0) {
                            shouldRun = true;
                            break;
                        }
                    }
                } else {
                    shouldRun = includedQueues.some(q => postbackQueues[q] && postbackQueues[q].runningPostbacksCount > 0);
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
