import bindingHandlers from './all-handlers'

export default () => {
    for (const h of Object.keys(bindingHandlers)) {
        ko.bindingHandlers[h] = bindingHandlers[h];
    }
}
