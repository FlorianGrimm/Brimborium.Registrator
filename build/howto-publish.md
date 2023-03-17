# Publish to github

# Once per computer
https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry

```PowerShell
mkdir "${Env:\USERPROFILE}\Documents\secure"
notepad "${Env:\USERPROFILE}\Documents\secure\github.json"
```

```PowerShell
$json = Get-Content "${Env:\USERPROFILE}\Documents\secure\github.json" | ConvertFrom-Json

$NAMESPACE=$json.NAMESPACE
$USERNAME=$json.USERNAME
$GITHUB_TOKEN=$json.GITHUB_TOKEN

dotnet nuget add source --username $USERNAME --password $GITHUB_TOKEN --store-password-in-clear-text --name github "https://nuget.pkg.github.com/$NAMESPACE/index.json"
```


# Publish

nbgv tag 1.1.1
nbgv set-version 1.1.1

```PowerShell

$json = Get-Content "${Env:\USERPROFILE}\Documents\secure\github.json" | ConvertFrom-Json

$NAMESPACE=$json.NAMESPACE
$USERNAME=$json.USERNAME
$GITHUB_TOKEN=$json.GITHUB_TOKEN


dir Brimborium.Registrator.Abstractions\nupkg\*.nupkg | Remove-Item
dir Brimborium.Registrator\nupkg\*.nupkg | Remove-Item

dotnet pack --configuration Release /p:PublicRelease=true

dir Brimborium.Registrator.Abstractions\nupkg\*.nupkg
dir Brimborium.Registrator\nupkg\*.nupkg

$nupkg = dir Brimborium.Registrator.Abstractions\nupkg\*.nupkg | %{$_.FullName}
dotnet nuget push $nupkg --source "github" --api-key $GITHUB_TOKEN

$nupkg = dir Brimborium.Registrator\nupkg\*.nupkg | %{$_.FullName}
dotnet nuget push $nupkg --source "github" --api-key $GITHUB_TOKEN

```


nbgv set-version prepare-release 1.1.1

nbgv set-version 1.1.1

dotnet build --configuration Release /p:PublicRelease=true

nuget
https://www.nuget.org/packages/manage/upload
nuget SetApiKey x
dotnet nuget push Brimborium.Registrator.1.1.1.nupkg --source https://api.nuget.org/v3/index.json
dotnet nuget push Brimborium.Registrator.1.1.1.snupkg --source https://api.nuget.org/v3/index.json