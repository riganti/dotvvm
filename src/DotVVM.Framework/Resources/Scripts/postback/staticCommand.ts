import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { getViewModel, getInitialUrl } from '../dotvvm-base';
import * as events from '../events';
import * as updater from './updater';
import * as http from './http'
import { handleRedirect } from './redirect';
import { DotvvmPostbackError } from '../shared-classes';

export function staticCommandPostback_old(viewModelName: string, sender: HTMLElement, command: string, args: any[], callback = (a: any) => { }, errorCallback = (errorInfo: { xhr?: XMLHttpRequest, error?: any }) => { }) {
    return staticCommandPostback(sender, command, args).then(
        callback,
        errorCallback
    );

    // TODO
    // events.error.trigger(new DotvvmErrorEventArgs(sender, viewModel, null));
    // events.staticCommandMethodFailed.trigger({ ...data, xhr: response, error: err })
    // events.error.trigger(new DotvvmErrorEventArgs(sender, viewModel, null));
    // events.staticCommandMethodFailed.trigger({ data })
}

export function staticCommandPostback(sender: HTMLElement, command: string, args: any[]): Promise<any> {

    const promise = http.retryOnInvalidCsrfToken(async () => {
        const csrfToken = await http.fetchCsrfToken();

        const data = serialize({ args, command, $csrfToken: csrfToken });

        events.staticCommandMethodInvoking.trigger(data);

        const response = await http.postJSON<any>(
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

    promise.catch(err => {
        if (err instanceof DotvvmPostbackError) {
            const r = err.reason;
            if (r.type == "network") {
                events.error.trigger({ sender, handled: true, serverResponseObject: r.err });
            }
        }
    });
    return promise;
}
