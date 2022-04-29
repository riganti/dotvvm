<script lang="ts">
	import KoTemplate from './KnockoutTemplateSvelteComponent.svelte'

	export let condition
	export let template1
	export let template2
</script>

<div>
	<KoTemplate
		wrapperTag="p"
		templateName={condition ? template1 : template2}
		getChildContext={c => c.extend({ $kokos: 1 })} />

</div>
