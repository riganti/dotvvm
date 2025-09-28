<script lang="ts">
	import { LayerCake, Svg, Html } from 'layercake';
	import { line } from 'd3-shape';

	console.log(LayerCake, Svg, Html, line) // workaround for some bundler bug

	let { data, onSelected } = $props()

	let selectedItem = $state(null);

	let transformedData = $derived(data.map((row, index) => ({
		x: index,
		Line1: row.Line1,
		Line2: row.Line2,
		Line3: row.Line3
	})))

	const seriesNames = ['Line1', 'Line2', 'Line3'];
	const seriesColors = ['#ff3e00', '#40b0ff', '#40ff40'];

	$effect(() => {
		if (selectedItem && onSelected) {
			onSelected(`${selectedItem.series}: ${selectedItem.value}`);
		}
	});

	function handlePointClick(series, value, x, y) {
		selectedItem = { series, value, x, y };
	}
</script>

<div class="chart">
	<LayerCake
		padding={{ top: 20, right: 30, bottom: 40, left: 50 }}
		x={d => d.x}
		y={seriesNames}
		xDomain={[0, transformedData.length - 1]}
		yDomain={[0, null]}
		data={transformedData}
		let:xScale
		let:yScale
		let:width
		let:height
	>
		<Svg>
			<!-- Grid lines -->
			<g class="grid">
				{#each yScale.ticks(5) as tick}
					<line
						x1={0}
						x2={width}
						y1={yScale(tick)}
						y2={yScale(tick)}
						stroke="#f0f0f0"
						stroke-width="1"
					/>
				{/each}
				{#each xScale.ticks(5) as tick}
					<line
						x1={xScale(tick)}
						x2={xScale(tick)}
						y1={0}
						y2={height}
						stroke="#f0f0f0"
						stroke-width="1"
					/>
				{/each}
			</g>

			<!-- Axes -->
			<g class="axis x-axis">
				<line x1={0} x2={width} y1={height} y2={height} stroke="#333" stroke-width="1"/>
				{#each xScale.ticks(5) as tick}
					<g transform="translate({xScale(tick)}, {height})">
						<line y1="0" y2="6" stroke="#333"/>
						<text y="20" text-anchor="middle" font-size="12" fill="#666">
							{tick}
						</text>
					</g>
				{/each}
			</g>

			<g class="axis y-axis">
				<line x1={0} x2={0} y1={0} y2={height} stroke="#333" stroke-width="1"/>
				{#each yScale.ticks(5) as tick}
					<g transform="translate(0, {yScale(tick)})">
						<line x1="-6" x2="0" stroke="#333"/>
						<text x="-10" text-anchor="end" font-size="12" fill="#666" dy="0.35em">
							{tick}
						</text>
					</g>
				{/each}
			</g>

			<!-- Data lines -->
			{#each seriesNames as series, i}
				{@const pathData = line()
					.x(d => xScale(d.x))
					.y(d => yScale(d[series]))
					(transformedData)}
				<path
					d={pathData}
					fill="none"
					stroke={seriesColors[i]}
					stroke-width="2"
				/>
			{/each}

			<!-- Data points -->
			{#each transformedData as d, i}
				{#each seriesNames as series, j}
					<circle
						cx={xScale(d.x)}
						cy={yScale(d[series])}
						r="4"
						fill={seriesColors[j]}
						stroke="white"
						stroke-width="2"
						style="cursor: pointer"
						onclick={() => handlePointClick(series, d[series], xScale(d.x), yScale(d[series]))}
					/>
				{/each}
			{/each}
		</Svg>
	</LayerCake>

	{#if selectedItem}
		<div class="selection-info">
			Selected: {selectedItem.series} = {selectedItem.value}
		</div>
	{/if}
</div>

<style>
	.chart {
		height: 400px;
		padding: 1rem;
		border: 1px solid #ccc;
		border-radius: 4px;
		margin: 1rem 0;
		position: relative;
	}

	.selection-info {
		margin-top: 1rem;
		padding: 0.5rem;
		background-color: #f9f9f9;
		border-radius: 4px;
		font-size: 14px;
		border: 1px solid #eee;
	}

	:global(.layercake-container) {
		height: 100%;
	}

	.grid line {
		opacity: 0.7;
	}

	.axis text {
		font-family: sans-serif;
	}
</style>
