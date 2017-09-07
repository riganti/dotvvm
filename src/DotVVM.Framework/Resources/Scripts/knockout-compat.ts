/// <reference path="typings/virtual-dom/virtual-dom.d.ts" />
interface KnockoutStatic {
    originalContextFor: (node: Element) => KnockoutBindingContext
}
namespace DotvvmKnockoutCompat {

const ko_createBindingContext = (() => {
    var c;
    for (var i in ko) { if (ko[i].prototype && typeof (ko[i].prototype['createChildContext']) == "function") c = i }
    var context = ko[c]

    return (dataItemOrAccessor, parentContext?, dataItemAlias?, extendCallback?, options?): KnockoutBindingContext => {
        return new context(dataItemOrAccessor, parentContext, dataItemAlias, extendCallback, options)
    }
})();

(() => {
    const origFn = ko.contextFor;
    const fnCore = (element: Element) => {
        const context2 = element["@dotvvm-data-context"]
        if (context2) {
            // var observable = ko.observable(context2)
            // element["@dotvvm-data-context-refresh"] = c => observable(c)
            return createKnockoutContext(context2);
        }

        if (element.parentElement) return fnCore(element.parentElement)
    }
    const contextFor = ko.contextFor = (element: Element) => {
        return fnCore(element) || origFn(element);
        // const koContext = origFn(element)
        // if (koContext) return koContext;
    }

    ko.dataFor = function (node) {
        var context = contextFor(node);
        return context ? context['$data'] : undefined;
    };
    ko.originalContextFor = origFn;
})();

export const nonControllingBindingHandlers : { [bindingName: string]: boolean } = { visible: true, text: true, html: true, css: true, style: true, attr: true, enabled: true, textInput: true, disabled: true, value: true, options: true, selectedOptions: true, uniqueName: true, checked: true, hasFocus: true, submit: true, event: true, click: true, dotvvmValidation: true, "dotvvm-CheckState": true, "dotvvm-textbox-select-all-on-focus": true, "dotvvm-textbox-text": true, "dotvvm-table-columnvisible": true, "dotvvm-UpdateProgress-Visible": true, "dotvvm-checkbox-updateAfterPostback": true, "dotvvmEnable": true  }

export function createKnockoutContext(dataContext: KnockoutObservable<RenderContext<any>>): KnockoutBindingContext {
    const dataComputed = ko.pureComputed(() => wrapInObservables(ko.pureComputed(() => dataContext().dataContext), dataContext().update))
    const result: KnockoutBindingContext =
        dataContext.peek().parentContext ?
            createKnockoutContext(ko.pureComputed(() => dataContext().parentContext || { dataContext: null, update: u => { console.warn("Ou, updating non existent viewModel"); } }))
                .createChildContext(dataComputed) :
            ko_createBindingContext(dataComputed);

    result["$unwraped"] = ko.pureComputed(() => dataContext().dataContext);
    result["$betterContext"] = ko.pureComputed(() => dataContext());
    result["$createdForSelf"] = result;

    const extensions = dataContext.peek()["@extensions"]
    if (extensions != null) {
        for (const ext in extensions) {
            if (extensions.hasOwnProperty(ext)) {
                result[ext] = extensions[ext]
            }
        }
    }
    if (!ko.isObservable(result.$rawData))
        throw new Error("$rawData is not an observable");
    return result
}

export function wrapInObservables(objOrObservable: any, update: ((updater: StateUpdate<any>) => void) | null = null) {
    const obj = ko.unwrap(objOrObservable)

    const createComputed = (indexer: string | number, updateProperty: (vm: any, property: StateUpdate<any>) => any) => {
        if (obj[indexer] instanceof Array) {
            return wrapInObservables(
                ko.isObservable(objOrObservable) ? ko.pureComputed(() => (objOrObservable() || [])[indexer]) : obj[indexer],
                update == null ? null : u => update(vm => updateProperty(vm, u)))
        } else {
            // knockout does not like when the object gets replaced by a new one, so we will just update this one every time...
            let cache : any = undefined
            return ko.pureComputed({
                read: () => 
                    // when the cache contains non-object it's either empty or contain primitive (and immutable) value
                    cache != null && typeof cache == "object" ? cache :
                    (cache = wrapInObservables(
                        ko.isObservable(objOrObservable) ? ko.pureComputed(() => (objOrObservable() || {})[indexer]) : obj[indexer],
                        update == null ? null : u => update(vm => updateProperty(vm, u)))),
                write:
                    update == null ? undefined :
                    val => update(vm => updateProperty(vm, _ => ko.unwrap(val)))
            });
        }
    }

    const arrayUpdate = (index: number): ((vm: any[], prop: StateUpdate<any>) => any) => (vm, prop) => { const r = vm.slice(0); r[index] = prop(r[index]); return r };
    const objUpdate = (propName: string): ((vm: any, prop: StateUpdate<any>) => any) => (vm, prop) => ({ ...vm, [propName]: prop(vm[propName]) });


    if (typeof obj != "object" || obj == null) return obj

    if (obj instanceof Array) {
        const result: any[] = []
        result["__upwrapped_data"] = objOrObservable
        if (update) result["__update_function"] = update
        for (var index = 0; index < obj.length; index++) {
            result.push(createComputed(index, arrayUpdate(index)));
        }
        const rr = ko.observableArray(result)
        let isUpdating = false;
        rr.subscribe((newVal) => {
            if (isUpdating || newVal && newVal["__unwrapped_data"] == objOrObservable) return;
            if (update) {
                if (newVal && newVal["__unwrapped_data"]) update(f => ko.unwrap(newVal["__unwrapped_data"]))
                else update(f => dotvvm.serialization.deserialize(newVal))
            }
            else throw new Error("Array mutation is not supported.");
        })
        if (ko.isObservable(objOrObservable)) {
            objOrObservable.subscribe((newVal) => {
                try {
                    isUpdating = true;
                    if (!newVal) rr(newVal);
                    else {
                        const result: any[] = []
                        result["__upwrapped_data"] = objOrObservable
                        if (update) result["__update_function"] = update
                        for (var index = 0; index < newVal.length; index++) {
                            result.push(createComputed(index, arrayUpdate(index)));
                        }
                        rr(result);
                    }
                }
                finally {
                    isUpdating = false;
                }
            })
        }
        return rr
    } else {
        const result: any = {}
        result["__upwrapped_data"] = objOrObservable
        if (update) result["__update_function"] = update
        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                result[key] = createComputed(key, objUpdate(key))
            }
        }
        return result
    }
}

export class KnockoutBindingHook implements virtualDom.VHook {
    constructor(public readonly dataContext: RenderContext<any>) {}
    private lastState: KnockoutObservable<RenderContext<any>> | null = null;
    hook(node: Element, propertyName: string, previousValue: this) {
        if (this.lastState != null) throw new Error("Can not hook more than one time");
        if (previousValue) {
            if (previousValue.lastState == null) throw new Error(``);
            this.lastState = previousValue.lastState
            previousValue.lastState = null
            this.lastState(this.dataContext)
        } else {
            const lastState = this.lastState = ko.observable(this.dataContext)
            const context = createKnockoutContext(lastState)
            ko.applyBindingsToNode(node, null, context)
        }
    }
    unhoook(node: Element, propertyName: string, nextValue: any): any {
        // Knockout should dispose automatically when the node is dropped
    }
}

export class KnockoutBindingWidget implements virtualDom.Widget {
    // type KnockoutVirtualElement = { start: number; end: number; dataBind: string } 

    readonly type: string = "Widget"
    elementId = Math.floor(Math.random() * 1000000).toString()

    contentMapping: { element: Node; index: number; lastDom: virtualDom.VTree }[] | null = null;
    lastState: KnockoutObservable<RenderContext<any>>;
    domWatcher: MutationObserver;

    constructor(
        public readonly dataContext: RenderContext<any>,
        public readonly node: virtualDom.VNode,
        public readonly nodeChildren: ((context: RenderContext<any>) => virtualDom.VTree)[] | null,
        public readonly dataBind: string | null,
        public readonly koComments: { start: number; end: number; dataBind: string }[]
    ) {
        this.lastState = ko.observable(dataContext);
        this.koComments.sort((a, b) => a.start - b.start)
        for (var i = 1; i < this.koComments.length; i++) {
            if (koComments[i - 1].end > koComments[i].start) throw new Error("Knockout comments can't overlap.")
        }
    }

    getFakeContent(): Node[] {
        const comments = this.koComments
        const content: Node[] = []
        for (var i = 0, ci = 0; i < (this.nodeChildren || this.node.children).length; i++) {
            if (comments[ci] && comments[ci].start == i) {
                content.push(document.createComment("ko " + comments[ci].dataBind))
            }

            content.push(virtualDom.create(new virtualDom.VNode("span", { dataset: { index: i.toString(), commentIndex: ci.toString(), fakeContentFor: this.elementId } }), {}))

            if (comments[ci] && comments[ci].end <= i) {
                if (comments[ci].end != i) throw new Error();
                content.push(document.createComment("/ko"))
                ci++;
            }
        }
        return content;
    }

    init(): Element {
        const element = virtualDom.create(this.node, {})

        if (this.nodeChildren != null) for (const c of this.getFakeContent())
            element.appendChild(c);

        const rootKoContext = createKnockoutContext(this.lastState);
        let contentIsApplied = false;
        if (this.dataBind != null) {
            // apply data-bind of the top element
            element.setAttribute("data-bind", this.dataBind);

            const bindingResult = ko.applyBindingAccessorsToNode(element, (a, b) => {
                if (a != rootKoContext) throw new Error("Something is wrong.")
                this.lastState()
                const bindingAccessor = ko.bindingProvider.instance.getBindingAccessors!(element, rootKoContext)
                // const result = {}
                // for (const key in bindingAccessor) {
                //     if (bindingAccessor.hasOwnProperty(key)) {
                //         const element = bindingAccessor[key];
                //         result[key] = ko.pureComputed(() => {
                //             if (rootKoContext["_subscribable"])
                //                 rootKoContext["_subscribable"]()
                //             return ko.unwrap(element)
                //         })
                //     }
                // }
                // return result
                return bindingAccessor
            }, rootKoContext)
            contentIsApplied = !bindingResult["shouldBindDescendants"]
        }

        if (!contentIsApplied) {
            // apply knockout comments
            for (const e of createArray(element.childNodes)) {
                if (e.nodeType == Node.COMMENT_NODE && ko.bindingProvider.instance.nodeHasBindings(e)) {
                    ko.applyBindingsToNode(e, null, rootKoContext)
                }
            }
        }

        if (this.nodeChildren != null) {
            this.contentMapping = []
            // replace fake elements with real nodes
            this.replaceTmpSpans(createArray(element.getElementsByTagName("span")), element);
            this.setupDomWatcher(element);
        }


        return element;
    }

    private setupDomWatcher(element: Element) {
        if (!this.domWatcher)
            this.domWatcher = new MutationObserver(c => {
                for (const rec of c) {
                    this.replaceTmpSpans(createArray(rec.addedNodes), element);
                    // TODO removed nodes
                    for (const rm of createArray(rec.removedNodes)) {
                        if (rm["__bound_element"] && rm["__bound_element"].parentElement) {
                            rm["__bound_element"].remove()
                        }
                    }
                }
            });
        this.domWatcher.observe(element, { childList: true, subtree: true, attributes: true, characterData: true })
    }

    private static knockoutInternalDataPropertyName: string | null = null
    private copyKnockoutInternalDataProperty(from, to) {
        const name = KnockoutBindingWidget.knockoutInternalDataPropertyName || (() => {
            for (const n in from) {
                if (n.indexOf("__ko__") == 0) {
                    return KnockoutBindingWidget.knockoutInternalDataPropertyName = n;
                }
            }
            return null
        })();
        if (name && from[name]) {
            to[name] = from[name];
        }
    }

    private isElementRooted(element: Node, root: Node) {
        while (element.parentNode != null) {
            if (element.parentNode == root) return true;
            element = element.parentNode
        }
        return false;
    }

    private replaceTmpSpans(nodes: Node[], rootElement: Element) {
        for (var n of nodes) {
            const e = n as Element;
            if (n.nodeType == Node.ELEMENT_NODE && e.getAttribute("data-fake-content-for") == this.elementId && this.isElementRooted(e, rootElement) && !e["__bound_element"]) {

                const index = parseInt(e.getAttribute("data-index")!)
                const commentIndex = parseInt(e.getAttribute("data-comment-index")!)
                const context = (() => {
                    const koContext = ko.originalContextFor(e)
                    return koContext ? KnockoutBindingWidget.getBetterContext(koContext) : this.dataContext
                })();


                let vdomNode = this.nodeChildren![index](context)
                const element = virtualDom.create(vdomNode, {})
                // this.copyKnockoutInternalDataProperty(e, element);
                let subscribable : null | KnockoutObservable<RenderContext<any>> = null;
                if (context != this.dataContext) {
                    element["@dotvvm-data-context"] = subscribable = ko.pureComputed(() => {
                        this.lastState();
                        const koContext = ko.originalContextFor(e)
                        if (koContext && ko.isObservable(koContext["_subscribable"])) koContext["_subscribable"]()
                        return koContext ? KnockoutBindingWidget.getBetterContext(koContext) : this.dataContext
                    })
                } else {
                    element["@dotvvm-data-context-issame"] = true
                }
                e["__bound_element"] = element;
                e.parentElement!.insertBefore(element, e)
                this.contentMapping!.push({ element: e, index, lastDom: vdomNode })

                if (subscribable) {
                    const subscription = subscribable.subscribe(c => {
                        const vdom2 = this.nodeChildren![index](c)
                        const diff = virtualDom.diff(vdomNode, vdom2)
                        vdomNode = vdom2
                        virtualDom.patch(element, diff)
                    });
                }
            }
        }
    }

    private removeRemovedNodes(rootElement: Element) {
        if (this.contentMapping) for (const x of this.contentMapping) {
            if (!this.isElementRooted(x.element, rootElement) && x.element["__bound_element"]) {
                (x.element["__bound_element"] as Element).remove();
            }
        }
    }

    update(previousWidget: this, previousDomNode: Element): Element | undefined {
        if (previousWidget.dataBind != this.dataBind ||
            previousWidget.koComments.length != previousWidget.koComments.length ||
            !previousWidget.koComments.every((e, i) => this.koComments[i].dataBind == e.dataBind && this.koComments[i].start == e.start && this.koComments[i].end == e.end)) {
            // data binding has changed, rerender the widget
            return this.init();
        }
        if (!!previousWidget.nodeChildren != !!previousWidget.nodeChildren) throw new Error("");
        this.elementId = previousWidget.elementId;
        this.lastState = previousWidget.lastState;
        this.contentMapping = previousWidget.contentMapping;
        if (previousWidget.domWatcher) previousWidget.domWatcher.disconnect();
        if (this.nodeChildren != null) {
            this.contentMapping = this.contentMapping || []
            // replace fake elements with real nodes
            this.setupDomWatcher(previousDomNode);
            // TODO: for some reason the MutationObserver does not react to changes when the element is also observed by other oberver
            this.removeRemovedNodes(previousDomNode);
            this.replaceTmpSpans(createArray(previousDomNode.getElementsByTagName("span")), previousDomNode);
        }
        previousWidget.lastState(this.dataContext);
    }
    destroy(domNode: Element) {
        this.domWatcher.disconnect();
    }

    public static getBetterContext(dataContext: KnockoutBindingContext): RenderContext<any> {
        if (dataContext["$betterContext"] && dataContext["$createdForSelf"] === dataContext)
            return ko.unwrap(dataContext["$betterContext"])
        const parent = dataContext.$parentContext != null ? KnockoutBindingWidget.getBetterContext(dataContext.$parentContext) : undefined;
        const data = (dataContext["$createdForSelf"] === dataContext && ko.unwrap(dataContext["$unwrapped"])) || ko.unwrap(dataContext.$data["__upwrapped_data"]) || dotvvm.serialization.serialize(dataContext.$data)
        let extensions: undefined | { [name: string]: any } = undefined
        for (const prop in dataContext) {
            if (dataContext.hasOwnProperty(prop) && prop != "$data" && prop != "$parent" && prop != "$parents" && prop != "$root" && prop != "ko" && prop != "$rawData" && prop != "_subscribable") {
                extensions = extensions || {}
                extensions[prop] = dataContext[prop]
            }
        }
        return {
            dataContext: data,
            parentContext: parent,
            update: (updater: StateUpdate<any>) => {
                if (typeof dataContext.$data["__update_function"] == "function") {
                    console.log("Updating ", dataContext.$data);
                    dataContext.$data["__update_function"](updater);
                } else {
                    // deserialize the change to the knockout context
                    console.warn("Deserializing chnages to knockout context");
                    dotvvm.serialization.deserialize(updater(dotvvm.serialization.serialize(dataContext.$data)), dataContext.$data);
                }
            },
            "@extensions": extensions
        }
    }
}

export function createDecorator(element: Element): ((element: RendererInitializer.RenderNodeAst) => RendererInitializer.AssignedPropDescriptor) | undefined {
    const dataBindAttribute = element.getAttribute("data-bind")
    const hasCommentChild = createArray(element.childNodes).some(n => n.nodeType == Node.COMMENT_NODE && ko.bindingProvider.instance.nodeHasBindings(n))
    const commentNodesHaveTextProperty = document && document.createComment("test").text === "<!--test-->";
    const startCommentRegex = commentNodesHaveTextProperty ? /^<!--\s*ko(?:\s+([\s\S]+))?\s*-->$/ : /^\s*ko(?:\s+([\s\S]+))?\s*$/;
    const getKoCommentValue = (node) => {
        var regexMatch = (commentNodesHaveTextProperty ? node.text : node.nodeValue).match(startCommentRegex);
        return regexMatch ? regexMatch[1] : null;
    }

    if (dataBindAttribute && !hasCommentChild) {
        const binding = ko.expressionRewriting.parseObjectLiteral(dataBindAttribute)
        if (binding.every(b => nonControllingBindingHandlers[b.key])) {
            // add a simple hook, the complex widget is not needed
            return node => {
                return {
                    type: "attr",
                    attr: {
                        name: RendererInitializer.astConstant("knockout-data-bind-hook"),
                        value: RendererInitializer.astFunc(1000000, [], (dataContext) =>
                            new KnockoutBindingHook(dataContext)
                        )
                    }
                }
            }
        }
    }

    if (dataBindAttribute || hasCommentChild) {
        const kk: { start: number; end: number; dataBind: string }[] = []
        let elementIndex = 0;
        let skipToEndComment: number[] = [];
        let startComments: number[] = []
        for (const n of createArray(element.childNodes)) {
            if (n.nodeType == Node.COMMENT_NODE && ko.bindingProvider.instance.nodeHasBindings(n)) {
                skipToEndComment.push(ko.virtualElements.childNodes(n).length)
                startComments.push(kk.push({
                    start: elementIndex,
                    end: -1,
                    dataBind: getKoCommentValue(n)
                }) - 1);
            }

            if (skipToEndComment.length > 0)
                if (skipToEndComment[skipToEndComment.length - 1]-- <= 0) {
                    skipToEndComment.pop();
                    const dd = kk[startComments.pop()!]
                    dd.end = elementIndex
                }

            elementIndex++;
        }
        if (skipToEndComment.length > 0)
            if (skipToEndComment[skipToEndComment.length - 1]-- <= 0) {
                skipToEndComment.pop();
                const dd = kk[startComments.pop()!]
                dd.end = elementIndex
            }

        return node => {
            let content: RenderFunction<any>[] | null = null;
            if (node.type == "ast") {
                content = node.content.map(e => RendererInitializer.createRenderFunction<any>(e))
            }
            else throw new Error();
            return {
                type: "decorator", fn: RendererInitializer.astFunc(1000000, [], (dataContext, elements) => (node: virtualDom.VTree) => {
                    const a = node as virtualDom.VNode
                    if (a.type != "VirtualNode") throw new Error();
                    const wrapperElement = content == null ? a : new virtualDom.VNode(a.tagName, a.properties, [], a.key, a.namespace)
                    return new KnockoutBindingWidget(
                        dataContext,
                        wrapperElement,
                        content != null ? content.map(e => (dc: RenderContext<any>) => e(dc)) : null,
                        dataBindAttribute,
                        kk
                    )
                })
            } as RendererInitializer.AssignedPropDescriptor;
        }
    }
    return undefined;
}
}