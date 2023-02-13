import { serialize } from '../serialization/serialize';
import { getInitialUrl } from '../dotvvm-base';
import * as events from '../events';
import * as http from './http'
import { getKnownTypes, updateTypeInfo } from '../metadata/typeMap';
import { DotvvmPostbackError } from '../shared-classes';
import * as evaluator from '../utils/evaluator'

function resolveRelativeValidationPaths(paths: string[], options: PostbackOptions) {
    return paths?.map(p => {
        if (p == null) {
            return null
        }
        let context = options.knockoutContext
        while (context && /^..[\/$]/.test(p)) {
            context = context.$parentContext;
            p = p.substring(2);
            p = p.startsWith('/') ? p.substring(1) : ''
        }
        if (context == null) {
            return null
        }
        const absolutePath = evaluator.findPathToChildObject(dotvvm.state, context.$rawData.state, "/")
        return absolutePath ? absolutePath + p : null
    })
}

export async function staticCommandPostback(command: string, args: any[], paths: string[], options: PostbackOptions): Promise<any> {

    let data: any;
    let response: http.WrappedResponse<DotvvmStaticCommandResponse>;

    try {
        const absolutePaths = resolveRelativeValidationPaths(paths, options)

        await http.retryOnInvalidCsrfToken(async () => {
            const csrfToken = await http.fetchCsrfToken(options.abortSignal);
            data = { 
                args: args.map(a => serialize(a)), 
                command, 
                paths: absolutePaths,
                $csrfToken: csrfToken,
                knownTypeMetadata: getKnownTypes()
            };
        });

        events.staticCommandMethodInvoking.trigger({
            ...options,
            methodId: command,
            methodArgs: args
        });

        response = await http.postJSON<DotvvmStaticCommandResponse>(
            getInitialUrl(),
            JSON.stringify(data),
            options.abortSignal,
            { "X-PostbackType": "StaticCommand" }
        );

        if ("action" in response.result) {
            const action = response.result.action
            if (action == "redirect") {
                throw new DotvvmPostbackError({
                    type: "redirect",
                    response: response.response,
                    responseObject: response.result
                })
            } else if (action == "validationErrors") {
                throw new DotvvmPostbackError({
                    type: "validation",
                    response: response.response,
                    responseObject: response.result
                })
            } else {
                throw new Error(`Invalid action ${action}`);
            }
        }

        updateTypeInfo(response.result.typeMetadata);

        events.staticCommandMethodInvoked.trigger({ 
            ...options, 
            methodId: command,
            methodArgs: args,
            serverResponseObject: response.result,
            result: (response as any).result.result, 
            response: (response as any).response
        });

        return response.result.result;
        
    } catch (err: any) {
        events.staticCommandMethodFailed.trigger({ 
            ...options, 
            methodId: command,
            methodArgs: args,
            error: err,
            result: (err.reason as any)?.responseObject, 
            response: (err.reason as any)?.response 
        })
        
        throw err;
    }
}
