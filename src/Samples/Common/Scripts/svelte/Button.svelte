<script lang="ts">
	let { text, click, dataUI, ...restProps } = $props();
</script>

<button data-ui={dataUI} onclick={e => click()} {...restProps}>{text}</button>
