var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
/// <reference path="typings/virtual-dom/virtual-dom.d.ts" />
var DotvvmKnockoutCompat;
(function (DotvvmKnockoutCompat) {
    var ko_createBindingContext = (function () {
        var c;
        for (var i in ko) {
            if (ko[i].prototype && typeof (ko[i].prototype['createChildContext']) == "function")
                c = i;
        }
        var context = ko[c];
        return function (dataItemOrAccessor, parentContext, dataItemAlias, extendCallback, options) {
            return new context(dataItemOrAccessor, parentContext, dataItemAlias, extendCallback, options);
        };
    })();
    (function () {
        var origFn = ko.contextFor;
        var fnCore = function (element) {
            var context2 = element["@dotvvm-data-context"];
            if (context2) {
                // var observable = ko.observable(context2)
                // element["@dotvvm-data-context-refresh"] = c => observable(c)
                return createKnockoutContext(context2);
            }
            if (element.parentElement)
                return fnCore(element.parentElement);
        };
        var contextFor = ko.contextFor = function (element) {
            return fnCore(element) || origFn(element);
            // const koContext = origFn(element)
            // if (koContext) return koContext;
        };
        ko.dataFor = function (node) {
            var context = contextFor(node);
            return context ? context['$data'] : undefined;
        };
        ko.originalContextFor = origFn;
    })();
    DotvvmKnockoutCompat.nonControllingBindingHandlers = { visible: true, text: true, html: true, css: true, style: true, attr: true, enabled: true, textInput: true, disabled: true, value: true, options: true, selectedOptions: true, uniqueName: true, checked: true, hasFocus: true, submit: true, event: true, click: true, dotvvmValidation: true, "dotvvm-CheckState": true, "dotvvm-textbox-select-all-on-focus": true, "dotvvm-textbox-text": true, "dotvvm-table-columnvisible": true, "dotvvm-UpdateProgress-Visible": true, "dotvvm-checkbox-updateAfterPostback": true, "dotvvmEnable": true };
    function createKnockoutContext(dataContext) {
        var dataComputed = ko.pureComputed(function () { return wrapInObservables(ko.pureComputed(function () { return dataContext().dataContext; }), dataContext().update); });
        var result = dataContext.peek().parentContext ?
            createKnockoutContext(ko.pureComputed(function () { return dataContext().parentContext || { dataContext: null, update: function (u) { console.warn("Ou, updating non existent viewModel"); } }; }))
                .createChildContext(dataComputed) :
            ko_createBindingContext(dataComputed);
        result["$unwraped"] = ko.pureComputed(function () { return dataContext().dataContext; });
        result["$betterContext"] = ko.pureComputed(function () { return dataContext(); });
        result["$createdForSelf"] = result;
        var extensions = dataContext.peek()["@extensions"];
        if (extensions != null) {
            for (var ext in extensions) {
                if (extensions.hasOwnProperty(ext)) {
                    result[ext] = extensions[ext];
                }
            }
        }
        if (!ko.isObservable(result.$rawData))
            throw new Error("$rawData is not an observable");
        return result;
    }
    DotvvmKnockoutCompat.createKnockoutContext = createKnockoutContext;
    function wrapInObservables(objOrObservable, update) {
        if (update === void 0) { update = null; }
        var obj = ko.unwrap(objOrObservable);
        var createComputed = function (indexer, updateProperty) {
            if (obj[indexer] instanceof Array) {
                return wrapInObservables(ko.isObservable(objOrObservable) ? ko.pureComputed(function () { return (objOrObservable() || [])[indexer]; }) : obj[indexer], update == null ? null : function (u) { return update(function (vm) { return updateProperty(vm, u); }); });
            }
            else {
                // knockout does not like when the object gets replaced by a new one, so we will just update this one every time...
                var cache_1 = undefined;
                return ko.pureComputed({
                    read: function () {
                        // when the cache contains non-object it's either empty or contain primitive (and immutable) value
                        return cache_1 != null && typeof cache_1 == "object" ? cache_1 :
                            (cache_1 = wrapInObservables(ko.isObservable(objOrObservable) ? ko.pureComputed(function () { return (objOrObservable() || {})[indexer]; }) : obj[indexer], update == null ? null : function (u) { return update(function (vm) { return updateProperty(vm, u); }); }));
                    },
                    write: update == null ? undefined :
                        function (val) { return update(function (vm) { return updateProperty(vm, function (_) { return ko.unwrap(val); }); }); }
                });
            }
        };
        var arrayUpdate = function (index) { return function (vm, prop) { var r = vm.slice(0); r[index] = prop(r[index]); return r; }; };
        var objUpdate = function (propName) { return function (vm, prop) {
            return (__assign({}, vm, (_a = {}, _a[propName] = prop(vm[propName]), _a)));
            var _a;
        }; };
        if (typeof obj != "object" || obj == null)
            return obj;
        if (obj instanceof Array) {
            var result = [];
            result["__unwrapped_data"] = objOrObservable;
            if (update)
                result["__update_function"] = update;
            for (var index = 0; index < obj.length; index++) {
                result.push(createComputed(index, arrayUpdate(index)));
            }
            var rr_1 = ko.observableArray(result);
            var isUpdating_1 = false;
            rr_1.subscribe(function (newVal) {
                if (isUpdating_1 || newVal && newVal["__unwrapped_data"] == objOrObservable)
                    return;
                if (update) {
                    if (newVal && newVal["__unwrapped_data"])
                        update(function (f) { return ko.unwrap(newVal["__unwrapped_data"]); });
                    else
                        update(function (f) { return dotvvm.serialization.deserialize(newVal); });
                }
                else
                    throw new Error("Array mutation is not supported.");
            });
            if (ko.isObservable(objOrObservable)) {
                objOrObservable.subscribe(function (newVal) {
                    try {
                        isUpdating_1 = true;
                        if (!newVal)
                            rr_1(newVal);
                        else {
                            var result_1 = [];
                            result_1["__unwrapped_data"] = objOrObservable;
                            if (update)
                                result_1["__update_function"] = update;
                            for (var index = 0; index < newVal.length; index++) {
                                result_1.push(createComputed(index, arrayUpdate(index)));
                            }
                            rr_1(result_1);
                        }
                    }
                    finally {
                        isUpdating_1 = false;
                    }
                });
            }
            return rr_1;
        }
        else {
            var result = {};
            result["__unwrapped_data"] = objOrObservable;
            if (update)
                result["__update_function"] = update;
            for (var key in obj) {
                if (obj.hasOwnProperty(key)) {
                    result[key] = createComputed(key, objUpdate(key));
                }
            }
            return result;
        }
    }
    DotvvmKnockoutCompat.wrapInObservables = wrapInObservables;
    var KnockoutBindingHook = (function () {
        function KnockoutBindingHook(dataContext) {
            this.dataContext = dataContext;
            this.lastState = null;
        }
        KnockoutBindingHook.prototype.hook = function (node, propertyName, previousValue) {
            if (this.lastState != null)
                throw new Error("Can not hook more than one time");
            if (previousValue) {
                if (previousValue.lastState == null)
                    throw new Error("");
                this.lastState = previousValue.lastState;
                previousValue.lastState = null;
                this.lastState(this.dataContext);
            }
            else {
                var lastState = this.lastState = ko.observable(this.dataContext);
                var context = createKnockoutContext(lastState);
                ko.applyBindingsToNode(node, null, context);
            }
        };
        KnockoutBindingHook.prototype.unhoook = function (node, propertyName, nextValue) {
            // Knockout should dispose automatically when the node is dropped
        };
        return KnockoutBindingHook;
    }());
    DotvvmKnockoutCompat.KnockoutBindingHook = KnockoutBindingHook;
    var KnockoutBindingWidget = (function () {
        function KnockoutBindingWidget(dataContext, node, nodeChildren, dataBind, koComments) {
            this.dataContext = dataContext;
            this.node = node;
            this.nodeChildren = nodeChildren;
            this.dataBind = dataBind;
            this.koComments = koComments;
            // type KnockoutVirtualElement = { start: number; end: number; dataBind: string } 
            this.type = "Widget";
            this.elementId = Math.floor(Math.random() * 1000000).toString();
            this.contentMapping = null;
            this.lastState = ko.observable(dataContext);
            this.koComments.sort(function (a, b) { return a.start - b.start; });
            for (var i = 1; i < this.koComments.length; i++) {
                if (koComments[i - 1].end > koComments[i].start)
                    throw new Error("Knockout comments can't overlap.");
            }
        }
        KnockoutBindingWidget.prototype.getFakeContent = function () {
            var comments = this.koComments;
            var content = [];
            for (var i = 0, ci = 0; i < (this.nodeChildren || this.node.children).length; i++) {
                if (comments[ci] && comments[ci].start == i) {
                    content.push(document.createComment("ko " + comments[ci].dataBind));
                }
                content.push(virtualDom.create(new virtualDom.VNode("span", { dataset: { index: i.toString(), commentIndex: ci.toString(), fakeContentFor: this.elementId } }), {}));
                if (comments[ci] && comments[ci].end <= i) {
                    if (comments[ci].end != i)
                        throw new Error();
                    content.push(document.createComment("/ko"));
                    ci++;
                }
            }
            return content;
        };
        KnockoutBindingWidget.prototype.init = function () {
            var _this = this;
            var element = virtualDom.create(this.node, {});
            if (this.nodeChildren != null)
                for (var _i = 0, _a = this.getFakeContent(); _i < _a.length; _i++) {
                    var c = _a[_i];
                    element.appendChild(c);
                }
            var rootKoContext = createKnockoutContext(this.lastState);
            rootKoContext["__created_for_element"] = element;
            var contentIsApplied = false;
            if (this.dataBind != null) {
                // apply data-bind of the top element
                element.setAttribute("data-bind", this.dataBind);
                var bindingResult = ko.applyBindingAccessorsToNode(element, function (a, b) {
                    if (a != rootKoContext)
                        throw new Error("Something is wrong.");
                    _this.lastState();
                    var bindingAccessor = ko.bindingProvider.instance.getBindingAccessors(element, rootKoContext);
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
                    return bindingAccessor;
                }, rootKoContext);
                contentIsApplied = !bindingResult["shouldBindDescendants"];
            }
            if (!contentIsApplied) {
                // apply knockout comments
                for (var _b = 0, _c = createArray(element.childNodes); _b < _c.length; _b++) {
                    var e = _c[_b];
                    if (e.nodeType == Node.COMMENT_NODE && ko.bindingProvider.instance.nodeHasBindings(e)) {
                        ko.applyBindingsToNode(e, null, rootKoContext);
                    }
                }
            }
            if (this.nodeChildren != null) {
                this.contentMapping = [];
                // replace fake elements with real nodes
                this.replaceTmpSpans(createArray(element.getElementsByTagName("span")), element);
                this.setupDomWatcher(element);
            }
            return element;
        };
        KnockoutBindingWidget.prototype.setupDomWatcher = function (element) {
            var _this = this;
            if (!this.domWatcher)
                this.domWatcher = new MutationObserver(function (c) {
                    for (var _i = 0, c_1 = c; _i < c_1.length; _i++) {
                        var rec = c_1[_i];
                        _this.replaceTmpSpans(createArray(rec.addedNodes), element);
                        // TODO removed nodes
                        for (var _a = 0, _b = createArray(rec.removedNodes); _a < _b.length; _a++) {
                            var rm = _b[_a];
                            if (rm["__bound_element"] && rm["__bound_element"].parentElement) {
                                rm["__bound_element"].remove();
                            }
                        }
                    }
                });
            this.domWatcher.observe(element, { childList: true, subtree: true, attributes: true, characterData: true });
        };
        KnockoutBindingWidget.prototype.copyKnockoutInternalDataProperty = function (from, to) {
            var name = KnockoutBindingWidget.knockoutInternalDataPropertyName || (function () {
                for (var n in from) {
                    if (n.indexOf("__ko__") == 0) {
                        return KnockoutBindingWidget.knockoutInternalDataPropertyName = n;
                    }
                }
                return null;
            })();
            if (name && from[name]) {
                to[name] = from[name];
            }
        };
        KnockoutBindingWidget.prototype.isElementRooted = function (element, root) {
            while (element.parentNode != null) {
                if (element.parentNode == root)
                    return true;
                element = element.parentNode;
            }
            return false;
        };
        KnockoutBindingWidget.prototype.replaceTmpSpans = function (nodes, rootElement) {
            var _this = this;
            var _loop_1 = function () {
                var e = n;
                if (n.nodeType == Node.ELEMENT_NODE && e.getAttribute("data-fake-content-for") == this_1.elementId && this_1.isElementRooted(e, rootElement) && !e["__bound_element"]) {
                    var index_1 = parseInt(e.getAttribute("data-index"));
                    var commentIndex = parseInt(e.getAttribute("data-comment-index"));
                    var context = (function () {
                        var koContext = ko.originalContextFor(e);
                        return koContext ? KnockoutBindingWidget.getBetterContext(koContext) : _this.dataContext;
                    })();
                    var vdomNode_1 = this_1.nodeChildren[index_1](context);
                    var element_1 = virtualDom.create(vdomNode_1, {});
                    // this.copyKnockoutInternalDataProperty(e, element);
                    var subscribable = null;
                    if (context != this_1.dataContext) {
                        element_1["@dotvvm-data-context"] = subscribable = ko.pureComputed(function () {
                            _this.lastState();
                            var koContext = ko.originalContextFor(e);
                            if (koContext && ko.isObservable(koContext["_subscribable"]))
                                koContext["_subscribable"]();
                            return koContext ? KnockoutBindingWidget.getBetterContext(koContext) : _this.dataContext;
                        });
                    }
                    else {
                        element_1["@dotvvm-data-context-issame"] = true;
                        subscribable = this_1.lastState;
                    }
                    e["__bound_element"] = element_1;
                    e.parentElement.insertBefore(element_1, e);
                    this_1.contentMapping.push({ element: e, index: index_1, lastDom: vdomNode_1 });
                    if (subscribable) {
                        var subscription_1 = subscribable.subscribe(function (c) {
                            if (!_this.isElementRooted(e, rootElement)) {
                                element_1.remove();
                                subscription_1.dispose();
                                return;
                            }
                            var vdom2 = _this.nodeChildren[index_1](c);
                            var diff = virtualDom.diff(vdomNode_1, vdom2);
                            vdomNode_1 = vdom2;
                            virtualDom.patch(element_1, diff);
                        });
                    }
                }
            };
            var this_1 = this;
            for (var _i = 0, nodes_1 = nodes; _i < nodes_1.length; _i++) {
                var n = nodes_1[_i];
                _loop_1();
            }
        };
        KnockoutBindingWidget.prototype.removeRemovedNodes = function (rootElement) {
            if (this.contentMapping)
                for (var _i = 0, _a = this.contentMapping; _i < _a.length; _i++) {
                    var x = _a[_i];
                    if (!this.isElementRooted(x.element, rootElement) && x.element["__bound_element"]) {
                        x.element["__bound_element"].remove();
                    }
                }
        };
        KnockoutBindingWidget.prototype.update = function (previousWidget, previousDomNode) {
            var _this = this;
            if (previousWidget.dataBind != this.dataBind ||
                previousWidget.koComments.length != previousWidget.koComments.length ||
                !previousWidget.koComments.every(function (e, i) { return _this.koComments[i].dataBind == e.dataBind && _this.koComments[i].start == e.start && _this.koComments[i].end == e.end; })) {
                // data binding has changed, rerender the widget
                return this.init();
            }
            if (!!previousWidget.nodeChildren != !!previousWidget.nodeChildren)
                throw new Error("");
            this.elementId = previousWidget.elementId;
            this.lastState = previousWidget.lastState;
            this.contentMapping = previousWidget.contentMapping;
            if (previousWidget.domWatcher)
                previousWidget.domWatcher.disconnect();
            if (this.nodeChildren != null) {
                this.contentMapping = this.contentMapping || [];
                // replace fake elements with real nodes
                this.setupDomWatcher(previousDomNode);
                // TODO: for some reason the MutationObserver does not react to changes when the element is also observed by other oberver
                this.removeRemovedNodes(previousDomNode);
                this.replaceTmpSpans(createArray(previousDomNode.getElementsByTagName("span")), previousDomNode);
            }
            previousWidget.lastState(this.dataContext);
        };
        KnockoutBindingWidget.prototype.destroy = function (domNode) {
            this.domWatcher.disconnect();
        };
        KnockoutBindingWidget.getBetterContext = function (dataContext) {
            if (dataContext["$betterContext"] && dataContext["$createdForSelf"] === dataContext)
                return ko.unwrap(dataContext["$betterContext"]);
            var parent = dataContext.$parentContext != null ? KnockoutBindingWidget.getBetterContext(dataContext.$parentContext) : undefined;
            var data = (dataContext["$createdForSelf"] === dataContext && ko.unwrap(dataContext["$unwrapped"])) || ko.unwrap(dataContext.$data["__unwrapped_data"]) || dotvvm.serialization.serialize(dataContext.$data);
            var extensions = undefined;
            for (var prop in dataContext) {
                if (dataContext.hasOwnProperty(prop) && prop != "$data" && prop != "$parent" && prop != "$parents" && prop != "$root" && prop != "ko" && prop != "$rawData" && prop != "_subscribable") {
                    extensions = extensions || {};
                    extensions[prop] = dataContext[prop];
                }
            }
            return {
                dataContext: data,
                parentContext: parent,
                update: function (updater) {
                    if (typeof dataContext.$data["__update_function"] == "function") {
                        console.log("Updating ", dataContext.$data);
                        dataContext.$data["__update_function"](updater);
                    }
                    else {
                        // deserialize the change to the knockout context
                        console.warn("Deserializing chnages to knockout context");
                        dotvvm.serialization.deserialize(updater(dotvvm.serialization.serialize(dataContext.$data)), dataContext.$data);
                    }
                },
                "@extensions": extensions
            };
        };
        return KnockoutBindingWidget;
    }());
    KnockoutBindingWidget.knockoutInternalDataPropertyName = null;
    DotvvmKnockoutCompat.KnockoutBindingWidget = KnockoutBindingWidget;
    var commentNodesHaveTextProperty = document && document.createComment("test").text === "<!--test-->";
    var startCommentRegex = commentNodesHaveTextProperty ? /^<!--\s*ko(?:\s+([\s\S]+))?\s*-->$/ : /^\s*ko(?:\s+([\s\S]+))?\s*$/;
    function createDecorator(element) {
        var dataBindAttribute = element.getAttribute("data-bind");
        var hasCommentChild = false;
        for (var index = 0; index < element.childNodes.length; index++) {
            var n = element.childNodes[index];
            if (hasCommentChild = n.nodeType == Node.COMMENT_NODE && ko.bindingProvider.instance.nodeHasBindings(n))
                break;
        }
        var getKoCommentValue = function (node) {
            var regexMatch = (commentNodesHaveTextProperty ? node.text : node.nodeValue).match(startCommentRegex);
            return regexMatch ? regexMatch[1] : null;
        };
        if (dataBindAttribute && !hasCommentChild) {
            var binding = ko.expressionRewriting.parseObjectLiteral(dataBindAttribute);
            if (binding.every(function (b) { return DotvvmKnockoutCompat.nonControllingBindingHandlers[b.key]; })) {
                // add a simple hook, the complex widget is not needed
                return function (node) {
                    return {
                        type: "attr",
                        attr: {
                            name: RendererInitializer.astConstant("knockout-data-bind-hook"),
                            value: RendererInitializer.astFunc(1000000, [], function (dataContext) {
                                return new KnockoutBindingHook(dataContext);
                            })
                        }
                    };
                };
            }
        }
        if (dataBindAttribute || hasCommentChild) {
            var kk_1 = [];
            var elementIndex = 0;
            var skipToEndComment = [];
            var startComments = [];
            for (var index = 0; index < element.childNodes.length; index++) {
                var n = element.childNodes[index];
                if (n.nodeType == Node.COMMENT_NODE && ko.bindingProvider.instance.nodeHasBindings(n)) {
                    skipToEndComment.push(ko.virtualElements.childNodes(n).length);
                    startComments.push(kk_1.push({
                        start: elementIndex,
                        end: -1,
                        dataBind: getKoCommentValue(n)
                    }) - 1);
                }
                if (skipToEndComment.length > 0)
                    if (skipToEndComment[skipToEndComment.length - 1]-- <= 0) {
                        skipToEndComment.pop();
                        var dd = kk_1[startComments.pop()];
                        dd.end = elementIndex;
                    }
                if (n.nodeType == Node.COMMENT_NODE)
                    n.parentElement.replaceChild(document.createTextNode(""), n);
                elementIndex++;
            }
            if (skipToEndComment.length > 0)
                if (skipToEndComment[skipToEndComment.length - 1]-- <= 0) {
                    skipToEndComment.pop();
                    var dd = kk_1[startComments.pop()];
                    dd.end = elementIndex;
                }
            return function (node) {
                var content = null;
                if (node.type == "ast") {
                    content = node.content.map(function (e) { return RendererInitializer.createRenderFunction(e); });
                }
                else
                    throw new Error();
                return {
                    type: "decorator", fn: RendererInitializer.astFunc(1000000, [], function (dataContext, elements) { return function (node) {
                        var a = node;
                        if (a.type != "VirtualNode")
                            throw new Error();
                        var wrapperElement = content == null ? a : new virtualDom.VNode(a.tagName, a.properties, [], a.key, a.namespace);
                        return new KnockoutBindingWidget(dataContext, wrapperElement, content != null ? content.map(function (e) { return function (dc) { return e(dc); }; }) : null, dataBindAttribute, kk_1);
                    }; })
                };
            };
        }
        return undefined;
    }
    DotvvmKnockoutCompat.createDecorator = createDecorator;
})(DotvvmKnockoutCompat || (DotvvmKnockoutCompat = {}));
var TwoWayBinding = (function () {
    function TwoWayBinding(update, value) {
        this.update = update;
        this.value = value;
    }
    return TwoWayBinding;
}());
var createArray = function (a) { return Array.prototype.slice.call(a); };
var HtmlElementPatcher = (function () {
    function HtmlElementPatcher(element, initialDom) {
        this.element = element;
        this.previousDom = initialDom;
    }
    HtmlElementPatcher.prototype.applyDom = function (dom) {
        if (this.previousDom == null) {
            var newElement = virtualDom.create(dom, {});
            this.element.parentElement.replaceChild(newElement, this.element);
            this.element = newElement;
        }
        else {
            var diff = virtualDom.diff(this.previousDom, dom);
            this.element = virtualDom.patch(this.element, diff);
        }
        this.previousDom = dom;
    };
    return HtmlElementPatcher;
}());
var Renderer = (function () {
    function Renderer(initialState, renderFunctions, vdomDispatcher) {
        var _this = this;
        this.renderFunctions = renderFunctions;
        this.vdomDispatcher = vdomDispatcher;
        this.currentFrameNumber = 0;
        this.startTime = null;
        this.setState(initialState);
        this.renderedStateObservable = ko.observable(initialState);
        this.rootDataContextObservable = ko.computed(function () { return ({
            dataContext: _this.renderedStateObservable(),
            update: _this.update.bind(_this)
        }); });
    }
    Object.defineProperty(Renderer.prototype, "state", {
        get: function () {
            return this._state;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Renderer.prototype, "isDirty", {
        get: function () {
            return this._isDirty;
        },
        enumerable: true,
        configurable: true
    });
    Renderer.prototype.dispatchUpdate = function () {
        if (!this._isDirty) {
            this._isDirty = true;
            this.currentFrameNumber = window.requestAnimationFrame(this.rerender.bind(this));
        }
    };
    Renderer.prototype.doUpdateNow = function () {
        if (this.currentFrameNumber !== null)
            window.cancelAnimationFrame(this.currentFrameNumber);
        this.rerender(performance.now());
    };
    Renderer.prototype.rerender = function (time) {
        var _this = this;
        if (this.startTime === null)
            this.startTime = time;
        var realStart = performance.now();
        this._isDirty = false;
        this.renderedStateObservable(this._state);
        var vdom = this.renderFunctions.map(function (f) { return f({
            update: _this.update.bind(_this),
            dataContext: _this._state
        }); });
        console.log("Dispatching new VDOM, t = ", performance.now() - time, "; t_cpu = ", performance.now() - realStart);
        this.vdomDispatcher(vdom);
        console.log("VDOM dispatched, t = ", performance.now() - time, "; t_cpu = ", performance.now() - realStart);
    };
    Renderer.prototype.setState = function (newState) {
        if (newState == null)
            throw new Error("State can't be null or undefined.");
        if (newState == this._state)
            return;
        this.dispatchUpdate();
        return this._state = newState;
    };
    Renderer.prototype.update = function (updater) {
        return this.setState(updater(this._state));
    };
    return Renderer;
}());
var RendererInitializer;
(function (RendererInitializer) {
    RendererInitializer.astConstant = function (val) { return ({ type: "constant", constant: val }); };
    RendererInitializer.astFunc = function (dataContextDepth, elements, func) { return ({ type: "func", dataContextDepth: dataContextDepth, elements: elements, func: func }); };
    // export const bindConstantOrFunction = <T, U>(source: ConstantOrFunction<T>, map: (val: T) => ConstantOrFunction<U>, maxDataContextDepth = 1000000) : ConstantOrFunction<U> => {
    //     if (source.type == "constant") return map(source.constant);
    //     else return { type: "func", dataContextDepth: Math.max(source.dataContextDepth, maxDataContextDepth), elements }
    // }
    RendererInitializer.mapConstantOrFunction = function (source, map, myElements) {
        if (source.type == "constant")
            return RendererInitializer.astFunc(0, myElements, function (a, e) { return map(source.constant, e); });
        else
            return { type: "func", dataContextDepth: source.dataContextDepth, elements: myElements.concat(source.elements), func: function (a, b) { return map(source.func(a, myElements.length == 0 ? b : b.slice(myElements.length)), b); } };
    };
    var createAttrAst = function (node) {
        return {
            type: "attr",
            attr: {
                name: RendererInitializer.astConstant(node.name),
                value: RendererInitializer.astConstant(node.value)
            }
        };
    };
    var applyPropsToElement = function (el, props) {
        if (props.length == 0)
            return el;
        if (el.type != "ast")
            throw new Error();
        var attributes = [];
        for (var _i = 0, props_1 = props; _i < props_1.length; _i++) {
            var p = props_1[_i];
            if (p.type == "attr") {
                attributes.push(p.attr);
            }
        }
        for (var _a = 0, _b = el.attributes; _a < _b.length; _a++) {
            var a = _b[_a];
            attributes.push(a);
        }
        el = __assign({}, el, { attributes: attributes });
        for (var _c = 0, props_2 = props; _c < props_2.length; _c++) {
            var decorator = props_2[_c];
            if (decorator.type == "decorator") {
                el = RendererInitializer.mapConstantOrFunction(decorator.fn, function (v, e) { return v(e[0]); }, [el]);
            }
        }
        return el;
    };
    var createElementAst = function (node) {
        var name = node.tagName.toLowerCase();
        var attributes = [];
        var realAttributes = {};
        var knockoutDecorator = DotvvmKnockoutCompat.createDecorator(node);
        if (knockoutDecorator != null)
            attributes.push(null); // this is replaced when result is created
        for (var i = 0; i < node.attributes.length; i++) {
            attributes.push(createAttrAst(node.attributes[i]));
            realAttributes[node.attributes[i].name] = node.attributes[i].value;
        }
        var children = [];
        var realChildren = [];
        for (var i = 0; i < node.childNodes.length; i++) {
            var c = createRenderAst(node.childNodes[i]);
            if (c != null) {
                children.push(c[0]);
                realChildren.push(c[1]);
            }
        }
        var result = {
            type: "ast",
            name: RendererInitializer.astConstant(name),
            content: children,
            attributes: []
        };
        if (knockoutDecorator != null)
            attributes[0] = knockoutDecorator(result);
        return [
            applyPropsToElement(result, attributes),
            new virtualDom.VNode(name, { attributes: realAttributes }, realChildren)
        ];
    };
    var createRenderAst = function (node) {
        if (node.nodeType == node.ELEMENT_NODE) {
            return createElementAst(node);
        }
        else if (node.nodeType == node.TEXT_NODE) {
            var text = node.data;
            return [
                { type: "text", content: RendererInitializer.astConstant(text) },
                new virtualDom.VText(text)
            ];
        }
        else if (node.nodeType == node.COMMENT_NODE) {
            node.parentElement.removeChild(node);
            return null;
        }
        else {
            throw new Error();
        }
    };
    RendererInitializer.immutableMap = function (array, fn) {
        var result = null;
        for (var i = 0; i < array.length; i++) {
            var rr = fn(array[i], i);
            if (result === null) {
                if (rr === array[i]) {
                    // ignore
                }
                else {
                    result = array.slice();
                    result[i] = rr;
                }
            }
            else {
                result[i] = rr;
            }
        }
        return result || array;
    };
    var optimizeConstants = function (ast, allowFirstLevel) {
        if (allowFirstLevel === void 0) { allowFirstLevel = true; }
        var optimizeFunction = function (fn) {
            if (fn.type == "constant")
                return fn;
            else {
                var elements2 = RendererInitializer.immutableMap(fn.elements, function (a) { return optimizeConstants(a); });
                var fn2 = elements2 === fn.elements ? fn : { elements: elements2, type: fn.type, dataContextDepth: fn.dataContextDepth, func: fn.func };
                if (fn2.dataContextDepth == 0 && fn2.elements.every(function (e) { return e.type == "constant"; })) {
                    return RendererInitializer.astConstant(fn2.func(undefined, fn2.elements.map(function (e) { return e["constant"]; })));
                }
                else
                    return fn2;
            }
        };
        var optimizeAttr = function (attr) {
            var name = optimizeFunction(attr.name);
            var value = optimizeFunction(attr.value);
            if (name == attr.name && value == attr.value)
                return attr;
            else
                return { name: name, value: value };
        };
        if (ast.type == "constant")
            return ast;
        else if (ast.type == "func") {
            return optimizeFunction(ast);
        }
        else if (ast.type == "text") {
            var text = optimizeFunction(ast.content);
            if (text.type == "constant") {
                return RendererInitializer.astConstant(new virtualDom.VText(text.constant));
            }
            else {
                return { type: "text", content: text };
            }
        }
        else {
            var ast2 = {
                type: ast.type,
                attributes: RendererInitializer.immutableMap(ast.attributes, optimizeAttr),
                content: RendererInitializer.immutableMap(ast.content, function (a) { return optimizeConstants(a); }),
                name: optimizeFunction(ast.name)
            };
            if (allowFirstLevel && ast2.name.type == "constant" && ast2.content.every(function (e) { return e.type == "constant"; }) && ast2.attributes.every(function (a) { return a.name.type == "constant" && a.value.type == "constant"; })) {
                var attributes = { attributes: {} };
                for (var _i = 0, _a = ast2.attributes; _i < _a.length; _i++) {
                    var attr = _a[_i];
                    var name_1 = attr.name["constant"], value = attr.value["constant"];
                    if (typeof value == "object" && (name_1 == "style" || name_1 == "dataset"))
                        attributes[name_1] = value;
                    else if (name_1 == "value" || name_1 == "defaultValue")
                        attributes[name_1] = value;
                    else
                        attributes.attributes[name_1] = value;
                }
                return RendererInitializer.astConstant(new virtualDom.VNode(ast2.name.constant, attributes, ast2.content.map(function (t) { return t["constant"]; })));
            }
            else {
                return ast2;
            }
        }
    };
    RendererInitializer.createRenderFunction = function (ast) {
        var evalFunction = function (fn, opt) {
            if (fn.type == "constant")
                return fn.constant;
            else {
                var elements = fn.elements.map(function (el) { return evalElement(opt, el); });
                return fn.func(opt, elements);
            }
        };
        var evalElement = function (dataContext, ast, options) {
            if (ast.type == "text") {
                return new virtualDom.VText(evalFunction(ast.content, dataContext));
            }
            else if (ast.type == "constant") {
                return ast.constant;
            }
            else if (ast.type == "func") {
                return evalFunction(ast, dataContext);
            }
            else {
                var dcAttr = ast.attributes.filter(function (e) { return e.name.type == "constant" && e.name.constant == "data-context"; })[0];
                if (dcAttr) {
                    var value_1 = evalFunction(dcAttr.value, dataContext);
                    dataContext =
                        ko.isObservable(value_1) ? { update: function (u) { return value_1(u(value_1())); }, dataContext: value_1() } :
                            value_1 instanceof TwoWayBinding ? { update: value_1.update, dataContext: value_1.value } :
                                { update: function (_) { throw new Error("Update is not supported"); }, dataContext: value_1 };
                }
                var attributes = { attributes: {} };
                for (var _i = 0, _a = ast.attributes; _i < _a.length; _i++) {
                    var attr = _a[_i];
                    var name_2 = evalFunction(attr.name, dataContext);
                    var value = evalFunction(attr.value, dataContext);
                    if (typeof value == "object" && (name_2 == "style" || name_2 == "dataset" || 'hook' in value))
                        attributes[name_2] = value;
                    else if (name_2 == "value" || name_2 == "defaultValue")
                        attributes[name_2] = value;
                    else if (name_2 == "data-context") { }
                    else
                        attributes.attributes[name_2] = value;
                }
                if (dcAttr || options && options.isRoot)
                    attributes["data-context-hook"] = new DataContextSetHook(dataContext);
                var element = new virtualDom.VNode(evalFunction(ast.name, dataContext), attributes, ast.content.map(function (t) { return evalElement(dataContext, t); }));
                if (dcAttr)
                    dataContext = dataContext.parentContext;
                return element;
            }
        };
        ast = optimizeConstants(ast, false);
        return function (opt) {
            return evalElement(opt, ast, { isRoot: true });
        };
    };
    var DataContextSetHook = (function () {
        function DataContextSetHook(dataContext) {
            this.dataContext = dataContext;
        }
        DataContextSetHook.prototype.hook = function (node, propertyName, previousValue) {
            var currentValue = node["@dotvvm-data-context"];
            if (ko.isWriteableObservable(currentValue))
                node["@dotvvm-data-context"](this.dataContext);
            else if (ko.isObservable(currentValue)) {
                if (currentValue() != this.dataContext)
                    console.error('Node ', node, ' contains a unwritable datacontext observable that does not corresponds with the hooked one', currentValue(), this.dataContext);
            }
            else if (currentValue)
                throw new Error('Node contains a @dotvvm-data-context prop that is not an observable.');
            else
                node["@dotvvm-data-context"] = ko.observable(this.dataContext);
        };
        DataContextSetHook.prototype.unhoook = function (node, propertyName, nextValue) {
        };
        return DataContextSetHook;
    }());
    function initFromNode(elements, viewModel) {
        var functions = elements.map(function (element) {
            var ast = createRenderAst(element);
            return { fn: RendererInitializer.createRenderFunction(ast[0]), initialDom: ast[1] };
        });
        var vdomDispatchers = elements.map(function (e, index) {
            return new HtmlElementPatcher(e, functions[index].initialDom);
        });
        return new Renderer(viewModel, functions.map(function (f) { return f.fn; }), function (d) { return d.map(function (a, i) { return vdomDispatchers[i].applyDom(a); }); });
    }
    RendererInitializer.initFromNode = initFromNode;
})(RendererInitializer || (RendererInitializer = {}));
var DotvvmDomUtils = (function () {
    function DotvvmDomUtils() {
    }
    DotvvmDomUtils.prototype.onDocumentReady = function (callback) {
        // many thanks to http://dustindiaz.com/smallest-domready-ever
        /in/.test(document.readyState) ? setTimeout('dotvvm.domUtils.onDocumentReady(' + callback + ')', 9) : callback();
    };
    DotvvmDomUtils.prototype.attachEvent = function (target, name, callback, useCapture) {
        if (useCapture === void 0) { useCapture = false; }
        if (target.addEventListener) {
            target.addEventListener(name, callback, useCapture);
        }
        else {
            target.attachEvent("on" + name, callback);
        }
    };
    return DotvvmDomUtils;
}());
var DotvvmEvents = (function () {
    function DotvvmEvents() {
        this.init = new DotvvmEvent("dotvvm.events.init", true);
        this.beforePostback = new DotvvmEvent("dotvvm.events.beforePostback");
        this.afterPostback = new DotvvmEvent("dotvvm.events.afterPostback");
        this.error = new DotvvmEvent("dotvvm.events.error");
        this.spaNavigating = new DotvvmEvent("dotvvm.events.spaNavigating");
        this.spaNavigated = new DotvvmEvent("dotvvm.events.spaNavigated");
        this.redirect = new DotvvmEvent("dotvvm.events.redirect");
    }
    return DotvvmEvents;
}());
// DotvvmEvent is used because CustomEvent is not browser compatible and does not support 
// calling missed events for handler that subscribed too late.
var DotvvmEvent = (function () {
    function DotvvmEvent(name, triggerMissedEventsOnSubscribe) {
        if (triggerMissedEventsOnSubscribe === void 0) { triggerMissedEventsOnSubscribe = false; }
        this.name = name;
        this.triggerMissedEventsOnSubscribe = triggerMissedEventsOnSubscribe;
        this.handlers = [];
        this.history = [];
    }
    DotvvmEvent.prototype.subscribe = function (handler) {
        this.handlers.push(handler);
        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                handler(history[i]);
            }
        }
    };
    DotvvmEvent.prototype.unsubscribe = function (handler) {
        var index = this.handlers.indexOf(handler);
        if (index >= 0) {
            this.handlers = this.handlers.splice(index, 1);
        }
    };
    DotvvmEvent.prototype.trigger = function (data) {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i](data);
        }
        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    };
    return DotvvmEvent;
}());
var DotvvmErrorEventArgs = (function () {
    function DotvvmErrorEventArgs(sender, viewModel, viewModelName, xhr, postbackClientId, serverResponseObject, isSpaNavigationError) {
        if (serverResponseObject === void 0) { serverResponseObject = undefined; }
        if (isSpaNavigationError === void 0) { isSpaNavigationError = false; }
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.xhr = xhr;
        this.postbackClientId = postbackClientId;
        this.serverResponseObject = serverResponseObject;
        this.isSpaNavigationError = isSpaNavigationError;
        this.handled = false;
    }
    return DotvvmErrorEventArgs;
}());
var DotvvmBeforePostBackEventArgs = (function () {
    function DotvvmBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, postbackClientId) {
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
        this.postbackClientId = postbackClientId;
        this.cancel = false;
        this.clientValidationFailed = false;
    }
    return DotvvmBeforePostBackEventArgs;
}());
var DotvvmAfterPostBackEventArgs = (function () {
    function DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, serverResponseObject, postbackClientId, commandResult) {
        if (commandResult === void 0) { commandResult = null; }
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
        this.serverResponseObject = serverResponseObject;
        this.postbackClientId = postbackClientId;
        this.commandResult = commandResult;
        this.isHandled = false;
        this.wasInterrupted = false;
    }
    return DotvvmAfterPostBackEventArgs;
}());
var DotvvmSpaNavigatingEventArgs = (function () {
    function DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, newUrl) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.newUrl = newUrl;
        this.cancel = false;
    }
    return DotvvmSpaNavigatingEventArgs;
}());
var DotvvmSpaNavigatedEventArgs = (function () {
    function DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, serverResponseObject) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.serverResponseObject = serverResponseObject;
        this.isHandled = false;
    }
    return DotvvmSpaNavigatedEventArgs;
}());
var DotvvmRedirectEventArgs = (function () {
    function DotvvmRedirectEventArgs(viewModel, viewModelName, url, replace) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.url = url;
        this.replace = replace;
        this.isHandled = false;
    }
    return DotvvmRedirectEventArgs;
}());
var DotvvmFileUpload = (function () {
    function DotvvmFileUpload() {
    }
    DotvvmFileUpload.prototype.showUploadDialog = function (sender) {
        // trigger the file upload dialog
        var iframe = this.getIframe(sender);
        this.createUploadId(sender, iframe);
        this.openUploadDialog(iframe);
    };
    DotvvmFileUpload.prototype.getIframe = function (sender) {
        return sender.parentElement.previousSibling;
    };
    DotvvmFileUpload.prototype.openUploadDialog = function (iframe) {
        var fileUpload = iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
    };
    DotvvmFileUpload.prototype.createUploadId = function (sender, iframe) {
        iframe = iframe || this.getIframe(sender);
        var uploadId = "DotVVM_upl" + new Date().getTime().toString();
        sender.parentElement.parentElement.setAttribute("data-dotvvm-upload-id", uploadId);
        iframe.setAttribute("data-dotvvm-upload-id", uploadId);
    };
    DotvvmFileUpload.prototype.reportProgress = function (targetControlId, isBusy, progress, result) {
        // find target control viewmodel
        var targetControl = document.querySelector("div[data-dotvvm-upload-id='" + targetControlId.value + "']");
        var viewModel = ko.dataFor(targetControl.firstChild);
        // determine the status
        if (typeof result === "string") {
            // error during upload
            viewModel.Error(result);
        }
        else {
            // files were uploaded successfully
            viewModel.Error("");
            for (var i = 0; i < result.length; i++) {
                viewModel.Files.push(dotvvm.serialization.wrapObservable(dotvvm.serialization.deserialize(result[i])));
            }
            // call the handler
            if ((targetControl.attributes["data-dotvvm-upload-completed"] || { value: null }).value) {
                new Function(targetControl.attributes["data-dotvvm-upload-completed"].value).call(targetControl);
            }
        }
        viewModel.Progress(progress);
        viewModel.IsBusy(isBusy);
    };
    return DotvvmFileUpload;
}());
var DotvvmFileUploadCollection = (function () {
    function DotvvmFileUploadCollection() {
        this.Files = ko.observableArray();
        this.Progress = ko.observable(0);
        this.Error = ko.observable();
        this.IsBusy = ko.observable();
    }
    return DotvvmFileUploadCollection;
}());
var DotvvmFileUploadData = (function () {
    function DotvvmFileUploadData() {
        this.FileId = ko.observable();
        this.FileName = ko.observable();
        this.FileSize = ko.observable();
        this.IsFileTypeAllowed = ko.observable();
        this.IsMaxSizeExceeded = ko.observable();
        this.IsAllowed = ko.observable();
    }
    return DotvvmFileUploadData;
}());
var DotvvmFileSize = (function () {
    function DotvvmFileSize() {
        this.Bytes = ko.observable();
        this.FormattedText = ko.observable();
    }
    return DotvvmFileSize;
}());
var DotvvmGlobalize = (function () {
    function DotvvmGlobalize() {
    }
    DotvvmGlobalize.prototype.format = function (format) {
        var _this = this;
        var values = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            values[_i - 1] = arguments[_i];
        }
        return format.replace(/\{([1-9]?[0-9]+)(:[^}])?\}/g, function (match, group0, group1) {
            var value = values[parseInt(group0)];
            if (group1) {
                return _this.formatString(group1, value);
            }
            else {
                return value;
            }
        });
    };
    DotvvmGlobalize.prototype.formatString = function (format, value) {
        value = ko.unwrap(value);
        if (value == null)
            return "";
        if (typeof value === "string") {
            // JSON date in string
            value = this.parseDotvvmDate(value);
        }
        if (format === "" || format === null) {
            format = "G";
        }
        return dotvvm_Globalize.format(value, format, dotvvm.culture);
    };
    DotvvmGlobalize.prototype.parseDotvvmDate = function (value) {
        var match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})$");
        if (match) {
            return new Date(parseInt(match[1]), parseInt(match[2]) - 1, parseInt(match[3]), parseInt(match[4]), parseInt(match[5]), parseInt(match[6]), match.length > 7 ? parseInt(match[7].substring(1, 4)) : 0);
        }
        return null;
    };
    DotvvmGlobalize.prototype.parseNumber = function (value) {
        return dotvvm_Globalize.parseFloat(value, 10, dotvvm.culture);
    };
    DotvvmGlobalize.prototype.parseDate = function (value, format, previousValue) {
        return dotvvm_Globalize.parseDate(value, format, dotvvm.culture, previousValue);
    };
    DotvvmGlobalize.prototype.bindingDateToString = function (value, format) {
        if (format === void 0) { format = "G"; }
        var unwrapedVal = ko.unwrap(value);
        var date = typeof unwrapedVal == "string" ? this.parseDotvvmDate(unwrapedVal) : unwrapedVal;
        if (date == null)
            return "";
        if (ko.isWriteableObservable(value)) {
            var setter_1 = typeof unwrapedVal == "string" ? function (v) { return value(dotvvm.serialization.serializeDate(v)); } : value;
            return ko.pureComputed({
                read: function () { return dotvvm_Globalize.format(date, format, dotvvm.culture); },
                write: function (val) { return setter_1(dotvvm_Globalize.parseDate(val, format, dotvvm.culture)); }
            });
        }
        else {
            return dotvvm_Globalize.format(date, format, dotvvm.culture);
        }
    };
    DotvvmGlobalize.prototype.bindingNumberToString = function (value, format) {
        if (format === void 0) { format = "G"; }
        var unwrapedVal = ko.unwrap(value);
        var num = typeof unwrapedVal == "string" ? this.parseNumber(unwrapedVal) : unwrapedVal;
        if (num == null)
            return "";
        if (ko.isWriteableObservable(value)) {
            return ko.pureComputed({
                read: function () { return dotvvm_Globalize.format(num, format, dotvvm.culture); },
                write: function (val) { return value(dotvvm_Globalize.parseFloat(val, 10, dotvvm.culture)); }
            });
        }
        else {
            return dotvvm_Globalize.format(num, format, dotvvm.culture);
        }
    };
    return DotvvmGlobalize;
}());
var DotvvmPostBackHandler = (function () {
    function DotvvmPostBackHandler() {
    }
    DotvvmPostBackHandler.prototype.execute = function (callback, sender) {
    };
    return DotvvmPostBackHandler;
}());
var ConfirmPostBackHandler = (function (_super) {
    __extends(ConfirmPostBackHandler, _super);
    function ConfirmPostBackHandler(message) {
        var _this = _super.call(this) || this;
        _this.message = message;
        return _this;
    }
    ConfirmPostBackHandler.prototype.execute = function (callback, sender) {
        if (confirm(this.message)) {
            callback();
        }
    };
    return ConfirmPostBackHandler;
}(DotvvmPostBackHandler));
var PostbackOptions = (function () {
    function PostbackOptions(postbackId, sender, args, viewModel, viewModelName, validationTargetPath) {
        if (args === void 0) { args = []; }
        this.postbackId = postbackId;
        this.sender = sender;
        this.args = args;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
    }
    return PostbackOptions;
}());
var ConfirmPostBackHandler2 = (function () {
    function ConfirmPostBackHandler2(message) {
        this.message = message;
    }
    ConfirmPostBackHandler2.prototype.execute = function (callback, options) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            if (confirm(_this.message)) {
                callback().then(resolve, reject);
            }
            else {
                reject({ type: "handler", handler: _this, message: "The postback was not confirmed" });
            }
        });
    };
    return ConfirmPostBackHandler2;
}());
var DotvvmSerialization = (function () {
    function DotvvmSerialization() {
    }
    DotvvmSerialization.prototype.deserialize = function (viewModel, target, deserializeAll) {
        if (deserializeAll === void 0) { deserializeAll = false; }
        if (typeof (viewModel) == "undefined" || viewModel == null) {
            return viewModel;
        }
        if (typeof (viewModel) == "string" || typeof (viewModel) == "number" || typeof (viewModel) == "boolean") {
            return viewModel;
        }
        if (viewModel instanceof Date) {
            return dotvvm.serialization.serializeDate(viewModel);
        }
        // handle arrays
        if (viewModel instanceof Array) {
            if (ko.isObservable(target) && "removeAll" in target && target() != null && target().length === viewModel.length) {
                // the array has the same number of items, update it
                var targetArray = target();
                for (var i = 0; i < viewModel.length; i++) {
                    var targetItem = targetArray[i]();
                    var deserialized = this.deserialize(viewModel[i], targetItem, deserializeAll);
                    if (targetItem !== deserialized) {
                        // update the observable only if the item has changed
                        targetArray[i](deserialized);
                    }
                }
            }
            else {
                // rebuild the array because it is different
                var array = [];
                for (var i = 0; i < viewModel.length; i++) {
                    array.push(this.wrapObservable(this.deserialize(viewModel[i], {}, deserializeAll)));
                }
                if (ko.isObservable(target)) {
                    if (!("removeAll" in target)) {
                        // if the previous value was null, the property is not an observable array - make it
                        ko.utils.extend(target, ko.observableArray['fn']);
                        target = target.extend({ 'trackArrayChanges': true });
                    }
                    target(array);
                }
                else {
                    target = ko.observableArray(array);
                }
            }
            return target;
        }
        // handle objects
        if (typeof (target) === "undefined") {
            target = {};
        }
        var result = ko.unwrap(target);
        if (result == null) {
            target = result = {};
        }
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop) && !/\$options$/.test(prop)) {
                var value = viewModel[prop];
                if (typeof (value) === "undefined") {
                    continue;
                }
                if (!ko.isObservable(value) && typeof (value) === "function") {
                    continue;
                }
                var options = viewModel[prop + "$options"];
                if (!deserializeAll && options && options.doNotUpdate) {
                    continue;
                }
                // deserialize value
                var deserialized = ko.isObservable(value) ? value : this.deserialize(value, result[prop], deserializeAll);
                if (value instanceof Date) {
                    // if we get Date value from API, it was converted to string, but we should note that it was date to convert it back
                    result[prop + "$options"] = result[prop + "$options"] || {};
                    result[prop + "$options"].isDate = true;
                }
                // update the property
                if (ko.isObservable(deserialized)) {
                    if (ko.isObservable(result[prop])) {
                        if (deserialized() !== result[prop]()) {
                            result[prop](deserialized());
                        }
                    }
                    else {
                        result[prop] = deserialized;
                    }
                }
                else {
                    if (ko.isObservable(result[prop])) {
                        if (deserialized !== result[prop]())
                            result[prop](deserialized);
                    }
                    else {
                        result[prop] = ko.observable(deserialized);
                    }
                }
                if (options && options.clientExtenders && ko.isObservable(result[prop])) {
                    for (var j = 0; j < options.clientExtenders.length; j++) {
                        var extenderOptions = {};
                        var extenderInfo = options.clientExtenders[j];
                        extenderOptions[extenderInfo.name] = extenderInfo.parameter;
                        result[prop].extend(extenderOptions);
                    }
                }
            }
        }
        // copy the property options metadata
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop) && /\$options$/.test(prop)) {
                result[prop] = result[prop] || {};
                for (var optProp in viewModel[prop]) {
                    if (viewModel[prop].hasOwnProperty(optProp)) {
                        result[prop][optProp] = viewModel[prop][optProp];
                    }
                }
                var originalName = prop.substring(0, prop.length - "$options".length);
                if (typeof result[originalName] === "undefined") {
                    result[originalName] = ko.observable();
                }
            }
        }
        return target;
    };
    DotvvmSerialization.prototype.wrapObservable = function (obj) {
        if (!ko.isObservable(obj))
            return ko.observable(obj);
        return obj;
    };
    DotvvmSerialization.prototype.serialize = function (viewModel, opt) {
        if (opt === void 0) { opt = {}; }
        opt = ko.utils.extend({}, opt);
        if (opt.pathOnly && opt.path && opt.path.length === 0)
            opt.pathOnly = false;
        if (viewModel == null) {
            return null;
        }
        if (typeof (viewModel) === "string" || typeof (viewModel) === "number" || typeof (viewModel) === "boolean") {
            return viewModel;
        }
        if (ko.isObservable(viewModel)) {
            return this.serialize(viewModel(), opt);
        }
        if (typeof (viewModel) === "function") {
            return null;
        }
        if (viewModel instanceof Array) {
            if (opt.pathOnly && opt.path) {
                var index = parseInt(opt.path.pop());
                var array = new Array(index + 1);
                array[index] = this.serialize(viewModel[index], opt);
                opt.path.push(index.toString());
                return array;
            }
            else {
                var array = [];
                for (var i = 0; i < viewModel.length; i++) {
                    array.push(this.serialize(viewModel[i], opt));
                }
                return array;
            }
        }
        if (viewModel instanceof Date) {
            if (opt.restApiTarget) {
                return viewModel;
            }
            else {
                return this.serializeDate(viewModel);
            }
        }
        var pathProp = opt.path && opt.path.pop();
        var result = {};
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop)) {
                if (opt.pathOnly && prop !== pathProp) {
                    continue;
                }
                var value = viewModel[prop];
                if (opt.ignoreSpecialProperties && prop[0] === "$")
                    continue;
                if (!opt.serializeAll && (/\$options$/.test(prop) || prop === "$validationErrors")) {
                    continue;
                }
                if (typeof (value) === "undefined") {
                    continue;
                }
                if (!ko.isObservable(value) && typeof (value) === "function") {
                    continue;
                }
                var options = viewModel[prop + "$options"];
                if (!opt.serializeAll && options && options.doNotPost) {
                    // continue
                }
                else if (opt.oneLevel) {
                    result[prop] = ko.unwrap(value);
                }
                else if (!opt.serializeAll && options && options.pathOnly && opt.pathMatcher) {
                    var path = options.pathOnly;
                    if (!(path instanceof Array)) {
                        path = opt.path || this.findObject(value, opt.pathMatcher);
                    }
                    if (path) {
                        if (path.length === 0) {
                            result[prop] = this.serialize(value, opt);
                        }
                        else {
                            result[prop] = this.serialize(value, { ignoreSpecialProperties: opt.ignoreSpecialProperties, serializeAll: opt.serializeAll, path: path, pathOnly: true });
                        }
                    }
                }
                else {
                    result[prop] = this.serialize(value, opt);
                }
                if (options && options.type && !this.validateType(result[prop], options.type)) {
                    delete result[prop];
                    options.wasInvalid = true;
                }
            }
        }
        if (pathProp && opt.path)
            opt.path.push(pathProp);
        return result;
    };
    DotvvmSerialization.prototype.validateType = function (value, type) {
        var nullable = type[type.length - 1] === "?";
        if (nullable) {
            type = type.substr(0, type.length - 1);
        }
        if (nullable && (value == null || value == "")) {
            return true;
        }
        if (!nullable && (value === null || typeof value === "undefined")) {
            return false;
        }
        var intmatch = /(u?)int(\d*)/.exec(type);
        if (intmatch) {
            if (!/^-?\d*$/.test(value))
                return false;
            var unsigned = intmatch[1] === "u";
            var bits = parseInt(intmatch[2]);
            var minValue = 0;
            var maxValue = Math.pow(2, bits) - 1;
            if (!unsigned) {
                minValue = -((maxValue / 2) | 0);
                maxValue = maxValue + minValue;
            }
            var int = parseInt(value);
            return int >= minValue && int <= maxValue && int === parseFloat(value);
        }
        if (type === "number" || type === "single" || type === "double" || type === "decimal") {
            // should check if the value is numeric or number in a string
            return +value === value || (!isNaN(+value) && typeof value === "string");
        }
        return true;
    };
    DotvvmSerialization.prototype.findObject = function (obj, matcher) {
        if (matcher(obj))
            return [];
        obj = ko.unwrap(obj);
        if (matcher(obj))
            return [];
        if (typeof obj != "object")
            return null;
        for (var p in obj) {
            if (obj.hasOwnProperty(p)) {
                var match = this.findObject(obj[p], matcher);
                if (match) {
                    match.push(p);
                    return match;
                }
            }
        }
        return null;
    };
    DotvvmSerialization.prototype.flatSerialize = function (viewModel) {
        return this.serialize(viewModel, { ignoreSpecialProperties: true, oneLevel: true, serializeAll: true });
    };
    DotvvmSerialization.prototype.getPureObject = function (viewModel) {
        viewModel = ko.unwrap(viewModel);
        if (viewModel instanceof Array)
            return viewModel.map(this.getPureObject.bind(this));
        var result = {};
        for (var prop in viewModel) {
            if (prop[0] != '$')
                result[prop] = viewModel[prop];
        }
        return result;
    };
    DotvvmSerialization.prototype.pad = function (value, digits) {
        while (value.length < digits) {
            value = "0" + value;
        }
        return value;
    };
    DotvvmSerialization.prototype.serializeDate = function (date, convertToUtc) {
        if (convertToUtc === void 0) { convertToUtc = true; }
        if (typeof date == "string") {
            // just print in the console if it's invalid
            if (dotvvm.globalize.parseDotvvmDate(date) != null)
                console.error(new Error("Date " + date + " is invalid."));
            return date;
        }
        var date2 = new Date(date.getTime());
        if (convertToUtc) {
            date2.setMinutes(date.getMinutes() + date.getTimezoneOffset());
        }
        else {
            date2 = date;
        }
        var y = this.pad(date2.getFullYear().toString(), 4);
        var m = this.pad((date2.getMonth() + 1).toString(), 2);
        var d = this.pad(date2.getDate().toString(), 2);
        var h = this.pad(date2.getHours().toString(), 2);
        var mi = this.pad(date2.getMinutes().toString(), 2);
        var s = this.pad(date2.getSeconds().toString(), 2);
        var ms = this.pad(date2.getMilliseconds().toString(), 3);
        return y + "-" + m + "-" + d + "T" + h + ":" + mi + ":" + s + "." + ms + "0000";
    };
    return DotvvmSerialization;
}());
/// <reference path="typings/globalize/globalize.d.ts" />
document.getElementByDotvvmId = function (id) {
    return document.querySelector("[data-dotvvm-id='" + id + "']");
};
var DotVVM = (function () {
    function DotVVM() {
        var _this = this;
        this.postBackCounter = 0;
        this.lastStartedPostack = 0;
        this.resourceSigns = {};
        this.isViewModelUpdating = true;
        this.receivedViewModel = {};
        // warning this property is referenced in ModelState.cs and KnockoutHelper.cs
        this.isSpaReady = ko.observable(false);
        this.serialization = new DotvvmSerialization();
        this.postBackHandlers = {
            confirm: function (options) { return new ConfirmPostBackHandler(options.message); }
        };
        this.postbackHandlers2 = {
            confirm: function (options) { return new ConfirmPostBackHandler2(options.message); }
        };
        this.beforePostbackEventPostbackHandler = {
            execute: function (callback, options) {
                // trigger beforePostback event
                var beforePostbackArgs = new DotvvmBeforePostBackEventArgs(options.sender, options.viewModel, options.viewModelName, options.validationTargetPath, options.postbackId);
                _this.events.beforePostback.trigger(beforePostbackArgs);
                if (beforePostbackArgs.cancel) {
                    return Promise.reject({ type: "event", options: options });
                }
                return callback();
            }
        };
        this.isPostBackRunningHandler = {
            execute: function (callback, options) {
                _this.isPostbackRunning(true);
                var promise = callback();
                promise.then(function () { return _this.isPostbackRunning(false); }, function () { return _this.isPostbackRunning(false); });
                return promise;
            }
        };
        this.windowsSetTimeoutHandler = {
            execute: function (callback, options) {
                return new Promise(function (resolve, reject) { return window.setTimeout(resolve, 0); })
                    .then(function () { return callback(); });
            }
        };
        this.defaultConcurrencyPostbackHandler = {
            execute: function (callback, options) {
                return callback().then(function (result) {
                    if (_this.lastStartedPostack == options.postbackId)
                        return result;
                    else
                        return (function () { return Promise.reject(null); });
                });
            }
        };
        this.globalPostbackHandlers = [this.isPostBackRunningHandler];
        this.globalLaterPostbackHandlers = [this.beforePostbackEventPostbackHandler];
        this.events = new DotvvmEvents();
        this.globalize = new DotvvmGlobalize();
        this.evaluator = new DotvvmEvaluator();
        this.domUtils = new DotvvmDomUtils();
        this.fileUpload = new DotvvmFileUpload();
        this.extensions = {};
        this.isPostbackRunning = ko.observable(false);
    }
    Object.defineProperty(DotVVM.prototype, "viewModels", {
        get: function () {
            return this._viewModels || (this._viewModels = {
                root: {
                    viewModel: DotvvmKnockoutCompat.createKnockoutContext(this.rootRenderer.rootDataContextObservable).$data,
                    validationRules: this.receivedViewModel.validationRules
                }
            });
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DotVVM.prototype, "viewModelObservables", {
        get: function () {
            return this._viewModelObservables || (this._viewModelObservables = {
                root: {
                    viewModel: DotvvmKnockoutCompat.createKnockoutContext(this.rootRenderer.rootDataContextObservable).$rawData
                }
            });
        },
        enumerable: true,
        configurable: true
    });
    DotVVM.prototype.convertOldHandler = function (handler) {
        return {
            execute: function (callback, options) {
                return new Promise(function (resolve, reject) {
                    var timeout = setTimeout(function () { return reject({ type: "handler", options: options, handler: handler, message: "The postback handler can't indicate that the postback was rejected and the timeout has passed." }); }, 10000);
                    handler.execute(function () {
                        clearTimeout(timeout);
                        callback().then(resolve, reject);
                    }, options.sender);
                });
            }
        };
    };
    DotVVM.prototype.init = function (viewModelName, culture) {
        var _this = this;
        this.addKnockoutBindingHandlers();
        // load the viewmodel
        var thisViewModel = this.receivedViewModel = JSON.parse(document.getElementById("__dot_viewmodel_" + viewModelName).value);
        if (typeof thisViewModel != "object" || thisViewModel.viewModel == null)
            throw new Error("Received viewmodel is invalid");
        if (thisViewModel.resources) {
            for (var r in thisViewModel.resources) {
                this.resourceSigns[r] = true;
            }
        }
        if (thisViewModel.renderedResources) {
            thisViewModel.renderedResources.forEach(function (r) { return _this.resourceSigns[r] = true; });
        }
        var idFragment = thisViewModel.resultIdFragment;
        var viewModel = thisViewModel.viewModel;
        // initialize services
        this.culture = culture;
        this.validation = new DotvvmValidation(this);
        // wrap it in the observable
        // this.viewModelObservables[viewModelName] = ko.observable(viewModel);
        // ko.applyBindings(this.viewModelObservables[viewModelName], document.documentElement);
        if (createArray(document.body.childNodes).some(function (n) { return n.nodeType == Node.COMMENT_NODE && ko.bindingProvider.instance.nodeHasBindings(n); })) {
            var c = document.body.firstChild;
            var wrapperElement = document.createElement("div");
            document.body.replaceChild(wrapperElement, c);
            while (c != null) {
                if (c.nodeType == Node.ELEMENT_NODE && c.tagName.toLowerCase() == "script")
                    break;
                wrapperElement.appendChild(c);
                c = wrapperElement.nextSibling;
            }
        }
        var elements = [];
        for (var _i = 0, _a = createArray(document.body.children); _i < _a.length; _i++) {
            var e = _a[_i];
            if (e.tagName.toLowerCase() == "script") {
                break;
            }
            else {
                elements.push(e);
            }
        }
        var renderer = this.rootRenderer = RendererInitializer.initFromNode(elements, viewModel);
        renderer.doUpdateNow();
        // trigger the init event
        this.events.init.trigger({ viewModel: viewModel });
        // handle SPA requests
        var spaPlaceHolder = this.getSpaPlaceHolder();
        if (spaPlaceHolder != null) {
            this.domUtils.attachEvent(window, "hashchange", function () { return _this.handleHashChange(viewModelName, spaPlaceHolder, false); });
            this.handleHashChange(viewModelName, spaPlaceHolder, true);
        }
        this.isViewModelUpdating = false;
        if (idFragment) {
            if (spaPlaceHolder) {
                var element = document.getElementById(idFragment);
                if (element && "function" == typeof element.scrollIntoView)
                    element.scrollIntoView(true);
            }
            else
                location.hash = idFragment;
        }
        // persist the viewmodel in the hidden field so the Back button will work correctly
        this.domUtils.attachEvent(window, "beforeunload", function (e) {
            _this.persistViewModel(viewModelName);
        });
    };
    DotVVM.prototype.handleHashChange = function (viewModelName, spaPlaceHolder, isInitialPageLoad) {
        if (document.location.hash.indexOf("#!/") === 0) {
            // the user requested navigation to another SPA page
            this.navigateCore(viewModelName, document.location.hash.substring(2));
        }
        else {
            var url = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");
            if (url) {
                // perform redirect to default page
                url = "#!/" + url;
                url = this.fixSpaUrlPrefix(url);
                this.performRedirect(url, isInitialPageLoad);
            }
            else if (!isInitialPageLoad) {
                // get startup URL and redirect there
                url = document.location.toString();
                var slashIndex = url.indexOf('/', 'https://'.length);
                if (slashIndex > 0) {
                    url = url.substring(slashIndex);
                }
                else {
                    url = "/";
                }
                this.navigateCore(viewModelName, url);
            }
            else {
                // the page was loaded for the first time
                this.isSpaReady(true);
                spaPlaceHolder.style.display = "";
            }
        }
    };
    // binding helpers
    DotVVM.prototype.postbackScript = function (bindingId) {
        var _this = this;
        return function (pageArea, sender, pathFragments, controlId, useWindowSetTimeout, validationTarget, context, handlers) {
            _this.postBack(pageArea, sender, pathFragments, bindingId, controlId, useWindowSetTimeout, validationTarget, context, handlers);
        };
    };
    DotVVM.prototype.persistViewModel = function (viewModelName) {
        document.getElementById("__dot_viewmodel_" + viewModelName).value = JSON.stringify(__assign({ viewModel: this.rootRenderer.state }, this.receivedViewModel));
    };
    DotVVM.prototype.backUpPostBackConter = function () {
        this.postBackCounter++;
        return this.postBackCounter;
    };
    DotVVM.prototype.isPostBackStillActive = function (currentPostBackCounter) {
        return this.postBackCounter === currentPostBackCounter;
    };
    DotVVM.prototype.staticCommandPostback = function (viewModelName, sender, command, args, callback, errorCallback) {
        var _this = this;
        if (callback === void 0) { callback = function (_) { }; }
        if (errorCallback === void 0) { errorCallback = function (xhr, error) { }; }
        if (this.isPostBackProhibited(sender))
            return;
        // TODO: events for static command postback
        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();
        var data = this.serialization.serialize({
            "args": args,
            "command": command,
            "$csrfToken": this.rootRenderer.state.$csrfToken
        });
        this.postJSON(this.receivedViewModel.url, "POST", ko.toJSON(data), function (response) {
            if (!_this.isPostBackStillActive(currentPostBackCounter))
                return;
            try {
                _this.isViewModelUpdating = true;
                callback(JSON.parse(response.responseText));
            }
            catch (error) {
                errorCallback(response, error);
            }
            finally {
                _this.isViewModelUpdating = false;
            }
        }, function (xhr) {
            console.warn("StaticCommand postback failed: " + xhr.status + " - " + xhr.statusText, xhr);
            errorCallback(xhr);
        }, function (xhr) {
            xhr.setRequestHeader("X-PostbackType", "StaticCommand");
        });
    };
    DotVVM.prototype.processPassedId = function (id, context) {
        if (typeof id == "string" || id == null)
            return id;
        if (typeof id == "object" && id.expr)
            return this.evaluator.evaluateOnViewModel(context, id.expr);
        throw new Error("invalid argument");
    };
    DotVVM.prototype.getPostbackHandler = function (name) {
        var _this = this;
        var handler = this.postbackHandlers2[name];
        if (handler) {
            return handler;
        }
        else {
            var handler_1 = this.postBackHandlers[name];
            if (!handler_1)
                throw new Error("Could not find postback handler of name '" + name + "'");
            return function (options) { return _this.convertOldHandler(handler_1(options)); };
        }
    };
    DotVVM.prototype.isPostbackHandler = function (obj) {
        return obj && typeof obj.execute == "function";
    };
    DotVVM.prototype.findPostbackHandlers = function (knockoutContext, config) {
        var _this = this;
        var createHandler = function (name, options) { return options.enabled === false ? null : _this.getPostbackHandler(name)(options); };
        return config.map(function (h) {
            return typeof h == 'string' ? createHandler(h, {}) :
                _this.isPostbackHandler(h) ? h :
                    createHandler(h.name, _this.evaluator.evaluateOnViewModel(knockoutContext, "(" + h.options.toString() + ")()"));
        })
            .filter(function (h) { return h != null; });
    };
    DotVVM.prototype.applyPostbackHandlersCore = function (callback, options, handlers) {
        if (handlers == null || handlers.length === 0) {
            return callback(options);
        }
        else {
            return new Promise(function (resolve, reject) {
                handlers
                    .reduceRight(function (prev, val, index) { return function () {
                    return val.execute(prev, options);
                }; }, function () {
                    var r = callback(options);
                    r.then(resolve, reject);
                    return r;
                })();
            });
        }
    };
    DotVVM.prototype.applyPostbackHandlers = function (callback, sender, handlers, args, validationPath, context, viewModel, viewModelName) {
        if (args === void 0) { args = []; }
        if (context === void 0) { context = ko.contextFor(sender); }
        if (viewModel === void 0) { viewModel = context.$root; }
        var options = new PostbackOptions(this.backUpPostBackConter(), sender, args, viewModel, viewModelName, validationPath);
        return this.applyPostbackHandlersCore(callback, options, this.findPostbackHandlers(context, handlers || []));
    };
    DotVVM.prototype.postbackCore = function (viewModelName, options, path, command, controlUniqueId, context, validationTargetPath, commandArgs) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            var state = _this.rootRenderer.state;
            _this.lastStartedPostack = options.postbackId;
            // perform the postback
            _this.updateDynamicPathFragments(context, path);
            var data = {
                viewModel: _this.serialization.serialize(state, { pathMatcher: function (val) { return context && val == context.$data; } }),
                currentPath: path,
                command: command,
                controlUniqueId: _this.processPassedId(controlUniqueId, context),
                validationTargetPath: validationTargetPath || null,
                renderedResources: _this.receivedViewModel.renderedResources,
                commandArgs: commandArgs
            };
            _this.postJSON(_this.receivedViewModel.url, "POST", ko.toJSON(data), function (result) {
                resolve(function () { return new Promise(function (resolve, reject) {
                    var locationHeader = result.getResponseHeader("Location");
                    var resultObject = locationHeader != null && locationHeader.length > 0 ?
                        { action: "redirect", url: locationHeader } :
                        JSON.parse(result.responseText);
                    _this.loadResourceList(resultObject.resources, function () {
                        var isSuccess = false;
                        if (resultObject.action === "successfulCommand") {
                            try {
                                _this.isViewModelUpdating = true;
                                // remove updated controls
                                var updatedControls = _this.cleanUpdatedControls(resultObject);
                                if (!resultObject.viewModel && resultObject.viewModelDiff) {
                                    // TODO: patch (~deserialize) it to ko.observable viewModel
                                    resultObject.viewModel = _this.patch(_this.rootRenderer.state, resultObject.viewModelDiff);
                                }
                                // update the viewmodel
                                if (resultObject.viewModel) {
                                    _this.rootRenderer.setState(resultObject.viewModel);
                                }
                                isSuccess = true;
                                // remove updated controls which were previously hidden
                                _this.cleanUpdatedControls(resultObject, updatedControls);
                                // add updated controls
                                _this.restoreUpdatedControls(resultObject, updatedControls, true);
                            }
                            finally {
                                _this.isViewModelUpdating = false;
                            }
                        }
                        else if (resultObject.action === "redirect") {
                            // redirect
                            _this.handleRedirect(resultObject, viewModelName);
                            return resolve();
                        }
                        var idFragment = resultObject.resultIdFragment;
                        if (idFragment) {
                            if (_this.getSpaPlaceHolder() || location.hash == "#" + idFragment) {
                                var element = document.getElementById(idFragment);
                                if (element && "function" == typeof element.scrollIntoView)
                                    element.scrollIntoView(true);
                            }
                            else
                                location.hash = idFragment;
                        }
                        // trigger afterPostback event
                        if (!isSuccess) {
<<<<<<< HEAD
                            reject(new DotvvmErrorEventArgs(state, result));
=======
                            reject(new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, result, options.postbackId, resultObject));
>>>>>>> js-postback-refactoring
                        }
                        else {
                            var afterPostBackArgs = new DotvvmAfterPostBackEventArgs(options.sender, state, viewModelName, validationTargetPath, resultObject, options.postbackId, resultObject.comandResult);
                            resolve(afterPostBackArgs);
                        }
                    });
                }); });
            }, function (xhr) {
<<<<<<< HEAD
                reject({ type: 'network', error: new DotvvmErrorEventArgs(state, xhr) });
=======
                reject({ type: 'network', options: options, error: new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, xhr, options.postbackId) });
>>>>>>> js-postback-refactoring
            });
        });
    };
    DotVVM.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId, useWindowSetTimeout, validationTargetPath, context, handlers, commandArgs) {
        var _this = this;
        if (this.isPostBackProhibited(sender))
            return new Promise(function (resolve, reject) { return reject("rejected"); });
        context = context || ko.contextFor(sender);
        var preHandlers = Array.prototype.concat.call([this.defaultConcurrencyPostbackHandler], this.globalPostbackHandlers);
        if (useWindowSetTimeout) {
            preHandlers.push(this.windowsSetTimeoutHandler);
        }
        var preparedHandlers = this.findPostbackHandlers(context, preHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers));
        var options = new PostbackOptions(this.backUpPostBackConter(), sender, commandArgs, context.$data, viewModelName, validationTargetPath);
        var promise = this.applyPostbackHandlersCore(function (options) {
            return _this.postbackCore(viewModelName, options, path, command, controlUniqueId, context, validationTargetPath, commandArgs);
        }, options, preparedHandlers);
        var result = promise.then(function (r) { return r().then(function (r) { return r; }, function (error) { return Promise.reject({ type: "commit", args: error }); }); }, Promise.reject);
        result.then(function (r) { return _this.events.afterPostback.trigger(r); }, function (error) {
            var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(sender, options.viewModel, viewModelName, validationTargetPath, error.type == "commit" ? error.args.serverResponseObject : null, options.postbackId);
            if (error.type == "handler" || error.type == "event") {
                // trigger afterPostback event
                afterPostBackArgsCanceled.wasInterrupted = true;
            }
            else if (error.type == "network") {
                _this.events.error.trigger(error.args);
            }
            _this.events.afterPostback.trigger(afterPostBackArgsCanceled);
        });
        return result;
    };
    DotVVM.prototype.loadResourceList = function (resources, callback) {
        var html = "";
        for (var name in resources) {
            if (!/^__noname_\d+$/.test(name)) {
                if (this.resourceSigns[name])
                    continue;
                this.resourceSigns[name] = true;
            }
            html += resources[name] + " ";
        }
        if (html.trim() === "") {
            setTimeout(callback, 4);
            return;
        }
        else {
            var tmp = document.createElement("div");
            tmp.innerHTML = html;
            var elements = [];
            for (var i = 0; i < tmp.children.length; i++) {
                elements.push(tmp.children.item(i));
            }
            this.loadResourceElements(elements, 0, callback);
        }
    };
    DotVVM.prototype.loadResourceElements = function (elements, offset, callback) {
        var _this = this;
        if (offset >= elements.length) {
            callback();
            return;
        }
        var el = elements[offset];
        var waitForScriptLoaded = false;
        if (el.tagName.toLowerCase() == "script") {
            // create the script element
            var script = document.createElement("script");
            if (el.src) {
                script.src = el.src;
                waitForScriptLoaded = true;
            }
            if (el.type) {
                script.type = el.type;
            }
            if (el.text) {
                script.text = el.text;
            }
            el = script;
        }
        else if (el.tagName.toLowerCase() == "link") {
            // create link
            var link = document.createElement("link");
            if (el.href) {
                link.href = el.href;
            }
            if (el.rel) {
                link.rel = el.rel;
            }
            if (el.type) {
                link.type = el.type;
            }
            el = link;
        }
        // load next script when this is finished
        if (waitForScriptLoaded) {
            el.onload = function () { return _this.loadResourceElements(elements, offset + 1, callback); };
        }
        document.head.appendChild(el);
        if (!waitForScriptLoaded) {
            this.loadResourceElements(elements, offset + 1, callback);
        }
    };
    DotVVM.prototype.getSpaPlaceHolder = function () {
        var elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
        if (elements.length == 1) {
            return elements[0];
        }
        return null;
    };
    DotVVM.prototype.navigateCore = function (viewModelName, url) {
        var _this = this;
        var viewModel = this.viewModels[viewModelName].viewModel;
        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();
        // trigger spaNavigating event
        var spaNavigatingArgs = new DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, url);
        this.events.spaNavigating.trigger(spaNavigatingArgs);
        if (spaNavigatingArgs.cancel) {
            return;
        }
        // add virtual directory prefix
        url = "/___dotvvm-spa___" + this.addLeadingSlash(url);
        var fullUrl = this.addLeadingSlash(this.concatUrl(this.viewModels[viewModelName].virtualDirectory || "", url));
        // find SPA placeholder
        var spaPlaceHolder = this.getSpaPlaceHolder();
        if (!spaPlaceHolder) {
            document.location.href = fullUrl;
            return;
        }
        // send the request
        var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-dotvvm-spacontentplaceholder"].value;
        this.getJSON(fullUrl, "GET", spaPlaceHolderUniqueId, function (result) {
            // if another postback has already been passed, don't do anything
            if (!_this.isPostBackStillActive(currentPostBackCounter))
                return;
            var resultObject = JSON.parse(result.responseText);
            _this.loadResourceList(resultObject.resources, function () {
                var isSuccess = false;
                if (resultObject.action === "successfulCommand" || !resultObject.action) {
                    try {
                        _this.isViewModelUpdating = true;
                        // remove updated controls
                        var updatedControls = _this.cleanUpdatedControls(resultObject);
                        // update the viewmodel
                        _this.viewModels[viewModelName] = {};
                        for (var p in resultObject) {
                            if (resultObject.hasOwnProperty(p)) {
                                _this.viewModels[viewModelName][p] = resultObject[p];
                            }
                        }
                        _this.rootRenderer.setState(resultObject.viewModel);
                        isSuccess = true;
                        // add updated controls
                        _this.viewModelObservables[viewModelName](_this.viewModels[viewModelName].viewModel);
                        _this.restoreUpdatedControls(resultObject, updatedControls, true);
                        _this.isSpaReady(true);
                    }
                    finally {
                        _this.isViewModelUpdating = false;
                    }
                }
                else if (resultObject.action === "redirect") {
                    _this.handleRedirect(resultObject, viewModelName, true);
                    return;
                }
                // trigger spaNavigated event
                var spaNavigatedArgs = new DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, resultObject);
                _this.events.spaNavigated.trigger(spaNavigatedArgs);
                if (!isSuccess && !spaNavigatedArgs.isHandled) {
                    throw "Invalid response from server!";
                }
            });
        }, function (xhr) {
            // if another postback has already been passed, don't do anything
            if (!_this.isPostBackStillActive(currentPostBackCounter))
                return;
            // execute error handlers
            var errArgs = new DotvvmErrorEventArgs(undefined, viewModel, viewModelName, xhr, -1, undefined, true);
            _this.events.error.trigger(errArgs);
            if (!errArgs.handled) {
                alert(xhr.responseText);
            }
        });
    };
    DotVVM.prototype.handleRedirect = function (resultObject, viewModelName, replace) {
        if (replace === void 0) { replace = false; }
        if (resultObject.replace != null)
            replace = resultObject.replace;
        var url;
        // redirect
        if (this.getSpaPlaceHolder() && resultObject.url.indexOf("//") < 0 && resultObject.allowSpa) {
            // relative URL - keep in SPA mode, but remove the virtual directory
            url = "#!" + this.removeVirtualDirectoryFromUrl(resultObject.url, viewModelName);
            if (url === "#!") {
                url = "#!/";
            }
            // verify that the URL prefix is correct, if not - add it before the fragment
            url = this.fixSpaUrlPrefix(url);
        }
        else {
            // absolute URL - load the URL
            url = resultObject.url;
        }
        // trigger redirect event
        var redirectArgs = new DotvvmRedirectEventArgs(dotvvm.viewModels[viewModelName], viewModelName, url, replace);
        this.events.redirect.trigger(redirectArgs);
        this.performRedirect(url, replace);
    };
    DotVVM.prototype.performRedirect = function (url, replace) {
        if (replace) {
            location.replace(url);
        }
        else {
            var fakeAnchor = this.fakeRedirectAnchor;
            if (!fakeAnchor) {
                fakeAnchor = document.createElement("a");
                fakeAnchor.style.display = "none";
                fakeAnchor.setAttribute("data-dotvvm-fake-id", "dotvvm_fake_redirect_anchor_87D7145D_8EA8_47BA_9941_82B75EE88CDB");
                document.body.appendChild(fakeAnchor);
                this.fakeRedirectAnchor = fakeAnchor;
            }
            fakeAnchor.href = url;
            fakeAnchor.click();
        }
    };
    DotVVM.prototype.fixSpaUrlPrefix = function (url) {
        var attr = this.getSpaPlaceHolder().attributes["data-dotvvm-spacontentplaceholder-urlprefix"];
        if (!attr) {
            return url;
        }
        var correctPrefix = attr.value;
        var currentPrefix = document.location.pathname;
        if (correctPrefix !== currentPrefix) {
            if (correctPrefix === "") {
                correctPrefix = "/";
            }
            url = correctPrefix + url;
        }
        return url;
    };
    DotVVM.prototype.removeVirtualDirectoryFromUrl = function (url, viewModelName) {
        var virtualDirectory = "/" + this.viewModels[viewModelName].virtualDirectory;
        if (url.indexOf(virtualDirectory) == 0) {
            return this.addLeadingSlash(url.substring(virtualDirectory.length));
        }
        else {
            return url;
        }
    };
    DotVVM.prototype.addLeadingSlash = function (url) {
        if (url.length > 0 && url.substring(0, 1) != "/") {
            return "/" + url;
        }
        return url;
    };
    DotVVM.prototype.concatUrl = function (url1, url2) {
        if (url1.length > 0 && url1.substring(url1.length - 1) == "/") {
            url1 = url1.substring(0, url1.length - 1);
        }
        return url1 + this.addLeadingSlash(url2);
    };
    DotVVM.prototype.patch = function (source, patch) {
        var _this = this;
        if (source instanceof Array && patch instanceof Array) {
            return patch.map(function (val, i) {
                return _this.patch(source[i], val);
            });
        }
        else if (source instanceof Array || patch instanceof Array)
            return patch;
        else if (typeof source == "object" && typeof patch == "object") {
            for (var p in patch) {
                if (patch[p] == null)
                    source[p] = null;
                else if (source[p] == null)
                    source[p] = patch[p];
                else
                    source[p] = this.patch(source[p], patch[p]);
            }
        }
        else
            return patch;
        return source;
    };
    DotVVM.prototype.updateDynamicPathFragments = function (context, path) {
        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i].indexOf("[$index]") >= 0) {
                path[i] = path[i].replace("[$index]", "[" + context.$index() + "]");
            }
            context = context.$parentContext;
        }
    };
    DotVVM.prototype.postJSON = function (url, method, postData, success, error, preprocessRequest) {
        if (preprocessRequest === void 0) { preprocessRequest = function (xhr) { }; }
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-PostBack", "true");
        xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        preprocessRequest(xhr);
        xhr.onreadystatechange = function () {
            if (xhr.readyState !== XMLHttpRequest.DONE)
                return;
            if (xhr.status < 400) {
                success(xhr);
            }
            else {
                error(xhr);
            }
        };
        xhr.send(postData);
    };
    DotVVM.prototype.getJSON = function (url, method, spaPlaceHolderUniqueId, success, error) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-SpaContentPlaceHolder", spaPlaceHolderUniqueId);
        xhr.onreadystatechange = function () {
            if (xhr.readyState !== XMLHttpRequest.DONE)
                return;
            if (xhr.status < 400) {
                success(xhr);
            }
            else {
                error(xhr);
            }
        };
        xhr.send();
    };
    DotVVM.prototype.getXHR = function () {
        return XMLHttpRequest ? new XMLHttpRequest() : new (window["ActiveXObject"])("Microsoft.XMLHTTP");
    };
    DotVVM.prototype.cleanUpdatedControls = function (resultObject, updatedControls) {
        if (updatedControls === void 0) { updatedControls = {}; }
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var control = document.getElementByDotvvmId(id);
                if (control) {
                    var dataContext = ko.contextFor(control);
                    var nextSibling = control.nextSibling;
                    var parent = control.parentNode;
                    ko.removeNode(control);
                    updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent, dataContext: dataContext };
                }
            }
        }
        return updatedControls;
    };
    DotVVM.prototype.restoreUpdatedControls = function (resultObject, updatedControls, applyBindingsOnEachControl) {
        var _this = this;
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var updatedControl = updatedControls[id];
                if (updatedControl) {
                    var wrapper = document.createElement(updatedControls[id].parent.tagName || "div");
                    wrapper.innerHTML = resultObject.updatedControls[id];
                    if (wrapper.childElementCount > 1)
                        throw new Error("Postback.Update control can not render more than one element");
                    var element = wrapper.firstElementChild;
                    if (element.id == null)
                        throw new Error("Postback.Update control always has to render id attribute.");
                    if (element.id !== updatedControls[id].control.id)
                        console.log("Postback.Update control changed id from '" + updatedControls[id].control.id + "' to '" + element.id + "'");
                    wrapper.removeChild(element);
                    if (updatedControl.nextSibling) {
                        updatedControl.parent.insertBefore(element, updatedControl.nextSibling);
                    }
                    else {
                        updatedControl.parent.appendChild(element);
                    }
                }
            }
        }
        if (applyBindingsOnEachControl) {
            window.setTimeout(function () {
                try {
                    _this.isViewModelUpdating = true;
                    for (var id in resultObject.updatedControls) {
                        var updatedControl = document.getElementByDotvvmId(id);
                        if (updatedControl) {
                            ko.applyBindings(updatedControls[id].dataContext, updatedControl);
                        }
                    }
                }
                finally {
                    _this.isViewModelUpdating = false;
                }
            }, 0);
        }
    };
    DotVVM.prototype.unwrapArrayExtension = function (array) {
        return ko.unwrap(ko.unwrap(array));
    };
    DotVVM.prototype.buildRouteUrl = function (routePath, params) {
        return routePath.replace(/\{([^\}]+?)\??(:(.+?))?\}/g, function (s, paramName, hsjdhsj, type) {
            if (!paramName)
                return "";
            return ko.unwrap(params[paramName.toLowerCase()]) || "";
        });
    };
    DotVVM.prototype.buildUrlSuffix = function (urlSuffix, query) {
        var resultSuffix, hashSuffix;
        if (urlSuffix.indexOf("#") !== -1) {
            resultSuffix = urlSuffix.substring(0, urlSuffix.indexOf("#"));
            hashSuffix = urlSuffix.substring(urlSuffix.indexOf("#"));
        }
        else {
            resultSuffix = urlSuffix;
            hashSuffix = "";
        }
        for (var property in query) {
            if (query.hasOwnProperty(property)) {
                if (!property)
                    continue;
                var queryParamValue = ko.unwrap(query[property]);
                if (queryParamValue != null)
                    continue;
                resultSuffix = resultSuffix.concat(resultSuffix.indexOf("?") !== -1
                    ? "&" + property + "=" + queryParamValue
                    : "?" + property + "=" + queryParamValue);
            }
        }
        return resultSuffix.concat(hashSuffix);
    };
    DotVVM.prototype.isPostBackProhibited = function (element) {
        if (element && element.tagName && element.tagName.toLowerCase() === "a" && element.getAttribute("disabled")) {
            return true;
        }
        return false;
    };
    DotVVM.prototype.addKnockoutBindingHandlers = function () {
        function createWrapperComputed(accessor, propertyDebugInfo) {
            if (propertyDebugInfo === void 0) { propertyDebugInfo = null; }
            var computed = ko.pureComputed({
                read: function () {
                    var property = accessor();
                    var propertyValue = ko.unwrap(property); // has to call that always as it is a dependency
                    return propertyValue;
                },
                write: function (value) {
                    var val = accessor();
                    if (ko.isObservable(val)) {
                        val(value);
                    }
                    else {
                        console.warn("Attempted to write to readonly property" + (propertyDebugInfo == null ? "" : " " + propertyDebugInfo) + ".");
                    }
                }
            });
            computed["wrappedProperty"] = accessor;
            return computed;
        }
        ko.virtualElements.allowedBindings["dotvvm_withControlProperties"] = true;
        ko.bindingHandlers["dotvvm_withControlProperties"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                if (!bindingContext)
                    throw new Error();
                var value = valueAccessor();
                var _loop_2 = function (prop) {
                    value[prop] = createWrapperComputed(function () { return valueAccessor()[prop]; }, "'" + prop + "' at '" + valueAccessor.toString() + "'");
                };
                for (var prop in value) {
                    _loop_2(prop);
                }
                var innerBindingContext = bindingContext.extend({ $control: value });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            }
        };
        ko.virtualElements.allowedBindings["dotvvm_introduceAlias"] = true;
        ko.bindingHandlers["dotvvm_introduceAlias"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                if (!bindingContext)
                    throw new Error();
                var value = valueAccessor();
                var extendBy = {};
                var _loop_3 = function (prop) {
                    propPath = prop.split('.');
                    obj = extendBy;
                    for (var i = 0; i < propPath.length - 1; i) {
                        obj = extendBy[propPath[i]] || (extendBy[propPath[i]] = {});
                    }
                    obj[propPath[propPath.length - 1]] = createWrapperComputed(function () { return valueAccessor()[prop]; }, "'" + prop + "' at '" + valueAccessor.toString() + "'");
                };
                var propPath, obj;
                for (var prop in value) {
                    _loop_3(prop);
                }
                var innerBindingContext = bindingContext.extend(extendBy);
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            }
        };
        ko.virtualElements.allowedBindings["withGridViewDataSet"] = true;
        ko.bindingHandlers["withGridViewDataSet"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                if (!bindingContext)
                    throw new Error();
                var value = valueAccessor();
                var innerBindingContext = bindingContext.extend({ $gridViewDataSet: value });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            }
        };
        ko.bindingHandlers['dotvvmEnable'] = {
            'update': function (element, valueAccessor) {
                var value = ko.utils.unwrapObservable(valueAccessor());
                if (value && element.disabled) {
                    element.disabled = false;
                    element.removeAttribute("disabled");
                }
                else if ((!value) && (!element.disabled)) {
                    element.disabled = true;
                    element.setAttribute("disabled", "disabled");
                }
            }
        };
        ko.bindingHandlers['dotvvm-checkbox-updateAfterPostback'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                dotvvm.events.afterPostback.subscribe(function (e) {
                    var bindings = allBindingsAccessor();
                    if (bindings["dotvvm-checked-pointer"]) {
                        var checked = bindings[bindings["dotvvm-checked-pointer"]];
                        if (ko.isObservable(checked)) {
                            if (checked.valueHasMutated) {
                                checked.valueHasMutated();
                            }
                            else {
                                checked.notifySubscribers();
                            }
                        }
                    }
                });
            }
        };
        ko.bindingHandlers['dotvvm-checked-pointer'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            }
        };
        ko.bindingHandlers["dotvvm-UpdateProgress-Visible"] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                element.style.display = "none";
                var delay = element.getAttribute("data-delay");
                var timeout;
                var running = false;
                var show = function () {
                    running = true;
                    if (delay == null) {
                        element.style.display = "";
                    }
                    else {
                        timeout = setTimeout(function (e) {
                            element.style.display = "";
                        }, delay);
                    }
                };
                var interrupt = function () {
                    clearTimeout(timeout);
                    element.style.display = "none";
                };
                var hide = function () {
                    running = false;
                    clearTimeout(timeout);
                    element.style.display = "none";
                };
                dotvvm.events.beforePostback.subscribe(function (e) {
                    if (running) {
                        interrupt();
                    }
                    show();
                });
                dotvvm.events.spaNavigating.subscribe(function (e) {
                    if (running) {
                        interrupt();
                    }
                    show();
                });
                dotvvm.events.afterPostback.subscribe(function (e) {
                    if (!e.wasInterrupted) {
                        hide();
                    }
                });
                dotvvm.events.redirect.subscribe(function (e) {
                    hide();
                });
                dotvvm.events.spaNavigated.subscribe(function (e) {
                    hide();
                });
                dotvvm.events.error.subscribe(function (e) {
                    hide();
                });
            }
        };
        ko.bindingHandlers['dotvvm-table-columnvisible'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var lastDisplay = "";
                var currentVisible = true;
                function changeVisibility(table, columnIndex, visible) {
                    if (currentVisible == visible)
                        return;
                    currentVisible = visible;
                    for (var i = 0; i < table.rows.length; i++) {
                        var row = table.rows.item(i);
                        var style = row.cells[columnIndex].style;
                        if (visible) {
                            style.display = lastDisplay;
                        }
                        else {
                            lastDisplay = style.display || "";
                            style.display = "none";
                        }
                    }
                }
                if (!(element instanceof HTMLTableCellElement))
                    return;
                // find parent table
                var table = element;
                while (!(table instanceof HTMLTableElement))
                    table = table.parentElement;
                var colIndex = [].slice.call(table.rows.item(0).cells).indexOf(element);
                element['dotvvmChangeVisibility'] = changeVisibility.bind(null, table, colIndex);
            },
            update: function (element, valueAccessor) {
                element.dotvvmChangeVisibility(ko.unwrap(valueAccessor()));
            }
        };
        ko.bindingHandlers['dotvvm-textbox-text'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var obs = valueAccessor();
                //generate metadata func 
                var elmMetadata = new DotvvmValidationElementMetadata();
                elmMetadata.dataType = (element.attributes["data-dotvvm-value-type"] || { value: "" }).value;
                elmMetadata.format = (element.attributes["data-dotvvm-format"] || { value: "" }).value;
                //add metadata for validation
                if (!obs.dotvvmMetadata) {
                    obs.dotvvmMetadata = new DotvvmValidationObservableMetadata();
                    obs.dotvvmMetadata.elementsMetadata = [elmMetadata];
                }
                else {
                    if (!obs.dotvvmMetadata.elementsMetadata) {
                        obs.dotvvmMetadata.elementsMetadata = [];
                    }
                    obs.dotvvmMetadata.elementsMetadata.push(elmMetadata);
                }
                setTimeout(function (metaArray, element) {
                    // remove element from collection when its removed from dom
                    ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                        for (var _i = 0, metaArray_1 = metaArray; _i < metaArray_1.length; _i++) {
                            var meta = metaArray_1[_i];
                            if (meta.element === element) {
                                metaArray.splice(metaArray.indexOf(meta), 1);
                                break;
                            }
                        }
                    });
                }, 0, obs.dotvvmMetadata.elementsMetadata, element);
                dotvvm.domUtils.attachEvent(element, "blur", function () {
                    if (!ko.isObservable(obs))
                        return;
                    // parse the value
                    var result, isEmpty, newValue;
                    if (elmMetadata.dataType === "datetime") {
                        // parse date
                        var currentValue = obs();
                        if (currentValue != null) {
                            currentValue = dotvvm.globalize.parseDotvvmDate(currentValue);
                        }
                        result = dotvvm.globalize.parseDate(element.value, elmMetadata.format, currentValue);
                        isEmpty = result === null;
                        newValue = isEmpty ? null : dotvvm.serialization.serializeDate(result, false);
                    }
                    else {
                        // parse number
                        result = dotvvm.globalize.parseNumber(element.value);
                        isEmpty = result === null || isNaN(result);
                        newValue = isEmpty ? null : result;
                    }
                    // update element validation metadata
                    if (newValue == null && element.value !== null && element.value !== "") {
                        element.attributes["data-dotvvm-value-type-valid"] = false;
                        elmMetadata.elementValidationState = false;
                    }
                    else {
                        element.attributes["data-dotvvm-value-type-valid"] = true;
                        elmMetadata.elementValidationState = true;
                    }
                    if (obs() === newValue) {
                        if (obs.valueHasMutated) {
                            obs.valueHasMutated();
                        }
                        else {
                            obs.notifySubscribers();
                        }
                    }
                    else {
                        obs(newValue);
                    }
                });
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var value = ko.unwrap(valueAccessor());
                if (element.attributes["data-dotvvm-value-type-valid"] != false) {
                    var format = (element.attributes["data-dotvvm-format"] || { value: "" }).value;
                    if (format) {
                        element.value = dotvvm.globalize.formatString(format, value);
                    }
                    else {
                        element.value = value;
                    }
                }
            }
        };
        ko.bindingHandlers["dotvvm-textbox-select-all-on-focus"] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                element.$selectAllOnFocusHandler = function () {
                    element.select();
                };
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var value = valueAccessor();
                if (typeof (value) === "function") {
                    value = value();
                }
                if (value === true) {
                    element.addEventListener("focus", element.$selectAllOnFocusHandler);
                }
                else {
                    element.removeEventListener("focus", element.$selectAllOnFocusHandler);
                }
            }
        };
        ko.bindingHandlers["dotvvm-CheckState"] = {
            init: function (element, valueAccessor, allBindings) {
                ko.getBindingHandler("checked").init(element, valueAccessor, allBindings);
            },
            update: function (element, valueAccessor, allBindings) {
                var value = ko.unwrap(valueAccessor());
                element.indeterminate = value == null;
            }
        };
    };
    return DotVVM;
}());
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />
var DotvvmValidationContext = (function () {
    function DotvvmValidationContext(valueToValidate, parentViewModel, parameters) {
        this.valueToValidate = valueToValidate;
        this.parentViewModel = parentViewModel;
        this.parameters = parameters;
    }
    return DotvvmValidationContext;
}());
var DotvvmValidationObservableMetadata = (function () {
    function DotvvmValidationObservableMetadata() {
    }
    return DotvvmValidationObservableMetadata;
}());
var DotvvmValidationElementMetadata = (function () {
    function DotvvmValidationElementMetadata() {
        this.elementValidationState = true;
    }
    return DotvvmValidationElementMetadata;
}());
var DotvvmValidatorBase = (function () {
    function DotvvmValidatorBase() {
    }
    DotvvmValidatorBase.prototype.isValid = function (context, property) {
        return false;
    };
    DotvvmValidatorBase.prototype.isEmpty = function (value) {
        return value == null || (typeof value == "string" && value.trim() === "");
    };
    DotvvmValidatorBase.prototype.getValidationMetadata = function (property) {
        return property.dotvvmMetadata;
    };
    return DotvvmValidatorBase;
}());
var DotvvmRequiredValidator = (function (_super) {
    __extends(DotvvmRequiredValidator, _super);
    function DotvvmRequiredValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmRequiredValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    };
    return DotvvmRequiredValidator;
}(DotvvmValidatorBase));
var DotvvmRegularExpressionValidator = (function (_super) {
    __extends(DotvvmRegularExpressionValidator, _super);
    function DotvvmRegularExpressionValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmRegularExpressionValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    };
    return DotvvmRegularExpressionValidator;
}(DotvvmValidatorBase));
var DotvvmIntRangeValidator = (function (_super) {
    __extends(DotvvmIntRangeValidator, _super);
    function DotvvmIntRangeValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmIntRangeValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val % 1 === 0 && val >= from && val <= to;
    };
    return DotvvmIntRangeValidator;
}(DotvvmValidatorBase));
var DotvvmEnforceClientFormatValidator = (function (_super) {
    __extends(DotvvmEnforceClientFormatValidator, _super);
    function DotvvmEnforceClientFormatValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmEnforceClientFormatValidator.prototype.isValid = function (context, property) {
        // parameters order: AllowNull, AllowEmptyString, AllowEmptyStringOrWhitespaces
        var valid = true;
        if (!context.parameters[0] && context.valueToValidate == null) {
            valid = false;
        }
        if (!context.parameters[1] && context.valueToValidate.length === 0) {
            valid = false;
        }
        if (!context.parameters[2] && this.isEmpty(context.valueToValidate)) {
            valid = false;
        }
        var metadata = this.getValidationMetadata(property);
        if (metadata && metadata.elementsMetadata) {
            for (var _i = 0, _a = metadata.elementsMetadata; _i < _a.length; _i++) {
                var metaElement = _a[_i];
                if (!metaElement.elementValidationState) {
                    valid = false;
                }
            }
        }
        return valid;
    };
    return DotvvmEnforceClientFormatValidator;
}(DotvvmValidatorBase));
var DotvvmRangeValidator = (function (_super) {
    __extends(DotvvmRangeValidator, _super);
    function DotvvmRangeValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmRangeValidator.prototype.isValid = function (context, property) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val >= from && val <= to;
    };
    return DotvvmRangeValidator;
}(DotvvmValidatorBase));
var DotvvmNotNullValidator = (function (_super) {
    __extends(DotvvmNotNullValidator, _super);
    function DotvvmNotNullValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmNotNullValidator.prototype.isValid = function (context) {
        return context.valueToValidate !== null && context.valueToValidate !== undefined;
    };
    return DotvvmNotNullValidator;
}(DotvvmValidatorBase));
var ValidationError = (function () {
    function ValidationError(validator, errorMessage) {
        this.validator = validator;
        this.errorMessage = errorMessage;
    }
    ValidationError.getOrCreate = function (validatedObservable) {
        if (validatedObservable.wrappedProperty) {
            var wrapped = validatedObservable.wrappedProperty();
            if (ko.isObservable(wrapped))
                validatedObservable = wrapped;
        }
        if (!validatedObservable.validationErrors) {
            validatedObservable.validationErrors = ko.observableArray();
        }
        return validatedObservable.validationErrors;
    };
    return ValidationError;
}());
var DotvvmValidation = (function () {
    function DotvvmValidation(dotvvm) {
        var _this = this;
        this.rules = {
            "required": new DotvvmRequiredValidator(),
            "regularExpression": new DotvvmRegularExpressionValidator(),
            "intrange": new DotvvmIntRangeValidator(),
            "range": new DotvvmRangeValidator(),
            "notnull": new DotvvmNotNullValidator(),
            "enforceClientFormat": new DotvvmEnforceClientFormatValidator()
        };
        this.errors = ko.observableArray([]);
        this.events = {
            validationErrorsChanged: new DotvvmEvent("dotvvm.validation.events.validationErrorsChanged")
        };
        this.elementUpdateFunctions = {
            // shows the element when it is valid
            hideWhenValid: function (element, errorMessages, param) {
                if (errorMessages.length > 0) {
                    element.style.display = "";
                }
                else {
                    element.style.display = "none";
                }
            },
            // adds a CSS class when the element is not valid
            invalidCssClass: function (element, errorMessages, className) {
                if (errorMessages.length > 0) {
                    element.className += " " + className;
                }
                else {
                    element.className = element.className.split(' ').filter(function (c) { return c != className; }).join(' ');
                }
            },
            // sets the error message as the title attribute
            setToolTipText: function (element, errorMessages, param) {
                if (errorMessages.length > 0) {
                    element.title = errorMessages.join(", ");
                }
                else {
                    element.title = "";
                }
            },
            // displays the error message
            showErrorMessageText: function (element, errorMessages, param) {
                element[element.innerText ? "innerText" : "textContent"] = errorMessages.join(", ");
            }
        };
        this.validObjectResult = {};
        // perform the validation before postback
        dotvvm.events.beforePostback.subscribe(function (args) {
            if (args.validationTargetPath) {
                // clear previous errors
                dotvvm.rootRenderer.update(_this.clearValidationErrors.bind(_this));
                // resolve target
                var context = ko.contextFor(args.sender);
                var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
                // TODO: replace this hack with a knockout-less variant
                // It will just reuire a change to dotvvm server to send obsevable-less validation targets
                if (!validationTarget["__unwrapped_data"] && validationTarget.viewModel)
                    validationTarget = ko.unwrap(validationTarget.viewModel);
                var unwrappedTarget = ko.unwrap(validationTarget["__unwrapped_data"]);
                var targetUpdate = ko.unwrap(validationTarget["__update_function"]);
                if (!unwrappedTarget)
                    throw new Error();
                // validate the object
                var validation_1 = _this.validateViewModel(unwrappedTarget);
                if (validation_1 != _this.validObjectResult) {
                    console.log("Validation failed: postback aborted; errors: ", validation_1);
                    args.cancel = true;
                    args.clientValidationFailed = true;
                    targetUpdate(function (vm) { return _this.applyValidationErrors(vm, validation_1); });
                }
            }
            _this.events.validationErrorsChanged.trigger(args);
        });
        dotvvm.events.afterPostback.subscribe(function (args) {
            if (!args.wasInterrupted && args.serverResponseObject) {
                if (args.serverResponseObject.action === "successfulCommand") {
                    // merge validation rules from postback with those we already have (required when a new type appears in the view model)
                    _this.mergeValidationRules(args);
                    args.isHandled = true;
                }
                else if (args.serverResponseObject.action === "validationErrors") {
                    // apply validation errors from server
                    _this.showValidationErrorsFromServer(args);
                    args.isHandled = true;
                }
            }
            _this.events.validationErrorsChanged.trigger(args);
        });
        // add knockout binding handler
        ko.bindingHandlers["dotvvmValidation"] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var observableProperty = valueAccessor();
                if (ko.isObservable(observableProperty)) {
                    // try to get the options
                    var options = allBindingsAccessor.get("dotvvmValidationOptions");
                    var updateFunction = function (element, errorMessages) {
                        for (var option in options) {
                            if (options.hasOwnProperty(option)) {
                                _this.elementUpdateFunctions[option](element, errorMessages.map(function (v) { return v.errorMessage; }), options[option]);
                            }
                        }
                    };
                    // subscribe to the observable property changes
                    var validationErrors = ValidationError.getOrCreate(observableProperty);
                    validationErrors.subscribe(function (newValue) { return updateFunction(element, newValue); });
                    updateFunction(element, validationErrors());
                }
            }
        };
    }
    /**
     * Validates the specified view model
    */
    DotvvmValidation.prototype.validateViewModel = function (viewModel) {
        if (!viewModel || !dotvvm.viewModels['root'].validationRules)
            return this.validObjectResult;
        // find validation rules
        var type = viewModel.$type;
        if (!type)
            return this.validObjectResult;
        var rulesForType = dotvvm.viewModels['root'].validationRules[type] || {};
        var validationResult = null;
        var _loop_4 = function (property) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0)
                return "continue";
            var value = viewModel[property];
            // run validation rules
            var errors = rulesForType.hasOwnProperty(property) ?
                this_2.validateProperty(viewModel, value, rulesForType[property]) :
                null;
            var options = viewModel[property + "$options"];
            if (options && options.type && errors == null && !dotvvm.serialization.validateType(value, options.type)) {
                error = new ValidationError(function (val) { return dotvvm.serialization.validateType(val, options.type); }, "The value of property " + property + " (" + value + ") is invalid value for type " + options.type + ".");
                errors = [error];
            }
            if (typeof value == "object" && value != null) {
                if (Array.isArray(value)) {
                    // handle collections
                    var a = this_2.validateArray(value);
                    if (a != this_2.validObjectResult) {
                        if (!validationResult)
                            validationResult = {};
                        validationResult[property] = a;
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    var a = this_2.validateViewModel(value);
                    if (a != this_2.validObjectResult) {
                        if (!validationResult)
                            validationResult = {};
                        validationResult[property] = a;
                    }
                }
            }
            if (errors) {
                if (validationResult && validationResult[property]) {
                    validationResult[property][""] = errors;
                }
                else {
                    if (!validationResult)
                        validationResult = {};
                    validationResult[property] = errors;
                }
            }
        };
        var this_2 = this, error;
        // validate all properties
        for (var property in viewModel) {
            _loop_4(property);
        }
        return validationResult || this.validObjectResult;
    };
    DotvvmValidation.prototype.validateArray = function (array) {
        var validationResult = null;
        for (var index = 0; index < array.length; index++) {
            var a = this.validateViewModel(array[index]);
            if (a != this.validObjectResult) {
                if (!validationResult)
                    validationResult = {};
                validationResult[index] = a;
            }
        }
        return validationResult || this.validObjectResult;
    };
    // validates the specified property in the viewModel
    DotvvmValidation.prototype.validateProperty = function (viewModel, value, rulesForProperty) {
        var errors = null;
        for (var _i = 0, rulesForProperty_1 = rulesForProperty; _i < rulesForProperty_1.length; _i++) {
            var rule = rulesForProperty_1[_i];
            // validate the rules
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);
            if (!ruleTemplate.isValid(context, value)) {
                // add error message
                if (!errors)
                    errors = [];
                errors.push(new ValidationError(value, rule.errorMessage));
            }
        }
        return errors;
    };
    // merge validation rules
    DotvvmValidation.prototype.mergeValidationRules = function (args) {
        if (args.serverResponseObject.validationRules) {
            // TODO
            throw new Error("Not implemented");
            // var existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            // if (typeof existingRules === "undefined") {
            //     dotvvm.viewModels[args.viewModelName].validationRules = {};
            //     existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            // }
            // for (var type in args.serverResponseObject) {
            //     if (!args.serverResponseObject.hasOwnProperty(type)) continue;
            //     existingRules![type] = args.serverResponseObject[type];
            // }
        }
    };
    DotvvmValidation.prototype.applyValidationErrors = function (object, errors) {
        var _this = this;
        if (typeof object != "object" || object == null || errors == this.validObjectResult)
            return object;
        // Do the same for every object in the array
        if (Array.isArray(object)) {
            return RendererInitializer.immutableMap(object, function (a, i) {
                if (i in errors) {
                    var e = errors[i];
                    if (Array.isArray(e))
                        throw new Error("Arrays can't contain values with validation errors");
                    else
                        return _this.applyValidationErrors(a, e);
                }
                else {
                    return a;
                }
            });
        }
        else {
            var result = __assign({}, object);
            // Do the same for every subordinate property
            for (var prop in errors) {
                if (!Object.prototype.hasOwnProperty.call(errors, prop))
                    continue;
                var validationProp = prop + "$validation";
                var err = errors[prop];
                if (Array.isArray(err)) {
                    if (validationProp in object) {
                        // clone ...$validation field
                        result[validationProp] = __assign({}, object[validationProp], { errors: Array.prototype.concat(object[validationProp].errors || [], err) });
                    }
                    else {
                        result[validationProp] = { errors: err };
                    }
                }
                else {
                    result[validationProp] = this.applyValidationErrors(object[validationProp], err);
                }
            }
            return __assign({}, object, result);
        }
    };
    /**
      * Clears validation errors from the passed viewModel including its children
    */
    DotvvmValidation.prototype.clearValidationErrors = function (validatedObject) {
        if (typeof validatedObject != "object" || validatedObject == null)
            return validatedObject;
        // Do the same for every object in the array
        if (Array.isArray(validatedObject)) {
            return RendererInitializer.immutableMap(validatedObject, this.clearValidationErrors.bind(this));
        }
        var result = null;
        // Do the same for every subordinate property
        for (var propertyName in validatedObject) {
            if (!validatedObject.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0)
                continue;
            if (propertyName.lastIndexOf("$validation") == propertyName.length - "$validation".length) {
                // remove ..$validation fields
                if (result == null)
                    result = __assign({}, validatedObject);
                delete result[propertyName];
            }
            else if (propertyName.indexOf('$') < 0) {
                // update children
                var r = this.clearValidationErrors(validatedObject[propertyName]);
                if (r !== validatedObject[propertyName]) {
                    if (result == null)
                        result = __assign({}, validatedObject);
                    result[propertyName] = r;
                }
            }
        }
        return result || validatedObject;
    };
    /**
     * Gets validation errors from the passed object and its children.
     * @param target Object that is supposed to contain the errors or properties with the errors
     * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
     * @param includeErrorsFromChildren Sets whether to include errors from children at all
     * @returns By default returns only errors from the viewModel's immediate children
     */
    DotvvmValidation.prototype.getValidationErrors = function (validationTargetObservable, includeErrorsFromGrandChildren, includeErrorsFromTarget, includeErrorsFromChildren) {
        if (includeErrorsFromChildren === void 0) { includeErrorsFromChildren = true; }
        // Check the passed viewModel
        if (!validationTargetObservable)
            return [];
        var errors = [];
        // Include errors from the validation target
        if (includeErrorsFromTarget) {
            // TODO: not supported
        }
        if (includeErrorsFromChildren) {
            var validationTarget = ko.unwrap(validationTargetObservable);
            if (Array.isArray(validationTarget)) {
                for (var _i = 0, validationTarget_1 = validationTarget; _i < validationTarget_1.length; _i++) {
                    var item = validationTarget_1[_i];
                    // This is correct because in the next children and further all children are grandchildren
                    errors = errors.concat(this.getValidationErrors(item, includeErrorsFromGrandChildren, false, includeErrorsFromGrandChildren));
                }
            }
            else {
                for (var propertyName in validationTarget) {
                    if (!validationTarget.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0)
                        continue;
                    var property = validationTarget[propertyName];
                    var val = validationTarget[propertyName + "$validation"];
                    if (val && val.errors) {
                        errors = errors.concat(val.errors);
                    }
                    if (includeErrorsFromGrandChildren) {
                        errors = errors.concat(this.getValidationErrors(property, true, false, true));
                    }
                }
            }
        }
        return errors;
    };
    /**
     * Adds validation errors from the server to the appropriate arrays
     */
    DotvvmValidation.prototype.showValidationErrorsFromServer = function (args) {
        dotvvm.rootRenderer.update(this.clearValidationErrors.bind(this));
        // resolve target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
        if (!validationTarget)
            return;
        // TODO: replace this hack with a knockout-less variant
        // It will just reuire a change to dotvvm server to send obsevable-less validation targets
        if (!validationTarget["__unwrapped_data"] && validationTarget.viewModel)
            validationTarget = ko.unwrap(validationTarget.viewModel);
        var unwrappedTarget = ko.unwrap(validationTarget["__unwrapped_data"]);
        var targetUpdate = ko.unwrap(validationTarget["__update_function"]);
        if (!unwrappedTarget)
            throw new Error();
        // add validation errors
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the property
            var propertyPath = modelState[i].propertyPath;
            this.addErrorToProperty(validationTarget, propertyPath, modelState[i].errorMessage);
            // var property;
            // if (propertyPath) {
            //     if (ko.isObservable(validationTarget)) {
            //         validationTarget = ko.unwrap(validationTarget);
            //     }
            //     property = dotvvm.evaluator.evaluateOnViewModel(validationTarget, propertyPath);
            // }
            // else {
            //     property = validationTarget
            // }
            // // add the error to appropriate collections
            // var error = new ValidationError(property, modelState[i].errorMessage);
            // this.addValidationError(property, error);
        }
    };
    DotvvmValidation.prototype.addErrorToProperty = function (target, propertyPath, error) {
        if (!propertyPath)
            throw new Error("Adding validation errors to validation target is not supported.");
        var _a = (function () {
            var match = /(\w|\d|_|\$)*$/.exec(propertyPath);
            return [match[0], propertyPath.substr(0, match.index)];
        })(), prop = _a[0], objectPath = _a[1];
        if (objectPath.lastIndexOf('.') == objectPath.length - 1)
            objectPath.substr(0, objectPath.length - 1);
        if (!prop)
            throw new Error();
        var object = dotvvm.evaluator.evaluateOnViewModel(target, objectPath);
        var targetUpdate = ko.unwrap(object["__update_function"]);
        targetUpdate(function (vm) {
            var validationProp = prop + "$validation";
            var newErrors = [new ValidationError(null, error)];
            if (validationProp in vm) {
                return __assign({}, vm, (_a = {}, _a[validationProp] = __assign({}, vm[validationProp], { errors: Array.prototype.concat(vm[validationProp].errors || [], newErrors) }), _a));
            }
            else {
                return __assign({}, vm, (_b = {}, _b[validationProp] = {
                    errors: newErrors
                }, _b));
            }
            var _a, _b;
        });
    };
    return DotvvmValidation;
}());
;
var DotvvmEvaluator = (function () {
    function DotvvmEvaluator() {
    }
    DotvvmEvaluator.prototype.evaluateOnViewModel = function (context, expression) {
        var result;
        if (context && context.$data) {
            result = eval("(function ($context) { with($context) { with ($data) { return " + expression + "; } } })")(context);
        }
        else {
            result = eval("(function ($context) { var $data=$context; with($context) { return " + expression + "; } })")(context);
        }
        if (result && result.$data) {
            result = result.$data;
        }
        return result;
    };
    DotvvmEvaluator.prototype.evaluateOnContext = function (context, expression) {
        var startsWithProperty = false;
        for (var prop in context) {
            if (expression.indexOf(prop) === 0) {
                startsWithProperty = true;
                break;
            }
        }
        if (!startsWithProperty)
            expression = "$data." + expression;
        return this.evaluateOnViewModel(context, expression);
    };
    DotvvmEvaluator.prototype.getDataSourceItems = function (viewModel) {
        var value = ko.unwrap(viewModel);
        if (typeof value === "undefined" || value == null)
            return [];
        return ko.unwrap(value.Items || value);
    };
    DotvvmEvaluator.prototype.tryEval = function (func) {
        try {
            return func();
        }
        catch (error) {
            return null;
        }
    };
    return DotvvmEvaluator;
}());
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />
var DotvvmEventHub = (function () {
    function DotvvmEventHub() {
        this.map = {};
    }
    DotvvmEventHub.prototype.notify = function (id) {
        if (id in this.map)
            this.map[id].notifySubscribers();
        else
            this.map[id] = ko.observable(0);
    };
    DotvvmEventHub.prototype.get = function (id) {
        return this.map[id] || (this.map[id] = ko.observable(0));
    };
    return DotvvmEventHub;
}());
function basicAuthenticatedFetch(input, init) {
    function requestAuth() {
        var a = prompt("You credentials for " + (input["url"] || input)) || "";
        sessionStorage.setItem("someAuth", a);
        return a;
    }
    var auth = sessionStorage.getItem("someAuth");
    if (auth != null) {
        if (init == null)
            init = {};
        if (init.headers == null)
            init.headers = {};
        if (init.headers['Authorization'] == null)
            init.headers["Authorization"] = 'Basic ' + btoa(auth);
    }
    if (init == null)
        init = {};
    if (!init.cache)
        init.cache = "no-cache";
    return window.fetch(input, init).then(function (response) {
        if (response.status === 401 && auth == null) {
            if (sessionStorage.getItem("someAuth") == null)
                requestAuth();
            return basicAuthenticatedFetch(input, init);
        }
        else {
            return response;
        }
    });
}
(function () {
    var cachedValues = {};
    DotVVM.prototype.invokeApiFn = function (callback, refreshTriggers, notifyTriggers, commandId) {
        if (refreshTriggers === void 0) { refreshTriggers = []; }
        if (notifyTriggers === void 0) { notifyTriggers = []; }
        if (commandId === void 0) { commandId = callback.toString(); }
        var cachedValue = cachedValues[commandId] || (cachedValues[commandId] = ko.observable(null));
        var load = function () {
            try {
                var result = window["Promise"].resolve(ko.ignoreDependencies(callback));
                return { type: 'result', result: result.then(function (val) {
                        if (val) {
                            cachedValue(ko.unwrap(dotvvm.serialization.deserialize(val, cachedValue)));
                            cachedValue.notifySubscribers();
                        }
                        for (var _i = 0, notifyTriggers_1 = notifyTriggers; _i < notifyTriggers_1.length; _i++) {
                            var t = notifyTriggers_1[_i];
                            dotvvm.eventHub.notify(t);
                        }
                        return val;
                    }, console.warn) };
            }
            catch (e) {
                console.warn(e);
                return { type: 'error', error: e };
            }
        };
        var cmp = ko.pureComputed(function () { return cachedValue(); });
        cmp.refreshValue = function (throwOnError) {
            var promise = cachedValue["promise"];
            if (!cachedValue["isLoading"]) {
                cachedValue["isLoading"] = true;
                promise = load();
                cachedValue["promise"] = promise;
            }
            if (promise.type == 'error') {
                cachedValue["isLoading"] = false;
                if (throwOnError)
                    throw promise.error;
                else
                    return;
            }
            else {
                promise.result.then(function (p) { return cachedValue["isLoading"] = false; }, function (p) { return cachedValue["isLoading"] = false; });
                return promise.result;
            }
        };
        if (!cachedValue.peek())
            cmp.refreshValue();
        ko.computed(function () { return refreshTriggers.map(function (f) { return typeof f == "string" ? dotvvm.eventHub.get(f)() : f(); }); }).subscribe(function (p) { return cmp.refreshValue(); });
        return cmp;
    };
    DotVVM.prototype.apiRefreshOn = function (value, refreshOn) {
        if (typeof value.refreshValue != "function")
            console.error("The object is not refreshable");
        refreshOn.subscribe(function () {
            if (typeof value.refreshValue != "function")
                console.error("The object is not refreshable");
            value.refreshValue && value.refreshValue();
        });
        return value;
    };
    DotVVM.prototype.api = {};
    DotVVM.prototype.eventHub = new DotvvmEventHub();
}());
//# sourceMappingURL=DotVVM.js.map