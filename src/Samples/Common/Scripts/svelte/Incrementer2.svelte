
<script lang="ts">
	import Incrementer from "./Incrementer.svelte"
	console.log(Incrementer)
	export let i = 0
	export let useSetProps = false
	export let setProps: (updatedProps: { i: number }) => void
</script>

{#if useSetProps}
	<div>
		Svelte: {i}
		<button type="button" on:click={() => setProps({ i: i + 1 })}> + </button>
		<button type="button" on:click={() => setProps({ i: i - 1 })}> - </button>
	</div>
{:else}
	<Incrementer bind:i={i} />
{/if}
