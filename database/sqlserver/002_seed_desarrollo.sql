/*
    GestorProveedores - Datos semilla de desarrollo SQL Server
    Fuente: C:\Users\david.rivera\Downloads\GestorProveedores-main\GestorProveedores-main\db\seed.sql
    Base de datos destino local: GESTORPROVEEDORES en (localdb)\MSSQLLocalDB

    Regla de migracion:
    - No generar ni ejecutar migraciones EF Core.
    - El esquema y los datos semilla se controlan con scripts T-SQL manuales.

    Credenciales de desarrollo:
    - Todos los usuarios usan password: 123
    - El valor se almacena en Usuarios.PasswordHash con formato ASP.NET Core Identity PasswordHasher.
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

DECLARE @PasswordHash NVARCHAR(500) = N'AQAAAAIAAYagAAAAENV6xzJB9I0nd9i6XbptzboAY86bJAeZlWLKlIfnJ9iziWXeQuXyRZMkHYmuKUmbtQ==';

DECLARE @Empresas TABLE
(
    Id INT NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(250) NOT NULL,
    Nit NVARCHAR(50) NOT NULL
);

INSERT INTO @Empresas (Id, Nombre, Nit)
VALUES
    (1, N'Comercializadora Andina S.A.S.', N'900123456-1'),
    (2, N'Distribuciones del Pacifico Ltda.', N'900654321-2');

UPDATE destino
SET
    Nombre = origen.Nombre,
    Nit = origen.Nit,
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.Empresas AS destino
INNER JOIN @Empresas AS origen ON origen.Id = destino.Id;

SET IDENTITY_INSERT dbo.Empresas ON;

INSERT INTO dbo.Empresas (Id, Nombre, Nit)
SELECT origen.Id, origen.Nombre, origen.Nit
FROM @Empresas AS origen
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Empresas AS destino
    WHERE destino.Id = origen.Id
);

SET IDENTITY_INSERT dbo.Empresas OFF;

DECLARE @Usuarios TABLE
(
    Id INT NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(250) NOT NULL,
    Email NVARCHAR(320) NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(30) NOT NULL,
    EmpresaId INT NULL
);

INSERT INTO @Usuarios (Id, Nombre, Email, Username, Rol, EmpresaId)
VALUES
    (1, N'Camila Rojas', N'camila.rojas@andina.com', N'crojas', N'solicitante', 1),
    (2, N'Julian Torres', N'julian.torres@andina.com', N'jtorres', N'solicitante', 1),
    (3, N'Laura Gomez', N'laura.gomez@pacifico.com', N'lgomez', N'solicitante', 2),
    (4, N'Andres Salazar', N'andres.salazar@pacifico.com', N'asalazar', N'solicitante', 2),
    (5, N'Marcela Duarte', N'marcela.duarte@andina.com', N'mduarte', N'aprobador', 1),
    (6, N'Felipe Castano', N'felipe.castano@pacifico.com', N'fcastano', N'aprobador', 2),
    (7, N'Diana Herrera', N'diana.herrera@gestion.com', N'dherrera', N'auxiliar', NULL),
    (8, N'Santiago Ruiz', N'santiago.ruiz@gestion.com', N'sruiz', N'auxiliar', NULL),
    (9, N'Valentina Leon', N'valentina.leon@gestion.com', N'vleon', N'auxiliar', NULL),
    (10, N'Roberto Paez', N'roberto.paez@gestion.com', N'rpaez', N'analista', NULL),
    (11, N'Natalia Cortes', N'natalia.cortes@gestion.com', N'ncortes', N'analista', NULL),
    (12, N'Hernan Vargas', N'hernan.vargas@contabilidad.com', N'hvargas', N'contable', NULL);

UPDATE destino
SET
    Nombre = origen.Nombre,
    Email = origen.Email,
    Username = origen.Username,
    PasswordHash = @PasswordHash,
    Rol = origen.Rol,
    EmpresaId = origen.EmpresaId,
    Activo = 1,
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.Usuarios AS destino
INNER JOIN @Usuarios AS origen ON origen.Id = destino.Id;

SET IDENTITY_INSERT dbo.Usuarios ON;

INSERT INTO dbo.Usuarios (Id, Nombre, Email, Username, PasswordHash, Rol, EmpresaId, Activo)
SELECT origen.Id, origen.Nombre, origen.Email, origen.Username, @PasswordHash, origen.Rol, origen.EmpresaId, 1
FROM @Usuarios AS origen
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Usuarios AS destino
    WHERE destino.Id = origen.Id
);

SET IDENTITY_INSERT dbo.Usuarios OFF;

IF NOT EXISTS (SELECT 1 FROM dbo.AsignacionContadores WHERE Rol = N'auxiliar')
BEGIN
    INSERT INTO dbo.AsignacionContadores (Rol, UltimoIndice)
    VALUES (N'auxiliar', -1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.AsignacionContadores WHERE Rol = N'analista')
BEGIN
    INSERT INTO dbo.AsignacionContadores (Rol, UltimoIndice)
    VALUES (N'analista', -1);
END;

COMMIT TRANSACTION;
GO

SELECT Username, Rol, EmpresaId
FROM dbo.Usuarios
ORDER BY Id;
GO