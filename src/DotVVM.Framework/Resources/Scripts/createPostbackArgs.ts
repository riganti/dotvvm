export function createPostbackArgs(options: PostbackOptions) {
    return {
        postbackClientId: options.postbackId,
        viewModelName: options.viewModelName || "root",
        viewModel: options.viewModel,
        sender: options.sender,
        postbackOptions: options
    };
}
