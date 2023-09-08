
export default function (context) {
    return {
        setViewModelProperty(name, value) {
            console.assert(context.state !== undefined)
            console.assert(context.setState)
            console.assert(context.updateState)
            console.assert(context.patchState)
            context.updateState(x => ({ ...x, [name]: value }))
        },
        setControlProperty(property, value) {
            const o = context.properties[property];
            console.assert(ko.isObservable(o))
            console.assert(o.state !== undefined)
            console.assert(o.setState)
            console.assert(o.updateState)
            console.assert(o.patchState)
            context.properties[property].setState(value)
        }
    }
}
