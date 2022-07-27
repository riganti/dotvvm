// Typescript could not find the module, IDK why...
import fc_types from '../../../node_modules/fast-check/lib/types/fast-check'
import { initDotvvm, watchEvents, fc, delay, waitForEnd } from './helper';
import { postBack } from '../postback/postback';
import { getStateManager } from '../dotvvm-base';
import { keys } from '../utils/objects';
import { DotvvmPostbackError } from '../shared-classes';
import enable from '../binding-handlers/enable';
import { getPostbackQueue, postbackQueues } from '../postback/queue';
import { WrappedResponse } from '../postback/http';

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

jest.mock("../postback/http", () => ({
    async fetchCsrfToken() {
        getStateManager().update(vm => ({ ...vm, $csrfToken: "test token" }))
    },

    retryOnInvalidCsrfToken<TResult>(postbackFunction: () => Promise<TResult>) {
        return postbackFunction()
    },

    async getJSON<T>(url: string, spaPlaceHolderUniqueId?: string, signal?: AbortSignal, additionalHeaders?: { [key: string]: string }): Promise<WrappedResponse<T>> {
        const headers = new Headers();
        headers.append('Accept', 'application/json');
        if (compileConstants.isSpa && spaPlaceHolderUniqueId) {
            headers.append('X-DotVVM-SpaContentPlaceHolder', spaPlaceHolderUniqueId);
        }
        appendAdditionalHeaders(headers, additionalHeaders);

        return { response: { fake: "get" } as any as Response, result: await fetchJson<T>(url, { headers: headers, signal }) };
    },

    async postJSON<T>(url: string, postData: any, signal?: AbortSignal, additionalHeaders?: { [key: string]: string }): Promise<WrappedResponse<T>> {
        const headers = new Headers();
        headers.append('Content-Type', 'application/json');
        headers.append('X-DotVVM-PostBack', 'true');
        appendAdditionalHeaders(headers, additionalHeaders);

        return { response: { fake: "post" } as any as Response, result: await fetchJson<T>(url, { body: postData, headers: headers, method: "POST", signal }) };
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


const originalViewModel = {
    viewModel: {
        $type: "t1",
        Property1: 0,
        Property2: 0
    },
    typeMetadata: {
        t1: {
            type: "object",
            properties: {
                Property1: {
                    type: "Int32"
                },
                Property2: {
                    type: "Int32"
                }
            }
        }
    },
    url: "/myPage",
    virtualDirectory: "",
    renderedResources: ["resource1", "resource2"]
}

function state() { return getStateManager().state as typeof originalViewModel.viewModel }

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

function makeRange<T>(num: number, create: (i: number) => T): T[] {
    return Array.from(new Array(num)).map((_, i) => create(i))
}

function makeEventRange<T>(num: number, scheduler: fc_types.Scheduler, label: string, create: (i: number) => Promise<T>): Promise<T>[] {
    return Array.from(new Array(num)).map((_, i) =>
        scheduler.schedule(Promise.resolve(i), `INIT ${label} ${i}`).then(create)
    )
}

function resetQueues() {
    for (const q of keys(postbackQueues)) {
        delete postbackQueues[q]
    }
}

function makeResponse(viewModelDiff: any): any {
    return {
        viewModelDiff,
        action: "successfulCommand",
        updatedControls: {},
        resources: {}
    }
}

test("Postback: sanity check", async () => {

    fetchJson = async <T>(url: string, init: RequestInit) => {
        expect(url).toBe("/myPage")
        const obj = JSON.parse(init.body as string)
        expect(obj.command).toBe("c")
        expect(obj.renderedResources).toStrictEqual(["resource1", "resource2"])
        expect(obj.validationTargetPath).toBe("/")
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

    await postBack(window.document.body, [], "c", "", undefined, [ "validate-root" ])

    expect(window["resource3_script_loaded" as any]).toBe(true)
    expect(state().Property1).toBe(1)
    expect(state().Property2).toBe(0)
})

// test("Test runAllTimers", () => {
//     // check that https://github.com/facebook/jest/pull/6876 is not an issue
//     let x = false
//     Promise.resolve().then(() => x = true)
//     jest.runAllTimers()
//     expect(x).toBe(true)
// })

test("Run postbacks [Queue | no failures]", async () => {
    jest.setTimeout(120_000);

    // watchEvents()

    await fc.assert(fc.asyncProperty(
        fc.integer(1, 30),
        fc.scheduler(),
        async (parallelism, s) => {
            // wait for stuff to settle down
            // postbacks from the previous run may have been running or so
            await delay(1)
            resetQueues()

            // console.debug("Starting new test ", parallelism)
            let index = 0
            fetchJson = async(url: string, init: RequestInit) => {
                return makeResponse({
                    Property1: ++index
                })
            }

            getStateManager().update(x => ({...x, Property1: 0}))

            const postbacks =
                makeRange(parallelism, i =>
                    postBack(window.document.body, [], "c", "", undefined, [ "concurrency-queue", cancerPostbackHandler(s, i + "") ])
                )

            await waitForEnd(postbacks, s, () => {
                expect(state().Property1).toBeLessThanOrEqual(index)
                // another postback did not start before committing the original one
                expect(state().Property1).toBeGreaterThanOrEqual(index - 1)
            })


            expect(state().Property1).toBe(parallelism)
        }
    ), { timeout: 2000 })
})

test("Run postbacks [Queue + Deny | no failures]", async () => {
    jest.setTimeout(120_000);

    // watchEvents()

    await fc.assert(fc.asyncProperty(
        fc.integer(1, 10),
        fc.integer(0, 10),
        fc.scheduler(),
        async (parallelismQ, parallelismD, s) => {
            // wait for stuff to settle down
            // postbacks from the previous run may have been running or so
            await delay(1)
            resetQueues()

            // console.debug("Starting new test ", parallelismQ, parallelismD)
            let index = 0
            let index2 = 0
            fetchJson = async(url: string, init: RequestInit) => {
                const { command } = JSON.parse(init.body as string)
                if (command == "queue") {
                    return makeResponse({
                        Property1: ++index
                    })
                } else if (command == "deny") {
                    return makeResponse({
                        Property2: ++index2
                    })
                }
            }

            getStateManager().update(x => ({...x, Property1: 0, Property2: 0}))

            // these postbacks must pass
            const queuePostbacks: Promise<any>[] =
                makeEventRange(parallelismQ, s, "queue postback", i =>
                    postBack(window.document.body, [], "queue", "", undefined, [ "concurrency-queue", cancerPostbackHandler(s, i + "Q") ])
                )

            const initDenyPostback = postBack(window.document.body, [], "deny", "", undefined, [ "concurrency-deny", cancerPostbackHandler(s, "initial D") ])

            const denyPostbacks =
                makeEventRange(parallelismD, s, "deny postback", i =>
                    postBack(window.document.body, [], "deny", "", undefined, [ "concurrency-deny", cancerPostbackHandler(s, i + "D") ]).catch(_ => {})
                )

            await waitForEnd(queuePostbacks.concat(denyPostbacks).concat([initDenyPostback]), s, () => {
                expect(state().Property1).toBeLessThanOrEqual(index)
                // another postback did not start before committing the original one
                expect(state().Property1).toBeGreaterThanOrEqual(index - 1)
            })

            // all Queue postback got through
            expect(state().Property1).toBe(parallelismQ)
            // no results were dropped
            expect(state().Property1).toBe(index)
            expect(state().Property2).toBe(index2)
            // the initDenyPostback should get through
            expect(state().Property2).toBeGreaterThan(0)
            await initDenyPostback
        }
    ), { timeout: 2000 })
})

test("Run postbacks [Queue + Default | no failures]", async () => {
    jest.setTimeout(120_000);

    // watchEvents()

    await fc.assert(fc.asyncProperty(
        fc.integer(1, 10),
        fc.integer(1, 10),
        fc.scheduler(),
        async (parallelismQ, parallelismD, s) => {
            // wait for stuff to settle down
            // postbacks from the previous run may have been running or so
            await delay(1)
            resetQueues()

            // console.debug("Starting new test ", parallelismQ, parallelismD)
            let index = 0
            let index2 = 0
            fetchJson = async(url: string, init: RequestInit) => {
                const { command } = JSON.parse(init.body as string)
                if (command == "queue") {
                    return makeResponse({
                        Property1: ++index
                    })
                } else if (command == "default") {
                    return makeResponse({
                        Property2: ++index2
                    })
                }
            }

            getStateManager().update(x => ({...x, Property1: 0, Property2: 0}))

            // Queue postbacks can fail in commit when overrun by the Default (basically no concurrency mode)
            const queuePostbacks: Promise<any>[] =
                makeEventRange(parallelismQ, s, "queue postback", i =>
                    postBack(window.document.body, [], "queue", "", undefined, [ "concurrency-queue", cancerPostbackHandler(s, i + "Q") ]).catch(_ => {})
                )

            const defaultPostbacks =
                makeEventRange(parallelismD, s, "default postback", i =>
                    postBack(window.document.body, [], "default", "", undefined, [ "concurrency-default", cancerPostbackHandler(s, i + "D") ]).catch(_ => {})
                )

            // queuePostbacks.map((p, i) => p.catch(err => console.error("Queue postback failed", i, err)))

            await waitForEnd(queuePostbacks.concat(defaultPostbacks), s, () => {
                expect(state().Property1).toBeLessThanOrEqual(index)
                expect(state().Property2).toBeLessThanOrEqual(index2)

                // another postback did not start before committing the original one, oh NO
                // expect(state().Property1).toBeGreaterThanOrEqual(index - 1)
            })

            // console.log(queuePostbacks, defaultPostbacks)

            // all Queue postback got through, oh NO
            // expect(state().Property1).toBe(parallelismQ)
            // no postbacks were dropped
            expect(index).toBe(parallelismQ)
            expect(index2).toBe(parallelismD)

            // try one more without the concurrency
            // that one must get through just fine
            await waitForEnd([postBack(window.document.body, [], "default", "", undefined, [ "concurrency-default" ])], s, () => { })
            expect(index2).toBe(parallelismD + 1)
            expect(state().Property2).toBe(index2)
        }
    ), { timeout: 2000 })
})

test("Postback: AbortSignal", async () => {

    fetchJson = <T>(url: string, init: RequestInit) => {
        return new Promise<T>((resolve, reject) => {
            console.assert(init.signal != null)
            init.signal!.onabort = () => reject(new Error("Request aborted"))
            setTimeout(() => resolve({ viewModelDiff: { Property1: 1 }, action: "successfulCommand", updatedControls: {} } as any), 500)
        })
    }

    const abortController = new AbortController()
    let postbackPromise = postBack(window.document.body, [], "c", "", undefined, [ "validate-root" ], [], abortController.signal)
    await delay(50)
    abortController.abort()

    expect(postbackPromise).rejects.toMatchObject( {reason: {type: "abort"}})
})
