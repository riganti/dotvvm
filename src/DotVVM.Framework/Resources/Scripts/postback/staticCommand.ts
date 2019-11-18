import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { getViewModel, getInitialUrl } from '../dotvvm-base';
import * as events from '../DotVVM.Events';
import * as updater from './updater';
import * as http from './http'
import { handleRedirect } from './redirect';

export function staticCommandPostback(viewModelName: string, sender: HTMLElement, command: string, args: any[], callback = _ => { }, errorCallback = (errorInfo: { xhr?: XMLHttpRequest, error?: any }) => { }) {
    return staticCommandPostbackCore(sender, command, args).then(
        callback,
        errorCallback
    );

    // TODO
    // events.error.trigger(new DotvvmErrorEventArgs(sender, viewModel, null));
    // events.staticCommandMethodFailed.trigger({ ...data, xhr: response, error: err })
    // events.error.trigger(new DotvvmErrorEventArgs(sender, viewModel, null));
    // events.staticCommandMethodFailed.trigger({ data })
}

async function staticCommandPostbackCore(sender: HTMLElement, command: string, args: any[]) : Promise<any> {

    return await http.retryOnInvalidCsrfToken(async () => {
        var csrfToken = await http.fetchCsrfToken();

        var data = serialize({ args, command, "$csrfToken": csrfToken });

        events.staticCommandMethodInvoking.trigger(data);

        var response = await http.postJSON<any>(
            getInitialUrl(),
            ko.toJSON(data),
            { "X-PostbackType": "StaticCommand" }
        );

        const result = response.result;
        if ("action" in response) {
            if (response.action == "redirect") {
                // redirect
                handleRedirect(response);
                throw { xhr: response, error: "redirect" };
            } else {
                throw new Error(`Invalid action ${response.action}`);
            }
        }
        events.staticCommandMethodInvoked.trigger({ ...data, result, xhr: response });

        return result;
    });
}
