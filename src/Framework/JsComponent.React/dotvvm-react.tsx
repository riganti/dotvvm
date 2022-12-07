import * as React from 'react';
import * as ReactDOM from 'react-dom';
import type { StateManager } from 'state-manager';

export type KnockoutTemplateReactComponent_Props = {
    /** HTML element name which will contain the knockout template. By default a `<div>` wrapper tag is used. */
    wrapperTag: string

    /** ID of the knockout template to be rendered, knockout will search for a matching `<template id="...">` element.
     * You can get this ID in a DotVVM JsComponent as a property when you use inner element in the dothtml markup. */
    templateName: string

    /** A function which returns the knockout context for the rendered template. This property can not be updated after initialization
     *  @example getChildContext={c => c.extend({ $myTag: 1234 })} */
    getChildContext?: (context: KnockoutBindingContext) => KnockoutBindingContext

    /** When set, a new knockout binding context with `$this` being the specified viewModel.
     * Parent context will be the context of the parent element.
     * Note that this viewModel is expected to be a plain JS object, not wrapped in knockout observables.
     * Upading this property will update the template's data context. */
    viewModel?: any
}

// We don't let knockout initialize the React elements, so we need to get the context from the parent element
function getKnockoutContext(element: HTMLElement) {
    while (element) {
        const cx = ko.contextFor(element)
        if (cx) return cx
        element = element.parentElement
    }
    throw new Error("Could not find knockout context")
}

/** React wrapper for knockout `ko.renderTemplate` function.
 * Specify the `templateName` property to select which template should be rendered.
 * Optionally, you can use the `viewModel` or `getChildContext` property to set a data context for the template. */
export class KnockoutTemplateReactComponent extends React.Component<KnockoutTemplateReactComponent_Props> {
    static defaultProps = {
        wrapperTag: "div"
    }
    wrapRef: React.RefObject<HTMLElement> = React.createRef()
    templateName = ko.observable()
    viewModelStateManager?: StateManager<any>

    // TODO: how to dispose the template?
    // componentWillUnmount() {
    // }
    componentDidMount() {
        this.initializeTemplate()
    }
    initializeTemplate() {
        const e = this.wrapRef.current
        let context: KnockoutBindingContext = getKnockoutContext(e)
        if (this.props.getChildContext) {
            context = this.props.getChildContext(context)
        }
        else if (this.props.viewModel !== undefined) {
            const updateEvent = new dotvvm.DotvvmEvent("templateInReact.newState")
            this.viewModelStateManager = new dotvvm.StateManager(this.props.viewModel, updateEvent)
            context = context.createChildContext(this.viewModelStateManager.stateObservable)
        }
        this.updateStuff()
        ko.renderTemplate(this.templateName, context, {}, e)
    }
    componentDidUpdate() {
        this.updateStuff()
    }
    updateStuff() {
        if (this.templateName() !== this.props.templateName)
            this.templateName(this.props.templateName)
        if (this.viewModelStateManager) {
            this.viewModelStateManager.setState(this.props.viewModel)
        }
    }
    render() {
        
        return React.createElement(this.props.wrapperTag, { ref: this.wrapRef })
    }
}

/** Converts React component to DotVVM component usable through `<js:MyComponent />` syntax (or the JsComponent class)
 * See [the complete guide](https://www.dotvvm.com/docs/4.0/pages/concepts/client-side-development/integrate-third-party-controls/react).
 * 
 * The component will receive all properties, commands and templates as it's React props.
 *  * Properties are plain JS objects and values, notably they don't contain any knockout observables
 *  * Commands are functions returning a promise, optionally expecting arguments if they were specified in the dothtml markup
 *  * Templates are only string IDs which can be passed to the `<KnockoutTemplateReactComponent templateName={props.theTemplate} />` component
 * 
 * Additional property `setProps` is passed to the component, which can be used to update the component's properties (if the bound expression is updatable, otherwise it will throw an error).
 * * Usage: `props.setProps({ myProperty: props.myProperty + 1 })`
 */
export const registerReactControl = (ReactControl, defaultProps = {}) => ({
    create: (elm, props, commands, templates, setPropsRaw) => {
        const initialProps = { setProps, ...defaultProps, ...commands, ...templates }
        let currentProps = { ...initialProps, ...props }
        rerender()
        return {
            updateProps(updatedProps) {
                currentProps = { ...currentProps, ...updatedProps }
                rerender()
            },
            dispose() {
                ReactDOM.unmountComponentAtNode(elm)
            }
        }

        function rerender() {
            ReactDOM.render(<ReactControl {...currentProps} />, elm);
        }

        function setProps(updatedProps) {
            currentProps = { ...currentProps, ...updatedProps }
            setPropsRaw(updatedProps)
            rerender()
        }
    }
});

