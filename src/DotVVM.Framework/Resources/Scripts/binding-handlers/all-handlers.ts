import textbox from './textbox-text'
import textboxFocus from './textbox-select-all-on-focus'
import ssrForeach from './SSR-foreach'
import aliases from './introduce-alias'
import columnVisible from './table-columnvisible'
import enable from './enable'
import checkbox from './checkbox'
import updateProgress from './update-progress'

type KnockoutHandlerDictionary = {
    [name: string]: KnockoutBindingHandler
}
const allHandlers: KnockoutHandlerDictionary = {
    ...textbox,
    ...ssrForeach,
    ...aliases,
    ...textboxFocus,
    ...columnVisible,
    ...enable,
    ...checkbox,
    ...updateProgress
}

export default allHandlers
