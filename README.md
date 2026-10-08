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

## 📁 Estructura

```
Dsw2025Tpi.sln
├── Dsw2025Tpi.Api          → Proyecto de inicio (controllers, Program.cs)
├── Dsw2025Tpi.Application  → Casos de uso / servicios
├── Dsw2025Tpi.Domain       → Entidades y reglas de negocio
├── Dsw2025Tpi.Data         → DbContext, Migrations, Identity, Repositorios
├── Dsw2025Tpi.Shared       → Utilidades comunes
└── Dsw2025Tpi.Tests        → Tests
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
```

Al final, tu `secrets.json` debería verse así:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Dsw2025TpiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": { "Key": "..." },
  "SeedAdmin": { "UserName": "admin", "Email": "admin@ics2026.local", "Password": "..." }
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
dotnet test
```

## 🩺 Problemas comunes

| Error | Solución |
|---|---|
| `A network-related or instance-specific error...` | Revisá el nombre del servidor en la cadena de conexión y que el servicio de SQL Server esté iniciado (`services.msc`) o ejecutá `sqllocaldb start MSSQLLocalDB` |
| `The certificate chain was issued by an authority that is not trusted` | Agregá `TrustServerCertificate=True` a la cadena |
| `Falta la configuracion 'Jwt:Key'` | Repetí el paso 3 |
| `dotnet ef` no se reconoce | `dotnet tool install --global dotnet-ef --version 8.*` y reabrí la terminal |
| `Invalid object name 'X'` | Faltan migraciones: ejecutá el paso 5 |

## Convención de Ramas