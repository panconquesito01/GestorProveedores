param(
    [string]$ServerName = '(localdb)\MSSQLLocalDB',
    [string]$DatabaseName = 'GESTORPROVEEDORES',
    [string]$OutputFile = (Join-Path $PSScriptRoot '002_data_from_localdb.sql')
)

$ErrorActionPreference = 'Stop'

$connectionString = "Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True;"

$tablesInOrder = @(
    'Empresas',
    'Usuarios',
    'AsignacionContadores',
    'Solicitudes',
    'ProveedoresCandidatos',
    'Documentos',
    'SolicitudHistorial'
)

function Format-SqlValue {
    param([object]$Value, [string]$DataType)

    if ($null -eq $Value -or [DBNull]::Value.Equals($Value)) {
        return 'NULL'
    }

    switch -Regex ($DataType) {
        '^(bit)$' { if ([bool]$Value) { return '1' } else { return '0' } }
        '^(tinyint|smallint|int|bigint|decimal|numeric|float|real|money|smallmoney)$' { return $Value.ToString() }
        '^(binary|varbinary|image|timestamp|rowversion)$' {
            $bytes = [byte[]]$Value
            return '0x' + ([BitConverter]::ToString($bytes) -replace '-', '')
        }
        '^(datetime|datetime2|smalldatetime|date|time|datetimeoffset)$' {
            if ($Value -is [datetimeoffset]) {
                return "N'$($Value.ToString('yyyy-MM-dd HH:mm:ss.fffffff zzz').Replace("'", "''"))'"
            }

            return "N'$($Value.ToString('yyyy-MM-dd HH:mm:ss.fff').Replace("'", "''"))'"
        }
        default {
            return "N'$($Value.ToString().Replace("'", "''"))'"
        }
    }
}

$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()

$writer = New-Object System.IO.StreamWriter($OutputFile, $false, [System.Text.UTF8Encoding]::new($false))
try {
    $writer.WriteLine('/*')
    $writer.WriteLine("    Datos exportados desde $ServerName / $DatabaseName")
    $writer.WriteLine('    Ejecutar despues de 001_schema_from_localdb.sql o tras restaurar el .bak')
    $writer.WriteLine('*/')
    $writer.WriteLine('')
    $writer.WriteLine('SET NOCOUNT ON;')
    $writer.WriteLine('SET XACT_ABORT ON;')
    $writer.WriteLine('GO')
    $writer.WriteLine('')
    $writer.WriteLine('BEGIN TRANSACTION;')
    $writer.WriteLine('GO')
    $writer.WriteLine('')

    foreach ($table in $tablesInOrder) {
        $columnsQuery = @"
SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_identity,
    c.column_id
FROM sys.columns c
INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID(N'dbo.$table')
ORDER BY c.column_id;
"@

        $columnsCommand = $connection.CreateCommand()
        $columnsCommand.CommandText = $columnsQuery
        $columnsReader = $columnsCommand.ExecuteReader()

        $columns = @()
        $identityColumns = @()
        while ($columnsReader.Read()) {
            $dataType = [string]$columnsReader['DataType']
            if ($dataType -in @('timestamp', 'rowversion')) {
                continue
            }

            $column = [PSCustomObject]@{
                Name = $columnsReader['ColumnName']
                DataType = $dataType
                IsIdentity = [bool]$columnsReader['is_identity']
            }
            $columns += $column
            if ($column.IsIdentity) {
                $identityColumns += $column.Name
            }
        }
        $columnsReader.Close()

        if ($columns.Count -eq 0) {
            continue
        }

        $selectColumns = ($columns | ForEach-Object { "[$($_.Name)]" }) -join ', '
        $dataCommand = $connection.CreateCommand()
        $dataCommand.CommandText = "SELECT $selectColumns FROM dbo.[$table];"
        $dataReader = $dataCommand.ExecuteReader()

        $writer.WriteLine("-- Tabla dbo.$table")
        if ($identityColumns.Count -gt 0) {
            $writer.WriteLine("SET IDENTITY_INSERT dbo.[$table] ON;")
            $writer.WriteLine('GO')
        }

        $rowCount = 0
        while ($dataReader.Read()) {
            $values = @()
            for ($i = 0; $i -lt $columns.Count; $i++) {
                $values += (Format-SqlValue -Value $dataReader.GetValue($i) -DataType $columns[$i].DataType)
            }

            $insertColumns = ($columns | ForEach-Object { "[$($_.Name)]" }) -join ', '
            $insertValues = $values -join ', '
            $writer.WriteLine("INSERT INTO dbo.[$table] ($insertColumns) VALUES ($insertValues);")
            $rowCount++
        }

        $dataReader.Close()

        if ($identityColumns.Count -gt 0) {
            $writer.WriteLine('GO')
            $writer.WriteLine("SET IDENTITY_INSERT dbo.[$table] OFF;")
        }

        $writer.WriteLine("GO")
        $writer.WriteLine("-- Filas exportadas: $rowCount")
        $writer.WriteLine('GO')
        $writer.WriteLine('')
    }

    $sequenceCommand = $connection.CreateCommand()
    $sequenceCommand.CommandText = "SELECT CAST(current_value AS INT) FROM sys.sequences WHERE name = 'RadicadoSeq';"
    $currentValue = $sequenceCommand.ExecuteScalar()
    if ($null -ne $currentValue) {
        $writer.WriteLine("ALTER SEQUENCE dbo.RadicadoSeq RESTART WITH $currentValue;")
        $writer.WriteLine('GO')
    }

    $writer.WriteLine('')
    $writer.WriteLine('COMMIT TRANSACTION;')
    $writer.WriteLine('GO')
}
finally {
    $writer.Close()
    $connection.Close()
}

Write-Host "Datos exportados a $OutputFile"
