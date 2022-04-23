type ReceivedMessage = { messageId: number } &
    (
        { action: "HttpRequest", body: string, headers: [{ Key: string, Value: string }], status: number }
    );

const pendingRequests: { resolve: (result: any) => void, reject: (result: any) => void }[] = [];

// send messages
export function sendMessage(message: any) {
    (window.external as any).sendMessage(message);
}

export async function sendMessageAndWaitForResponse<T>(message: any): Promise<T> {
    message.id = pendingRequests.length;
    const promise = new Promise<T>((resolve, reject) => {
        pendingRequests[message.id] = { resolve, reject };
        sendMessage(message);
    });
    return await promise;
}

// handle commands from the webview
(window.external as any).receiveMessage(async (json: any) => {

    function handleCommand(message: ReceivedMessage) {
        if (message.action === "HttpRequest") {
            // handle incoming HTTP request responses
            const promise = pendingRequests[message.messageId]

            const headers = new Headers();
            for (const h of message.headers) {
                headers.append(h.Key, h.Value);
            }
            const response = new Response(message.body, { headers, status: message.status });
            promise.resolve(response);
            return;

        } else {
            // allow register custom message processors
            for (const processor of messageProcessors) {
                const result = processor(message);
                if (typeof result !== "undefined") {
                    return result;
                }
            }
            throw `Command ${message.action} not found!`;
        }
    }

    const message = <ReceivedMessage>JSON.parse(json);
    try {
        const result = await handleCommand(message);
        if (typeof result !== "undefined") {
            sendMessage({
                type: "HandlerCommand",
                id: message.messageId,
                result: JSON.stringify(result)
            });
        }
    }
    catch (err) {
        sendMessage({
            type: "HandlerCommand",
            id: message.messageId,
            errorMessage: JSON.stringify(err)
        });
    }
});

type MessageProcessor = (processor: { action: string }) => any;
const messageProcessors: MessageProcessor[] = [];

export function registerMessageProcessor(processor: MessageProcessor) {
    messageProcessors.push(processor);
}

export async function webMessageFetch(url: string, init: RequestInit): Promise<Response> {
    if (init.method?.toUpperCase() === "GET") {
        return await window.fetch(url, init);
    }

    const headers: any = {};
    (<Headers>init.headers)?.forEach((v, k) => headers[k] = v);

    return await sendMessageAndWaitForResponse<Response>({
        type: "HttpRequest",
        url,
        method: init.method || "GET",
        headers: headers,
        body: init.body as string
    });
}
