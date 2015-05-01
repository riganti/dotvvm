open canopy
open runner
open types
open System
  
let redwoodPath = "http://localhost:8628"


"Redirect Sample (10)" &&& fun _ ->
    url (redwoodPath + "/Sample10")
    let button = "input[type='button']"

    on "/Sample10?time="
    let url1 = currentUrl()

    click button
    on "/Sample10?time="
    if url1 = currentUrl() then raise (CanopyException("not redirected"))

"Validation Sample (11)" &&& fun _ ->
    url (redwoodPath + "/Sample11")
    let button = "input[type='button']"
    let input = "input[type='text']"
    let checkNotValid() =
        displayed "#hideWhenValid"
        displayed "#addCssClass.validator"
        displayed "#displayErrorMessage"
        displayed "#validationSummary li"

    click button
    checkNotValid()
    input << "not valid email"
    click button
    checkNotValid()
    input << "email@addre.ss"
    click button
    count "table.table tr" 4

  
start firefox
run()
quit()
