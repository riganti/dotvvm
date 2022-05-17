/// <reference path="../../Framework/Framework/obj/typescript-types/dotvvm.d.ts" />
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
        setTimeout(() => this.initializeTemplate(), 5)
    }
    initializeTemplate() {
        const e = this.wrapRef.current
        let context: KnockoutBindingContext = ko.contextFor(e)
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

export const registerReactControl = (ReactControl, defaultProps = {}) => ({
    create: (elm, props, commands, templates) => {
        const initialProps = { ...defaultProps, ...commands, ...templates }
        let currentProps = { ...initialProps, ...props };
        ReactDOM.render(<ReactControl {...currentProps} />, elm);
        return {
            updateProps(updatedProps) {
                currentProps = { ...currentProps, ...updatedProps }
                ReactDOM.render(<ReactControl {...currentProps} />, elm);
            },
            dispose() {
                ReactDOM.unmountComponentAtNode(elm)
            }
        }
    }
});

