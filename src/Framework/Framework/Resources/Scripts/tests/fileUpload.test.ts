import { uploadFiles } from "../controls/fileUpload";
import { getCsrfToken } from "../postback/http";
import { updateTypeInfo } from "../metadata/typeMap";

jest.mock("../postback/http", () => ({ getCsrfToken: jest.fn(() => Promise.resolve("csrf")) }));
jest.mock("../metadata/typeMap", () => ({ updateTypeInfo: jest.fn() }));

type Response = { result?: unknown, token?: string, status?: number, body?: string, networkError?: boolean };
let responses: Response[];
let requests: MockXHR[];

class MockXHR {
    headers: Record<string, string> = {};
    upload = { onprogress: null as any };
    onload: () => void = () => {};
    onerror: () => void = () => {};
    onabort: () => void = () => {};
    status = 200;
    responseText = "";
    formData!: FormData;
    response!: Response;
    open = jest.fn();
    setRequestHeader(name: string, value: string) { this.headers[name] = value; }
    getResponseHeader() { return this.response.token ?? null; }
    send(formData: FormData) {
        this.formData = formData;
        requests.push(this);
        this.response = responses.shift()!;
        this.status = this.response.status ?? 200;
        this.responseText = this.response.body ?? JSON.stringify({ result: this.response.result, typeMetadata: {} });
        this.upload.onprogress({ lengthComputable: true, loaded: 1, total: 1 });
        if (this.response.networkError) this.onerror();
        else this.onload();
    }
}

beforeEach(() => {
    jest.clearAllMocks();
    responses = [];
    requests = [];
    Object.defineProperty(window, "XMLHttpRequest", { configurable: true, value: MockXHR });
});

function upload(files: File[], chunkSize = 2, allowMultiple = true) {
    const state = { IsBusy: false, Progress: 0, Error: null, Files: [{ FileName: "existing.txt" }] };
    const collection = { state, patchState: jest.fn(patch => Object.assign(state, patch)) };
    const completed = jest.fn();
    const promise = uploadFiles(collection as any, "initial", allowMultiple, "/upload", files as any, completed, chunkSize);
    return { promise, state, collection, completed };
}

test("sends sequential chunks with fresh tokens, filename and MIME type", async () => {
    responses = [{ result: [], token: "second" }, { result: [], token: "third" }, { result: [{ FileName: "file.txt" }] }];
    const { promise, state, completed, collection } = upload([new File(["abcde"], "file.txt", { type: "text/plain" })]);
    await promise;
    expect(requests.map(r => r.headers["X-DotVVM-UploadToken"])).toEqual(["initial", "second", "third"]);
    expect(requests.every(r => !("X-DotVVM-UploadFinalChunk" in r.headers))).toBe(true);
    expect(requests.map(r => r.headers["X-DotVVM-UploadFileSize"])).toEqual(["5", undefined, undefined]);
    expect(requests.map(r => (r.formData.get("upload") as File).size)).toEqual([2, 2, 1]);
    for (const request of requests) {
        expect(request.headers["X-DotVVM-UploadChunk"]).toBe("true");
        expect(request.headers["X-DotVVM-CsrfToken"]).toBe("csrf");
        expect(request.formData.get("upload")).toMatchObject({ name: "file.txt", type: "text/plain" });
    }
    expect(state).toMatchObject({ IsBusy: false, Progress: 100, Error: null, Files: [{ FileName: "existing.txt" }, { FileName: "file.txt" }] });
    expect(completed).toHaveBeenCalledTimes(1);
    expect(updateTypeInfo).toHaveBeenCalledTimes(3);
    const progress = collection.patchState.mock.calls.map(([patch]) => patch.Progress);
    expect(progress).toEqual([0, 40, 80, 99, 100, 100]);
});

test("starts each file with the initial token and finishes exact-size and empty files", async () => {
    responses = [{ result: [{ FileName: "first" }] }, { result: [{ FileName: "empty" }] }];
    const { promise, state, completed } = upload([new File(["ab"], "first"), new File([], "empty")], 2, false);
    await promise;
    expect(requests.map(r => r.headers["X-DotVVM-UploadToken"])).toEqual(["initial", "initial"]);
    expect(requests.every(r => !("X-DotVVM-UploadFinalChunk" in r.headers))).toBe(true);
    expect(requests.map(r => r.headers["X-DotVVM-UploadFileSize"])).toEqual(["2", "0"]);
    expect(requests.map(r => (r.formData.get("upload") as File).size)).toEqual([2, 0]);
    expect(state.Files).toEqual([{ FileName: "first" }, { FileName: "empty" }]);
    expect(completed).toHaveBeenCalledTimes(1);
});

test.each([
    [{ result: [] }, "Upload failed."],
    [{ status: 413 }, "Uploaded file is too large."],
    [{ status: 500 }, "Upload failed."],
    [{ body: "invalid JSON" }, "Upload failed."],
    [{ networkError: true }, "Upload failed."]
])("stops on a failed chunk without completing or publishing partial files", async (response, error) => {
    responses = [{ result: [], token: "next" }, response];
    const { promise, state, completed } = upload([new File(["abcde"], "file.txt")]);
    await promise;
    expect(requests).toHaveLength(2);
    expect(state).toMatchObject({ IsBusy: false, Error: error, Files: [{ FileName: "existing.txt" }] });
    expect(completed).not.toHaveBeenCalled();
});

test("falls back to whole-file multipart uploads when chunking is disabled", async () => {
    responses = [{ result: [{ FileName: "a" }, { FileName: "b" }] }];
    const { promise, completed } = upload([new File(["a"], "a"), new File(["b"], "b")], 0);
    await promise;
    expect(requests).toHaveLength(1);
    expect(requests[0].headers["X-DotVVM-UploadChunk"]).toBeUndefined();
    expect(requests[0].headers["X-DotVVM-UploadFileSize"]).toBeUndefined();
    expect(requests[0].formData.getAll("upload[]")).toHaveLength(2);
    expect(completed).toHaveBeenCalledTimes(1);
});

test("sets busy state before waiting for CSRF and ignores overlapping uploads", async () => {
    let resolveCsrf!: (token: string) => void;
    (getCsrfToken as jest.Mock).mockImplementationOnce(() => new Promise(resolve => { resolveCsrf = resolve; }));
    responses = [{ result: [{ FileName: "a" }] }];
    const { promise, state, collection, completed } = upload([new File(["a"], "a")]);
    expect(state.IsBusy).toBe(true);
    await uploadFiles(collection as any, "initial", true, "/upload", [new File(["b"], "b")] as any, completed, 2);
    resolveCsrf("csrf");
    await promise;
    expect(requests).toHaveLength(1);
});

test.each([
    [new File(["abc"], "file"), { result: [{ FileName: "partial" }], token: "next" }],
    [new File(["a"], "file"), { result: [] }],
    [new File(["a"], "file"), { result: {} }]
])("rejects invalid chunk results without publishing them", async (file, response) => {
    responses = [response];
    const { promise, state, completed } = upload([file]);
    await promise;
    expect(state).toMatchObject({ IsBusy: false, Error: "Upload failed.", Files: [{ FileName: "existing.txt" }] });
    expect(completed).not.toHaveBeenCalled();
});

test("does not upload an empty selection", async () => {
    const { promise, state, completed } = upload([]);
    await promise;
    expect(requests).toHaveLength(0);
    expect(state.IsBusy).toBe(false);
    expect(completed).not.toHaveBeenCalled();
});
