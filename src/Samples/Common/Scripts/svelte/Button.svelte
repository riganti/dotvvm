<script>
	export let text
	export let click
	export let dataUI
</script>

<button data-ui={dataUI} on:click={e => click()}>{text}</button>
