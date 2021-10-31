/// <reference path="../../../../Framework/Framework/obj/typescript-types/dotvvm.d.ts" />
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { KnockoutTemplateReactComponent, registerReactControl} from 'dotvvm.jscomponent.react';
import { LineChart, XAxis, Tooltip, CartesianGrid, Line, Dot } from 'recharts';
import type { StateManager } from 'state-manager';

// react component
function RechartComponent(props: any) {
    const onMouse = props.onMouse ?? (() => { })
    return (
        <LineChart
            width={400}
            height={400}
            data={props.data}
            margin={{ top: 5, right: 20, left: 10, bottom: 5 }} >
            <XAxis dataKey="name" />
            <Tooltip />
            <CartesianGrid stroke="#f5f5f5" />
            {
                Object.keys(props.data[0]).slice(1).map((s, i) =>
                    <Line type="monotone"
                        dataKey={s}
                        stroke={"#" + (i * 4).toString() + "87908"}
                        yAxisId={i}
                        onMouseEnter={(_) => onMouse(s)} />)
            }
        </LineChart>
    );
}

function TemplateSelector(props) {
    return <div>
        <KnockoutTemplateReactComponent
            templateName={props.condition ? props.template1 : props.template2}
            getChildContext={c => c.extend({ $kokos: 1 })} />
    </div>
}

// DotVVM Context importer 
export default (context) => ({
    $controls: {
        recharts: registerReactControl(RechartComponent, { context }),
        TemplateSelector: registerReactControl(TemplateSelector)
    }
})
