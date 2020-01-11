
type DotvvmValidationActions = {
    [name: string]: (element: HTMLElement, errorMessages: string[], param: any) => void;
}

export const elementActions: DotvvmValidationActions = {
    // shows the element when it is valid
    hideWhenValid(element: HTMLElement, errorMessages: string[]) {
        if (errorMessages.length > 0) {
            element.style.display = "";
        } else {
            element.style.display = "none";
        }
    },

    // adds a CSS class when the element is not valid
    invalidCssClass(element: HTMLElement, errorMessages: string[], classAttribute: string) {
        const classes = classAttribute.split(/\s+/);
        for (const className in classes) {
            if (errorMessages.length > 0) {
                element.classList.add(className);
            } else {
                element.classList.remove(className);
            }
        }
    },

    // sets the error message as the title attribute
    setToolTipText(element: HTMLElement, errorMessages: string[]) {
        if (errorMessages.length > 0) {
            element.title = errorMessages.join(" ");
        } else {
            element.title = "";
        }
    },

    // displays the error message
    showErrorMessageText(element: any, errorMessages: string[]) {
        element[element.innerText ? "innerText" : "textContent"] = errorMessages.join(" ");
    }
}
