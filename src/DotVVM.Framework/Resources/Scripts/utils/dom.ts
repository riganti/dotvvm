export const getElementByDotvvmId = (id: string) => {
    return <HTMLElement>document.querySelector(`[data-dotvvm-id='${id}']`);
}

/**
 * @deprecated Use addEventListener directly
 */
export function attachEvent(target: any, name: string, callback: (ev: PointerEvent) => any, useCapture: boolean = false) {
    target.addEventListener(name, callback, useCapture);
}
