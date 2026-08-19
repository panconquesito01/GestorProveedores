$ErrorActionPreference = 'Stop'

$settingsPath = Join-Path $PSScriptRoot '..\..\gestorproveedoresauro.runasp.net-WebDeploy.publishSettings'
[xml]$publishData = Get-Content $settingsPath
$password = $publishData.publishData.publishProfile.userPWD

$projectPath = Join-Path $PSScriptRoot '..\..\GestorProveedores.WebApp\GestorProveedores.WebApp.csproj'

dotnet publish $projectPath `
    -c Release `
    /p:PublishProfile=MonsterASP `
    "/p:Password=$password" `
    /p:EnvironmentName=Production
