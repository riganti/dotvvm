export function initDotvvm(viewModel: any, culture: string = "en-US") {
    const input = window.document.createElement("input")
    input.value = JSON.stringify(viewModel)
    input.id = "__dot_viewmodel_root"
    document.body.appendChild(input)

    dotvvm.init(culture)

}
