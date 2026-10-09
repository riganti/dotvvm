import bindingHandlers from "../binding-handlers/file-upload-paste-drop";
import { uploadFiles } from "../controls/fileUpload";

jest.mock("../controls/fileUpload", () => ({ uploadFiles: jest.fn() }));

beforeEach(() => jest.clearAllMocks());

function dispatchFiles(eventType: string, files: File[], options: any = {}) {
    const element = document.createElement("textarea");
    const collection = options.collection ?? { patchState: jest.fn() };
    bindingHandlers["dotvvm-FileUpload-UploadOnPasteOrDrop"].init(element, () => ({
        collection, url: "/upload", token: "token", multiple: true, ...options
    }));
    const event = new Event(eventType, { cancelable: true });
    Object.defineProperty(event, eventType === "paste" ? "clipboardData" : "dataTransfer", { value: { files } });
    element.dispatchEvent(event);
    return collection;
}

describe.each(["paste", "drop"])("%s file validation", eventType => {
    test("rejects oversized files before uploading any of the batch", () => {
        const collection = dispatchFiles(eventType, [
            new File(["ok"], "small.txt"),
            new File([new Uint8Array(1024 * 1024 + 1)], "large.txt")
        ], { maxFileSize: 1 });
        expect(uploadFiles).not.toHaveBeenCalled();
        expect(collection.patchState).toHaveBeenCalledWith({
            Error: "Uploaded file is too large."
        });
    });

    test.each([
        [".txt", "file.md", "text/plain"],
        ["image/*", "file.txt", "text/plain"],
        ["text/plain", "file.txt", ""]
    ])("rejects files not matching %s", (allowedFileTypes, name, type) => {
        const collection = dispatchFiles(eventType, [new File(["file"], name, { type })], { allowedFileTypes });
        expect(uploadFiles).not.toHaveBeenCalled();
        expect(collection.patchState).toHaveBeenCalledWith({
            Error: "Uploaded file type is not allowed."
        });
    });

    test.each([
        [" .png, .TXT ", "file.txt", ""],
        ["text/plain", "file.txt", "text/plain"],
        ["image/*", "file.png", "image/png"],
        [null, "file.bin", ""],
        ["   ", "file.bin", ""]
    ])("allows files matching %s at the size limit", (allowedFileTypes, name, type) => {
        const files = [new File([new Uint8Array(1024 * 1024)], name, { type })];
        const collection = dispatchFiles(eventType, files, { allowedFileTypes, maxFileSize: 1 });
        expect(uploadFiles).toHaveBeenCalledWith(collection, "token", true, "/upload", files, expect.any(Function));
        expect(collection.patchState).not.toHaveBeenCalled();
    });

    test("allows unrestricted file sizes", () => {
        dispatchFiles(eventType, [new File([new Uint8Array(1024 * 1024 + 1)], "file.bin")]);
        expect(uploadFiles).toHaveBeenCalledTimes(1);
    });

    test("treats a zero size limit as a limit", () => {
        dispatchFiles(eventType, [new File(["file"], "file.txt")], { maxFileSize: 0 });
        expect(uploadFiles).not.toHaveBeenCalled();
    });

    test("preserves an ongoing upload and existing files when rejecting a file", () => {
        const state = { IsBusy: true, Progress: 50, Files: [{ FileName: "existing.txt" }], Error: null };
        const collection = { patchState: jest.fn(patch => Object.assign(state, patch)) };
        dispatchFiles(eventType, [new File(["file"], "file.md")], { collection, allowedFileTypes: ".txt" });
        expect(uploadFiles).not.toHaveBeenCalled();
        expect(state).toEqual({
            IsBusy: true, Progress: 50, Files: [{ FileName: "existing.txt" }],
            Error: "Uploaded file type is not allowed."
        });
    });
});
