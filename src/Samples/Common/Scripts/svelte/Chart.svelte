
<script lang="ts">
	// adjusted example from https://pancake-charts.surge.sh/
	import * as Pancake from '@sveltejs/pancake';
	import { createEventDispatcher } from 'svelte';

	export let data

	let y1, y2, keys
	$: {
		y1 = +Infinity
		y2 = -Infinity

		keys = Object.keys(data[0]).slice(1).filter(x => x != "Name")
		
		data.forEach(row => {
			keys.forEach(k => {
				const d = row[k]
				if (d < y1) y1 = d;
				if (d > y2) y2 = d;
			})
		})
	}

	let closest;

	function getPoints(data, key) {
		return data.map((row, x) => ({ y: row[key], x }))
	}

	const dispatch = createEventDispatcher();

	$: {
		if (closest)
			dispatch("selected", closest.key)
	}

	$: points = data.flatMap((row, x) => {
		return keys.map(key => ({
			y: row[key],
			x,
			key
		}))
	});
</script>

<div class="chart">
	<Pancake.Chart {y1} {y2} x1={0} x2={data.length}>
		<Pancake.Grid horizontal count={5} let:value>
			<div class="grid-line horizontal"><span>{value}</span></div>
		</Pancake.Grid>

		<Pancake.Grid vertical count={5} let:value>
			<span class="x-label">{value}</span>
		</Pancake.Grid>

		<Pancake.Svg>
			{#each keys as k}
				<Pancake.SvgLine data={getPoints(data, k)} let:d>
					<path class="data" {d}></path>
				</Pancake.SvgLine>
			{/each}

			{#if closest}
				<Pancake.SvgLine data={getPoints(data, closest.key)} let:d>
					<path class="highlight" {d}></path>
				</Pancake.SvgLine>
			{/if}
		</Pancake.Svg>

		{#if closest}
			<Pancake.Point x={closest.x} y={closest.y}>
				<span class="annotation-point"></span>
				<div class="annotation" style="transform: translate(-{100 * (closest.x / data.length)}%,0)">
					<strong>{closest.key}</strong>
					<span>{closest.x}: {closest.y}</span>
				</div>
			</Pancake.Point>
		{/if}

		<Pancake.Quadtree data={points} bind:closest/>
	</Pancake.Chart>
</div>
<style>
	.chart {
		height: 400px;
		padding: 3em 0 2em 2em;
		margin: 0 0 36px 0;
	}

	.grid-line {
		position: relative;
		display: block;
	}

	.grid-line.horizontal {
		width: calc(100% + 2em);
		left: -2em;
		border-bottom: 1px dashed #ccc;
	}

	.grid-line span {
		position: absolute;
		left: 0;
		bottom: 2px;
		font-family: sans-serif;
		font-size: 14px;
		color: #999;
	}

	.x-label {
		position: absolute;
		width: 4em;
		left: -2em;
		bottom: -22px;
		font-family: sans-serif;
		font-size: 14px;
		color: #999;
		text-align: center;
	}

	path.data {
		stroke: rgba(0,0,0,0.2);
		stroke-linejoin: round;
		stroke-linecap: round;
		stroke-width: 1px;
		fill: none;
	}

	.highlight {
		stroke: #ff3e00;
		fill: none;
		stroke-width: 2;
	}

	.annotation {
		position: absolute;
		white-space: nowrap;
		bottom: 1em;
		line-height: 1.2;
		background-color: rgba(255,255,255,0.9);
		padding: 0.2em 0.4em;
		border-radius: 2px;
	}

	.annotation-point {
		position: absolute;
		width: 10px;
		height: 10px;
		background-color: #ff3e00;
		border-radius: 50%;
		transform: translate(-50%,-50%);
	}

	.annotation strong {
		display: block;
		font-size: 20px;
	}

	.annotation span {
		display: block;
		font-size: 14px;
	}
</style>
