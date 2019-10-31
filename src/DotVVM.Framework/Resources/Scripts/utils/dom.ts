export const getElementByDotvvmId = (id) => {
    return <HTMLElement>document.querySelector(`[data-dotvvm-id='${id}']`);
}
