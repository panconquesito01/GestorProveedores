# Plan de despliegue Azure — GestorProveedores (tier gratuito)

**Estado:** Borrador — pendiente de aprobación  
**Fecha:** 2026-08-19  
**Modo:** MODIFY (proyecto .NET existente)  
**Presupuesto:** $0 / mes (servicios Always Free + límites de cuenta gratuita)

---

## 1. Resumen del proyecto

| Componente | Proyecto | Rol |
|---|---|---|
| Frontend | `GestorProveedores.WebApp` | Blazor Server (.NET 10) — accede directo a BD vía Infrastructure |
| API | `GestorProveedores.WebApi` | REST + JWT — integraciones futuras |
| Base de datos | `database/sqlserver/*.sql` | Esquema T-SQL manual (sin EF migrations) |
| Repo | `github.com/panconquesito01/GestorProveedores` | Origen del pipeline |

**Conexión local actual:** `(localdb)\MSSQLLocalDB` / `GESTORPROVEEDORES`

---

## 2. Arquitectura objetivo (gratis)

```
┌─────────────────────────────────────────────────────────────┐
│  GitHub Actions (CI/CD — 2.000 min/mes gratis)              │
│  ├── build + test                                           │
│  ├── deploy DB (sqlcmd → Azure SQL)                         │
│  ├── deploy WebApp → App Service F1                         │
│  └── deploy WebApi  → App Service F1 (otra región)          │
└─────────────────────────────────────────────────────────────┘
         │                              │
         ▼                              ▼
┌──────────────────┐          ┌──────────────────┐
│ App Service F1   │          │ App Service F1   │
│ WebApp (Blazor)  │          │ WebApi (REST)    │
│ Región A         │          │ Región B         │
└────────┬─────────┘          └────────┬─────────┘
         │                              │
         └──────────────┬───────────────┘
                        ▼
              ┌──────────────────┐
              │ Azure SQL        │
              │ Tier: Free       │
              │ DB: GESTORPROV.. │
              └──────────────────┘
```

### Por qué dos regiones para App Service F1

Azure permite **solo 1 plan F1 (Free) por suscripción por región**.  
Para hospedar WebApp y WebApi gratis hay que usar **dos regiones distintas** (por ejemplo `East US` y `West US 2`).

> **Alternativa mínima:** desplegar solo WebApp en F1 (la UI ya usa Business/Infrastructure directo). Dejar WebApi para una fase posterior.

---

## 3. Servicios Azure a crear (Portal)

### 3.0 Prerrequisitos

- [ ] Cuenta Azure (gratuita o suscripción existente)
- [ ] Acceso de propietario o colaborador a la suscripción
- [ ] Repo GitHub conectado (`panconquesito01/GestorProveedores`)

**Región recomendada principal:** `East US` (o la más cercana con cuota disponible)

---

### 3.1 Grupo de recursos

| Campo | Valor sugerido |
|---|---|
| Nombre | `rg-gestorproveedores-dev` |
| Región | `East US` |
| Etiquetas | `env=dev`, `project=gestorproveedores`, `cost=free` |

**Portal:** Home → Resource groups → Create

---

### 3.2 Azure SQL Database (Free)

**Portal:** Create a resource → Azure SQL → Single database

| Paso | Valor |
|---|---|
| Resource group | `rg-gestorproveedores-dev` |
| Database name | `GESTORPROVEEDORES` |
| Server name | `sql-gestorprov-dev` (único globalmente) |
| Compute + storage | **Free** (100.000 vCore-seg/mes, 32 GB) |
| Authentication | SQL authentication (más simple para pipeline inicial) |
| Admin login | `sqladmin` (o el que prefieras) |
| Password | Generar fuerte → guardar en GitHub Secrets |

**Después de crear:**

- [ ] Networking → **Allow Azure services and resources to access this server** = Yes
- [ ] (Opcional dev) Agregar tu IP pública para pruebas manuales con SSMS/Azure Data Studio
- [ ] Crear la base `GESTORPROVEEDORES` si el asistente no la creó ya

**Connection string (formato):**

```
Server=tcp:sql-gestorprov-dev.database.windows.net,1433;Initial Catalog=GESTORPROVEEDORES;User ID=sqladmin;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=True
```

---

### 3.3 App Service — WebApp (Blazor)

**Portal:** Create a resource → Web App

| Campo | Valor sugerido |
|---|---|
| Name | `app-gestorprov-web-dev` |
| Publish | Code |
| Runtime stack | `.NET 10 (LTS)` o `.NET 9` si 10 no aparece aún |
| Operating System | **Windows** (Blazor Server) |
| Region | `East US` |
| Pricing plan | **Free F1** (crear nuevo plan `asp-gestorprov-web-dev-f1`) |

**Application settings (Configuration → Application settings):**

| Name | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | *(connection string SQL)* |
| `Jwt__Issuer` | `GestorProveedores` |
| `Jwt__Audience` | `GestorProveedores.WebApi` |
| `Jwt__SigningKey` | *(32+ bytes, secreto)* |
| `ASPNETCORE_ENVIRONMENT` | `Development` o `Staging` |

---

### 3.4 App Service — WebApi (opcional en fase 1)

Mismos pasos, pero:

| Campo | Valor sugerido |
|---|---|
| Name | `app-gestorprov-api-dev` |
| Region | **`West US 2`** (u otra distinta a la WebApp) |
| Plan | **Free F1** nuevo (`asp-gestorprov-api-dev-f1`) |

**Application settings adicionales:**

| Name | Value |
|---|---|
| `Cors__AllowedOrigins__0` | `https://app-gestorprov-web-dev.azurewebsites.net` |

---

### 3.5 CI/CD — GitHub Actions (recomendado, $0)

No requiere recursos extra en Azure Portal. Configurar en GitHub:

**Secrets** (Settings → Secrets and variables → Actions):

| Secret | Uso |
|---|---|
| `AZURE_SQL_CONNECTION_STRING` | Pipeline de base de datos |
| `AZURE_WEBAPP_PUBLISH_PROFILE_WEB` | Deploy WebApp |
| `AZURE_WEBAPP_PUBLISH_PROFILE_API` | Deploy WebApi |
| `JWT_SIGNING_KEY` | Inyectar en App Settings si se prefiere desde pipeline |

**Publish Profile:** App Service → Overview → Download publish profile

**Service Principal (alternativa moderna a publish profile):**

1. Portal → Microsoft Entra ID → App registrations → New
2. Crear federated credential para GitHub OIDC
3. Asignar rol **Website Contributor** sobre los App Services

*(Para empezar rápido, publish profile es suficiente en dev.)*

---

### 3.6 Alternativa: Azure DevOps Pipelines

Si prefieres Azure DevOps en lugar de GitHub Actions:

| Recurso | Gratis |
|---|---|
| Organización | dev.azure.com |
| Parallel jobs | 1 job Microsoft-hosted |
| Minutos | 1.800 min/mes (repo privado) |

**Portal/DevOps:**

- [ ] Crear organización y proyecto `GestorProveedores`
- [ ] Service connection → Azure Resource Manager
- [ ] Variable group con connection string y JWT
- [ ] Environments: `dev`

---

## 4. Pipelines a implementar (siguiente fase en repo)

### Pipeline 1 — CI (build + test)

```
Trigger: push/PR a main
Steps:
  - dotnet restore
  - dotnet build
  - dotnet test
```

### Pipeline 2 — Base de datos

```
Trigger: cambios en database/sqlserver/**
Steps:
  - sqlcmd / AzureSqlDeployment@1
  - Ejecutar 001_schema.sql
  - Ejecutar 002_seed_desarrollo.sql (solo dev)
```

> **Nota:** `001_schema.sql` asume que la BD ya existe. En Azure SQL la BD se crea al provisionar el servidor; ajustar el script si hace falta quitar el `THROW` de existencia.

### Pipeline 3 — Deploy aplicaciones

```
Trigger: push a main (después de CI verde)
Steps:
  - dotnet publish WebApp → deploy F1 Web
  - dotnet publish WebApi → deploy F1 API
```

---

## 5. Checklist Portal (orden recomendado)

| # | Tarea | Estado |
|---|---|---|
| 1 | Verificar suscripción activa (Free/PAYG) | ☐ |
| 2 | Crear `rg-gestorproveedores-dev` | ☐ |
| 3 | Crear Azure SQL Server + DB tier **Free** | ☐ |
| 4 | Configurar firewall SQL (Azure services + tu IP) | ☐ |
| 5 | Crear App Service WebApp F1 | ☐ |
| 6 | Crear App Service WebApi F1 (región distinta) | ☐ |
| 7 | Configurar App Settings en ambas apps | ☐ |
| 8 | Descargar Publish Profiles | ☐ |
| 9 | Crear GitHub Secrets | ☐ |
| 10 | Ejecutar scripts SQL manualmente (primera vez) | ☐ |
| 11 | Probar URL WebApp | ☐ |
| 12 | Crear YAML de pipelines en repo | ☐ |

---

## 6. Límites del tier gratuito (importante)

| Servicio | Límite | Impacto |
|---|---|---|
| App Service F1 | 60 CPU min/día, 1 GB RAM, sin SLA | Solo dev/demo; Blazor Server puede ser lento |
| App Service F1 | 1 plan/región/suscripción | Obliga a 2 regiones o 1 sola app |
| Azure SQL Free | 100k vCore-seg/mes, 32 GB | Suficiente para dev |
| GitHub Actions | 2.000 min/mes (privado) | Suficiente para CI/CD ligero |
| Key Vault | No incluido en free | Usar App Settings + GitHub Secrets en dev |

---

## 7. Variables de entorno (referencia)

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__SigningKey
Cors__AllowedOrigins__0          # solo WebApi
ASPNETCORE_ENVIRONMENT
```

---

## 8. Decisiones pendientes

- [ ] ¿Región principal? (default: East US)
- [ ] ¿Desplegar WebApi en fase 1 o solo WebApp?
- [ ] ¿GitHub Actions o Azure DevOps?
- [ ] ¿Entorno `Development` o `Staging` en Azure?

---

## 9. Próximos pasos (después de aprobar este plan)

1. Completar checklist Portal (sección 5)
2. Generar archivos `.github/workflows/*.yml` en el repo
3. Ajustar `001_schema.sql` para Azure SQL si aplica
4. Ejecutar validación (`azure-validate`) y primer despliegue

---

## 10. Aprobación

- [ ] Usuario aprueba arquitectura free tier
- [ ] Usuario confirma región y CI (GitHub vs DevOps)
- [ ] Listo para generar pipelines en el repo
