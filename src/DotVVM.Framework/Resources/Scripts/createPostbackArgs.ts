export function createPostbackArgs(options: PostbackOptions) {
    return {
        postbackClientId: options.postbackId,
        viewModel: options.viewModel,
        sender: options.sender,
        postbackOptions: options
    };
}
