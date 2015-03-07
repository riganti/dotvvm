function Generate-RedwoodSecurityKeys()
{
    #generate random security keys
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $data1 = new-object System.Byte[] 32
    $rng.GetBytes($data1)
    $data2 = new-object System.Byte[] 32
    $rng.GetBytes($data2)
 
    #build redwood.json
    return "{`r`n    // Do not reveal these keys and do not copy them into another projects. It can make your app vulnerable.`r`n    // If you need to generate another keys, execute the command Generate-RedwoodSecurityKeys in the Package Manager Console window in Visual Studio.`r`n    ""security"": {`r`n        ""encryptionKey"": """ + [System.Convert]::ToBase64String($data1) + """,`r`n        ""signingKey"": """ + [System.Convert]::ToBase64String($data2) + """`r`n    }`r`n}"
}