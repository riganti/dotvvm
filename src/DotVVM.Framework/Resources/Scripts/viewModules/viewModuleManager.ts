import { deserialize } from "../serialization/deserialize";
import { serialize } from "../serialization/serialize";
import { keys } from "../utils/objects";

const registeredModules: { [name: string]: ModuleHandler } = {};

type ModuleCommand = (...args: any) => Promise<unknown>;

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

export function initViewModule(name: string, viewId: string, rootElement: HTMLElement) {
    if (viewId == null) { throw new Error("viewId has to have a value"); }
    if (rootElement == null) { throw new Error("rootElement has to have a value"); }

    const handler = ensureModuleHandler(name);

    if (!("default" in handler.module) || typeof handler.module.default !== "function") {
        console.error(`The module ${name} referenced in the @js directive must have a default export that is a function.`);
        return;
    }
    setupModuleDisposeHandlers(viewId, name, rootElement);

    if (handler.contexts[viewId]) {
        handler.contexts[viewId].elements.push(rootElement);
        return;
    }

    const elementContext = ko.contextFor(rootElement);
    const context = new ModuleContext(
        name,
        viewId,
        [rootElement],
        elementContext && typeof elementContext.$control === "object" ? { ...elementContext.$control } : {}
    );
    const moduleInstance = createModuleInstance(handler.module.default, context);
    handler.contexts[viewId] = context;

    context.module = moduleInstance;
    Object.freeze(context);
}

function createModuleInstance(fn: Function, ...args: any) {
    if (fn.prototype && fn.prototype.constructor === fn) {
        // the module exports a class
        return new (fn as any)(...args);
    } else {
        // the module exports a function
        return fn(...args);
    }
}

export function callViewModuleCommand(viewId: string, commandName: string, args: any[]) {
    if (commandName == null) { throw new Error("commandName has to have a value"); }

    const foundModules: { moduleName: string; context: ModuleContext }[] = [];
    
    for (let moduleName of keys(registeredModules)) {
        const context = ensureViewModuleContext(viewId, moduleName, false);
        if (!context) continue;
        if (commandName in context.module && typeof context.module[commandName] === "function") {
            foundModules.push({ moduleName, context });
        }
    }

    if (!foundModules.length) {
        throw new Error(`Command ${commandName} could not be found in any of the imported modules in view ${viewId}.`);
    }

    if (foundModules.length > 1) {
        throw new Error(`Conflict: There were multiple commands named ${commandName} the in imported modules in view ${viewId}. Check modules: ${foundModules.map(m => m.moduleName).join(', ')}.`);
    }

    return foundModules[0].context.module[commandName](...args.map(v => serialize(v)));
}

export function registerNamedCommand(viewId: string, commandName: string, command: ModuleCommand, rootElement: HTMLElement) {
    if (viewId == null) { throw new Error("Parameter viewId has to have a value"); }
    if (commandName == null) { throw new Error("Parameter commandName has to have a value"); }

    for (const moduleName of keys(registeredModules)) {
        const module = registeredModules[moduleName];

        const context = module.contexts[viewId];
        if (context) {
            context.registerNamedCommand(commandName, command);
        }
    }

    setupNamedCommandDisposeHandlers(viewId, commandName, rootElement);
}

function setupModuleDisposeHandlers(viewId: string, name: string, rootElement: HTMLElement) {
    function elementDisposeCallback() {
        disposeModule(viewId, name, rootElement);
        ko.utils.domNodeDisposal.removeDisposeCallback(rootElement, elementDisposeCallback);
    }
    ko.utils.domNodeDisposal.addDisposeCallback(rootElement, elementDisposeCallback);
}

function disposeModule(viewId: string, name: string, rootElement: HTMLElement) {
    const handler = ensureModuleHandler(name);
    const context = ensureViewModuleContext(viewId, name);

    const index = context.elements.indexOf(rootElement);
    if (index < 0) {
        throw new Error(`Cannot dispose module on a root element ${viewId}. It has already been disposed.`);
    }
    context.elements.splice(index, 1);

    if (!context.elements.length) {
        callIfDefined(context.module, '$dispose', context);
        delete handler.contexts[viewId];
    }
}

function setupNamedCommandDisposeHandlers(viewId: string, name: string, rootElement: HTMLElement) {
    function elementDisposeCallback() {
        unregisterNamedCommand(viewId, name);
        ko.utils.domNodeDisposal.removeDisposeCallback(rootElement, elementDisposeCallback);
    }
    ko.utils.domNodeDisposal.addDisposeCallback(rootElement, elementDisposeCallback);
}

export function unregisterNamedCommand(viewId: string, commandName: string) {
    if (viewId == null) { throw new Error("Parameter viewId has to have a value"); }
    if (commandName == null) { throw new Error("Parameter commandName has to have a value"); }

    for (const moduleName of keys(registeredModules)) {
        const module = registeredModules[moduleName];

        var context = module.contexts[viewId];
        if (context) {
            context.unregisterNamedCommand(commandName);
        }
    }
}

function ensureViewModuleContext(viewId: string, name: string, throwError: boolean = true): ModuleContext {
    const handler = ensureModuleHandler(name);
    const context = handler.contexts[viewId];

    if (!context && throwError) {
        throw new Error('Module ' + name + 'has not been initialized for view ' + viewId + ', or the view has been disposed');
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
    if (module[name] && typeof module[name] === 'function') {
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
        public readonly viewId: string,
        public readonly elements: HTMLElement[],
        public readonly properties: { [name: string]: any }) {
    }
    
    public registerNamedCommand = (name: string, command: (...args: any[]) => Promise<any>) => {
        if (name == null) {
            throw new Error("Parameter name has to have a value");
        }
        if (!command || typeof command !== 'function') {
            throw new Error('Named command has to be a function');
        }
        if (this.namedCommands[name]) {
            throw new Error(`A named command is already registered under the name: ${name}. The conflict occured in: ${this.moduleName} view ${this.viewId}.`);
        }

        this.namedCommands[name] = (...innerArgs) => command.apply(this, innerArgs.map(v => deserialize(v)));
    }

    public unregisterNamedCommand = (name: string) => {
        if (name in this.namedCommands) {
            delete this.namedCommands[name];
        }
    }
}
