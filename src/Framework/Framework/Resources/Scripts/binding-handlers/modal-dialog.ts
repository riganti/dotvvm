export default {
    "dotvvm-modal-open": {
        init(element: HTMLDialogElement, valueAccessor: () => any) {
            element.addEventListener("close", () => {
                const value = valueAccessor();
                if (ko.isWriteableObservable(value)) {
                    // if the value is object, set it to null
                    value(typeof value.peek() == "boolean" ? false : null)
                }
            })
        },
        update(element: HTMLDialogElement, valueAccessor: () => any) {
            const value = ko.unwrap(valueAccessor()),
                  shouldOpen = value != null && value !== false;
            if (shouldOpen != element.open) {
                if (shouldOpen) {
                    element.showModal()
                } else {
                    element.close()
                }
            }
        },
    },
    "dotvvm-model-backdrop-close": {
        init(element: HTMLDialogElement, valueAccessor: () => any) {
            // closes the dialog when the backdrop is clicked
            element.addEventListener("click", (e) => {
            if (e.target == element) {
                    const elementRect = element.getBoundingClientRect(),
                          x = e.clientX,
                          y = e.clientY;
                    if (x < elementRect.left || x > elementRect.right || y < elementRect.top || y > elementRect.bottom) {
                        if (ko.unwrap(valueAccessor())) {
                            element.close();
                        }
                    }
                }
            })
        }
    }
}
