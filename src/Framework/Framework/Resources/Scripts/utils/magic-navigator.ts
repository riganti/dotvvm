let fakeAnchor: HTMLAnchorElement | undefined;
export function navigate(url: string, downloadName: string | null | undefined = null) {
    if (!fakeAnchor) {
        fakeAnchor = <HTMLAnchorElement> document.createElement("a");
        fakeAnchor.style.display = "none";
        document.body.appendChild(fakeAnchor);
    }
    if (downloadName == null) {
        fakeAnchor.removeAttribute("download");
    } else {
        fakeAnchor.download = downloadName
    }
    fakeAnchor.href = url;
    fakeAnchor.click();
}
