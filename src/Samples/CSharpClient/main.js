import { dotnet } from './dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet.withDiagnosticTracing(true).create();

setModuleImports("/wasm/main.js", {
    ready: () => {
        console.log("DotVVM interop is ready.");
    },
    callNamedCommand: (typeName, instanceName, commandName, args) => {
        console.log(typeName, instanceName, commandName, args);
    },
    getViewModelSnapshot: () => {
        return JSON.stringify(dotvvm.state);
    },
    patchViewModel: (patchJson) => {
        dotvvm.patchState(JSON.parse(patchJson));
    }    
});

const config = getConfig();
const exports = await getAssemblyExports("DotVVM.Framework.Interop.DotnetWasm");

await dotnet.run();

const interop = exports.DotVVM.Framework.Interop.DotnetWasm.DotnetWasmInterop;

const type = "DotVVM.Samples.BasicSamples.CSharpClient.TestCsharpModule, DotVVM.Samples.BasicSamples.CSharpClient";

interop.CreateViewModuleInstance(type, "p0", ["TestCommand"]);
console.log("Created");

interop.CallViewModuleCommand(type, "p0", "Hello", []);
console.log("Command called");

var viewModelValue = interop.CallViewModuleCommand(type, "p0", "TestViewModelAccess", []);
console.log("View model value: " + viewModelValue);

interop.CallViewModuleCommand(type, "p0", "PatchViewModel", ["15"]);
console.log("New view model value: " + dotvvm.state.Value);

var commandResult = interop.CallViewModuleCommand(type, "p0", "CallNamedCommand", ["30"]);
console.log("Command result: " + commandResult);

interop.DisposeViewModuleInstance(type, "p0");
console.log("Disposed");
