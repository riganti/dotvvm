#!/usr/bin/env powershell

dotnet run --project ../DotVVM.CommandLine/ -- api regen ./GithubApiClient.cs

tsc ./Scripts/GithubApiClient.ts

git checkout ./GithubApiClient.cs # the generation of C# is broken for some reason -> revert it
