import { type Component, mount, unmount } from 'svelte';

export type KnockoutTemplateSvelteComponent_Props = {
    /** HTML element name which will contain the knockout template. By default a `<div>` wrapper tag is used. */
    wrapperTag?: string

    /** ID of the knockout template to be rendered, knockout will search for a matching `<template id="...">` element.
     * You can get this ID in a DotVVM JsComponent as a property when you use inner element in the dothtml markup. */
    templateName: string

    /** A function which returns the knockout context for the rendered template. This property can not be updated after initialization
     *  @example getChildContext={c => c.extend({ $myTag: 1234 })} */
    getChildContext?: (context: KnockoutBindingContext) => KnockoutBindingContext

    /** When set, a new knockout binding context with `$this` being the specified viewModel.
     * Parent context will be the context of the parent element.
     * Note that this viewModel is expected to be a plain JS object, not wrapped in knockout observables.
     * Updating this property will update the template's data context. */
    viewModel?: any
}

/**
 * Converts Svelte 5 component to DotVVM component usable through `<js:MyComponent />` syntax (or the JsComponent class).
 * See [the complete guide](https://www.dotvvm.com/docs/4.0/pages/concepts/client-side-development/integrate-third-party-controls/svelte).
 *
 * The component will receive all properties, commands and templates as Svelte props.
 *  * Properties are plain JS objects and values, notably they don't contain any knockout observables
 *  * Commands are functions returning a promise, optionally expecting arguments if they were specified in the dothtml markup
 *  * Templates are only string IDs which can be passed to the `KnockoutTemplateSvelteComponent`
 *
 * Additional property `setProps` is passed to the component, which can be used to update the component's properties (if the bound expression is updatable, otherwise it will throw an error).
 * * Usage: `props.setProps({ myProperty: props.myProperty + 1 })`
 */
export const registerSvelteControl = <T extends Record<string, any>>(
    SvelteControl: Component<T>,
    defaultProps: Partial<T> = {}
) => ({
    create: (elm: HTMLElement, props: any, commands: any, templates: any, setProps: (props: any) => void) => {
        const events: Record<string, any> = {}
        for (const cmd of Object.keys(commands)) {
            if (cmd.startsWith('on')) {
                // Convert onHover -> hover, on:hover -> hover
                const eventName = cmd[2] === ':' ? cmd.substring(3) : cmd[2].toLowerCase() + cmd.substring(3)

                // Listen to custom events from the component
                events[eventName] = (event: any) => {
                    commands[cmd](event.detail)
                }
            }
        }

        // Create Svelte 5 component instance
        const propsState = $state({ setProps, ...defaultProps, ...commands, ...templates, ...props })
        const componentProps = new Proxy(propsState, {
            set(target, name, newValue) {
                const success = Reflect.set(target, name, newValue)
                if (success && name in props) {
                    setProps({ [name]: newValue })
                }
                return success
            }
        })
        const componentExports = mount(SvelteControl, { target: elm, props: componentProps, events })

        return {
            updateProps(updatedProps: any) {
                // Bypass componentProps so DotVVM-originated updates are not written back to DotVVM.
                Object.assign(propsState, updatedProps)
            },
            dispose() {
                unmount(componentExports, { outro: true })
            }
        }
    }
});
