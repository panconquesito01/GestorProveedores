# Plan de migracion a .NET por capas y SQL Server

Fecha: 2026-08-05

Estado: migracion iniciada.

Proyecto origen oficial:

```text
C:\Users\david.rivera\Downloads\GestorProveedores-main\GestorProveedores-main
```

Repositorio destino de la nueva solucion .NET:

```text
https://github.com/panconquesito01/GestorProveedores.git
```

Base de datos SQL Server destino en desarrollo local:

```text
(localdb)\MSSQLLocalDB / GESTORPROVEEDORES
```

La carpeta origen contiene el sistema actual a migrar: `backend`, `frontend`, `db`, documentacion y archivos de despliegue. Todo inventario funcional, tecnico y de datos debe contrastarse contra esa ruta antes de reimplementar comportamiento en la nueva solucion .NET. El repositorio de GitHub corresponde al proyecto nuevo de migracion, no al sistema original. La base `GESTORPROVEEDORES` en `(localdb)\MSSQLLocalDB` corresponde al destino local de SQL Server para los scripts manuales y la configuracion de desarrollo.

## 1. Objetivo del plan

Migrar el sistema actual Gestor de Proveedores desde FastAPI + PostgreSQL + React hacia una solucion full .NET por capas, usando SQL Server como motor de base de datos y Blazor Web App como frontend recomendado, manteniendo el comportamiento funcional actual y corrigiendo debilidades criticas de seguridad, mantenibilidad y operacion.

El plan esta organizado por fases para evitar una migracion riesgosa tipo big bang. La recomendacion actual es migrar backend, base de datos y frontend dentro de la misma solucion .NET, separando claramente las capas de negocio de las capas de presentacion.

## 2. Alcance

### Incluido

- Migracion del backend Python/FastAPI a .NET Web API.
- Migracion del frontend React a Blazor Web App.
- Migracion de PostgreSQL a SQL Server.
- Diseno por capas.
- Reimplementacion del workflow de solicitudes.
- Nueva estrategia de autenticacion y autorizacion.
- Migracion del modelo de datos.
- Nueva interfaz full .NET para roles, bandejas, formularios, documentos y trazabilidad.
- Plan de pruebas, despliegue y corte.

### No incluido en la primera version

- Redisenar toda la experiencia de usuario.
- Integrar ERP real, salvo que se defina como fase posterior.
- Integrar SSO corporativo, salvo decision explicita del equipo.
- Construir una aplicacion movil o PWA offline.

## 3. Principios de migracion

1. Mantener paridad funcional antes de agregar funciones nuevas.
2. Separar reglas de negocio del transporte HTTP y de la base de datos.
3. Migrar por contratos: endpoint actual, DTO actual, respuesta esperada.
4. Corregir seguridad desde el inicio: no repetir `X-User-Id` como mecanismo final.
5. Usar SQL Server con scripts T-SQL revisables y versionados.
6. No generar ni ejecutar migraciones EF Core. EF Core se usara como ORM, pero el esquema de SQL Server se controlara mediante scripts T-SQL manuales, revisables y versionados.
7. Probar el workflow como pieza critica del sistema.
8. Mantener trazabilidad de cada decision tecnica.

## 4. Arquitectura objetivo

### 4.1 Estructura de solucion

```text
GestorProveedores.sln
src/
  GestorProveedores.Domain/
  GestorProveedores.Business/
  GestorProveedores.Infrastructure/
  GestorProveedores.Shared/
  GestorProveedores.WebApi/
  GestorProveedores.WebApp/
tests/
  GestorProveedores.Domain.Tests/
  GestorProveedores.Business.Tests/
  GestorProveedores.Infrastructure.Tests/
  GestorProveedores.WebApi.Tests/
  GestorProveedores.WebApp.Tests/
database/
  sqlserver/
    001_schema.sql
    002_seed_desarrollo.sql
    003_indexes_adicionales.sql
    rollback/
docs/
  migration/
```

### 4.2 Regla de dependencias

```text
GestorProveedores.WebApi
  -> GestorProveedores.Business
  -> GestorProveedores.Infrastructure
  -> GestorProveedores.Shared

GestorProveedores.WebApp
  -> GestorProveedores.Business
  -> GestorProveedores.Infrastructure
  -> GestorProveedores.Shared

GestorProveedores.Infrastructure
  -> GestorProveedores.Business
  -> GestorProveedores.Domain

GestorProveedores.Business
  -> GestorProveedores.Domain
  -> GestorProveedores.Shared

GestorProveedores.Domain
  -> Sin dependencias externas
```

Reglas clave:

- `Domain` no debe depender de EF Core, ASP.NET, SQL Server ni servicios externos.
- `Business` no debe conocer controladores ni detalles de infraestructura.
- `Infrastructure` implementa persistencia, email, archivos y servicios externos.
- `WebApi` solo expone HTTP, seguridad, middleware, Swagger y DI.
- `WebApp` solo implementa UI Blazor, navegacion, componentes, estado de pantalla y DI.

### 4.3 Responsabilidad por capa

| Capa | Responsabilidad | Contenido esperado |
| --- | --- | --- |
| `Domain` | Reglas puras del negocio | Entidades, value objects, enums, excepciones de dominio, eventos de dominio, invariantes del workflow. |
| `Business` | Casos de uso | Servicios de aplicacion, DTOs, validadores, contratos/puertos, politicas de permisos. |
| `Infrastructure` | Detalles tecnicos | EF Core, SQL Server, repositorios, email Resend, almacenamiento de documentos, generacion de radicados. |
| `Shared` | Contratos compartidos | Respuestas comunes, paginacion, errores, constantes no sensibles. |
| `WebApi` | Entrada HTTP | Controllers/Minimal APIs, JWT, CORS, Swagger, middleware de errores, composition root. |
| `WebApp` | Interfaz de usuario | Blazor Web App, layouts, paginas por rol, componentes, formularios, grillas, carga/descarga de documentos. |

## 5. Stack recomendado

| Area | Tecnologia recomendada |
| --- | --- |
| Backend | .NET 8 LTS o .NET 10 si el entorno corporativo lo permite. |
| API | ASP.NET Core Web API. |
| Frontend .NET | Blazor Web App con renderizado interactivo server-side. |
| Componentes UI | Microsoft Fluent UI Blazor o Telerik UI for Blazor si se requiere grilla empresarial avanzada. |
| ORM | EF Core con provider SQL Server. |
| Base de datos | Microsoft SQL Server. |
| Validacion | FluentValidation. |
| Auth | ASP.NET Core Identity o autenticacion propia con cookie para WebApp; JWT Bearer para WebApi si habra integraciones externas. |
| Hashing | ASP.NET Core Identity PasswordHasher o BCrypt/Argon2id. |
| Documentacion API | Swagger/OpenAPI. |
| Tests | xUnit, FluentAssertions, Testcontainers o SQL Server local para integracion. |
| Logs | ILogger + Serilog si se requiere salida estructurada. |

## 5.1 Decision frontend: Blazor, Razor Pages o MVC

### Recomendacion

Para este sistema se recomienda **Blazor Web App con interactividad server-side**.

Motivos:

- La aplicacion es operacional e interna, con usuarios autenticados y flujos por rol.
- Tiene pantallas interactivas: bandejas, filtros, formularios, badges de estado, trazabilidad, carga de archivos y acciones condicionales por etapa.
- Permite construir componentes reutilizables en C# sin mantener React, Node y JavaScript como stack principal.
- Encaja bien con una arquitectura por capas: la UI consume casos de uso de `Business` sin duplicar reglas.
- Facilita usar componentes empresariales como `FluentDataGrid`, dialogos, toasts, formularios y layout lateral.

### Comparacion

| Opcion | Cuando conviene | Ventajas | Desventajas para este proyecto |
| --- | --- | --- | --- |
| Blazor Web App interactivo server-side | Aplicaciones internas con formularios, estados y componentes ricos | UI en C#, componentes reutilizables, menor JavaScript, buena integracion con .NET, rapido para dashboards operativos | Requiere conexion estable al servidor por SignalR; hay que cuidar estado de componentes. |
| Razor Pages | CRUD simple, pantallas mayormente estaticas, formularios clasicos | Muy simple, robusto, poco estado en cliente | Se queda corto para una experiencia tipo SPA con paneles dinamicos y acciones por etapa. |
| ASP.NET MVC | Equipos con experiencia MVC o vistas muy estructuradas por controlador | Patron conocido, buen control del HTML | Mas verboso para componentes interactivos; menos natural para UI rica moderna. |
| Blazor WebAssembly | SPA descargada al navegador, posible hosting estatico | Experiencia mas parecida a React, API separada obligatoria | Carga inicial mayor, mas complejidad de autenticacion/API; no parece necesario para intranet. |

### Decision propuesta

Usar:

```text
GestorProveedores.WebApp = Blazor Web App interactivo server-side
GestorProveedores.WebApi = API para integraciones externas o consumo futuro
```

La WebApp debe llamar directamente a servicios de `Business`. La WebApi expone los mismos casos de uso cuando se necesite integracion externa, app movil o automatizaciones.

### Libreria UI recomendada

Usar **Microsoft Fluent UI Blazor** como primera opcion:

- Es de Microsoft y encaja visualmente con aplicaciones empresariales.
- Incluye layout, botones, menus, formularios, dialogos, toasts, tooltips y grillas.
- Evita construir componentes basicos desde cero.

Si el sistema requiere grillas muy avanzadas, exportaciones, filtros complejos o soporte empresarial dedicado, evaluar Telerik UI for Blazor en una fase posterior.

## 6. Fases de migracion

## Fase 0. Preparacion y decisiones base

### Objetivo

Cerrar decisiones tecnicas antes de crear la solucion .NET.

### Actividades

- Confirmar version objetivo de .NET.
- Confirmar version y edicion de SQL Server.
- Confirmar que el frontend se migrara a Blazor Web App.
- Definir estrategia de autenticacion: cookie/Identity para WebApp, JWT para WebApi externa, SSO futuro.
- Definir estrategia de documentos: `VARBINARY(MAX)` inicialmente o storage externo.
- Definir politica de scripts: T-SQL manual versionado, sin migraciones EF Core.
- Registrar y validar la ruta del proyecto origen oficial.
- Resolver la divergencia actual de `envio_proveedor`.

### Entregables

- Documento de decisiones tecnicas.
- Lista de riesgos aceptados.
- Alcance cerrado de la primera version .NET.

### Criterios de salida

- Version .NET definida.
- Estrategia frontend definida: Blazor Web App interactivo server-side.
- SQL Server disponible para desarrollo.
- Base SQL Server local identificada: `(localdb)\MSSQLLocalDB / GESTORPROVEEDORES`.
- Ruta del proyecto origen verificada: `C:\Users\david.rivera\Downloads\GestorProveedores-main\GestorProveedores-main`.
- Decidido si `envio_proveedor` sera etapa real o solo evento de historial.

## Fase 1. Inventario funcional y contratos actuales

### Objetivo

Convertir el sistema actual en una especificacion verificable para la migracion.

### Actividades

- Enumerar endpoints actuales.
- Documentar requests y responses actuales.
- Documentar vistas actuales de React y mapearlas a paginas/componentes Blazor.
- Documentar roles y permisos por endpoint.
- Documentar transiciones del workflow.
- Documentar emails enviados por cada evento.
- Documentar modelo de datos actual.

### Matriz inicial de endpoints

| Area | Endpoint actual | Equivalente .NET |
| --- | --- | --- |
| Auth | `POST /api/auth/login` | `POST /api/auth/login` |
| Catalogos | `GET /api/catalogos/empresas` | `GET /api/catalogos/empresas` |
| Catalogos | `GET /api/catalogos/aprobadores` | `GET /api/catalogos/aprobadores` |
| Solicitudes | `GET /api/solicitudes` | `GET /api/solicitudes` |
| Solicitudes | `POST /api/solicitudes` | `POST /api/solicitudes` |
| Solicitudes | `GET /api/solicitudes/{id}` | `GET /api/solicitudes/{id}` |
| Solicitudes | `PUT /api/solicitudes/{id}` | `PUT /api/solicitudes/{id}` |
| Workflow | `POST /api/workflow/{id}/...` | `POST /api/workflow/{id}/...` |
| Documentos | `GET /api/documentos/{id}` | `GET /api/documentos/{id}` con autorizacion real |

### Entregables

- Matriz endpoint actual vs endpoint .NET.
- Matriz de permisos por rol.
- Matriz de transiciones del workflow.
- Mapa de pantallas actuales a paginas Blazor.
- Contratos de casos de uso que consumira la WebApp.

### Criterios de salida

- Cada endpoint actual tiene contrato definido.
- Cada transicion tiene precondiciones y resultado esperado.
- Cada vista actual tiene pagina o componente Blazor objetivo.

## Fase 2. Diseno de base de datos SQL Server

### Objetivo

Disenar el esquema SQL Server equivalente y mejorado.

### Actividades

- Mapear tablas PostgreSQL a SQL Server.
- Reemplazar `SERIAL` por `IDENTITY`.
- Reemplazar `TIMESTAMPTZ` por `DATETIMEOFFSET`.
- Reemplazar `BYTEA` por `VARBINARY(MAX)`.
- Tomar como entrada inicial los scripts actuales de `db/schema.sql` y `db/seed.sql` del proyecto origen.
- Definir catalogos para roles, etapas, estados, origenes y tipos de documento.
- Crear secuencia `RadicadoSeq`.
- Crear indices equivalentes y nuevos indices necesarios.
- Definir constraints y relaciones.
- Mantener los scripts SQL Server como fuente de verdad del esquema; no usar `dotnet ef migrations add`, `dotnet ef database update` ni carpeta `Migrations`.

### Mapeo de tipos

| PostgreSQL | SQL Server |
| --- | --- |
| `SERIAL` | `INT IDENTITY(1,1)` |
| `TEXT` | `NVARCHAR(MAX)` o `NVARCHAR(n)` |
| `TIMESTAMPTZ` | `DATETIMEOFFSET` |
| `BYTEA` | `VARBINARY(MAX)` |
| `BOOLEAN` | `BIT` |
| `CREATE TYPE ... ENUM` | Tabla catalogo o `NVARCHAR` con `CHECK` |
| `SEQUENCE` | `CREATE SEQUENCE` |

### Tablas objetivo

| Tabla SQL Server | Origen actual |
| --- | --- |
| `Empresas` | `empresas` |
| `Usuarios` | `usuarios` |
| `Solicitudes` | `solicitudes` |
| `ProveedoresCandidatos` | `proveedores_candidatos` |
| `Documentos` | `documentos` |
| `SolicitudHistorial` | `solicitud_historial` |
| `AsignacionContadores` | `asignacion_contadores` |
| `RolesUsuario` | enum `rol_usuario` |
| `EtapasSolicitud` | enum `etapa_solicitud` |
| `EstadosSolicitud` | enum `estado_solicitud` |
| `OrigenesProveedor` | enum `origen_proveedor` |
| `TiposDocumento` | enum `tipo_documento` |

### Entregables

- `database/sqlserver/001_schema.sql`.
- `database/sqlserver/002_seed_desarrollo.sql`.
- Scripts manuales SQL Server versionados, no migraciones EF.
- Diagrama entidad-relacion.
- Checklist de equivalencia contra PostgreSQL.

### Criterios de salida

- El esquema crea una base limpia sin errores.
- Los datos seed permiten login y flujo basico.
- Existen indices para filtros principales.
- Las relaciones impiden datos huerfanos.

## Fase 3. Creacion de la solucion .NET

### Objetivo

Crear el esqueleto tecnico de la solucion por capas.

### Actividades

- Crear solucion `.sln`.
- Crear proyectos por capa.
- Configurar referencias entre proyectos.
- Agregar paquetes NuGet base.
- Configurar `appsettings.json`, `appsettings.Development.json` y variables de entorno.
- Configurar Swagger.
- Configurar middleware global de errores.

Estado inicial en este workspace: ya existe una solucion `GestorProveedores.slnx` con proyectos por capa en la raiz (`Domain`, `Business`, `Infrastructure`, `Shared`, `WebApi`, `WebApp`). Antes de mover carpetas a `src/`, confirmar si se mantiene la estructura actual o si se normaliza a la estructura objetivo propuesta.

### Comandos base sugeridos

```powershell
dotnet new sln -n GestorProveedores

dotnet new classlib -n GestorProveedores.Domain -o src/GestorProveedores.Domain
dotnet new classlib -n GestorProveedores.Business -o src/GestorProveedores.Business
dotnet new classlib -n GestorProveedores.Infrastructure -o src/GestorProveedores.Infrastructure
dotnet new classlib -n GestorProveedores.Shared -o src/GestorProveedores.Shared
dotnet new webapi -n GestorProveedores.WebApi -o src/GestorProveedores.WebApi
dotnet new blazor -n GestorProveedores.WebApp -o src/GestorProveedores.WebApp

dotnet sln add src/GestorProveedores.Domain/GestorProveedores.Domain.csproj
dotnet sln add src/GestorProveedores.Business/GestorProveedores.Business.csproj
dotnet sln add src/GestorProveedores.Infrastructure/GestorProveedores.Infrastructure.csproj
dotnet sln add src/GestorProveedores.Shared/GestorProveedores.Shared.csproj
dotnet sln add src/GestorProveedores.WebApi/GestorProveedores.WebApi.csproj
dotnet sln add src/GestorProveedores.WebApp/GestorProveedores.WebApp.csproj
```

### Entregables

- Solucion .NET compilando.
- Estructura por capas creada.
- Proyecto WebApi exponiendo `/health` y Swagger.
- Proyecto WebApp Blazor exponiendo pantalla inicial y layout base.

### Criterios de salida

- `dotnet build` ejecuta sin errores.
- Las dependencias respetan la regla de capas.
- No hay acceso a datos en `WebApi` ni reglas de negocio en controllers.
- No hay reglas de negocio incrustadas en componentes Blazor.

## Fase 4. Implementacion del dominio

### Objetivo

Modelar el negocio sin depender de frameworks.

### Actividades

- Crear entidades de dominio.
- Crear enums o value objects.
- Crear excepciones de dominio.
- Implementar reglas de transicion de `Solicitud`.
- Implementar validaciones de actor asignado.
- Implementar eventos de dominio si se usaran para emails/historial.

### Entidades principales

```text
Empresa
Usuario
Solicitud
ProveedorCandidato
Documento
SolicitudHistorial
AsignacionContador
```

### Reglas minimas del dominio

- Una solicitud nueva inicia en `RevisionAuxiliar`.
- Solo el solicitante dueno edita una solicitud devuelta.
- Solo el auxiliar asignado avanza o devuelve la revision inicial.
- Para proveedores nuevos se requieren al menos dos candidatos validados.
- Los proveedores validados deben estar creados en ERP antes de pasar a seleccion.
- Solo el analista asignado selecciona proveedor o carga factura.
- Solo el aprobador asignado aprueba cuando aplica.
- Solo contabilidad puede cerrar u objetar.
- Toda transicion genera historial.

### Entregables

- Entidades de dominio.
- Enums del workflow.
- Excepciones de dominio.
- Tests unitarios de transiciones criticas.

### Criterios de salida

- Las transiciones invalidas fallan con excepciones controladas.
- Las transiciones validas cambian estado y generan historial.
- El dominio no referencia EF Core ni ASP.NET.

## Fase 5. Capa Business y casos de uso

### Objetivo

Implementar la logica de aplicacion y los contratos usados por la API.

### Actividades

- Crear DTOs de request y response.
- Crear validadores con FluentValidation.
- Crear servicios de aplicacion.
- Crear interfaces para repositorios y servicios externos.
- Implementar politicas de permisos por rol.
- Crear modelos de vista/DTOs pensados para WebApp Blazor y WebApi.

### Servicios sugeridos

| Servicio | Responsabilidad |
| --- | --- |
| `IAuthService` | Login, emision de token, validacion de credenciales. |
| `ISolicitudService` | Crear, listar, obtener detalle y editar solicitudes. |
| `IWorkflowService` | Ejecutar transiciones del workflow. |
| `ICatalogoService` | Empresas, aprobadores, catalogos. |
| `IDocumentoService` | Subida y descarga autorizada de documentos. |
| `IAssignmentService` | Round-robin de auxiliares y analistas. |
| `IRadicadoService` | Generacion de radicados. |
| `IEmailService` | Envio de correos transaccionales. |

### Entregables

- DTOs de entrada y salida.
- Validadores.
- Servicios de aplicacion.
- Contratos de repositorios y puertos.
- Tests de casos de uso.

### Criterios de salida

- Cada endpoint tiene un caso de uso asociado.
- Cada request relevante tiene validador.
- Los servicios no dependen de ASP.NET ni SQL Server directamente.

## Fase 6. Capa Infrastructure con EF Core y SQL Server

### Objetivo

Implementar persistencia y servicios externos.

### Actividades

- Crear `AppDbContext`.
- Crear configuraciones `IEntityTypeConfiguration<T>`.
- Implementar repositorios.
- Implementar `UnitOfWork` si se decide usarlo.
- Implementar Resend email service.
- Implementar almacenamiento inicial de documentos en SQL Server.
- Implementar generacion de radicado usando `SEQUENCE`.
- Implementar round-robin con bloqueo transaccional.

### Buenas practicas EF Core

- No usar migraciones EF Core para crear o actualizar el esquema.
- No agregar carpeta `Migrations` al proyecto `Infrastructure`.
- Alinear `AppDbContext` y configuraciones EF contra los scripts T-SQL manuales.
- Usar `AsNoTracking()` en consultas de lectura.
- Usar proyecciones para listados.
- Evitar N+1 con `Include` o proyecciones controladas.
- Usar `rowversion` para concurrencia en `Solicitudes`.
- Separar configuraciones por entidad.
- No exponer entidades EF directamente en responses.

### Entregables

- `AppDbContext`.
- Configuraciones de entidades.
- Repositorios o queries especializadas.
- Implementaciones de servicios externos.
- Verificacion de alineacion EF Core vs scripts SQL Server, sin migraciones EF generadas.
- Tests de integracion contra SQL Server.

### Criterios de salida

- La API puede leer y escribir en SQL Server.
- Los scripts T-SQL y el modelo EF estan alineados.
- Los documentos se guardan y descargan correctamente.
- El round-robin no duplica asignaciones bajo concurrencia normal.

## Fase 7. WebApi, seguridad y endpoints

### Objetivo

Exponer la funcionalidad a traves de HTTP cuando se requiera integracion externa y configurar la seguridad compartida de la solucion .NET.

### Actividades

- Crear controllers o Minimal APIs.
- Configurar autenticacion con cookie para `WebApp`.
- Configurar JWT Bearer para `WebApi` si se requiere consumo externo.
- Configurar CORS por ambiente.
- Crear middleware global de excepciones con Problem Details.
- Configurar Swagger con autenticacion.
- Mapear endpoints actuales.
- Eliminar dependencia de `X-User-Id` como autenticacion final.

### Endpoints objetivo

```text
POST /api/auth/login
GET  /api/catalogos/empresas
GET  /api/catalogos/aprobadores?empresa_id=1
GET  /api/solicitudes?vista=...
POST /api/solicitudes
GET  /api/solicitudes/{id}
PUT  /api/solicitudes/{id}
POST /api/workflow/{id}/paso1/devolver
POST /api/workflow/{id}/paso1/siguiente
POST /api/workflow/{id}/paso2/proveedor-erp
POST /api/workflow/{id}/paso2/proveedor-erp/siguiente
POST /api/workflow/{id}/paso2/proveedores-nuevos
POST /api/workflow/{id}/proveedores/{proveedor_id}/documento
POST /api/workflow/{id}/paso2/proveedores/{proveedor_id}/creado-en-erp
POST /api/workflow/{id}/paso2/proveedores-nuevos/siguiente
POST /api/workflow/{id}/paso3/seleccionar
POST /api/workflow/{id}/paso4/orden-compra
POST /api/workflow/{id}/paso5/solicitante
POST /api/workflow/{id}/paso5/aprobador
POST /api/workflow/{id}/paso6/factura
POST /api/workflow/{id}/paso7/solicitante
POST /api/workflow/{id}/paso8/contabilidad
POST /api/workflow/{id}/paso9/conforme
POST /api/workflow/{id}/paso9/objetar
POST /api/workflow/{id}/paso9/reenviar-factura
GET  /api/documentos/{id}
```

### Estado actual de migracion WebApi y WebApp

Completado funcionalmente con autenticacion real inicial:

- `Auth`, `Catalogos`, `Solicitudes`, `Workflow` completo y descarga de `Documentos`.
- `POST /api/auth/login` emite `access_token` JWT Bearer con claims de usuario y rol.
- Endpoints `Catalogos`, `Solicitudes`, `Workflow` y `Documentos` exigen Bearer token y validan usuario activo por policy.
- Autorizacion por rol y por actor asignado aplicada en los casos de uso migrados.
- Carga de archivos soportada con `multipart/form-data` y persistencia inicial en SQL Server `VARBINARY(MAX)`.
- Descarga de documentos protegida por participante de la solicitud, corrigiendo el endpoint abierto del origen.
- `WebApp` tiene vista `/login` independiente sin menu, cookie protegida de sesion, redireccion a `/` tras login y dashboard inicial responsive con microanimaciones.

Pendiente para cierre de seguridad final:

- Configurar Swagger con autenticacion Bearer visible en UI.
- Centralizar el mapeo de excepciones en middleware global.
- Persistencia/renovacion formal de sesion si se decide usar ASP.NET Core Identity o SSO.

### Entregables

- API .NET con endpoints funcionales.
- Autenticacion de WebApp por cookie segura.
- JWT para WebApi si se expone a clientes externos.
- Autorizacion por roles y por propiedad/asignacion.
- Swagger documentado.
- Manejo de errores estandarizado.

### Criterios de salida

- Login autentica al usuario en la WebApp.
- Endpoints protegidos rechazan usuarios no autorizados.
- WebApi queda disponible para integraciones o automatizaciones.
- Errores de negocio devuelven 400/403/404 controlados, no 500.

## Fase 8. Migracion de datos

### Objetivo

Mover datos desde PostgreSQL a SQL Server conservando integridad.

### Actividades

- Exportar datos desde PostgreSQL.
- Transformar tipos y valores.
- Cargar catalogos en SQL Server.
- Cargar tablas principales en orden de dependencias.
- Validar conteos.
- Validar relaciones.
- Validar documentos binarios.
- Definir estrategia para contrasenas.

### Orden de carga recomendado

1. Catalogos.
2. Empresas.
3. Usuarios.
4. AsignacionContadores.
5. Solicitudes.
6. ProveedoresCandidatos.
7. Documentos.
8. SolicitudHistorial.

### Consideracion de contrasenas

No se deben migrar contrasenas en texto plano como solucion final. Opciones:

- Generar hashes para los usuarios actuales.
- Forzar cambio de contrasena en primer login.
- Crear usuarios seed solo para desarrollo.
- Integrar proveedor de identidad corporativo en una fase posterior.

### Entregables

- Scripts de extraccion.
- Scripts de carga SQL Server.
- Reporte de validacion de conteos.
- Reporte de inconsistencias.

### Criterios de salida

- Conteos por tabla coinciden o las diferencias estan justificadas.
- Solicitudes mantienen historial y documentos.
- Usuarios pueden autenticarse con el nuevo mecanismo.

## Fase 9. Frontend .NET con Blazor Web App

### Objetivo

Reimplementar la interfaz actual en .NET usando Blazor Web App, manteniendo las pantallas por rol y mejorando la seguridad y la experiencia operativa.

### Actividades

- Crear layout principal con menu lateral por rol.
- Crear pantalla de login.
- Crear componentes reutilizables: badge de etapa, lista de solicitudes, filtros, documentos, trazabilidad y acciones de decision.
- Crear paginas Blazor para solicitante, auxiliar, analista, aprobador y contable.
- Crear formularios Blazor para proveedores, orden de compra, factura, objecion contable y edicion de solicitud.
- Integrar Microsoft Fluent UI Blazor.
- Configurar providers de Fluent UI en el layout raiz: toast, dialog, message bar, tooltip y key code.
- Usar `EditForm` con componentes Fluent para formularios.
- Implementar carga y descarga autorizada de archivos.
- Mostrar errores de negocio como mensajes controlados, no como excepciones crudas.

### Estructura sugerida de WebApp

```text
src/GestorProveedores.WebApp/
  Components/
    Layout/
    Solicitudes/
    Proveedores/
    Documentos/
    Workflow/
    Shared/
  Pages/
    Auth/
    Solicitante/
    Auxiliar/
    Analista/
    Aprobador/
    Contable/
  Services/
    UiState/
    Navigation/
  wwwroot/
```

### Mapeo inicial de pantallas

| React actual | Blazor objetivo |
| --- | --- |
| `Login.jsx` | `Pages/Auth/Login.razor` |
| `AppLayout.jsx` | `Components/Layout/MainLayout.razor` |
| `ListaSolicitudes.jsx` | `Components/Solicitudes/ListaSolicitudes.razor` |
| `SolicitudDetalle.jsx` | `Pages/Solicitudes/SolicitudDetalle.razor` |
| `NuevaSolicitud.jsx` | `Pages/Solicitante/NuevaSolicitud.razor` |
| Paginas de auxiliar | `Pages/Auxiliar/*.razor` |
| Paginas de analista | `Pages/Analista/*.razor` |
| Paginas de aprobador | `Pages/Aprobador/*.razor` |
| Paginas de contable | `Pages/Contable/*.razor` |

### Componentes UI recomendados

| Necesidad | Componente sugerido |
| --- | --- |
| Menu lateral | `FluentNavMenu` |
| Botones de accion | `FluentButton` |
| Listados | `FluentDataGrid` |
| Formularios | `EditForm` + `FluentTextField`, `FluentSelect`, `FluentCheckbox` |
| Confirmaciones | `IDialogService` |
| Notificaciones | `IToastService` |
| Estados | Badges propios o `FluentBadge` si aplica |
| Tooltips | `FluentTooltipProvider` |

### Entregables

- WebApp Blazor funcional.
- Layout por rol.
- Paginas principales migradas.
- Componentes reutilizables.
- Formularios con validacion.
- Carga y descarga de documentos.
- Matriz de pantallas probadas por rol.

### Criterios de salida

- Login funciona.
- Listados por rol funcionan.
- Detalle de solicitud funciona.
- Todas las acciones del workflow funcionan.
- Documentos se suben y descargan con autorizacion.
- La UI no depende de React, Vite ni Node para ejecutarse.

## Fase 10. Pruebas y calidad

### Objetivo

Crear red de seguridad automatizada para evitar regresiones.

### Actividades

- Tests unitarios de dominio.
- Tests de casos de uso.
- Tests de autorizacion por rol.
- Tests de endpoints principales.
- Tests de integracion SQL Server.
- Tests de flujo completo.
- Build de WebApp Blazor.
- Pipeline CI.

### Casos criticos de prueba

| Caso | Resultado esperado |
| --- | --- |
| Crear solicitud | Asigna auxiliar y crea historial. |
| Devolver solicitud | Cambia a `DevueltaSolicitante` y exige comentario. |
| Proveedores nuevos con menos de 2 validos | Falla con error de negocio. |
| Aprobar OC sin aprobador | Pasa a etapa posterior definida. |
| Aprobar OC con aprobador | Pasa a revision de aprobador. |
| Descargar documento sin permiso | Devuelve 403. |
| Usuario de otra empresa accede a solicitud ajena | Devuelve 403. |
| Contable cierra sin confirmacion ERP | Falla con error de negocio. |

### Entregables

- Suite de tests.
- Pipeline de CI.
- Reporte de cobertura inicial.
- Checklist de regresion manual.

### Criterios de salida

- `dotnet test` pasa.
- Build de WebApp Blazor pasa.
- Flujo completo validado en ambiente de pruebas.

## Fase 11. Despliegue y operacion

### Objetivo

Preparar la nueva version para ambientes reales.

### Actividades

- Definir ambientes: desarrollo, pruebas, staging, produccion.
- Configurar connection strings por ambiente.
- Configurar CORS por ambiente.
- Configurar secretos de forma segura.
- Configurar logs estructurados.
- Configurar health checks.
- Configurar backups SQL Server.
- Definir estrategia de rollback.

### Variables sugeridas

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__SigningKey
Authentication__CookieName
Resend__ApiKey
Resend__FromEmail
App__BaseUrl
Cors__AllowedOrigins__0
```

### Opciones de despliegue

| Opcion | Uso recomendado |
| --- | --- |
| IIS + SQL Server | Entorno Windows corporativo. |
| Docker + SQL Server externo | Portabilidad y despliegue controlado. |
| Azure App Service + Azure SQL | Nube Microsoft administrada. |
| VM Windows/Linux | Control total, mayor carga operativa. |

### Entregables

- Pipeline de despliegue.
- Documentacion de variables.
- Health checks.
- Plan de backups.
- Plan de rollback.

### Criterios de salida

- Ambiente de staging funcional.
- Logs y health checks visibles.
- Rollback documentado y probado.

## Fase 12. Corte productivo y post-migracion

### Objetivo

Cambiar de la version actual a la version .NET con riesgo controlado.

### Actividades

- Congelar cambios en la version anterior.
- Ejecutar migracion final de datos.
- Ejecutar validaciones de conteo e integridad.
- Apuntar DNS o publicacion IIS/Azure/Docker a la nueva WebApp .NET.
- Ejecutar pruebas smoke.
- Monitorear errores, tiempos y logs.
- Mantener ventana de rollback.

### Checklist de corte

| Item | Estado |
| --- | --- |
| Backup PostgreSQL tomado | Pendiente |
| Backup SQL Server tomado | Pendiente |
| Scripts de migracion probados | Pendiente |
| Ambiente staging aprobado | Pendiente |
| Usuarios clave validaron flujo | Pendiente |
| Rollback probado | Pendiente |
| Ventana de corte aprobada | Pendiente |

### Entregables

- Acta de migracion.
- Reporte de validacion post-corte.
- Lista de incidencias.
- Backlog post-migracion.

### Criterios de salida

- Usuarios pueden ingresar.
- Flujo principal se completa.
- Documentos son accesibles con permisos correctos.
- No hay errores criticos en logs.

## 7. Cronograma sugerido

Este cronograma depende del tamano del equipo y del nivel de pruebas requerido.

| Fase | Duracion estimada | Resultado |
| --- | --- | --- |
| Fase 0 | 1 a 2 dias | Decisiones cerradas. |
| Fase 1 | 2 a 4 dias | Contratos e inventario funcional. |
| Fase 2 | 3 a 5 dias | Diseno SQL Server y scripts base. |
| Fase 3 | 1 a 2 dias | Solucion .NET creada. |
| Fase 4 | 4 a 7 dias | Dominio y workflow base. |
| Fase 5 | 5 a 8 dias | Casos de uso. |
| Fase 6 | 5 a 8 dias | EF Core + SQL Server. |
| Fase 7 | 4 a 7 dias | API segura y endpoints. |
| Fase 8 | 3 a 6 dias | Migracion de datos. |
| Fase 9 | 5 a 10 dias | WebApp Blazor construida y validada. |
| Fase 10 | 5 a 10 dias | Pruebas y CI. |
| Fase 11 | 2 a 5 dias | Despliegue. |
| Fase 12 | 1 a 2 dias | Corte controlado. |

## 8. Riesgos principales

| Riesgo | Impacto | Mitigacion |
| --- | --- | --- |
| Diferencias entre PostgreSQL y SQL Server | Alto | Scripts de validacion y pruebas de migracion tempranas. |
| Workflow mal replicado | Alto | Tests unitarios de cada transicion. |
| Reimplementacion de UI puede omitir reglas visibles del React actual | Medio/Alto | Matriz pantalla actual vs pagina Blazor, validacion por rol y pruebas de flujo. |
| Documentos grandes en base de datos | Medio | Limites de tamano y evaluacion de storage externo. |
| Seguridad subestimada | Alto | Cookie segura/Identity para WebApp, JWT para WebApi externa, hashing y permisos por recurso desde la primera version. |
| Corte sin rollback | Alto | Backups y plan de retorno probado. |
| Concurrencia en asignacion round-robin | Medio | Bloqueo transaccional o actualizacion atomica. |

## 9. Decisiones pendientes

1. Definir .NET 8 LTS o .NET 10.
2. Confirmar Blazor Web App interactivo server-side como frontend objetivo.
3. Definir si documentos quedan en SQL Server o pasan a storage externo.
4. Definir si `envio_proveedor` sera etapa persistida o evento automatico.
5. Definir autenticacion final: Identity/cookie para WebApp, JWT para WebApi externa o SSO.
6. Definir ambiente destino: IIS, Docker, Azure o VM.
7. Definir politica de migracion de usuarios y contrasenas.

## 10. Recomendacion final

La ruta mas segura es:

1. Construir la solucion .NET por capas incluyendo `WebApp` Blazor y `WebApi`.
2. Usar Blazor Web App interactivo server-side como frontend principal.
3. Usar Razor Pages solo si se decide una UI mas simple y menos interactiva; para este proyecto no es la primera opcion.
4. Migrar PostgreSQL a SQL Server con scripts T-SQL controlados.
5. Reemplazar `X-User-Id` por autenticacion real: cookie segura/Identity para WebApp y JWT para WebApi externa.
6. Proteger documentos y detalle de solicitudes por permisos reales.
7. Agregar pruebas del workflow y pruebas de pantallas Blazor antes del corte.
8. Hacer el cambio productivo solo cuando la WebApp Blazor replique el flujo actual completo.

Esta estrategia reduce el riesgo porque convierte todo el sistema a .NET sin mezclar stacks de frontend, pero mantiene separadas las responsabilidades: `WebApp` para UI, `Business` para casos de uso, `Domain` para reglas, `Infrastructure` para SQL Server y servicios externos, y `WebApi` para integraciones futuras.