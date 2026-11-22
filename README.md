# ICS2026 – Backend (API .NET 8)

API REST con ASP.NET Core 8, Entity Framework Core (SQL Server), Identity y JWT.

## 🧰 Requisitos

| Herramienta | Versión | Notas |
|---|---|---|
| .NET SDK | 8.0.x | `dotnet --version` |
| Visual Studio 2022 | 17.8+ | Workload **"ASP.NET y desarrollo web"** (incluye SQL Server LocalDB) |
| SQL Server (opcional) | 2019 / 2022 (Express o Developer) | Solo si no usás LocalDB |
| SSMS | 19+ | Para ver y gestionar la base |
| dotnet-ef | 8.x | `dotnet tool install --global dotnet-ef --version 8.*` |
| Docker Desktop (opcional) | 4.x | Solo para ejecutar la API en contenedor (ver **Ejecución con Docker**) |

## 📁 Estructura

```
Dsw2025Tpi.sln
├── Dsw2025Tpi.Api          → Proyecto de inicio (controllers, Program.cs)
├── Dsw2025Tpi.Application  → Casos de uso / servicios
├── Dsw2025Tpi.Domain       → Entidades y reglas de negocio
├── Dsw2025Tpi.Data         → DbContext, Migrations, Identity, Repositorios
├── Dsw2025Tpi.Shared       → Utilidades comunes
├── Dsw2025Tpi.Tests        → Pruebas previas (TP3): rate limiting y seguridad de órdenes
└── tests/
    ├── Dsw2025Tpi.UnitTests         → Pruebas unitarias (xUnit + Moq)
    └── Dsw2025Tpi.IntegrationTests  → Pruebas de integración y E2E de API (SQLite en memoria)
```

## 🚀 Puesta en marcha

### 1. Clonar y restaurar

```bash
git clone <url-del-repo>
cd ICS2026-backend
dotnet restore
```

### 2. Configurar la cadena de conexión ⚠️ (obligatorio)

La cadena de conexión **no está en `appsettings.json`**. Cada colaborador la configura en su
máquina con **User Secrets**, que no se suben al repositorio.

Elegí **una** de estas dos opciones:

#### Opción A: LocalDB de Visual Studio (recomendada, no requiere instalar nada más)

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=Dsw2025TpiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project Dsw2025Tpi.Api
```

> Para comprobar que LocalDB está instalado, ejecutá `sqllocaldb info`. Tiene que aparecer `MSSQLLocalDB`.

#### Opción B: SQL Server instalado en tu PC

Instancia por defecto (`localhost`) con autenticación de Windows:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Dsw2025TpiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project Dsw2025Tpi.Api
```

Instancia con nombre (por ejemplo, SQL Express):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\\SQLEXPRESS;Database=Dsw2025TpiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project Dsw2025Tpi.Api
```

Con usuario y contraseña de SQL Server:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Dsw2025TpiDb;User Id=sa;Password=<tu-password>;TrustServerCertificate=True;MultipleActiveResultSets=true" --project Dsw2025Tpi.Api
```

> 💡 Desde Visual Studio también podés hacer clic derecho en `Dsw2025Tpi.Api` →
> **Administrar secretos de usuario** y editar el `secrets.json` directamente.

### 3. Configurar los demás secretos (JWT y administrador inicial)

```bash
dotnet user-secrets set "Jwt:Key" "<clave-aleatoria-de-al-menos-32-caracteres>" --project Dsw2025Tpi.Api
dotnet user-secrets set "SeedAdmin:UserName" "admin" --project Dsw2025Tpi.Api
dotnet user-secrets set "SeedAdmin:Email" "admin@ics2026.local" --project Dsw2025Tpi.Api
dotnet user-secrets set "SeedAdmin:Password" "<password-segura>" --project Dsw2025Tpi.Api
dotnet user-secrets set "SeedAdmin:DisplayName" "Administrador" --project Dsw2025Tpi.Api
```

Al final, tu `secrets.json` debería verse así:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Dsw2025TpiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": { "Key": "..." },
  "SeedAdmin": {
    "UserName": "admin",
    "Email": "admin@ics2026.local",
    "Password": "...",
    "DisplayName": "Administrador"
  }
}
```

> Si falta `Jwt:Key`, o tiene menos de 32 bytes, la API no arranca y muestra un error que lo indica.
> Si falta `SeedAdmin` y todavía no existe ningún administrador, pasa lo mismo.

### 4. (Opcional) appsettings de desarrollo

```bash
cp Dsw2025Tpi.Api/appsettings.Development.example.json Dsw2025Tpi.Api/appsettings.Development.json
```

Este archivo está en `.gitignore`, así que no se sube al repositorio.

### 5. Crear la base de datos (migraciones)

```bash
dotnet ef database update --project Dsw2025Tpi.Data --startup-project Dsw2025Tpi.Api
```

Esto crea `Dsw2025TpiDb` con todas las tablas (dominio + Identity).

### 6. Ejecutar la API

```bash
dotnet run --project Dsw2025Tpi.Api --launch-profile http
```

También podés abrir `Dsw2025Tpi.sln` en Visual Studio y presionar **F5** con el perfil `http`.

- API: http://localhost:5142
- Swagger: http://localhost:5142/swagger

En el primer arranque se crean los roles y el administrador inicial.

## 🗄️ Gestionar la base con SSMS

Abrí SSMS → **Conectar → Motor de base de datos**:

| Si usaste... | Nombre del servidor | Autenticación |
|---|---|---|
| LocalDB | `(localdb)\MSSQLLocalDB` | Windows |
| SQL Server (instancia por defecto) | `localhost` o `.` | Windows / SQL Server |
| SQL Express | `localhost\SQLEXPRESS` o `.\SQLEXPRESS` | Windows / SQL Server |

Marcá **"Trust server certificate"** (Confiar en el certificado del servidor) y conectate.
La base aparece como **`Dsw2025TpiDb`**.

## 🔁 Trabajar con migraciones

```bash
# Crear una migración nueva después de cambiar entidades
dotnet ef migrations add <NombreMigracion> --project Dsw2025Tpi.Data --startup-project Dsw2025Tpi.Api

# Aplicar migraciones pendientes (hacerlo después de cada git pull)
dotnet ef database update --project Dsw2025Tpi.Data --startup-project Dsw2025Tpi.Api
```

## 🧪 Tests

```bash
# Unitarias (xUnit + Moq)
dotnet test tests/Dsw2025Tpi.UnitTests

# Integración y E2E de API (WebApplicationFactory + SQLite en memoria)
dotnet test tests/Dsw2025Tpi.IntegrationTests

# Pruebas previas que no requieren SQL Server
dotnet test Dsw2025Tpi.Tests --filter "Category!=RequiresSqlServer"
```

Las pruebas unitarias y de integración no necesitan SQL Server ni secretos. Las de `OrdersSafetyTests`
están marcadas con `Category=RequiresSqlServer` porque usan un SQL Server real: se excluyen del pipeline y
solo se ejecutan a mano con una base configurada (`dotnet test Dsw2025Tpi.Tests`).

El pipeline ejecuta estos tres comandos en el job `run-tests` antes de construir y desplegar la imagen
(ver [Pipeline CI/CD](#-pipeline-cicd-github-actions)).

## 🩺 Problemas comunes

| Error | Solución |
|---|---|
| `A network-related or instance-specific error...` | Revisá el nombre del servidor en la cadena de conexión y que el servicio de SQL Server esté iniciado (`services.msc`) o ejecutá `sqllocaldb start MSSQLLocalDB` |
| `The certificate chain was issued by an authority that is not trusted` | Agregá `TrustServerCertificate=True` a la cadena |
| `Falta la configuracion 'Jwt:Key'` | Repetí el paso 3 |
| `dotnet ef` no se reconoce | `dotnet tool install --global dotnet-ef --version 8.*` y reabrí la terminal |
| `Invalid object name 'X'` | Faltan migraciones: ejecutá el paso 5 |

## 🌿 Convención de ramas

Este repositorio sigue un modelo de ramificación basado en ramas por característica y por corrección. **No se permiten commits directos sobre `main` ni `development`**: todo cambio debe integrarse mediante un Pull Request aprobado por al menos un integrante del equipo distinto del autor.

### Ramas principales

| Rama | Propósito | Protección |
|---|---|---|
| `main` | Código en producción, estable y desplegable | PR obligatorio + 1 aprobación + checks `run-tests` y `build-and-deploy` |
| `development` | Integración continua de funcionalidades terminadas | PR obligatorio + 1 aprobación + checks `run-tests` y `build-and-deploy` |

### Ramas de trabajo

| Tipo | Formato | Nace de | Se integra en |
|---|---|---|---|
| Funcionalidad | `feature/nombre-funcionalidad` | `development` | `development` |
| Corrección urgente | `hotfix/descripcion` | `main` | `main` y luego `development` |

**Reglas de nomenclatura:**
- Usar minúsculas y separar palabras con guiones: `feature/carrito-de-compras`.
- Incluir el ID de la tarjeta de Trello cuando exista: `feature/AT-03-extraer-servicio-pedidos`.
- Evitar tildes, espacios y caracteres especiales.

### Flujo para una funcionalidad

```bash
git switch development
git pull
git switch -c feature/nombre-funcionalidad
# ... cambios y commits ...
git push -u origin feature/nombre-funcionalidad
```

1. Abrir un Pull Request hacia `development`, con una descripción y el enlace a la tarjeta de Trello.
2. Solicitar la revisión de un integrante del equipo.
3. Una vez aprobado y con el pipeline en verde, fusionar el PR y eliminar la rama.

### Flujo para una corrección urgente (hotfix)

```bash
git switch main
git pull
git switch -c hotfix/descripcion
# ... corrección y commits ...
git push -u origin hotfix/descripcion
```

1. Abrir un Pull Request hacia `main` y obtener la aprobación de un par.
2. Fusionar el PR.
3. Abrir un segundo Pull Request de `main` hacia `development`, para que la corrección no se pierda en la próxima versión.

### Mensajes de commit

Se recomienda el formato *Conventional Commits*: `tipo: descripción breve (ID-tarjeta)`.

| Tipo | Uso |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de errores |
| `refactor` | Cambio de código sin alterar el comportamiento |
| `docs` | Cambios en la documentación |
| `test` | Agregado o modificación de pruebas |
| `chore` | Tareas de mantenimiento o configuración |

Ejemplo: `fix: corregir cálculo del total del carrito (AT-05)`

### Después de fusionar

```bash
git switch development
git pull
git branch -d feature/nombre-funcionalidad
```

## 🐳 Ejecución con Docker

La API puede ejecutarse dentro de un contenedor Docker, sin necesidad de tener instalado el SDK de .NET. La imagen se construye con un Dockerfile multi-stage: una etapa compila la solución con el SDK de .NET 8 y otra ejecuta solo los binarios publicados sobre el runtime de ASP.NET Core, lo que reduce el tamaño de la imagen en alrededor de un 70 %.

### Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y en ejecución (en Windows, con WSL2), o Docker Engine en Linux.
- Una instancia de SQL Server accesible por red (ver [Base de datos](#base-de-datos)).

### 1. Configurar las variables de entorno

La API no incluye secretos en el código ni en la imagen: se inyectan al ejecutar el contenedor mediante un archivo `.env`. Copiá la plantilla y completá los valores:

```bash
Copy-Item .env.example .env   # Windows (PowerShell)
cp .env.example .env          # Linux / macOS
```

| Variable | Descripción |
|---|---|
| `SeedAdmin__UserName` | Nombre de usuario del administrador inicial que la API crea al iniciar |
| `SeedAdmin__Password` | Contraseña del administrador inicial |
| `SeedAdmin__Email` | Correo del administrador inicial |
| `SeedAdmin__DisplayName` | Nombre visible del administrador inicial |
| `Jwt__Key` | Clave secreta para firmar los tokens JWT (aleatoria, de al menos 32 caracteres) |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a SQL Server |
| `Swagger__Enabled` | Opcional. `true` habilita Swagger aunque el entorno sea `Production` |

Para generar una clave JWT aleatoria:

```bash
# Windows (PowerShell)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))

# Linux / macOS
openssl rand -base64 48
```

**Formato del archivo `.env`:** una variable por línea, sin espacios alrededor del `=` y sin comillas. El doble guion bajo (`__`) equivale a los dos puntos de `appsettings.json` (por ejemplo, `Jwt__Key` corresponde a `Jwt:Key`).

> ⚠️ El archivo `.env` contiene credenciales y está excluido del repositorio por el `.gitignore`. Nunca debe commitearse. Para verificarlo: `git check-ignore -v .env`.

### Base de datos

Dentro del contenedor, `localhost` se refiere al propio contenedor y no a tu PC, y la autenticación de Windows (`Trusted_Connection=True`) no está disponible. LocalDB tampoco funciona desde un contenedor. Tenés dos opciones:

**Opción A – SQL Server instalado en tu PC:**

1. Habilitá **TCP/IP en el puerto 1433** (*SQL Server Configuration Manager → Protocols → TCP/IP*, y en *IP Addresses → IPAll* el puerto `1433`) y reiniciá el servicio.
2. Habilitá la **autenticación de SQL Server** (modo mixto).
3. Creá un usuario SQL con permisos solo sobre la base de la API.
4. Permití conexiones entrantes al puerto 1433 en el firewall.

```
Server=host.docker.internal,1433;Database=Dsw2025TpiDb;User Id=<usuario>;Password=<contraseña>;TrustServerCertificate=True;
```

`host.docker.internal` es un nombre que Docker Desktop traduce a la dirección de tu PC. **En Linux no existe por defecto**: agregá `--add-host=host.docker.internal:host-gateway` al `docker run`.

**Opción B – Base de datos de Azure:** usá la cadena de conexión ADO.NET de Azure SQL y agregá tu IP pública en el firewall del servidor (*SQL Server → Seguridad → Redes*).

### 2. Construir la imagen

Desde la raíz del repositorio (donde está `Dsw2025Tpi.sln`):

```bash
docker build -t dsw2025tpi-api:local .
```

### 3. Ejecutar el contenedor

```bash
docker run -d -p 8080:8080 --name mi-api-backend --env-file .env -e ASPNETCORE_ENVIRONMENT=Development dsw2025tpi-api:local
```

| Parámetro | Significado |
|---|---|
| `-d` | Ejecuta el contenedor en segundo plano |
| `-p 8080:8080` | Expone el puerto 8080 del contenedor en el puerto 8080 de tu PC |
| `--name` | Nombre del contenedor |
| `--env-file .env` | Carga las variables de entorno del archivo `.env` |
| `-e ASPNETCORE_ENVIRONMENT=Development` | Ejecuta en modo Development (habilita Swagger). Si se omite, la imagen usa `Production` |

### 4. Verificar que funciona

```bash
docker logs -f mi-api-backend
```

Los logs deben mostrar `Now listening on: http://[::]:8080` y el entorno activo (`Hosting environment: Development`). Salí de los logs con `Ctrl + C`: el contenedor sigue corriendo.

| Recurso | URL |
|---|---|
| Estado de la API | http://localhost:8080/health (responde `Healthy`) |
| Swagger (en Development) | http://localhost:8080/swagger |

```bash
curl http://localhost:8080/health        # Linux / macOS
curl.exe http://localhost:8080/health    # Windows (PowerShell)
```

Si la base de datos de Azure está pausada (oferta gratuita), la API reintenta la conexión automáticamente durante el arranque: puede tardar alrededor de un minuto en mostrar `Now listening on`.

### 5. Detener y eliminar el contenedor

```bash
docker rm -f mi-api-backend
```

### Problemas frecuentes con Docker

| Síntoma | Causa probable | Solución |
|---|---|---|
| La API se detiene con *Falta la configuración 'Jwt:Key'* | No se pasó el archivo `.env` o falta la variable | Ejecutá con `--env-file .env` y verificá que `Jwt__Key` tenga valor |
| Docker rechaza el `.env` por espacios | Hay espacios alrededor del `=` | Escribí cada línea como `VARIABLE=valor` |
| Error de conexión a SQL Server | La cadena usa `localhost`, `(localdb)` o autenticación de Windows | Usá `host.docker.internal,1433` con un usuario SQL, o la base de Azure |
| Error **40615** (*Client with IP address … is not allowed*) | Tu IP no está en el firewall de Azure SQL | Agregala en *SQL Server → Seguridad → Redes* y esperá hasta 5 minutos |
| Error **40613** (*Database … is not currently available*) | La base gratuita de Azure estaba pausada | Esperá: la API reintenta sola |
| Swagger devuelve 404 | El contenedor corre en modo Production | Ejecutá con `-e ASPNETCORE_ENVIRONMENT=Development` o `-e Swagger__Enabled=true` |
| `docker` no responde o no se conecta | Docker Desktop está cerrado | Abrí Docker Desktop y esperá *Engine running* |

---

## 🚀 Pipeline CI/CD (GitHub Actions)

El workflow `.github/workflows/deploy.yml` ejecuta la suite de pruebas como *Quality Gate* y, solo si pasa, construye la imagen Docker de la API, la publica en Docker Hub y la despliega en Azure App Service.

### Cuándo se ejecuta

| Evento | Qué hace | Propósito |
|---|---|---|
| `pull_request` hacia `development` o `main` | Ejecuta las pruebas y construye la imagen, **sin publicarla** | Validar el cambio antes de fusionar (CI) |
| `push` a `development` o `main` (al fusionar un PR) | Ejecuta las pruebas, construye, publica en Docker Hub y despliega en Azure | Entregar la versión aprobada (CD) |
| `workflow_dispatch` (*Actions → Run workflow*) | Igual que un push, sobre la rama elegida | Despliegue manual o verificación en QA |

Como `development` y `main` están protegidas, el evento `push` solo ocurre al fusionar un Pull Request aprobado.

### Jobs del pipeline

El workflow tiene dos jobs. `build-and-deploy` declara `needs: run-tests`: si falla cualquier prueba, GitHub lo marca como *skipped* y no se construye, publica ni despliega la imagen.

**1. `run-tests` (Quality Gate)**

1. **Obtener el código** de la rama e **instalar .NET 8.**
2. **Restaurar y compilar** la solución en Release.
3. **Ejecutar las pruebas por nivel**: unitarias, integración/E2E y las previas que no requieren SQL Server. Cada nivel corre aunque falle el anterior, para reportar todos los fallos en una sola ejecución.
4. **Publicar los resultados** (`.trx`) como artefacto `test-results`, también cuando fallan.
5. **Notificar el fallo** con una anotación `::error` y un resumen en la página de la ejecución.

**2. `build-and-deploy`** *(solo si `run-tests` terminó en verde)*

1. **Construir la imagen Docker**, etiquetada con el SHA del commit y con `latest`.
2. **Iniciar sesión en Docker Hub** *(solo fuera de los PR)*.
3. **Subir la imagen a Docker Hub** *(solo fuera de los PR)*.
4. **Desplegar el contenedor en Azure App Service** con la imagen del SHA *(solo fuera de los PR)*.

Etiquetar la imagen con el SHA del commit permite rastrear qué código está desplegado y volver a una versión anterior desplegando su etiqueta.

### Secretos del repositorio

Configurados en *Settings → Secrets and variables → Actions*. Solo el dueño del repositorio puede administrarlos.

| Secreto | Contenido |
|---|---|
| `DOCKERHUB_USERNAME` | Usuario de Docker Hub |
| `DOCKERHUB_TOKEN` | Token de acceso de Docker Hub con permisos *Read, Write, Delete* |
| `APP_SERVICE_NAME` | Nombre de la Web App en Azure |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Contenido completo del *publish profile* del App Service |

### Protección asociada

El ruleset `protection-main-development` exige, para fusionar en `development` o `main`, la aprobación de al menos un integrante distinto del autor y que los checks **`run-tests`** y **`build-and-deploy`** terminen con éxito.

> ℹ️ Exigir `run-tests` es indispensable: cuando las pruebas fallan, `build-and-deploy` queda *skipped*, y GitHub no bloquea el merge por un check requerido omitido.

---

## ☁️ Despliegue en Azure

| Recurso | Nombre | Detalle |
|---|---|---|
| Grupo de recursos | `rg-ics2026` | Región Brazil South |
| App Service | `ics2026-api-4k3` | Plan Free F1, contenedor Linux |
| Servidor SQL | `ics2026-sql-4k3` | Autenticación SQL, firewall por IP |
| Base de datos | `Dsw2025TpiDb` | Oferta gratuita (se pausa por inactividad) |
| Imagen | `lautarocastilloj/dsw2025tpi-api` | Repositorio público en Docker Hub |

**API pública:** https://ics2026-api-4k3-a4gpczh2bvgphkcx.brazilsouth-01.azurewebsites.net

| Recurso | Ruta |
|---|---|
| Estado | `/health` |
| Swagger | `/swagger` |

### Configuración del App Service

Variables en *Configuración → Variables de entorno*:

| Variable | Pestaña | Propósito |
|---|---|---|
| `WEBSITES_PORT=8080` | Configuración de la aplicación | Indica a Azure en qué puerto escucha el contenedor |
| `Jwt__Key` | Configuración de la aplicación | Clave JWT de producción (distinta de la local) |
| `SeedAdmin__*` | Configuración de la aplicación | Administrador inicial de producción |
| `Swagger__Enabled=true` | Configuración de la aplicación | Habilita Swagger en Production |
| `DefaultConnection` (tipo SQLAzure) | Cadenas de conexión | Cadena de conexión a Azure SQL |

### Cómo se despliega una versión nueva

1. Abrí un Pull Request hacia `development` y obtené la aprobación de un par.
2. Fusioná el PR: el pipeline publica la imagen y la despliega automáticamente.
3. Esperá 2 o 3 minutos y verificá `/health`.

Para desplegar una rama sin fusionarla (por ejemplo, durante el QA): *Actions → CI/CD - Build and Deploy to Azure App Service → Run workflow* → elegí la rama.

### Notas

- El plan **F1** detiene la aplicación tras un período sin tráfico: la primera petición posterior puede tardar varios segundos.
- La base gratuita se **pausa** por inactividad y tarda alrededor de un minuto en reanudarse. La API usa reintentos ante fallos transitorios (`EnableRetryOnFailure`) para tolerarlo.
- Los logs del contenedor se consultan en *App Service → Supervisión → Secuencia de registro* o en *Centro de implementación → Registros*.