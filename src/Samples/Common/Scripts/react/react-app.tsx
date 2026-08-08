import * as React from 'react';
import { KnockoutTemplateReactComponent, registerReactControl } from 'dotvvm-jscomponent-react';
import { LineChart, XAxis, YAxis, Tooltip, CartesianGrid, Line } from 'recharts';

// react component
function RechartComponent(props: any) {
    const seriesNames = ["Line1", "Line2", "Line3"]
    return (
        <LineChart
            width={400}
            height={400}
            data={props.data}
            margin={{ top: 5, right: 20, left: 10, bottom: 5 }} >
            <XAxis dataKey="Name" />
            <YAxis hide />
            <Tooltip />
            <CartesianGrid stroke="#f5f5f5" />
            {
                seriesNames.map((s, i) =>
                    <Line type="monotone"
                        dataKey={s}
                        stroke={"#" + (i * 4).toString() + "87908"}
                        onMouseEnter={(_) => props.onMouse(s)} />)
            }
        </LineChart>
    );
}

function TemplateSelector(props) {
    return <div>
        <KnockoutTemplateReactComponent
            wrapperTag="p"
            wrapperAttributes={{ className: props.condition ? "template1" : "template2" }}
            templateName={props.condition ? props.template1 : props.template2}
            getChildContext={c => c.extend({ $kokos: 1 })} />
    </div>
}

const Button = ({ text, click, dataUI }) =>
    <button onClick={e => click()} data-ui={dataUI}>{text}</button>

// DotVVM Context importer 
export default (context) => ({
    $controls: {
        recharts: registerReactControl(RechartComponent, { context, onMouse() { /* default empty method */ } }),
        TemplateSelector: registerReactControl(TemplateSelector),
        Button: registerReactControl(Button)
    }
})
