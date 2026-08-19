# Publicar GestorProveedores en MonsterASP.NET (gratis)

Proyecto a desplegar: **GestorProveedores.WebApp** (Blazor Server + SQL Server).

Documentacion oficial MonsterASP: https://help.monsterasp.net/books/deploy/

---

## Base de datos local de referencia

Revisada en `(localdb)\MSSQLLocalDB` / `GESTORPROVEEDORES`:

| Elemento | Estado |
|---|---|
| Motor | SQL Server 2025 Express |
| Tablas | 7 (`Empresas`, `Usuarios`, `AsignacionContadores`, `Solicitudes`, `ProveedoresCandidatos`, `Documentos`, `SolicitudHistorial`) |
| Secuencia | `RadicadoSeq` (valor actual: 13) |
| Usuarios | 14 (incluye `davidrivera` y `admin`) |
| Solicitudes | 13 con workflow de prueba |
| Documentos | 12 (binarios incluidos) |

Todos los usuarios tienen password **`123`**.

---

## 1. Crear cuenta y recursos

1. Registrate en https://www.monsterasp.net/ (plan **Free**, sin tarjeta).
2. En el **Control Panel**:
   - **Websites** → crear sitio (.NET 10).
   - **Cloud Database** → crear **Free MSSQL** (1 GB).
3. Anota del panel:
   - URL del sitio (`*.runasp.net` o `*.tryasp.net`)
   - Servidor SQL, nombre de BD, usuario y password
   - Connection string completo (boton copiar en el panel)

---

## 2. Subir la base de datos (elige una opcion)

### Opcion A — Restaurar backup .bak (recomendada)

Copia exacta de tu BD local, con datos y documentos.

1. Genera o usa el backup local:
   ```powershell
   sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "BACKUP DATABASE [GESTORPROVEEDORES] TO DISK = N'...\GESTORPROVEEDORES_local.bak' WITH INIT, FORMAT"
   ```
   O ejecuta `database/sqlserver/monsterasp/export-from-localdb.ps1` si tienes SMO instalado.

2. Archivo generado (~6.4 MB):
   `database/sqlserver/monsterasp/GESTORPROVEEDORES_local.bak`

3. En MonsterASP Control Panel → **Manage Databases** → **Restore** → sube el `.bak`.

### Opcion B — Scripts SQL

1. Conecta SSMS/Azure Data Studio a la BD de MonsterASP.
2. Ejecuta en orden:
   - `database/sqlserver/monsterasp/001_schema.sql`
   - `database/sqlserver/monsterasp/002_data_from_localdb.sql`

Para regenerar los datos desde tu LocalDB:

```powershell
powershell -ExecutionPolicy Bypass -File database/sqlserver/monsterasp/export-data-from-localdb.ps1
```

### Logins de prueba (datos reales exportados)

| Usuario | Password | Rol |
|---|---|---|
| `davidrivera` | `123` | superusuario |
| `admin` | `123` | superusuario |
| `crojas` | `123` | solicitante |
| `dherrera` | `123` | auxiliar |

---

## 3. Configurar variables en MonsterASP

En el Control Panel del **sitio web** → **App Settings**:

| Variable | Valor |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | *(connection string del panel)* |
| `Jwt__SigningKey` | Clave aleatoria de 32+ caracteres |

Ejemplo de connection string:

```text
Server=TU_SERVIDOR.runasp.net;Database=TU_BASE;User Id=TU_USUARIO;Password=TU_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

---

## 4. Publicar desde Visual Studio

1. En MonsterASP: activar **WebDeploy** y descargar `.publishSettings`.
2. Clic derecho en **GestorProveedores.WebApp** → **Publish** → **Import Profile**.
3. **Show all settings**: .NET 10, Release.
4. **Publish**.

> No subas el `.publishSettings` al repositorio.

---

## 5. HTTPS (plan free)

1. Control Panel → **Domains / HTTPS** → **Let's Encrypt**.
2. Renovar manualmente cada **90 dias** en plan free.
3. Activar redireccion HTTP → HTTPS cuando la app funcione.

---

## 6. Verificar

1. `https://tu-sitio.runasp.net/login`
2. Login: `davidrivera` / `123`
3. Revisar solicitudes existentes (deberian aparecer las 13 de prueba si restauraste datos)

---

## Limitaciones del plan free

- 256 MB RAM (Blazor Server puede ir lento)
- 1 GB de base de datos
- Solo datacenter EU
- Trafico limitado
- Sin dominio propio

---

## Solucion de problemas

| Error | Causa probable |
|---|---|
| `Connection string 'DefaultConnection' is required` | Falta variable en App Settings |
| Error 500 | Revisar logs del panel MonsterASP |
| Login falla | BD sin datos; ejecutar scripts o restaurar `.bak` |
| Blazor desconecta | RAM insuficiente en plan free |

---

## Nota sobre rol `administrador`

La BD local permite el rol `administrador` en constraints SQL, pero el codigo .NET actual solo reconoce:
`superusuario`, `solicitante`, `auxiliar`, `analista`, `aprobador`, `contable`.

El usuario `admin` en local tiene rol `superusuario`, asi que funciona sin cambios de codigo.

---

## WebApi (fase 2)

La WebApp no depende de la API. El plan free incluye 1 sitio web.
