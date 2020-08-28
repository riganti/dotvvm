// Typescript could not find the module, IDK why...
import fc_types from '../../../node_modules/fast-check/lib/types/fast-check'
import { initDotvvm, watchEvents } from './helper';
import { postBack } from '../postback/postback';
import { getViewModel } from '../dotvvm-base';
import { keys } from '../utils/objects';
import { DotvvmPostbackError } from '../shared-classes';
import enable from '../binding-handlers/enable';
import { getPostbackQueue } from '../postback/queue';
import { serialize } from '../serialization/serialize';

var fetchJson = async function<T>(url: string, init: RequestInit): Promise<T> {
    // the implementation is replaced by individual tests
    let response: Response;
    try {
        response = await fetch(url, init);
    }
    catch (err) {
        throw new DotvvmPostbackError({ type: "network", err });
    }

    const errorResponse = response.status >= 400;
    const isJson = response.headers.get("content-type") && response.headers.get('content-type')!.match(/^application\/json/);

    if (errorResponse || !isJson) {
        throw new DotvvmPostbackError({ type: "serverError", status: response.status, responseObject: (isJson ? await response.json() : null), response });
    }

    return response.json();
}

function appendAdditionalHeaders(headers: Headers, additionalHeaders?: { [key: string]: string }) {
    if (additionalHeaders) {
        for (const key of keys(additionalHeaders)) {
            headers.append(key, additionalHeaders[key]);
        }
    }
}

function delay(time: number) {
    return new Promise((r) => setTimeout(r, time))
}

jest.mock("../postback/http", () => ({
    async fetchCsrfToken() {
        getViewModel().$csrfToken = "test token"
    },

    retryOnInvalidCsrfToken<TResult>(postbackFunction: () => Promise<TResult>) {
        return postbackFunction()
    },

    async getJSON<T>(url: string, spaPlaceHolderUniqueId?: string, additionalHeaders?: { [key: string]: string }): Promise<T> {
        const headers = new Headers();
        headers.append('Accept', 'application/json');
        if (compileConstants.isSpa && spaPlaceHolderUniqueId) {
            headers.append('X-DotVVM-SpaContentPlaceHolder', spaPlaceHolderUniqueId);
        }
        appendAdditionalHeaders(headers, additionalHeaders);

        return await fetchJson<T>(url, { headers: headers });
    },

    async postJSON<T>(url: string, postData: any, additionalHeaders?: { [key: string]: string }): Promise<T> {
        const headers = new Headers();
        headers.append('Content-Type', 'application/json');
        headers.append('X-DotVVM-PostBack', 'true');
        appendAdditionalHeaders(headers, additionalHeaders);

        return await fetchJson<T>(url, { body: postData, headers: headers, method: "POST" });
    }
}));

function limitRuntime<TArgs extends any[], TResult>(milis: number, fn: (...args: TArgs) => Promise<TResult>): (...args: TArgs) => Promise<TResult> {
    return (...args) => {
        return Promise.race<Promise<TResult>>([
            fn(...args),
            new Promise<TResult>((resolve, reject) => {
                setTimeout(() => reject("Reached timeout - likely deadlock or something"), milis)
            })
        ]);
    }
}

const fc: typeof fc_types = require('fast-check');

const originalViewModel = {
    viewModel: {
        Property1: 0,
        Property2: 0
    },
    url: "/myPage",
    virtualDirectory: "",
    renderedResources: ["resource1", "resource2"]
}

function state() { return serialize(getViewModel()) as typeof originalViewModel.viewModel }

function cancerPostbackHandler(s: fc_types.Scheduler, lbl: string): DotvvmPostbackHandler {
    return {
        name: "cancer-handler",
        async execute(next) {
            await s.schedule(Promise.resolve(), `Postback handler ${lbl} BEFORE`)
            const commit = await next()
            await s.schedule(Promise.resolve(), `Postback handler ${lbl} AFTER`)
            // return s.scheduleFunction(commit)
            return async () => {
                await s.schedule(Promise.resolve(), `Postback handler ${lbl} commit BEFORE`)
                const result = await commit()
                await s.schedule(Promise.resolve(), `Postback handler ${lbl} commit AFTER`)
                return result
            }
        }
    }
}

initDotvvm(originalViewModel)

test("Postback: sanity check", async () => {

    fetchJson = async <T>(url: string, init: RequestInit) => {
        expect(url).toBe("/myPage")
        const obj = JSON.parse(init.body as string)
        expect(obj.command).toBe("c")
        expect(obj.renderedResources).toStrictEqual(["resource1", "resource2"])
        expect(obj.additionalData.validationTargetPath).toBe("$data")
        expect(obj.viewModel.$csrfToken).toBe("test token")
        expect(obj.viewModel.Property1).toBe(0)

        return {
            viewModelDiff: {
                Property1: 1
            },
            action: "successfulCommand",
            resources: {
                "resource3": "<script> window.resource3_script_loaded = true </script>" // TODO: try loading
            },
            updatedControls: {}
        } as any
    }

    await postBack(window.document.body, [], "c", "", undefined, [ "validate-this" ])

    expect(window["resource3_script_loaded" as any]).toBe(true)
    expect(state().Property1).toBe(1)
    expect(state().Property2).toBe(0)
})

test("Run postbacks [Queue | no failures]", async () => {
    jest.setTimeout(120_0000);

    // watchEvents()

    await fc.assert(fc.asyncProperty(
        fc.integer(2, 30),
        fc.scheduler(),
        limitRuntime(600, async (parallelism, s) => {
            (getViewModel() as any).Property1(0)
            // console.debug("Starting new test ", parallelism)
            let index = 0
            let enableScheduling = false
            fetchJson = async(url: string, init: RequestInit) => {
                if (enableScheduling)
                    await s.schedule(Promise.resolve(), `POST request ${url} ${init.body}`)
                return {
                    viewModelDiff: {
                        Property1: ++index
                    },
                    action: "successfulCommand",
                    updatedControls: {},
                    resources: {}
                } as any
            }

            // wait for stuff to settle down
            // postbacks from the previous run may have been running or so
            delay(1)
            const queue = getPostbackQueue("default")
            queue.queue.length = 0
            queue.noRunning = 0
            await postBack(window.document.body, [], "c", "", undefined, [ "concurrency-queue" ]);
            (getViewModel() as any).Property1(0)
            index = 0
            enableScheduling = true

            const postbacks =
                Array.from(
                    new Array(parallelism)
                ).map((_, i) => postBack(window.document.body, [], "c", "", undefined, [ "concurrency-queue", cancerPostbackHandler(s, i + "") ]))

            let done = false
            Promise.all(postbacks).then(_ => done = true, _ => done = true)
            await delay(1)

            while (!done) {
                // console.log(s.report())
                expect(s.count()).toBeGreaterThan(0)
                await s.waitOne()

                expect(state().Property1).toBeLessThanOrEqual(index)
                // another postback did not start before committing the original one
                expect(state().Property1).toBeGreaterThanOrEqual(index - 1)

                if (s.count() == 0) {
                    await delay(1)
                }
            }
            expect(state().Property1).toBe(parallelism)
        }
    )))
})
