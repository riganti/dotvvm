param(
    [string][parameter(Mandatory = $true)]$internalFeed,
    [string][parameter(Mandatory = $true)]$internalFeedUser,
    [string][parameter(Mandatory = $true)]$internalFeedPat,
    [string]$internalFeedName = "riganti"
)

dotnet nuget add source `
    --username "$internalFeedUser" `
    --password "$internalFeedPat" `
    --store-password-in-clear-text `
    --name "$internalFeedName" `
    "$internalFeed"
