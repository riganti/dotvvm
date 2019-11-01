import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { viewModel, currentUrl } from '../dotvvm-root';
import { events } from '../DotVVM.Events'; 
import * as updater from './updater';
import * as http from './http'

export function staticCommandPostback(viewModelName: string, sender: HTMLElement, command: string, args: any[], callback = _ => { }, errorCallback = (errorInfo: { xhr?: XMLHttpRequest, error?: any }) => { }) {
    return staticCommandPostbackCore(sender, command, args).then(
        callback,
        errorCallback
    );

    events.error.trigger(new DotvvmErrorEventArgs(sender, viewModel, null));
}

async function staticCommandPostbackCore(sender: HTMLElement, command: string, args: any[]) : Promise<any> {
    var csrfToken = await http.fetchCsrfToken();
    
    var data = serialize({ args, command, "$csrfToken": csrfToken });

    events.staticCommandMethodInvoking.trigger(data);
    try {
        var response = await http.postJSON(
            currentUrl, 
            ko.toJSON(data),
            { "X-PostbackType": "StaticCommand" }
        );
        
        const result = response.result;
        updater.updateViewModel(() => {
            if ("action" in response) {
                if (response.action == "redirect") {
                    // redirect
                    this.handleRedirect(response);
                    throw { xhr: response, error: "redirect" };
                } else {
                    throw new Error(`Invalid action ${response.action}`);
                }
            }
            events.staticCommandMethodInvoked.trigger({ ...data, result, xhr: response });
        });

        return result;
    }
    catch (err) {
        events.staticCommandMethodFailed.trigger({ ...data, xhr: response, error: err })

        // if the CSRF token is invalid, retry the postback
        if (err.type === "serverError") {
            if (err.resultObject.action === "invalidCsrfToken") {
                console.log("Resending postback due to invalid CSRF token.") // this may loop indefinitely (in some extreme case), we don't currently have any loop detection mechanism, so at least we can log it.
                
                viewModel.$csrfToken = null;
                return await staticCommandPostbackCore(sender, command, args);
            }
        }

        events.error.trigger(new DotvvmErrorEventArgs(sender, viewModel, null));
        events.staticCommandMethodFailed.trigger({ data })

        throw err;
    }
}
