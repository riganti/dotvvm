// Type definitions for virtual-dom
// Project: https://github.com/Matt-Esch/virtual-dom
// Definitions by: Joseph Abrahamson <me@jspha.com>
// Definitions: https://github.com/borisyankov/DefinitelyTyped

declare namespace virtualDom {
  export var h: H;
  
  export function diff(a: VNode, b: VNode): VPatches;
  export function patch(rootNode: HTMLElement, patches: VPatches): HTMLElement;
  export function create(vnode: VNode, opts: CreateOptions): HTMLElement;
  
  export type VTree = VNode | VText | Widget | Thunk
  
  export class VNode {
    constructor(tagName: string);
    constructor(tagName: string, properties: Props);
    constructor(tagName: string, properties: Props, children: (VNode | VText | string)[]);
    constructor(tagName: string, properties: Props, children: (VNode | VText | string)[], key: any);
    constructor(tagName: string, properties: Props, children: (VNode | VText | string)[], key: any, namespace: string);
    
    tagName: string;
    properties: Props;
    children: VTree[];
    key: string;
    namespace: string;
    
    count: number;
    hasWidgets: boolean;
    hasThunks: boolean;
    hooks: { [key: string]: VHook };
    descendantHooks: boolean;
    
    type: string;
    version: string;
  }
  
  export class VText {
    constructor(text: string);
    text: string;
    
    type: string;
    version: string;
  }
  
  /**
   * 
   * Boilerplate Widget
   * 
   *     var Widget = function (){}
   *     Widget.prototype.type = "Widget"
   *     Widget.prototype.init = function(){}
   *     Widget.prototype.update = function(previous, domNode){}
   *     Widget.prototype.destroy = function(domNode){}
   * 
   * See <https://github.com/Matt-Esch/virtual-dom/blob/master/docs/widget.md>
   */
  export interface Widget {
    init(): Element;
    update(previousWidget: Widget, previousDomNode: Element): any;
    destroy(domNode: Element): any;
    
    /** Must be "Widget" */
    type: string;
  }
  
  /**   
   * Boilerplate Thunk
   * 
   *     var Thunk = function (){}
   *     Thunk.prototype.type = "Thunk"
   *     Thunk.prototype.render = function(previous){}
   * 
   * See <https://github.com/Matt-Esch/virtual-dom/blob/master/docs/thunk.md>
   */
  export interface Thunk {
    /** 
     * Can examine the previous value here to determine whether or not to do
     * work re-rendering.
     * 
     * When render is called a second time the previously rendered result will be 
     * available at the 'vnode' property of 'previous'. Coerce to access.
     */
    render(previous: VTree): VNode | VText | Widget;
    
    /** Must be "Thunk" */
    type: string
  }

  export interface VPatches {
    [index: number]: VPatch
    a: VNode
  }

  export class VPatch {
    constructor(type: number, vNode: VNode, patch: any);
    
    type: number;
    vNode: VNode;
    patch: any;
        
    static NONE: number;
    static VTEXT: number;
    static VNODE: number;
    static WIDGET: number;
    static PROPS: number;
    static ORDER: number;
    static INSERT: number;
    static REMOVE: number;
    static THUNK: number;
  }

  export interface Map {
    [key: string]: string
  }

  export interface Props {
    /** See <https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes> */
    [key: string]: any
    
    /** See <https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes> */
    attributes?: Map;
    style?: Map;
    /** 'data-*' attributes */
    dataset?: Map
    className?: string;
    
    /**
     * If an input element is reset, its value will be returned to its value set 
     * in properties.attributes.value. If you've set the value using 
     * properties.value, this will not happen. However, you may set 
     * properties.defaultValue to get the desired result.
     */
    value?: any
    defaultValue?: any
  }
  
  export interface H {
    (): VNode;
    (tagName: string): VNode;
    (tagName: string, children: VNode[]): VNode;
    (tagName: string, properties: Props): VNode;
    (tagName: string, properties: Props, children: (VNode | VText | string)[]): VNode;
  }
  
  export interface CreateOptions {
    document?: HTMLDocument;
    warn?: boolean;
  }
  
  /**
   * While the VHook interface merely demands that the hook and unhook attributes
   * are available, more generally it's required that hook and unhook NOT be own 
   * attribtues of the object. This is achievable by setting them on the prototype.
   * 
   * See <https://github.com/Matt-Esch/virtual-dom/blob/master/docs/hooks.md>
   */
  export interface VHook {
    hook:    (node: Element, propertyName: string, previousValue: any) => any;
    unhoook: (node: Element, propertyName: string, nextValue: any) => any;
  }
}