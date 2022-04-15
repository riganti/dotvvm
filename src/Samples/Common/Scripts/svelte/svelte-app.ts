/// <reference path="../../../../Framework/Framework/obj/typescript-types/dotvvm.d.ts" />

import Chart from './Chart.svelte'
import Incrementer from './Incrementer2.svelte'

function svelte_bind(component, name, callback) {
    const index = component.$$.props[name];
    if (index !== undefined) {
        component.$$.bound[index] = callback;
        callback(component.$$.ctx[index]);
    }
}

export const registerSvelteControl = (Control, defaultProps = {}) => ({
    create: (elm, props, commands, templates, setProps) => {
        const initialProps = { ...defaultProps, ...commands, ...templates }
        let currentProps = { ...initialProps, ...props };

        const c = new Control({
            target: elm,
            props: currentProps,
        });

        const propertyUpdated = (p: string) => (val: any) => {
            if (val !== currentProps[p]) {
                const updateObj = { [p]: val }
                currentProps = { ...currentProps, ...updateObj }
                setProps(updateObj)
            }
        }

        for (const p of Object.keys(props)) {
            svelte_bind(c, p, propertyUpdated(p))
        }

        for (const cmd of Object.keys(commands)) {
            if (cmd.startsWith('on')) {
                // convert onHover -> hover, on:hover -> hover
                const eventName = cmd[2] == ':' ? cmd.substring(3) : cmd[2].toLowerCase() + cmd.substring(3)
                c.$on(eventName, (event) => commands[cmd](event.detail))
            }
        }


        // ReactDOM.render(<ReactControl {...currentProps} />, elm);
        return {
            updateProps(updatedProps) {
                // currentProps = { ...currentProps, ...updatedProps }
                // ReactDOM.render(<ReactControl {...currentProps} />, elm)
                currentProps = { ...currentProps, ...updatedProps }
                c.$set(updatedProps)
            },
            dispose() {
                c.$destroy()
            }
        }
    }
});


// DotVVM Context importer 
export default (context) => ({
    $controls: {
        chart: registerSvelteControl(Chart, { context, onMouse() { /* default empty method */ } }),
        incrementer: registerSvelteControl(Incrementer)
    }
})
