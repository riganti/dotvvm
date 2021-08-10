let fakeAnchor: HTMLAnchorElement | undefined;
export function navigate(url: string) {
    if (!fakeAnchor) {
        fakeAnchor = <HTMLAnchorElement> document.createElement("a");
        fakeAnchor.style.display = "none";
        document.body.appendChild(fakeAnchor);
    }
    fakeAnchor.href = url;
    fakeAnchor.click();
}
