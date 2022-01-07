
<script lang="ts">
	export let i = 0
</script>

<div>
	Svelte: {i}
	<button type="button" on:click={_ => i++}> + </button>
	<button type="button" on:click={_ => i--}> - </button>
</div>
<style>
	
</style>
