<script lang="ts">
	import { KnockoutTemplateSvelteComponent } from 'dotvvm-jscomponent-svelte';

	// const { condition, template1, template2, ...restProps } = $props();
	export let condition: boolean;
	export let template1: string;
	export let template2: string;


	console.assert(KnockoutTemplateSvelteComponent != null)
</script>

<div {...$$restProps}>
	<KnockoutTemplateSvelteComponent
		wrapperTag="p"
		class={condition ? 'template1' : 'template2'}
		templateName={condition ? template1 : template2}
		getChildContext={c => c.extend({ $kokos: 1 })} />

</div>
