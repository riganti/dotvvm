import { DotvvmPostbackError } from "../shared-classes";

type LogLevel = "normal" | "verbose";

export const level = getLogLevel();

export function logInfoVerbose(area: string, ...args: any[]) {
    if (compileConstants.debug && level === "verbose") {
        console.log(`%c${area}`, ...args);
    }
}

export function logInfo(area: string, ...args: any[]) {
    console.log(`%c${area}`, ...args);
}

export function logWarning(area: string, ...args: any[]) {
    console.warn(`%c${area}`, ...args);
}

export function logError(area: string, ...args: any[]) {
    console.error(`%c${area}`, ...args);
}

export function logPostBackScriptError(err: any) {
    if (err instanceof DotvvmPostbackError) {
        return;     // this was logged or handled in the postback pipeline
    }
    logError("postback", "Uncaught error returned from promise!", err);
}

function getLogLevel() : LogLevel {
    if (compileConstants.debug) {
        var logLevel = window.localStorage.getItem("dotvvm-loglevel");
        if (!logLevel) return "normal";
        if (logLevel === "normal" || logLevel === "verbose") return logLevel;
        
        logWarning("log", "Invalid value of 'dotvvm-loglevel' config value! Supported values: 'normal', 'verbose'");
    }
    return "normal";
}

/** puts the string in quotes, escaping weird characters if it is more complex than just letters */
export function debugQuoteString(s: string) {
    if (/[\w-_]/.test(s)) {
        return s;
    } else {
        return JSON.stringify(s);
    }
}
