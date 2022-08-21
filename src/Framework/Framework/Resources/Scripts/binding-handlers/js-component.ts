import { newState } from '../events'
import { unmapKnockoutObservables } from '../state-manager'
import { keys } from '../utils/objects'
import { findComponent } from '../viewModules/viewModuleManager'

// handler dotvvm-textbox-text
export default {
    "dotvvm-js-component": {
        // dotvvm-js-component: {
        //    name: "name of component"
        //    view: "element or view id" or omitted if looking for globally registered control
        //    props: { x: MyObservable, y: "1234" }
        //    commands: { click: (args) => ...command }
        //    templates: { headerContent: "knockout template id" }
        //    update: { y: (newValue) => ... explicit action when y changes }
        init(element: HTMLInputElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) {

            function getCurrentProps({ props }: any) {
                if (!props) return {}

                const result: { [key: string]: any } = {}
                for (const [n, v] of Object.entries(props)) {

                    if (ko.isObservable(v)) {
                        result[n] = "state" in v ? v.state : unmapKnockoutObservables(v)
                    } else {
                        result[n] = v
                    }
                }
                return result
            }

            function setProps(newProps: { [key: string]: any }) {
                ko.ignoreDependencies(() => {
                    const { props, update } = valueAccessor()
                    for (const [name, val] of Object.entries(newProps)) {
                        lastProps[name] = val
                        if (update && name in update) {
                            update[name](val)
                        } else {
                            const prop = props[name]
                            if (!ko.isObservable(prop) || !("setState" in prop)) {
                                throw new Error(`Can not set property ${name} as it's not observable with setState method: ${prop}`)
                            }
                            prop.setState!(val)
                        }
                    }
                })
            }

            const value0 = valueAccessor()

            let lastProps = getCurrentProps(value0)
            var [module, componentF] = findComponent(value0.view, value0.name)
            const component = componentF.create(
                element,
                lastProps,
                value0.commands ?? {},
                value0.templates ?? {},
                setProps
            )

            function update() {
                const value = valueAccessor()
                const currentProps = getCurrentProps(value)

                const toUpdate: { [key: string]: any } = {}
                for (const [n, v] of Object.entries(currentProps)) {
                    if (lastProps[n] !== v) {
                        toUpdate[n] = v
                    }
                }
                if (keys(toUpdate).length > 0) {
                    component.updateProps(toUpdate)
                }

                lastProps = currentProps
            }
            newState.subscribe(update)
            // run update when something observable changes
            let updaterComputed = ko.computed(() => update())
            ko.utils.domNodeDisposal.addDisposeCallback(element, () => {
                updaterComputed.dispose()
                component.dispose()
                newState.unsubscribe(update)
            })
        }
    }
}
