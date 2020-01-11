export const getElementByDotvvmId = (id: string) => {
    return <HTMLElement> document.querySelector(`[data-dotvvm-id='${id}']`);
}

/**
 * @deprecated Use addEventListener directly
 */
export function attachEvent(target: any, name: string, callback: (ev: PointerEvent) => any, useCapture: boolean = false) {
    target.addEventListener(name, callback, useCapture);
}

export const isElementDisabled = (element: HTMLElement | null | undefined) =>
    element &&
    ["A", "INPUT", "BUTTON"].indexOf(element.tagName) > -1 &&
    element.hasAttribute("disabled")

export function setIdFragment(idFragment: string | null | undefined) {
    if (idFragment != null) {
        if (location.hash == "#" + idFragment) {
            const element = document.getElementById(idFragment);
            if (element && "function" == typeof element.scrollIntoView) {
                element.scrollIntoView(true);
            }
        } else {
            location.hash = idFragment;
        }
    }
}
