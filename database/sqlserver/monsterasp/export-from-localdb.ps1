param(
    [string]$ServerName = '(localdb)\MSSQLLocalDB',
    [string]$DatabaseName = 'GESTORPROVEEDORES',
    [string]$OutputDirectory = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName 'Microsoft.SqlServer.Smo'
Add-Type -AssemblyName 'Microsoft.SqlServer.ConnectionInfo'

$schemaFile = Join-Path $OutputDirectory '001_schema_from_localdb.sql'
$dataFile = Join-Path $OutputDirectory '002_data_from_localdb.sql'

$server = New-Object Microsoft.SqlServer.Management.Smo.Server $ServerName
$database = $server.Databases[$DatabaseName]

if ($null -eq $database) {
    throw "No se encontro la base de datos '$DatabaseName' en '$ServerName'."
}

function Write-SqlScript {
    param(
        [Microsoft.SqlServer.Management.Smo.Database]$Db,
        [Microsoft.SqlServer.Management.Smo.Server]$Srv,
        [string]$Path,
        [bool]$IncludeData
    )

    $scripter = New-Object Microsoft.SqlServer.Management.Smo.Scripter $Srv
    $scripter.Options.ScriptSchema = -not $IncludeData
    $scripter.Options.ScriptData = $IncludeData
    $scripter.Options.ScriptDrops = $false
    $scripter.Options.IncludeHeaders = $true
    $scripter.Options.Indexes = $true
    $scripter.Options.ClusteredIndexes = $true
    $scripter.Options.NonClusteredIndexes = $true
    $scripter.Options.DriAll = $true
    $scripter.Options.Triggers = $true
    $scripter.Options.SchemaQualify = $true
    $scripter.Options.ToFileOnly = $true
    $scripter.Options.FileName = $Path
    $scripter.Options.AppendToFile = $false
    $scripter.Options.Encoding = [System.Text.Encoding]::UTF8

    $objects = New-Object System.Collections.Generic.List[object]
    $objects.AddRange(@($Db.Sequences | Where-Object { $_.Schema -eq 'dbo' }))

    foreach ($tableName in @(
            'Empresas',
            'Usuarios',
            'AsignacionContadores',
            'Solicitudes',
            'ProveedoresCandidatos',
            'Documentos',
            'SolicitudHistorial')) {
        $table = $Db.Tables[$tableName, 'dbo']
        if ($null -ne $table) {
            [void]$objects.Add($table)
        }
    }

    [void]$scripter.Script($objects.ToArray())
}

Write-SqlScript -Db $database -Srv $server -Path $schemaFile -IncludeData $false
Write-SqlScript -Db $database -Srv $server -Path $dataFile -IncludeData $true

Write-Host "Esquema: $schemaFile"
Write-Host "Datos:   $dataFile"
