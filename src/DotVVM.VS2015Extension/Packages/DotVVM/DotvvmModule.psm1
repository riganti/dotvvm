function Generate-DotvvmSecurityKeys()
{
    #generate random security keys
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $data1 = new-object System.Byte[] 32
    $rng.GetBytes($data1)
    $data2 = new-object System.Byte[] 32
    $rng.GetBytes($data2)
 
    #build dotvvm.json
    return "{`r`n    ""`$schema"": ""http://www.riganti.cz/download/dotvvm.schema.0.6.json"",`r`n`r`n    // Do not reveal the following keys and do not copy them into another projects. It can make your app vulnerable.`r`n    // If you need to generate another keys, execute the command Generate-DotvvmSecurityKeys in the Package Manager Console window in Visual Studio.`r`n    ""security"": {`r`n        ""encryptionKey"": """ + [System.Convert]::ToBase64String($data1) + """,`r`n        ""signingKey"": """ + [System.Convert]::ToBase64String($data2) + """`r`n    }`r`n}"
}