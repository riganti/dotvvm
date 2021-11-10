/// <reference path="../../Framework/Framework/obj/typescript-types/dotvvm.d.ts" />
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import type { StateManager } from 'state-manager';

export type KnockoutTemplateReactComponent_Props = {
    wrapperTag: string
    templateName: string
    getChildContext?: (context: KnockoutBindingContext) => KnockoutBindingContext
    viewModel?: any
}

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

