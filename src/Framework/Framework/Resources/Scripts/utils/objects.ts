export function isPrimitive(viewModel: any) {
    return !viewModel || typeof viewModel != "object";
}

export const createArray = Array.from;

export const hasOwnProperty = (obj: any, prop: string) => Object.prototype.hasOwnProperty.call(obj, prop);

export const keys = Object.keys
