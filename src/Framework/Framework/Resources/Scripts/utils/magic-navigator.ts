export function navigate(url: string, downloadName: string | null | undefined = null, target: string | null | undefined = null) {
    const fakeAnchor = <HTMLAnchorElement> document.createElement("a");
    fakeAnchor.style.display = "none";
    document.body.appendChild(fakeAnchor);
    if (downloadName != null) {
        fakeAnchor.download = downloadName
    }
    if (target != null) {
        fakeAnchor.target = target;
    }
    fakeAnchor.rel = "noopener noreferrer";
    fakeAnchor.href = url;
    fakeAnchor.click();
    fakeAnchor.remove();
}
