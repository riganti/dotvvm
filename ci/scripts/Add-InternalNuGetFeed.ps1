param(
    [string][parameter(Mandatory = $true)]$internalFeed,
    [string][parameter(Mandatory = $true)]$internalFeedUser,
    [string][parameter(Mandatory = $true)]$internalFeedPat,
    [string]$internalFeedName = "riganti"
)

nuget sources add `
    -Usename "$internalFeedUser" `
    -Password "$internalFeedPat" `
    -StorePasswordInClearText `
    -Name "$internalFeedName" `
    -Source "$internalFeed"
