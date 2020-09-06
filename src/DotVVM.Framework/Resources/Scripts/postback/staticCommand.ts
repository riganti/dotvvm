import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { getViewModel, getInitialUrl } from '../dotvvm-base';
import * as events from '../events';
import * as updater from './updater';
import * as http from './http'
import { handleRedirect } from './redirect';

export async function staticCommandPostback(sender: HTMLElement, command: string, args: any[], options: PostbackOptions): Promise<any> {

    let data: any;
    let response: http.WrappedResponse<any>;

    return (async () => {
        try {
            await http.retryOnInvalidCsrfToken(async () => {
                const csrfToken = await http.fetchCsrfToken();
                data = serialize({ args, command, $csrfToken: csrfToken });
            });

            events.staticCommandMethodInvoking.trigger({
                ...options,
                command,
                args
            });

            response = await http.postJSON<any>(
                getInitialUrl(),
                JSON.stringify(data),
                { "X-PostbackType": "StaticCommand" }
            );

            if ("action" in response.result) {
                if (response.result.action == "redirect") {
                    // redirect
                    handleRedirect(options, response.result, response.response!);
                    return;
                } else {
                    throw new Error(`Invalid action ${response.result.action}`);
                }
            }

            events.staticCommandMethodInvoked.trigger({ 
                ...options, 
                command,
                args,
                serverResponseObject: response.result,
                result: (response as any).result.result, 
                response: (response as any).response
            });

            return response.result;
            
        } catch (err) {
            events.staticCommandMethodFailed.trigger({ 
                ...options, 
                command,
                args,
                error: err,
                result: (response as any).result.result, 
                response: (response as any).response 
            })
            
            throw err;
        }
    });
}
