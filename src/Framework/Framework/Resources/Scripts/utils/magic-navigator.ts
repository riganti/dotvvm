let fakeAnchor: HTMLAnchorElement | undefined;
export function navigate(url: string, downloadName: string | null | undefined = null, target: string | null | undefined = null) {
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
    if (target == null) {
        fakeAnchor.removeAttribute("target");
        fakeAnchor.removeAttribute("rel");
    } else {
        fakeAnchor.target = target;
        fakeAnchor.rel = "noopener";
    }
    fakeAnchor.href = url;
    fakeAnchor.click();
}
