/// <reference path="typings/virtual-dom/virtual-dom.d.ts" />
type StateUpdate<TViewModel> = (initial: TViewModel) => TViewModel;
type RenderContext<TViewModel> = {
    // timeFromStartGetter: () => number
    // secondsTimeGetter: () => Date
    update: (updater: StateUpdate<TViewModel>) => void
    dataContext: TViewModel
    parentContext?: RenderContext<any>
    "@extensions"?: { [name: string]: any }
}
type RenderFunction<TViewModel> = (context: RenderContext<TViewModel>) => virtualDom.VTree;
class TwoWayBinding<T> {
    constructor(
        public readonly update: (updater: StateUpdate<T>) => void,
        public readonly value: T
    ) {}
}

const createArray = <T>(a: { [i: number]: T }): T[] => Array.prototype.slice.call(a)

class HtmlElementPatcher {
    private previousDom: virtualDom.VNode | null
    constructor(
        public element: HTMLElement,
        initialDom: virtualDom.VNode | null) {
        this.previousDom = initialDom;
    }
    public applyDom(dom: virtualDom.VNode) {
        if (this.previousDom == null) {
            const newElement = virtualDom.create(dom, {})
            this.element.parentElement!.replaceChild(
                newElement,
                this.element
            )
            this.element = newElement
        } else {
            var diff = virtualDom.diff(this.previousDom, dom);
            virtualDom.patch(this.element, diff);
        }
        this.previousDom = dom;
    }
}
class Renderer<TViewModel> {
    public readonly renderedStateObservable;
    private _state: TViewModel
    public get state() {
        return this._state
    }
    private _isDirty: boolean;
    public get isDirty() {
        return this._isDirty
    }

    constructor(
        initialState: TViewModel,
        public readonly renderFunctions: RenderFunction<TViewModel>[],
        public readonly vdomDispatcher: (dom: virtualDom.VNode[]) => void) {
        this.setState(initialState)
        this.renderedStateObservable = ko.observable(initialState)
    }

    public dispatchUpdate() {
        if (!this._isDirty) {
            this._isDirty = true;
            window.requestAnimationFrame(this.rerender.bind(this))
        }
    }

    private startTime: number | null = null
    private rerender(time: number) {
        if (this.startTime === null) this.startTime = time
        this._isDirty = false
        this.renderedStateObservable(this._state);
        var vdom = this.renderFunctions.map(f => f({
            update: this.update.bind(this),
            dataContext: this._state
        }))
        console.log("Dispatching new VDOM")
        this.vdomDispatcher(<virtualDom.VNode[]>vdom)
    }

    public setState(newState: TViewModel) {
        if (newState == null) throw new Error("State can't be null or undefined.")
        this.dispatchUpdate();
        return this._state = newState
    }

    public update(updater: StateUpdate<TViewModel>) {
        return this.setState(updater(this._state))
    }
}


namespace RendererInitializer {
    export type ConstantOrFunction<T> =
        | { readonly type: "constant"; readonly constant: T }
        | { readonly type: "func"; readonly dataContextDepth: number; readonly elements: RenderNodeAst[]; readonly func: (dataContext: RenderContext<any>, elements: virtualDom.VTree[]) => T }
    export interface AttrDescriptor { name: ConstantOrFunction<string>; value: ConstantOrFunction<any> }
    export type AssignedPropDescriptor =
        | { readonly type: "attr", readonly attr: AttrDescriptor }
        | { readonly type: "decorator", readonly fn: ConstantOrFunction<(node: virtualDom.VTree) => virtualDom.VTree> }
    export type RenderNodeAst =
        | ConstantOrFunction<virtualDom.VTree>
        | { readonly type: "ast"; readonly name: ConstantOrFunction<string>; readonly attributes: AttrDescriptor[]; readonly content: RenderNodeAst[] }
        | { readonly type: "text"; readonly content: ConstantOrFunction<string> }


    export const astConstant = <T>(val: T): ConstantOrFunction<T> => ({ type: "constant", constant: val })
    export const astFunc = <T>(dataContextDepth: number, elements: RenderNodeAst[], func: (dataContext: RenderContext<any>, elements: virtualDom.VTree[]) => T): ConstantOrFunction<T> => ({ type: "func", dataContextDepth: dataContextDepth, elements: elements, func: func })
    // export const bindConstantOrFunction = <T, U>(source: ConstantOrFunction<T>, map: (val: T) => ConstantOrFunction<U>, maxDataContextDepth = 1000000) : ConstantOrFunction<U> => {
    //     if (source.type == "constant") return map(source.constant);
    //     else return { type: "func", dataContextDepth: Math.max(source.dataContextDepth, maxDataContextDepth), elements }
    // }
    export const mapConstantOrFunction = <T, U>(source: ConstantOrFunction<T>, map: (val: T, myElements: virtualDom.VTree[]) => U, myElements: RenderNodeAst[]) : ConstantOrFunction<U> => {
        if (source.type == "constant") return astFunc(0, myElements, (a, e) => map(source.constant, e));
        else return { type: "func", dataContextDepth: source.dataContextDepth, elements: myElements.concat(source.elements), func: (a, b) => map(source.func(a, myElements.length == 0 ? b : b.slice(myElements.length)), b) }
    }

    const createAttrAst = (node: Attr): AssignedPropDescriptor => {
        return {
            type: "attr",
            attr: {
                name: astConstant(node.name),
                value: astConstant(node.value)
            }
        }
    }

    const applyPropsToElement = (el: RenderNodeAst, props: AssignedPropDescriptor[]): RenderNodeAst => {
        if (el.type != "ast") throw new Error()

        const attributes = props.filter(a => a.type == "attr").map(a => (a as { attr: AttrDescriptor }).attr).concat(el.attributes)

        el = { ...el, attributes: attributes }

        for (var decorator of props) if (decorator.type == "decorator") {
            el = mapConstantOrFunction(decorator.fn, (v, e) => v(e[0]), [el])
        }

        return el
    }


    const createElementAst = (node: Element): RenderNodeAst => {
        const name = node.tagName.toLowerCase()
        let attributes = createArray(node.attributes).map(createAttrAst)
        const children = createArray(node.childNodes).map(e => createRenderAst(e))
        const result: RenderNodeAst = {
            type: "ast",
            name: astConstant(name),
            content: children,
            attributes: []
        }
        const knockoutDecorator = DotvvmKnockoutCompat.createDecorator(node)
        if (knockoutDecorator != null) attributes = [knockoutDecorator(result)].concat(attributes);
        return applyPropsToElement(result, attributes)
    }

    const createRenderAst = (node: Node): RenderNodeAst => {
        if (node.nodeType == node.ELEMENT_NODE) {
            return createElementAst(<Element>node)
        } else if (node.nodeType == node.TEXT_NODE) {
            return { type: "text", content: astConstant((<CharacterData>node).data) }
        } else {
            return { type: "text", content: astConstant("") }
        }
    }

    const optimizeConstants = (ast: RenderNodeAst, allowFirstLevel = true): RenderNodeAst => {
        const optimizeFunction = <T>(fn: ConstantOrFunction<T>): ConstantOrFunction<T> => {
            if (fn.type == "constant") return fn;
            else { //if (fn.type == "func") {
                const fn2 = { elements: fn.elements.map(a => optimizeConstants(a)), type: fn.type, dataContextDepth: fn.dataContextDepth, func: fn.func }
                if (fn2.dataContextDepth == 0 && fn2.elements.every(e => e.type == "constant")) {
                    return astConstant(fn2.func(<any>undefined, fn2.elements.map(e => e["constant"])))
                }
                else return fn2;
            }
        }

        if (ast.type == "constant") return ast
        else if (ast.type == "func") {
            return optimizeFunction(ast)
        } else if (ast.type == "text") {
            const text = optimizeFunction(ast.content)
            if (text.type == "constant") {
                return astConstant(new virtualDom.VText(text.constant))
            } else {
                return { type: "text", content: text }
            }
        } else {
            const ast2 = {
                type: ast.type,
                attributes: ast.attributes.map(a => ({ name: optimizeFunction(a.name), value: optimizeFunction(a.value) })),
                content: ast.content.map(a => optimizeConstants(a)),
                name: optimizeFunction(ast.name)
            }
            if (allowFirstLevel && ast2.name.type == "constant" && ast2.content.every(e => e.type == "constant") && ast2.attributes.every(a => a.name.type == "constant" && a.value.type == "constant")) {
                const attributes = { attributes: {} }
                for (var attr of ast2.attributes) {
                    const name: string = attr.name["constant"],
                        value: string = attr.value["constant"]
                    if (typeof value == "object" && (name == "style" || name == "dataset"))
                        attributes[name] = value
                    else if (name == "value" || name == "defaultValue")
                        attributes[name] = value
                    else
                        attributes.attributes[name] = value
                }
                return astConstant(new virtualDom.VNode(
                    ast2.name.constant,
                    attributes,
                    ast2.content.map(t => t["constant"])
                ))
            } else {
                return ast2
            }
        }
    }

    export const createRenderFunction = <TViewModel>(ast: RenderNodeAst): RenderFunction<TViewModel> => {
        const evalFunction = <T>(fn: ConstantOrFunction<T>, opt: RenderContext<any>): T => {
            if (fn.type == "constant") return fn.constant;
            else {// if (fn.type == "func") {
                const elements = fn.elements.map(el => evalElement(opt, el))
                return fn.func(opt, elements)
            }
        }
        const evalElement = (dataContext: RenderContext<any>, ast: RenderNodeAst, options?: { isRoot?: true }): virtualDom.VTree => {
            if (ast.type == "text") {
                return new virtualDom.VText(evalFunction(ast.content, dataContext))
            } else if (ast.type == "constant") {
                return ast.constant;
            } else if (ast.type == "func") {
                return evalFunction(ast, dataContext)
            } else {//} if (ast.type == "ast") {
                const dcAttr = ast.attributes.filter(e => e.name.type == "constant" && e.name.constant == "data-context")[0]
                if (dcAttr) {
                    const value = evalFunction(dcAttr.value, dataContext)
                    dataContext =
                        ko.isObservable(value) ? { update: (u) => value(u(value())), dataContext: value() } :
                        value instanceof TwoWayBinding ? { update: value.update, dataContext: value.value } :
                        { update: _ => { throw new Error("Update is not supported") }, dataContext: value };
                }

                const attributes = { attributes: {} }
                for (var attr of ast.attributes) {
                    const name = evalFunction(attr.name, dataContext)
                    const value = evalFunction(attr.value, dataContext)
                    if (typeof value == "object" && (name == "style" || name == "dataset" || 'hook' in value))
                        attributes[name] = value
                    else if (name == "value" || name == "defaultValue")
                        attributes[name] = value
                    else if (name == "data-context") { }
                    else
                        attributes.attributes[name] = value
                }
                if (dcAttr || options && options.isRoot) attributes["data-context-hook"] = new DataContextSetHook(dataContext)
                var element = new virtualDom.VNode(
                    evalFunction(ast.name, dataContext),
                    attributes,
                    ast.content.map(t => evalElement(dataContext, t))
                );

                if (dcAttr) dataContext = dataContext.parentContext!
                return element;
            }
        };

        ast = optimizeConstants(ast, false)

        return (opt) => {
            return evalElement(opt, ast, {isRoot: true})
        }
    }

    class DataContextSetHook implements virtualDom.VHook {
        constructor(public readonly dataContext: RenderContext<any>) {}
        hook(node: Element, propertyName: string, previousValue: any): any {
            const currentValue = node["@dotvvm-data-context"]
            if (ko.isWriteableObservable(currentValue))
                node["@dotvvm-data-context"](this.dataContext);
            else if (ko.isObservable(currentValue)) {
                if (currentValue() != this.dataContext)
                    console.error('Node ', node, ' contains a unwritable datacontext observable that does not corresponds with the hooked one', currentValue(), this.dataContext)
            }
            else if (currentValue)
                throw new Error('Node contains a @dotvvm-data-context prop that is not an observable.')
            else node["@dotvvm-data-context"] = ko.observable(this.dataContext);
        }
        unhoook(node: Element, propertyName: string, nextValue: any): any {

        }
    }

    export function initFromNode<TViewModel>(elements: Element[], viewModel: TViewModel): Renderer<TViewModel> {
        const functions = elements.map(element => {
            const ast = createRenderAst(element)
            return createRenderFunction(ast)
        })



        const vdomDispatchers = elements.map((e, index) => {
            return new HtmlElementPatcher(<HTMLElement>e, null)
        });

        return new Renderer<TViewModel>(
            viewModel,
            functions,
            d => d.map((a, i) => vdomDispatchers[i].applyDom(a))
        )
    }
}