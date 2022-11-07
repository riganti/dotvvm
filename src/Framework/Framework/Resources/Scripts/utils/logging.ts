import { DotvvmPostbackError } from "../shared-classes";

type LogLevel = "normal" | "verbose";

export const level = getLogLevel();

export type DotvvmLoggingArea = (
    | "debug"
    | "log"
    | "postback"
    | "spa"
    | "static-command"
    | "binding-handler"
    | "resource-loader"
    | "coercer"
    | "state-manager"
    | "validation"
    | "events"
    | "rest-api"
)

export function logInfoVerbose(area: DotvvmLoggingArea, ...args: any[]) {
    if (compileConstants.debug && level === "verbose") {
        console.log(`%c${area}`, ...args);
    }
}

export function logInfo(area: DotvvmLoggingArea, ...args: any[]) {
    console.log(area, ...args);
}

export function logWarning(area: DotvvmLoggingArea, ...args: any[]) {
    console.warn(area, ...args);
}

export function logError(area: DotvvmLoggingArea, ...args: any[]) {
    console.error(area, ...args);
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
