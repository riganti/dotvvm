export default {
    'dotvvm-enable': {
        update: (element: HTMLInputElement, valueAccessor: () => KnockoutObservable<boolean>) => {
            const value = ko.unwrap(valueAccessor());
            if (value && element.disabled) {
                element.disabled = false;
                element.removeAttribute("disabled");
            } else if (!value && !element.disabled) {
                element.disabled = true;
                element.setAttribute("disabled", "disabled");
            }
        }
    }
}
