dotnet ..\..\Tools\CommandLine\bin\Debug\netcoreapp3.1\dotnet-dotvvm.dll api regen http://localhost:50001/swagger/v1/swagger.json
tsc .\Scripts\TestWebApiClientAspNetCore.ts

dotnet ..\..\Tools\CommandLine\bin\Debug\netcoreapp3.1\dotnet-dotvvm.dll api regen http://localhost:61453/swagger/v1/swagger.json
tsc .\Scripts\TestWebApiClientOwin.ts
