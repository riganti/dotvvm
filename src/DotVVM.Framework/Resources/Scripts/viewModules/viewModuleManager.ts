import { deserialize } from "../serialization/deserialize";
import { serialize } from "../serialization/serialize";
import { unmapKnockoutObservables } from "../state-manager";
import { keys } from "../utils/objects";

const registeredModules: { [name: string]: ModuleHandler } = {};

type ModuleCommand = (...args: any) => Promise<unknown>;

export const viewModulesSymbol = Symbol("viewModules");

export function registerViewModule(name: string, moduleObject: any) {
    if (name == null) { throw new Error("Parameter name has to have a value"); }
    if (moduleObject == null) { throw new Error("Parameter moduleObject has to have a value"); }

    //If multiple views on the same page use the module, we only want to register it the first time,
    //the other views then get initialized on their own
    if (!registeredModules[name]) {
        registeredModules[name] = new ModuleHandler(name, moduleObject);
    }
}

export function registerViewModules(modules: { [name: string]: any }) {
    for (const moduleName of keys(modules)) {
        const moduleObject = modules[moduleName];
        registerViewModule(moduleName, moduleObject);
    }
}

export function initViewModule(name: string, viewIdOrElement: string | HTMLElement, rootElement: HTMLElement): ModuleContext {
    if (rootElement == null) { throw new Error("rootElement has to have a value"); }

    const handler = ensureModuleHandler(name);
    setupModuleDisposeHandlers(viewIdOrElement, name, rootElement);

    if (typeof viewIdOrElement === "string" && handler.contexts[viewIdOrElement]) {
        // contexts with the same viewId are shared
        const context = handler.contexts[viewIdOrElement];
        context.elements.push(rootElement);
        return context;
    }

    if (!("default" in handler.module) || typeof handler.module.default !== "function") {
        throw new Error(`The module ${name} referenced in the @js directive must have a default export that is a function.`);
    }

    const elementContext = ko.contextFor(rootElement);
    const context = new ModuleContext(
        name,
        [rootElement],
        elementContext && elementContext.$control ? { ...elementContext.$control } : {}
    );
    const moduleInstance = createModuleInstance(handler.module.default, context);
    context.module = moduleInstance;
    Object.freeze(context);

    if (typeof viewIdOrElement === "string") {
        handler.contexts[viewIdOrElement] = context;
    }
    
    return context;
}

function createModuleInstance(fn: Function, ...args: any) {
    return fn(...args);
}

export function callViewModuleCommand(viewIdOrElement: string | HTMLElement, commandName: string, args: any[]) {
    if (commandName == null) { throw new Error("commandName has to have a value"); }

    const foundModules: { moduleName: string; context: ModuleContext }[] = [];

    for (let moduleName of keys(registeredModules)) {
        const context = tryFindViewModuleContext(viewIdOrElement, moduleName);
        if (!(context && context.module)) continue;
        if (commandName in context.module && typeof context.module[commandName] === "function") {
            foundModules.push({ moduleName, context });
        }
    }

    if (!foundModules.length) {
        throw new Error(`Command ${commandName} could not be found in any of the imported modules in view ${viewIdOrElement}.`);
    }

    if (foundModules.length > 1) {
        throw new Error(`Conflict: There were multiple commands named ${commandName} the in imported modules in view ${viewIdOrElement}. Check modules: ${foundModules.map(m => m.moduleName).join(', ')}.`);
    }

    try {
        return foundModules[0].context.module[commandName](...args.map(v => serialize(v)));
    }
    catch (e: unknown) {
        throw new Error(`While executing command ${commandName}(${args.map(v => JSON.stringify(serialize(v)))}), an error occurred. ${e}`);
    }
}

function setupModuleDisposeHandlers(viewIdOrElement: string | HTMLElement, name: string, rootElement: HTMLElement) {
    function elementDisposeCallback() {
        disposeModule(viewIdOrElement, name, rootElement);
        ko.utils.domNodeDisposal.removeDisposeCallback(rootElement, elementDisposeCallback);
    }
    ko.utils.domNodeDisposal.addDisposeCallback(rootElement, elementDisposeCallback);
}

function disposeModule(viewIdOrElement: string | HTMLElement, name: string, rootElement: HTMLElement) {
    const context = ensureViewModuleContext(viewIdOrElement, name);

    const index = context.elements.indexOf(rootElement);
    if (index < 0) {
        throw new Error(`Cannot dispose module on a root element ${viewIdOrElement}. It has already been disposed.`);
    }
    context.elements.splice(index, 1);

    if (!context.elements.length) {
        callIfDefined(context.module, '$dispose', context);
        
        if (typeof viewIdOrElement === "string") {
            const handler = ensureModuleHandler(name);
            delete handler.contexts[viewIdOrElement];
        }
    }
}

export function registerNamedCommand(viewIdOrElement: string | HTMLElement, commandName: string, command: ModuleCommand, rootElement: HTMLElement) {
    for (const moduleName of keys(registeredModules)) {
        const context = tryFindViewModuleContext(viewIdOrElement, moduleName);
        if (context) {
            context.registerNamedCommand(commandName, command);
        }
    }
    setupNamedCommandDisposeHandlers(viewIdOrElement, commandName, rootElement);
}

function setupNamedCommandDisposeHandlers(viewIdOrElement: string | HTMLElement, name: string, rootElement: HTMLElement) {
    function elementDisposeCallback() {
        unregisterNamedCommand(viewIdOrElement, name);
        ko.utils.domNodeDisposal.removeDisposeCallback(rootElement, elementDisposeCallback);
    }
    ko.utils.domNodeDisposal.addDisposeCallback(rootElement, elementDisposeCallback);
}

export function unregisterNamedCommand(viewIdOrElement: string | HTMLElement, commandName: string) {
    for (const moduleName of keys(registeredModules)) {
        var context = tryFindViewModuleContext(viewIdOrElement, moduleName);
        if (context) {
            context.unregisterNamedCommand(commandName);
        }
    }
}

function tryFindViewModuleContext(viewIdOrElement: string | HTMLElement, name: string): ModuleContext | undefined {
    if (typeof viewIdOrElement === "string") {
        const handler = ensureModuleHandler(name);
        return handler.contexts[viewIdOrElement];
    } else {
        let contexts = (viewIdOrElement as any)[viewModulesSymbol];
        if (contexts) {
            return contexts[name];
        } else {
            let contexts = ko.contextFor(viewIdOrElement);
            while (contexts && !("$viewModules" in contexts)) {
                contexts = contexts.$parentContext;
            }
            return contexts["$viewModules"] && contexts["$viewModules"][name];
        }
    }
}

function ensureViewModuleContext(viewIdOrElement: string | HTMLElement, name: string): ModuleContext {
    const context = tryFindViewModuleContext(viewIdOrElement, name);
    if (!context) {
        throw new Error('Module ' + name + 'has not been initialized for view ' + viewIdOrElement + ', or the view has been disposed');
    }
    return context;
}

function ensureModuleHandler(name: string): ModuleHandler {
    if (name == null) { throw new Error("name has to have a value"); }

    const handler = registeredModules[name];
    if (!handler) {
        throw new Error('Could not find module ' + name + '. Module is not registered, or has been disposed.');
    }
    return handler;
}

function callIfDefined(module: any, name: string, ...args: any[]) {
    if(!module) return; 
    if (typeof module[name] === 'function') {
        module[name](...args);
    }
}

class ModuleHandler {
    public contexts: { [viewId: string]: ModuleContext } = {};
    constructor(public readonly name: string, public readonly module: any) {
    }
}

export class ModuleContext {
    private readonly namedCommands: { [name: string]: (...args: any[]) => Promise<any> } = {};
    public module: any;
    
    constructor(
        public readonly moduleName: string,
        public readonly elements: HTMLElement[],
        public readonly properties: { [name: string]: any }) {
    }
    
    public registerNamedCommand = (name: string, command: (...args: any[]) => Promise<any>) => {
        if (this.namedCommands[name]) {
            throw new Error(`A named command is already registered under the name: ${name}. The conflict occured in: ${this.moduleName}.`);
        }

        this.namedCommands[name] = (...innerArgs) => command.apply(this, innerArgs.map(unmapKnockoutObservables));
    }

    public unregisterNamedCommand = (name: string) => {
        if (name in this.namedCommands) {
            delete this.namedCommands[name];
        }
    }
}
