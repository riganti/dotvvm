/** Runs the callback in the next event loop cycle */ 
export const defer = <T>(callback: () => T) => Promise.resolve().then(callback)
export const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms))
