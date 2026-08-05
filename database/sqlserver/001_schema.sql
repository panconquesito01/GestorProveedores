/*
    GestorProveedores - Esquema inicial SQL Server
    Fuente: C:\Users\david.rivera\Downloads\GestorProveedores-main\GestorProveedores-main\db\schema.sql
    Base de datos destino local: GESTORPROVEEDORES en (localdb)\MSSQLLocalDB

    Regla de migracion:
    - No generar ni ejecutar migraciones EF Core.
    - El esquema se controla con scripts T-SQL manuales, revisables y versionados.
*/

IF DB_ID(N'GESTORPROVEEDORES') IS NULL
BEGIN
    THROW 50000, 'La base de datos GESTORPROVEEDORES no existe en la instancia local de SQL Server.', 1;
END;
GO

USE [GESTORPROVEEDORES];
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'RadicadoSeq' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    EXEC(N'CREATE SEQUENCE dbo.RadicadoSeq AS INT START WITH 1 INCREMENT BY 1;');
END;
GO

IF OBJECT_ID(N'dbo.Empresas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Empresas
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Empresas PRIMARY KEY,
        Nombre NVARCHAR(250) NOT NULL,
        Nit NVARCHAR(50) NOT NULL,
        CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Empresas_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Empresas_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Empresas_Nit UNIQUE (Nit)
    );
END;
GO

IF COL_LENGTH(N'dbo.Empresas', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.Empresas ADD UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Empresas_UpdatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Nombre NVARCHAR(250) NOT NULL,
        Email NVARCHAR(320) NOT NULL,
        Username NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        Rol NVARCHAR(30) NOT NULL,
        EmpresaId INT NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT 1,
        CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Usuarios_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Usuarios_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
        CONSTRAINT UQ_Usuarios_Username UNIQUE (Username),
        CONSTRAINT FK_Usuarios_Empresas FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresas(Id),
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'solicitante', N'auxiliar', N'analista', N'aprobador', N'contable')),
        CONSTRAINT CK_Usuarios_EmpresaSegunRol CHECK
        (
            (Rol IN (N'solicitante', N'aprobador') AND EmpresaId IS NOT NULL)
            OR (Rol IN (N'auxiliar', N'analista', N'contable') AND EmpresaId IS NULL)
        )
    );
END;
GO

IF COL_LENGTH(N'dbo.Usuarios', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.Usuarios ADD UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Usuarios_UpdatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuarios_Rol' AND object_id = OBJECT_ID(N'dbo.Usuarios'))
    CREATE INDEX IX_Usuarios_Rol ON dbo.Usuarios(Rol);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuarios_EmpresaId' AND object_id = OBJECT_ID(N'dbo.Usuarios'))
    CREATE INDEX IX_Usuarios_EmpresaId ON dbo.Usuarios(EmpresaId);
GO

IF OBJECT_ID(N'dbo.AsignacionContadores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AsignacionContadores
    (
        Rol NVARCHAR(30) NOT NULL CONSTRAINT PK_AsignacionContadores PRIMARY KEY,
        UltimoIndice INT NOT NULL CONSTRAINT DF_AsignacionContadores_UltimoIndice DEFAULT -1,
        CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_AsignacionContadores_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_AsignacionContadores_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT CK_AsignacionContadores_Rol CHECK (Rol IN (N'auxiliar', N'analista'))
    );

    INSERT INTO dbo.AsignacionContadores (Rol, UltimoIndice)
    VALUES (N'auxiliar', -1), (N'analista', -1);
END;
GO

IF COL_LENGTH(N'dbo.AsignacionContadores', N'CreatedAt') IS NULL
    ALTER TABLE dbo.AsignacionContadores ADD CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_AsignacionContadores_CreatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
GO

IF COL_LENGTH(N'dbo.AsignacionContadores', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.AsignacionContadores ADD UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_AsignacionContadores_UpdatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
GO

IF OBJECT_ID(N'dbo.Solicitudes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Solicitudes
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Solicitudes PRIMARY KEY,
        Radicado NVARCHAR(50) NOT NULL,
        Titulo NVARCHAR(250) NOT NULL,
        Descripcion NVARCHAR(MAX) NOT NULL,
        Frecuencia NVARCHAR(100) NULL,
        SolicitanteId INT NOT NULL,
        EmpresaId INT NOT NULL,
        AprobadorId INT NULL,
        RequiereAprobacion BIT NOT NULL CONSTRAINT DF_Solicitudes_RequiereAprobacion DEFAULT 0,
        AuxiliarId INT NULL,
        AnalistaId INT NULL,
        ProveedorOrigen NVARCHAR(30) NULL,
        Etapa NVARCHAR(50) NOT NULL CONSTRAINT DF_Solicitudes_Etapa DEFAULT N'revision_auxiliar',
        Estado NVARCHAR(30) NOT NULL CONSTRAINT DF_Solicitudes_Estado DEFAULT N'en_proceso',
        CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Solicitudes_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Solicitudes_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT UQ_Solicitudes_Radicado UNIQUE (Radicado),
        CONSTRAINT FK_Solicitudes_Solicitante FOREIGN KEY (SolicitanteId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT FK_Solicitudes_Empresa FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresas(Id),
        CONSTRAINT FK_Solicitudes_Aprobador FOREIGN KEY (AprobadorId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT FK_Solicitudes_Auxiliar FOREIGN KEY (AuxiliarId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT FK_Solicitudes_Analista FOREIGN KEY (AnalistaId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_Solicitudes_ProveedorOrigen CHECK (ProveedorOrigen IS NULL OR ProveedorOrigen IN (N'erp_existente', N'nuevo')),
        CONSTRAINT CK_Solicitudes_Estado CHECK (Estado IN (N'en_proceso', N'devuelta', N'completada')),
        CONSTRAINT CK_Solicitudes_Etapa CHECK
        (
            Etapa IN
            (
                N'revision_auxiliar',
                N'devuelta_solicitante',
                N'revision_proveedores',
                N'seleccion_proveedor',
                N'carga_orden_compra',
                N'revision_oc_solicitante',
                N'oc_devuelta_auxiliar',
                N'revision_oc_aprobador',
                N'envio_proveedor',
                N'revision_anomalias',
                N'revision_factura_solicitante',
                N'factura_devuelta_analista',
                N'factura_aprobada_auxiliar',
                N'validacion_contable',
                N'factura_objetada_contable',
                N'completada'
            )
        )
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Solicitudes_Etapa' AND object_id = OBJECT_ID(N'dbo.Solicitudes'))
    CREATE INDEX IX_Solicitudes_Etapa ON dbo.Solicitudes(Etapa);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Solicitudes_EmpresaId' AND object_id = OBJECT_ID(N'dbo.Solicitudes'))
    CREATE INDEX IX_Solicitudes_EmpresaId ON dbo.Solicitudes(EmpresaId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Solicitudes_SolicitanteId' AND object_id = OBJECT_ID(N'dbo.Solicitudes'))
    CREATE INDEX IX_Solicitudes_SolicitanteId ON dbo.Solicitudes(SolicitanteId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Solicitudes_AuxiliarId' AND object_id = OBJECT_ID(N'dbo.Solicitudes'))
    CREATE INDEX IX_Solicitudes_AuxiliarId ON dbo.Solicitudes(AuxiliarId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Solicitudes_AnalistaId' AND object_id = OBJECT_ID(N'dbo.Solicitudes'))
    CREATE INDEX IX_Solicitudes_AnalistaId ON dbo.Solicitudes(AnalistaId);
GO

IF OBJECT_ID(N'dbo.ProveedoresCandidatos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProveedoresCandidatos
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProveedoresCandidatos PRIMARY KEY,
        SolicitudId INT NOT NULL,
        Orden SMALLINT NOT NULL CONSTRAINT DF_ProveedoresCandidatos_Orden DEFAULT 1,
        Origen NVARCHAR(30) NOT NULL,
        Nombre NVARCHAR(250) NOT NULL,
        Nit NVARCHAR(50) NULL,
        IdentificadorErp NVARCHAR(100) NULL,
        CorreoContacto NVARCHAR(320) NULL,
        TelefonoContacto NVARCHAR(50) NULL,
        Validado BIT NOT NULL CONSTRAINT DF_ProveedoresCandidatos_Validado DEFAULT 0,
        CreadoEnErp BIT NOT NULL CONSTRAINT DF_ProveedoresCandidatos_CreadoEnErp DEFAULT 0,
        Seleccionado BIT NOT NULL CONSTRAINT DF_ProveedoresCandidatos_Seleccionado DEFAULT 0,
        CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_ProveedoresCandidatos_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_ProveedoresCandidatos_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_ProveedoresCandidatos_Solicitudes FOREIGN KEY (SolicitudId) REFERENCES dbo.Solicitudes(Id) ON DELETE CASCADE,
        CONSTRAINT CK_ProveedoresCandidatos_Origen CHECK (Origen IN (N'erp_existente', N'nuevo'))
    );
END;
GO

IF COL_LENGTH(N'dbo.ProveedoresCandidatos', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.ProveedoresCandidatos ADD UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_ProveedoresCandidatos_UpdatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProveedoresCandidatos_SolicitudId' AND object_id = OBJECT_ID(N'dbo.ProveedoresCandidatos'))
    CREATE INDEX IX_ProveedoresCandidatos_SolicitudId ON dbo.ProveedoresCandidatos(SolicitudId);
GO

IF OBJECT_ID(N'dbo.Documentos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Documentos
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Documentos PRIMARY KEY,
        SolicitudId INT NOT NULL,
        ProveedorCandidatoId INT NULL,
        Tipo NVARCHAR(50) NOT NULL,
        NombreArchivo NVARCHAR(260) NOT NULL,
        MimeType NVARCHAR(150) NOT NULL,
        Contenido VARBINARY(MAX) NOT NULL,
        SubidoPor INT NOT NULL,
        CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Documentos_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Documentos_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_Documentos_Solicitudes FOREIGN KEY (SolicitudId) REFERENCES dbo.Solicitudes(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Documentos_ProveedoresCandidatos FOREIGN KEY (ProveedorCandidatoId) REFERENCES dbo.ProveedoresCandidatos(Id),
        CONSTRAINT FK_Documentos_Usuarios FOREIGN KEY (SubidoPor) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_Documentos_Tipo CHECK (Tipo IN (N'cotizacion', N'certificado_existencia', N'rut', N'orden_compra', N'factura', N'soporte_contable'))
    );
END;
GO

IF COL_LENGTH(N'dbo.Documentos', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.Documentos ADD UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_Documentos_UpdatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documentos_SolicitudId' AND object_id = OBJECT_ID(N'dbo.Documentos'))
    CREATE INDEX IX_Documentos_SolicitudId ON dbo.Documentos(SolicitudId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documentos_ProveedorCandidatoId' AND object_id = OBJECT_ID(N'dbo.Documentos'))
    CREATE INDEX IX_Documentos_ProveedorCandidatoId ON dbo.Documentos(ProveedorCandidatoId);
GO

IF OBJECT_ID(N'dbo.SolicitudHistorial', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SolicitudHistorial
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SolicitudHistorial PRIMARY KEY,
        SolicitudId INT NOT NULL,
        Etapa NVARCHAR(50) NOT NULL,
        Accion NVARCHAR(500) NOT NULL,
        ActorId INT NULL,
        Comentario NVARCHAR(MAX) NULL,
        CreatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_SolicitudHistorial_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_SolicitudHistorial_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SolicitudHistorial_Solicitudes FOREIGN KEY (SolicitudId) REFERENCES dbo.Solicitudes(Id) ON DELETE CASCADE,
        CONSTRAINT FK_SolicitudHistorial_Usuarios FOREIGN KEY (ActorId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_SolicitudHistorial_Etapa CHECK
        (
            Etapa IN
            (
                N'revision_auxiliar',
                N'devuelta_solicitante',
                N'revision_proveedores',
                N'seleccion_proveedor',
                N'carga_orden_compra',
                N'revision_oc_solicitante',
                N'oc_devuelta_auxiliar',
                N'revision_oc_aprobador',
                N'envio_proveedor',
                N'revision_anomalias',
                N'revision_factura_solicitante',
                N'factura_devuelta_analista',
                N'factura_aprobada_auxiliar',
                N'validacion_contable',
                N'factura_objetada_contable',
                N'completada'
            )
        )
    );
END;
GO

IF COL_LENGTH(N'dbo.SolicitudHistorial', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.SolicitudHistorial ADD UpdatedAt DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_SolicitudHistorial_UpdatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SolicitudHistorial_SolicitudId' AND object_id = OBJECT_ID(N'dbo.SolicitudHistorial'))
    CREATE INDEX IX_SolicitudHistorial_SolicitudId ON dbo.SolicitudHistorial(SolicitudId);
GO

COMMIT TRANSACTION;
GO