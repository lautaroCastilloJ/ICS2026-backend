# Trabajo Práctico Integrador
## Desarrollo de Software
### Backend

## Introducción
Se desea desarrollar una plataforma de comercio electrónico (E-commerce). 
En esta primera etapa el objetivo es construir el módulo de Órdenes, permitiendo la gestión completa de éstas.

## Visión General del Producto
Del relevamiento preliminar se identificaron los siguientes requisitos:
- Los visitantes pueden consultar los productos sin necesidad de estar registrados o iniciar sesión.
- Para realizar un pedido se requiere el inicio de sesión.
- Una orden, para ser aceptada, debe incluir la información básica del cliente, envío y facturación.
- Antes de registrar la orden se debe verificar la disponibilidad de stock (o existencias) de los productos.
- Si la orden es exitosa hay que actualizar el stock de cada producto.
- Se deben poder consultar órdenes individuales o listar varias con posibilidad de filtrado.
- Será necesario el cambio de estado de una orden a medida que avanza en su ciclo de vida.
- Los administradores solo pueden gestionar los productos (alta, modificación y baja) y actualizar el estado de la orden.
- Los clientes pueden crear y consultar órdenes.

[Documento completo](https://frtutneduar.sharepoint.com/:b:/s/DSW2025/ETueAd4rTe1Gilj_Yfi64RYB5oz9s2dOamxKSfMFPREbiA?e=azZcwg) 

## Alcance para el Primer Parcial
> [!IMPORTANT]
> Del apartado `IMPLEMENTACIÓN` (Pag. 7), completo hasta el punto `6` (inclusive)


### Características de la Solución

- Lenguaje: C# 12.0
- Plataforma: .NET 8

## Configuración local

Los secretos **no** se versionan. `appsettings.json` solo contiene valores no sensibles
(`Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpireInMinutes`). La API no arranca si faltan
`ConnectionStrings:DefaultConnection` o `Jwt:Key`.

### 1. Secretos de desarrollo (User Secrets)

Desde la carpeta del backend:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Dsw2025TpiDb;Trusted_Connection=True;TrustServerCertificate=True;" --project Dsw2025Tpi.Api

# Clave JWT: al menos 32 bytes aleatorios (este comando genera 64, en Base64)
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 64 | tr -d '\n')" --project Dsw2025Tpi.Api
```

En PowerShell, la clave se puede generar con:
`[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))`

Los valores quedan en `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`, fuera del repositorio.

### Administradores

El registro público (`POST /api/auth/register`) crea **solo clientes**: no acepta rol.

- **Primer administrador:** al iniciar, la API crea los roles y, si no existe ningún
  administrador, crea uno con los datos de la sección `SeedAdmin`:

  ```bash
  dotnet user-secrets set "SeedAdmin:UserName" "admin" --project Dsw2025Tpi.Api
  dotnet user-secrets set "SeedAdmin:Email" "admin@ejemplo.com" --project Dsw2025Tpi.Api
  dotnet user-secrets set "SeedAdmin:Password" "<contraseña de 12+ caracteres>" --project Dsw2025Tpi.Api
  ```

  Si ya existe un administrador, no hace nada.
- **Más administradores:** los crea un administrador desde el panel (`/admin/users/create`)
  o con `POST /api/admin/users`, protegido con `[Authorize(Roles = "Administrador")]`.
  Exige contraseñas de al menos 12 caracteres y deja registro de quién creó a quién.
- **Bloqueo:** 5 intentos fallidos de login bloquean la cuenta durante 15 minutos.
- **Cambio de contraseña:** `POST /api/auth/change-password` (usuario autenticado, solo la
  propia). Pide la contraseña actual; los intentos fallidos suman al bloqueo. En el panel:
  `/admin/account/password`.

### 2. Configuración de desarrollo opcional

`appsettings.Development.json` está en `.gitignore`. Para crearlo, copiar `Dsw2025Tpi.Api/appsettings.Development.example.json`.

### 3. Base de datos

```bash
dotnet ef database update --project Dsw2025Tpi.Data --startup-project Dsw2025Tpi.Api
```

### Producción

Usar variables de entorno (`ConnectionStrings__DefaultConnection`, `Jwt__Key`) o un gestor de
secretos como Azure Key Vault. Nunca reutilizar la clave de desarrollo.
