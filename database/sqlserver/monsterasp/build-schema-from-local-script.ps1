$sourcePath = Join-Path $PSScriptRoot '..\001_schema.sql'
$targetPath = Join-Path $PSScriptRoot '001_schema.sql'

$lines = Get-Content $sourcePath
$filtered = New-Object System.Collections.Generic.List[string]
$skipUntilSetXActAbort = $false

foreach ($line in $lines) {
    if ($line -match 'IF DB_ID\(N''GESTORPROVEEDORES''\)') {
        $skipUntilSetXActAbort = $true
        continue
    }

    if ($skipUntilSetXActAbort) {
        if ($line -eq 'SET XACT_ABORT ON;') {
            $skipUntilSetXActAbort = $false
            [void]$filtered.Add($line)
        }

        continue
    }

    if ($line -match '^/\*' -and $filtered.Count -eq 0) {
        continue
    }

    if ($filtered.Count -eq 0 -and $line -match '^\s*\*/') {
        [void]$filtered.Add('/*')
        [void]$filtered.Add('    GestorProveedores - Esquema para MonsterASP.NET')
        [void]$filtered.Add('    Alineado con la BD real local: (localdb)\MSSQLLocalDB / GESTORPROVEEDORES')
        [void]$filtered.Add('')
        [void]$filtered.Add('    Ejecutar conectado a la base creada en MonsterASP (no master).')
        [void]$filtered.Add('    Alternativa: restaurar GESTORPROVEEDORES_local.bak')
        [void]$filtered.Add('*/')
        continue
    }

    [void]$filtered.Add($line)
}

$content = ($filtered.ToArray() -join [Environment]::NewLine)
$content = $content.Replace("N'superusuario', N'solicitante'", "N'superusuario', N'administrador', N'solicitante'")
$content = $content.Replace("N'superusuario', N'auxiliar'", "N'superusuario', N'administrador', N'auxiliar'")

Set-Content -Path $targetPath -Value $content -Encoding UTF8
Write-Host "Generado: $targetPath"
