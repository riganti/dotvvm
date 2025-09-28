
<script lang="ts">
	import Incrementer from "./Incrementer.svelte"
	console.log(Incrementer)
	export let i = 0
</script>

<Incrementer bind:i={i} />
