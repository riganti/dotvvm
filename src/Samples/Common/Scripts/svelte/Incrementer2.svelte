
<script lang="ts">
	import Incrementer from "./Incrementer.svelte"
	export let i = 0
</script>

<Incrementer bind:i={i} />
