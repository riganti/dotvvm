import React from 'react';
import ReactDOM from 'react-dom';
import { LineChart, XAxis, Tooltip, CartesianGrid, Line, Dot } from 'recharts';

// react component
function RechartComponent(props) {
    const onMouse = props.onMouse ?? (() => {})
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

const registerReactControl = (ReactControl, defaultProps) => ({
    create: (elm, props, commands, templates) => {
        // TODO: templates
        const initialProps = { ...defaultProps, ...commands }
        let currentProps = { ...initialProps, ...props };
        ReactDOM.render(<ReactControl {...currentProps} />, elm);
        return {
            updateProps(updatedProps) {
                currentProps = { ...currentProps, ...updatedProps }
                ReactDOM.render(<ReactControl {...currentProps} />, elm);
            },
            dispose() {
                ReactDOM.unmountComponentAtNode(elm)
            }
        }
    }
});

// DotVVM Context importer 
export default (context) => ({
    $controls: {
        recharts: registerReactControl(RechartComponent, { context })
    }
})
