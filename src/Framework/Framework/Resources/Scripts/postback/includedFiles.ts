import * as magicNavigator from '../utils/magic-navigator'

const includedReturnedFilesPropertyName = "_dotvvm_IncludedReturnedFiles";

type IncludedReturnedFile = {
    url: string,
    download?: string | null
}

export function handleIncludedReturnedFiles(response: any) {
    const files = response?.customProperties?._dotvvm_IncludedReturnedFiles as IncludedReturnedFile[] | null | undefined;
    for (const file of files ?? []) {
        magicNavigator.navigate(file.url, file.download, file.download == null ? "_blank" : null);
    }
}
