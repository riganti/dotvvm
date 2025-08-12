import Chart from './Chart.svelte'
import TemplateSelector from './TemplateSelector.svelte'
import Button from './Button.svelte'
import { registerSvelteControl, KnockoutTemplateSvelteComponent } from 'dotvvm-jscomponent-svelte'
import Incrementer from './Incrementer.svelte'
import Incrementer2 from './Incrementer2.svelte'

// DotVVM Context importer 
export default (context) => ({
    $controls: {
        chart: registerSvelteControl(Chart, { context, onSelected() { /* default empty method */ } }),
        incrementer: registerSvelteControl(Incrementer2),
        TemplateSelector: registerSvelteControl(TemplateSelector),
        Button: registerSvelteControl(Button),
    }
})
