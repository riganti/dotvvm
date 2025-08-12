<!--
@component
Svelte 5 wrapper for knockout `ko.renderTemplate` function.
Specify the `templateName` property to select which template should be rendered.
Optionally, you can use the `viewModel` or `getChildContext` property to set a data context for the template.
Wrapper element may be configured using the `wrapperTag` property, plus any other property will be used as an attribute.
-->

<script lang="ts">
    import { onMount, onDestroy } from "svelte";
    import type { KnockoutTemplateSvelteComponent_Props } from './dotvvm-svelte.svelte.ts';

    interface Props extends KnockoutTemplateSvelteComponent_Props {
        [key: string]: any;
    }

    let {
        wrapperTag = 'div',
        templateName,
        getChildContext,
        viewModel,
        ...restProps
    }: Props = $props();

    let wrapperElement: HTMLElement;
    let viewModelStateManager: any;
    let templateNameObservable: KnockoutObservable<string> = ko.observable('');

    // Use Svelte 5 $effect for reactive updates
    $effect(() => {
        templateNameObservable(templateName);
    });

    $effect(() => {
        if (viewModelStateManager && viewModelStateManager.state != viewModel) {
            viewModelStateManager.setState(viewModel);
        }
    });

    // We don't let knockout initialize the Svelte elements, so we need to get the context from the parent element
    function getKnockoutContext(element: HTMLElement): KnockoutBindingContext {
        while (element) {
            const cx = ko.contextFor(element);
            if (cx) return cx;
            element = element.parentElement!;
        }
        throw new Error("Could not find knockout context");
    }

    function initializeTemplate() {
        if (!wrapperElement) return;
        
        let context: KnockoutBindingContext = getKnockoutContext(wrapperElement);
        
        if (getChildContext) {
            context = getChildContext(context);
        } else if (viewModel !== undefined) {
            const updateEvent = new (window as any).dotvvm.DotvvmEvent("templateInSvelte.newState");
            viewModelStateManager = new (window as any).dotvvm.StateManager(viewModel, updateEvent);
            context = context.createChildContext(viewModelStateManager.stateObservable);
        }
        
        ko.renderTemplate(templateNameObservable, context, {}, wrapperElement);
    }

    onMount(() => {
        initializeTemplate();
    });

    onDestroy(() => {
        ko.cleanNode(wrapperElement)
    });
</script>

<svelte:element this={wrapperTag} bind:this={wrapperElement} {...restProps}>
</svelte:element>
