<!--
@component
Svelte wrapper for knockout `ko.renderTemplate` function.
Specify the `templateName` property to select which template should be rendered.
Optionally, you can use the `viewModel` or `getChildContext` property to set a data context for the template.
-->

<script lang="ts">
    import { onMount } from "svelte";


    /** HTML element name which will contain the knockout template. By default a `<div>` wrapper tag is used. */
    export let wrapperTag: string = "div"

    /** ID of the knockout template to be rendered, knockout will search for a matching `<template id="...">` element.
     * You can get this ID in a DotVVM JsComponent as a property when you use inner element in the dothtml markup. */
    export let templateName: string

    /** A function which returns the knockout context for the rendered template. This property can not be updated after initialization
     *  @example getChildContext={c => c.extend({ $myTag: 1234 })} */
    export let getChildContext: (context: KnockoutBindingContext) => KnockoutBindingContext | undefined = undefined
    
    /** When set, a new knockout binding context with `$this` being the specified viewModel.
     * Parent context will be the context of the parent element.
     * Note that this viewModel is expected to be a plain JS object, not wrapped in knockout observables.
     * Upading this property will update the template's data context. */
    export let viewModel: any | undefined = undefined

    let wrapperElement: HTMLElement
    let viewModelStateManager
    let templateNameObservable: KnockoutObservable<string> = ko.observable()

    $: templateNameObservable(templateName)

    function setNewViewModel(vm) {
        if (viewModelStateManager) {
            viewModelStateManager.setState(vm)
        }
    }
    $: setNewViewModel(viewModel)

    // We don't let knockout initialize the Svelte elements, so we need to get the context from the parent element
    function getKnockoutContext(element: HTMLElement) {
        while (element) {
            const cx = ko.contextFor(element)
            if (cx) return cx
            element = element.parentElement
        }
        throw new Error("Could not find knockout context")
    }


    function initializeTemplate() {
        const e = wrapperElement
        let context: KnockoutBindingContext = getKnockoutContext(e)
        if (getChildContext) {
            context = getChildContext(context)
        }
        else if (viewModel !== undefined) {
            const updateEvent = new dotvvm.DotvvmEvent("templateInSvelte.newState")
            viewModelStateManager = new dotvvm.StateManager(viewModel, updateEvent)
            context = context.createChildContext(viewModelStateManager.stateObservable)
        }
        ko.renderTemplate(templateNameObservable, context, {}, e)
    }

    onMount(() => {
        initializeTemplate()
    })
</script>


<svelte:element this={wrapperTag} bind:this={wrapperElement}>
</svelte:element>
