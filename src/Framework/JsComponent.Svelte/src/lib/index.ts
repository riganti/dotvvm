// Reexport your entry components here

export type { KnockoutTemplateSvelteComponent_Props } from './dotvvm-svelte.svelte.js';
import KnockoutTemplateSvelteComponent from './KnockoutTemplateSvelteComponent.svelte';
import { registerSvelteControl } from './dotvvm-svelte.svelte.js';

export {
    KnockoutTemplateSvelteComponent,
    registerSvelteControl
}
