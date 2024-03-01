import { DotvvmPostbackError } from "../shared-classes";

type LogLevel = "normal" | "verbose";

export const level = getLogLevel();

let logger = function defaultLogger(level: "warn" | "log" | "error" | "trace", area: DotvvmLoggingArea, ...args: any) {
    console[level](area, ...args)
}

/**
 * Instead of calling console.log, console.warn or console.error, DotVVM will call this function instead.
 * Please keep in mind that the exact wording of error message is not DotVVM public API and may change without notice.
 * @example
 * dotvvm.log.setLogger((previous, level, area, ...args) => {
 *     if (area == "validation" && /^This message should be an error$/.test(args[0])) {
 *          level = "error"
 *     }
*      previous(level, area, ...args) // call the default logger
 * })
 */
export function setLogger(newLogger: (previous: typeof logger, level: "warn" | "log" | "error" | "trace", area: DotvvmLoggingArea, ...args: any) => void) {
    const previousLogger = logger;
    logger = (...args) => newLogger(previousLogger, ...args);
}

export type DotvvmLoggingArea = (
    | "debug"
    | "configuration"
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
        logger("log", area, ...args);
    }
}

export function logInfo(area: DotvvmLoggingArea, ...args: any[]) {
    logger("log", area, ...args);
}

export function logWarning(area: DotvvmLoggingArea, ...args: any[]) {
    logger("warn", area, ...args);
}

export function logError(area: DotvvmLoggingArea, ...args: any[]) {
    logger("error", area, ...args);
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

        logWarning("configuration", "Invalid value of 'dotvvm-loglevel' config value! Supported values: 'normal', 'verbose'");
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
