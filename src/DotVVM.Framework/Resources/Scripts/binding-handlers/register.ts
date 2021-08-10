import bindingHandlers from './all-handlers'
import { keys } from '../utils/objects';

export default () => {
    for (const h of keys(bindingHandlers)) {
        ko.bindingHandlers[h] = bindingHandlers[h];
    }
}
