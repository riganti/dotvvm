import { serialize } from '../serialization/serialize';
import { getInitialUrl } from '../dotvvm-base';
import * as events from '../events';
import * as http from './http'
import { handleRedirect } from './redirect';
import { getKnownTypes, updateTypeInfo } from '../metadata/typeMap';

export async function staticCommandPostback(sender: HTMLElement, command: string, args: any[], options: PostbackOptions): Promise<any> {

    let data: any;
    let response: http.WrappedResponse<StaticCommandResponse>;

    try {
        await http.retryOnInvalidCsrfToken(async () => {
            const csrfToken = await http.fetchCsrfToken();
            data = { 
                args: args.map(a => serialize(a)), 
                command, 
                $csrfToken: csrfToken,
                knownTypeMetadata: getKnownTypes()
            };
        });

        events.staticCommandMethodInvoking.trigger({
            ...options,
            methodId: command,
            methodArgs: args,
        });

        response = await http.postJSON<StaticCommandResponse>(
            getInitialUrl(),
            JSON.stringify(data),
            { "X-PostbackType": "StaticCommand" }
        );

        if ("action" in response.result) {
            if (response.result.action == "redirect") {
                // redirect
                await handleRedirect(options, response.result, response.response!);
                return;
            } else {
                throw new Error(`Invalid action ${response.result.action}`);
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
        
    } catch (err) {
        events.staticCommandMethodFailed.trigger({ 
            ...options, 
            methodId: command,
            methodArgs: args,
            error: err,
            result: (err.reason as any).responseObject, 
            response: (err.reason as any).response 
        })
        
        throw err;
    }
}
