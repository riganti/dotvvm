export function isPrimitive(viewModel: any) {
    return !(viewModel instanceof Object);
}

export const createArray =
    compileConstants.nomodules ?
        <T>(a: ArrayLike<T>) => {
            return ([] as T[]).slice.apply(a)
        } :
        Array.from;

export const hasOwnProperty = (obj: any, prop: string) => Object.prototype.hasOwnProperty.call(obj, prop);
