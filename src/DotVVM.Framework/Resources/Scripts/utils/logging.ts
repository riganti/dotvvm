import { DotvvmPostbackError } from "../shared-classes";

type LogLevel = "normal" | "verbose";

export const level = getLogLevel();

export function logInfoVerbose(area: string, ...args: any[]) {
    if (level === "verbose") {
        console.log(`%c${area}`, "background-color: #7fdbff", ...args);
    }
}

export function logInfo(area: string, ...args: any[]) {
    console.log(`%c${area}`, "background-color: #f0f0f0", ...args);
}

export function logWarning(area: string, ...args: any[]) {
    console.warn(`%c${area}`, "background-color: #ff851b", ...args);
}

export function logError(area: string, ...args: any[]) {
    console.error(`%c${area}`, "background-color: #ff4136; color: white", ...args);
}

export function logPostBackScriptError(err: any) {
    if (err instanceof DotvvmPostbackError) {
        return;     // this was logged or handled in the postback pipeline
    }
    logError("postback", "Uncaught error returned from promise!", err);
}

function getLogLevel() : LogLevel {
    var logLevel = window.localStorage.getItem("dotvvm-loglevel");
    if (!logLevel) return "normal";
    if (logLevel === "normal" || logLevel === "verbose") return logLevel;
    
    logWarning("log", "Invalid value of 'dotvvm-loglevel' config value! Supported values: 'normal', 'verbose'");
    return "normal";
}
