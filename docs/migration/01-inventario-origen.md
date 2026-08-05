# Inventario inicial del sistema origen

Fecha: 2026-08-05

Proyecto origen oficial:

```text
C:\Users\david.rivera\Downloads\GestorProveedores-main\GestorProveedores-main
```

Repositorio destino de la migracion .NET:

```text
https://github.com/panconquesito01/GestorProveedores.git
```

Este documento inicia la Fase 1 del plan de migracion. El objetivo es convertir el sistema FastAPI + PostgreSQL + React actual en contratos verificables antes de reimplementar comportamiento en .NET.

## 1. Fuentes revisadas

| Area | Fuente origen |
| --- | --- |
| Aplicacion FastAPI | `backend/app/main.py` |
| Rutas HTTP | `backend/app/routers/*.py` |
| DTOs Pydantic | `backend/app/schemas/*.py` |
| Workflow | `backend/app/services/workflow_engine.py` |
| Modelo de datos PostgreSQL | `db/schema.sql` |
| Datos semilla | `db/seed.sql` |

## 2. Endpoints actuales

| Area | Metodo | Endpoint | Entrada | Salida | Autorizacion origen | Equivalente .NET inicial |
| --- | --- | --- | --- | --- | --- | --- |
| Auth | POST | `/api/auth/login` | `LoginRequest` | `LoginResponse` | Sin sesion previa | `POST /api/auth/login` |
| Catalogos | GET | `/api/catalogos/empresas` | Ninguna | `list[EmpresaOut]` | Usuario actual | `GET /api/catalogos/empresas` |
| Catalogos | GET | `/api/catalogos/aprobadores?empresa_id={id}` | `empresa_id` query | `list[AprobadorOption]` | Usuario actual | `GET /api/catalogos/aprobadores` |
| Solicitudes | GET | `/api/solicitudes?vista={vista}` | Filtros query | `list[SolicitudListItem]` | Usuario actual + rol por vista | `GET /api/solicitudes` |
| Solicitudes | POST | `/api/solicitudes` | `SolicitudCreate` | `SolicitudDetalle` | Rol `solicitante` | `POST /api/solicitudes` |
| Solicitudes | GET | `/api/solicitudes/{solicitud_id}` | Id ruta | `SolicitudDetalle` | Usuario actual | `GET /api/solicitudes/{id}` |
| Solicitudes | PUT | `/api/solicitudes/{solicitud_id}` | `SolicitudUpdate` | `SolicitudDetalle` | Rol `solicitante` | `PUT /api/solicitudes/{id}` |
| Workflow | POST | `/api/workflow/{id}/paso1/devolver` | `ComentarioRequest` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso1/siguiente` | Ninguna | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso2/proveedor-erp` | `ProveedorErpRequest` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso2/proveedor-erp/siguiente` | Ninguna | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso2/proveedores-nuevos` | `ProveedoresNuevosRequest` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/proveedores/{proveedor_id}/documento` | Multipart `tipo`, `file` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada, con autorizacion reforzada |
| Workflow | POST | `/api/workflow/{id}/paso2/proveedores/{proveedor_id}/creado-en-erp` | `CreadoEnErpRequest` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso2/proveedores-nuevos/siguiente` | Ninguna | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso3/seleccionar` | `SeleccionarProveedorRequest` | `SolicitudDetalle` | Rol `analista` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso4/orden-compra` | Multipart `file`, `comentario` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso5/solicitante` | `DecisionRequest` | `SolicitudDetalle` | Rol `solicitante` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso5/aprobador` | `DecisionRequest` | `SolicitudDetalle` | Rol `aprobador` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso6/factura` | Multipart `file`, `comentario` | `SolicitudDetalle` | Rol `analista` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso7/solicitante` | `DecisionRequest` | `SolicitudDetalle` | Rol `solicitante` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso8/contabilidad` | Multipart `files`, `comentario` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso9/conforme` | `ConformeRequest` | `SolicitudDetalle` | Rol `contable` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso9/objetar` | `ObjetarContableRequest` | `SolicitudDetalle` | Rol `contable` | Igual ruta o versionada |
| Workflow | POST | `/api/workflow/{id}/paso9/reenviar-factura` | Multipart `file`, `comentario` | `SolicitudDetalle` | Rol `auxiliar` | Igual ruta o versionada |
| Documentos | GET | `/api/documentos/{documento_id}` | Id ruta | Binario | Sin restriccion en origen | `GET /api/documentos/{id}` con autenticacion y autorizacion real |

## 3. DTOs principales

### Auth

```text
LoginRequest: identificador, password
LoginResponse: usuario
UsuarioOut: id, nombre, email, username, rol, empresa_id, empresa_nombre
```

### Solicitudes

```text
SolicitudCreate: titulo, descripcion, frecuencia, aprobador_id
SolicitudUpdate: titulo, descripcion, frecuencia, aprobador_id
SolicitudListItem: id, radicado, titulo, etapa, estado, empresa_nombre, solicitante_nombre, created_at, updated_at
SolicitudDetalle: solicitud completa + usuarios asignados + proveedores + documentos + historial
```

### Acciones de workflow

```text
ComentarioRequest: comentario
ProveedorErpRequest: nombre, nit, identificador_erp, correo_contacto, telefono_contacto
ProveedoresNuevosRequest: candidatos[]
ProveedorNuevoItem: nombre, nit, correo_contacto, telefono_contacto, validado
CreadoEnErpRequest: creado_en_erp
SeleccionarProveedorRequest: proveedor_id, comentario
DecisionRequest: aprobado, comentario
ConformeRequest: confirmacion_erp
ObjetarContableRequest: motivo, comentario
```

## 4. Roles, estados y catalogos

### Roles

```text
solicitante, auxiliar, analista, aprobador, contable
```

### Estados de solicitud

```text
en_proceso, devuelta, completada
```

### Etapas de solicitud

```text
revision_auxiliar
devuelta_solicitante
revision_proveedores
seleccion_proveedor
carga_orden_compra
revision_oc_solicitante
oc_devuelta_auxiliar
revision_oc_aprobador
envio_proveedor
revision_anomalias
revision_factura_solicitante
factura_devuelta_analista
factura_aprobada_auxiliar
validacion_contable
factura_objetada_contable
completada
```

Nota: en el motor actual, `envio_proveedor` existe en el enum y la base de datos, pero el flujo operativo cambia directamente a `revision_anomalias` y registra el envio como historial/email. En .NET debe tratarse como evento de historial salvo decision explicita contraria.

### Origen de proveedor

```text
erp_existente, nuevo
```

### Tipos de documento

```text
cotizacion, certificado_existencia, rut, orden_compra, factura, soporte_contable
```

## 5. Vistas operativas de solicitudes

| Vista | Roles | Etapas | Campo asignacion |
| --- | --- | --- | --- |
| `solicitante_mias` | solicitante | Todas | `solicitante_id` |
| `solicitante_oc_revisar` | solicitante | `revision_oc_solicitante` | `solicitante_id` |
| `solicitante_facturas_revisar` | solicitante | `revision_factura_solicitante` | `solicitante_id` |
| `auxiliar_paso1` | auxiliar | `revision_auxiliar` | `auxiliar_id` |
| `auxiliar_paso2` | auxiliar | `revision_proveedores` | `auxiliar_id` |
| `auxiliar_paso4` | auxiliar | `carga_orden_compra` | `auxiliar_id` |
| `auxiliar_oc_devueltas` | auxiliar | `oc_devuelta_auxiliar` | `auxiliar_id` |
| `auxiliar_facturas_aprobadas` | auxiliar | `factura_aprobada_auxiliar` | `auxiliar_id` |
| `auxiliar_facturas_objetadas` | auxiliar | `factura_objetada_contable` | `auxiliar_id` |
| `analista_seleccion_proveedor` | analista | `seleccion_proveedor` | `analista_id` |
| `analista_revision_anomalias` | analista | `revision_anomalias`, `factura_devuelta_analista` | `analista_id` |
| `aprobador_pendientes` | aprobador | `revision_oc_aprobador` | `aprobador_id` |
| `contable_facturas_validar` | contable | `validacion_contable` | Ninguno |

## 6. Modelo de datos origen

| Tabla | Proposito | Observaciones de migracion SQL Server |
| --- | --- | --- |
| `empresas` | Empresas/areas con NIT | Migrar a `Empresas`; mantener unicidad de NIT. |
| `usuarios` | Usuarios, roles y empresa asociada | Reemplazar password plano por hash; mantener constraint rol/empresa. |
| `asignacion_contadores` | Round-robin para auxiliar/analista | Puede ser tabla transaccional o servicio con bloqueo/concurrencia. |
| `solicitudes` | Entidad central del workflow | Migrar enums PostgreSQL a constraints/catalogos en SQL Server. |
| `proveedores_candidatos` | Proveedores ERP o nuevos por solicitud | Mantener cascada por solicitud. |
| `documentos` | Archivos binarios | PostgreSQL `BYTEA` pasa a SQL Server `VARBINARY(MAX)` si se conserva en DB. |
| `solicitud_historial` | Bitacora de transiciones | Mantener etapa, accion, actor y comentario. |

No se deben generar ni ejecutar migraciones EF Core. El esquema SQL Server se creara con scripts T-SQL manuales, revisables y versionados.

## 7. Reglas de workflow detectadas

1. Al crear solicitud, se asigna auxiliar por round-robin y se genera radicado.
2. El auxiliar puede devolver al solicitante o avanzar a revision de proveedores.
3. Si el proveedor existe en ERP, se registra un unico proveedor seleccionado y se avanza a carga de orden de compra.
4. Si son proveedores nuevos, se requieren al menos 2 candidatos validados y marcados como creados en ERP para pasar a seleccion de proveedor.
5. El analista selecciona un proveedor validado y pasa a carga de orden de compra.
6. El auxiliar carga la orden de compra y la envia a revision del solicitante.
7. Si el solicitante objeta la orden de compra, vuelve al auxiliar; si aprueba y requiere aprobador, pasa al aprobador; si no requiere aprobador, se registra envio a proveedor y pasa a revision de anomalias.
8. El aprobador puede rechazar la orden y devolverla al auxiliar, o aprobar y disparar envio a proveedor.
9. El analista carga factura desde `revision_anomalias` o `factura_devuelta_analista`.
10. El solicitante aprueba o rechaza factura; si aprueba, pasa al auxiliar para envio a contabilidad.
11. El auxiliar envia factura y soportes a contabilidad.
12. Contabilidad confirma gestion ERP para completar o objeta por motivo controlado.
13. Si contabilidad objeta, el auxiliar reenvia factura y vuelve a revision del solicitante.

## 8. Riesgos y correcciones obligatorias en .NET

| Riesgo origen | Correccion en .NET |
| --- | --- |
| Password en texto plano | Usar hashing con ASP.NET Core Identity PasswordHasher, BCrypt o Argon2id. |
| Descarga de documentos sin autenticacion | Exigir usuario autenticado y validar acceso por solicitud/rol/asignacion. |
| CORS abierto `*` | Definir origenes permitidos por ambiente. |
| Autorizacion dependiente de usuario actual simple | Migrar a claims/roles y politicas de autorizacion. |
| Enums PostgreSQL acoplados al motor | Usar enums C# + conversion/control por scripts T-SQL. |
| Archivos en base de datos | Confirmar si se mantiene `VARBINARY(MAX)` o se mueve a storage externo en fase posterior. |

## 9. Siguiente paso

Completado: se creo y aplico `database/sqlserver/001_schema.sql` sobre `(localdb)\MSSQLLocalDB / GESTORPROVEEDORES`, manteniendo scripts T-SQL manuales y sin migraciones EF Core.

Completado: se iniciaron contratos y servicios de `Business` para autenticacion, usuario actual y catalogos. `Infrastructure` implementa consultas EF Core y verificacion con `PasswordHasher` contra `Usuarios.PasswordHash`. `WebApi` expone los endpoints iniciales `POST /api/auth/login`, `GET /api/catalogos/empresas` y `GET /api/catalogos/aprobadores?empresa_id={id}`.

Completado: se creo y aplico `database/sqlserver/002_seed_desarrollo.sql` con 2 empresas y 12 usuarios de prueba. La clave de desarrollo es `123`, almacenada como `PasswordHash` compatible con ASP.NET Core Identity.

Validacion ejecutada:

```text
POST /api/auth/login con crojas / 123 -> 200
GET /api/catalogos/empresas con X-User-Id: 1 -> 2 empresas
GET /api/catalogos/aprobadores?empresa_id=1 con X-User-Id: 1 -> opcion sin aprobacion + aprobador de empresa
```

Completado: se iniciaron casos de uso y endpoints de `Solicitudes` para listar, crear y consultar detalle. La creacion genera radicado con `dbo.RadicadoSeq`, asigna auxiliar por round-robin y registra historial `Solicitud radicada`.

Validacion ejecutada:

```text
POST /api/solicitudes con X-User-Id: 1 -> 201, etapa revision_auxiliar, estado en_proceso
GET /api/solicitudes/{id} con X-User-Id: 1 -> detalle con historial
GET /api/solicitudes?vista=solicitante_mias con X-User-Id: 1 -> incluye la solicitud creada
GET /api/solicitudes?vista=auxiliar_paso1 con X-User-Id: auxiliar asignado -> incluye la solicitud creada
```

Completado: se implemento `PUT /api/solicitudes/{id}` para editar y reenviar solicitudes devueltas. El endpoint exige rol `solicitante`, valida que el actor sea el solicitante asignado, requiere etapa `devuelta_solicitante`, vuelve la solicitud a `revision_auxiliar/en_proceso` y registra historial `Solicitud editada y reenviada`.

Validacion ejecutada:

```text
POST /api/solicitudes con X-User-Id: 1 -> solicitud creada
UPDATE SQL de prueba a devuelta_solicitante/devuelta -> simula devolucion del auxiliar
PUT /api/solicitudes/{id} con X-User-Id: 1 -> 200, etapa revision_auxiliar, estado en_proceso
GET /api/solicitudes/{id} -> historial incluye Solicitud editada y reenviada
```

Completado: se implemento `Workflow` paso 1 del auxiliar con `POST /api/workflow/{id}/paso1/devolver` y `POST /api/workflow/{id}/paso1/siguiente`. Ambos endpoints exigen rol `auxiliar`, validan que el actor sea el auxiliar asignado y requieren etapa `revision_auxiliar`.

Validacion ejecutada:

```text
POST /api/solicitudes con X-User-Id: 1 -> solicitud creada para devolver
POST /api/workflow/{id}/paso1/devolver con X-User-Id: auxiliar asignado -> 200, etapa devuelta_solicitante, estado devuelta
POST /api/solicitudes con X-User-Id: 1 -> solicitud creada para avanzar
POST /api/workflow/{id}/paso1/siguiente con X-User-Id: auxiliar asignado -> 200, etapa revision_proveedores, estado en_proceso
```

Completado: se inicio `Workflow` paso 2 del auxiliar con `POST /api/workflow/{id}/paso2/proveedor-erp`, `POST /api/workflow/{id}/paso2/proveedor-erp/siguiente` y `POST /api/workflow/{id}/paso2/proveedores-nuevos`. El camino ERP reemplaza proveedores previos, registra un proveedor `erp_existente` validado/creado/seleccionado y avanza a `carga_orden_compra`. El camino de proveedores nuevos descarta un ERP previo, hace upsert por orden 1..3 y conserva la solicitud en `revision_proveedores`.

Validacion ejecutada:

```text
POST /api/solicitudes con X-User-Id: 1 -> solicitud creada
POST /api/workflow/{id}/paso1/siguiente con X-User-Id: auxiliar asignado -> etapa revision_proveedores
POST /api/workflow/{id}/paso2/proveedor-erp -> proveedor erp_existente creado, seleccionado y proveedor_origen erp_existente
POST /api/workflow/{id}/paso2/proveedor-erp/siguiente -> etapa carga_orden_compra, estado en_proceso
POST /api/workflow/{id}/paso2/proveedores-nuevos -> 3 candidatos guardados, 2 validados, proveedor_origen nuevo
```

Completado: se cerro `Workflow` paso 2 de proveedores nuevos con `POST /api/workflow/{id}/paso2/proveedores/{proveedor_id}/creado-en-erp` y `POST /api/workflow/{id}/paso2/proveedores-nuevos/siguiente`. El marcado exige proveedor validado de la solicitud; el avance exige al menos 2 proveedores nuevos validados y todos los validados marcados como creados en ERP. Si no hay analista asignado, se asigna por round-robin y la solicitud pasa a `seleccion_proveedor/en_proceso`.

Validacion ejecutada:

```text
POST /api/solicitudes con X-User-Id: 1 -> solicitud creada
POST /api/workflow/{id}/paso1/siguiente con X-User-Id: auxiliar asignado -> etapa revision_proveedores
POST /api/workflow/{id}/paso2/proveedores-nuevos -> 3 candidatos guardados, 2 validados
POST /api/workflow/{id}/paso2/proveedores/{proveedor_id}/creado-en-erp para cada validado -> creado_en_erp true
POST /api/workflow/{id}/paso2/proveedores-nuevos/siguiente -> etapa seleccion_proveedor, estado en_proceso, analista asignado
```

Completado: se implemento `Workflow` paso 3 del analista con `POST /api/workflow/{id}/paso3/seleccionar`. El endpoint exige rol `analista`, valida que sea el analista asignado, requiere etapa `seleccion_proveedor`, permite seleccionar solo un proveedor validado de la solicitud, deja un unico proveedor seleccionado y pasa a `carga_orden_compra/en_proceso`.

Validacion ejecutada:

```text
POST /api/solicitudes con X-User-Id: 1 -> solicitud creada
POST /api/workflow/{id}/paso1/siguiente con X-User-Id: auxiliar asignado -> etapa revision_proveedores
POST /api/workflow/{id}/paso2/proveedores-nuevos -> candidatos registrados
POST /api/workflow/{id}/paso2/proveedores/{proveedor_id}/creado-en-erp -> validados marcados como creados
POST /api/workflow/{id}/paso2/proveedores-nuevos/siguiente -> etapa seleccion_proveedor, analista asignado
POST /api/workflow/{id}/paso3/seleccionar con X-User-Id: analista asignado -> etapa carga_orden_compra, un proveedor seleccionado
```

Completado: se implementaron los endpoints restantes del `Workflow` y descarga de documentos:

```text
POST /api/workflow/{id}/proveedores/{proveedor_id}/documento
POST /api/workflow/{id}/paso4/orden-compra
POST /api/workflow/{id}/paso5/solicitante
POST /api/workflow/{id}/paso5/aprobador
POST /api/workflow/{id}/paso6/factura
POST /api/workflow/{id}/paso7/solicitante
POST /api/workflow/{id}/paso8/contabilidad
POST /api/workflow/{id}/paso9/conforme
POST /api/workflow/{id}/paso9/objetar
POST /api/workflow/{id}/paso9/reenviar-factura
GET /api/documentos/{id}
```

Reglas migradas:

```text
Paso 4 carga orden de compra: auxiliar asignado, etapas carga_orden_compra u oc_devuelta_auxiliar, guarda documento orden_compra y pasa a revision_oc_solicitante.
Paso 5 solicitante: aprueba u objeta OC; si objeta exige comentario y vuelve a oc_devuelta_auxiliar/devuelta; si aprueba, va a aprobador cuando requiere_aprobacion o envia a proveedor y pasa a revision_anomalias.
Paso 5 aprobador: aprueba u objeta OC; si objeta exige comentario y vuelve a oc_devuelta_auxiliar/devuelta; si aprueba, envia a proveedor y pasa a revision_anomalias.
Paso 6 factura: analista asignado, etapas revision_anomalias o factura_devuelta_analista, guarda factura y pasa a revision_factura_solicitante.
Paso 7 solicitante: aprueba u objeta factura; si objeta exige comentario y vuelve a factura_devuelta_analista/devuelta; si aprueba, pasa a factura_aprobada_auxiliar.
Paso 8 contabilidad: auxiliar asignado, etapa factura_aprobada_auxiliar, guarda soportes contables y pasa a validacion_contable.
Paso 9 conforme: contable confirma gestion ERP y completa la solicitud.
Paso 9 objetar: contable objeta con motivo permitido y pasa a factura_objetada_contable/devuelta.
Paso 9 reenviar factura: auxiliar asignado reenvia factura objetada y vuelve a revision_factura_solicitante.
Documentos: descarga exige Bearer token y acceso por participante de la solicitud; mejora la version origen sin autenticacion.
WebApi: `X-User-Id` fue reemplazado por JWT Bearer con claims y policy de usuario activo.
WebApp: `/login` usa layout independiente sin menu, guarda sesion en cookie protegida y redirige al dashboard principal con menu.
```

Validacion ejecutada:

```text
Flujo sin aprobador: solicitud -> proveedor ERP -> documento proveedor -> orden compra -> revision solicitante aprobada -> factura -> factura aprobada -> contabilidad -> conforme -> completada/completada.
Descarga documento: GET /api/documentos/{id} con Bearer token del auxiliar asignado -> 200.
Rama con aprobador: aprobacion solicitante -> revision_oc_aprobador; aprobacion aprobador -> revision_anomalias.
Rama objecion contable: validacion_contable -> factura_objetada_contable/devuelta -> reenviar factura -> revision_factura_solicitante/en_proceso.
WebApp: `/login` renderiza sin menu; login `crojas/123` redirige a `/` con menu y sesion; logout vuelve a `/login`; validado sin overflow horizontal en 390x844 y 1440x900.
Dashboard WebApp: metricas, etapas y bandejas cargan desde `ISolicitudService` segun rol; se eliminaron los datos de demostracion.
Solicitudes WebApp: `/solicitudes` lista bandejas por rol con datos reales y cada fila navega a `/solicitudes/{id}`; el detalle muestra datos generales, proveedores, documentos e historial.
Panel administrativo WebApp: `/admin` centraliza crear, cargar/revisar por bandeja y consultar catalogos; `/solicitudes/nueva` crea una solicitud real y redirige al detalle creado.
Seguridad Business: `ObtenerDetalleAsync` valida acceso por participante/asignacion antes de devolver el detalle.
```

Siguiente paso: agregar acciones condicionales del workflow dentro del detalle de solicitud, empezando por paso 1 auxiliar y cargas documentales de proveedor/orden/factura.