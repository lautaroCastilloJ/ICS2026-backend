# Implementación de ImageUrl para Productos

## ?? Resumen de Cambios

Se ha implementado exitosamente el campo `ImageUrl` en el sistema de productos, permitiendo asociar imágenes a los productos mediante URLs externas.

---

## ? Archivos Modificados

### 1. **Domain Layer**
- ? `Dsw2025Tpi.Domain/Entities/Product.cs`
  - Agregada propiedad `ImageUrl` (nullable string)
  - Agregado parámetro `imageUrl` en método `Create()`
  - Agregado parámetro `imageUrl` en método `UpdateDetails()`
  - Agregado método privado `SetImageUrl()` para normalización

### 2. **Application Layer**

#### DTOs
- ? `Dsw2025Tpi.Application/Dtos/Products/ProductResponse.cs`
- ? `Dsw2025Tpi.Application/Dtos/Products/ProductRequest.cs`
- ? `Dsw2025Tpi.Application/Dtos/Products/ProductUpdateRequest.cs`
- ? `Dsw2025Tpi.Application/Dtos/Products/ProductListItemDto.cs`

#### Validators
- ? `Dsw2025Tpi.Application/Validators/ProductRequestValidator.cs`
  - Validación de longitud máxima (500 caracteres)
  - Validación de URL válida (http/https)
- ? `Dsw2025Tpi.Application/Validators/ProductUpdateRequestValidator.cs`
  - Mismas validaciones que ProductRequestValidator

#### Services
- ? `Dsw2025Tpi.Application/Services/ProductService.cs`
  - Actualizado `CreateAsync()` para incluir `ImageUrl`
  - Actualizado `UpdateAsync()` para incluir `ImageUrl`
  - Actualizado `GetPagedAsync()` para incluir `ImageUrl` en la proyección

#### Mappings
- ? `Dsw2025Tpi.Application/Mappings/MappingProfiles.cs`
  - Actualizado mapeo de `ProductRequest` ? `Product`

### 3. **Data Layer**
- ? `Dsw2025Tpi.Data/Configurations/ProductConfiguration.cs`
  - Configuración de columna `ImageUrl` (MaxLength: 500, Nullable)
- ? **Migración creada y aplicada**: `AddImageUrlToProduct`

---

## ?? Cómo Usar

### **Crear un Producto con Imagen**

```http
POST /api/products
Content-Type: application/json

{
  "sku": "LAP001",
  "internalCode": "LAPTOP-2024-001",
  "name": "Laptop Dell Inspiron 15",
  "description": "Laptop para uso general y productividad",
  "currentUnitPrice": 999.99,
  "stockQuantity": 25,
  "imageUrl": "https://i.imgur.com/example-laptop.jpg"
}
```

### **Actualizar un Producto con Imagen**

```http
PUT /api/products/{id}
Content-Type: application/json

{
  "sku": "LAP001",
  "internalCode": "LAPTOP-2024-001",
  "name": "Laptop Dell Inspiron 15",
  "description": "Laptop para uso general y productividad",
  "currentUnitPrice": 899.99,
  "stockQuantity": 30,
  "imageUrl": "https://i.imgur.com/new-laptop-image.jpg"
}
```

### **Respuesta del API**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sku": "LAP001",
  "internalCode": "LAPTOP-2024-001",
  "name": "Laptop Dell Inspiron 15",
  "description": "Laptop para uso general y productividad",
  "currentUnitPrice": 999.99,
  "stockQuantity": 25,
  "imageUrl": "https://i.imgur.com/example-laptop.jpg",
  "isActive": true
}
```

---

## ?? Validaciones

### ImageUrl es **OPCIONAL**
- Puede ser `null` o una cadena vacía
- Si se proporciona, debe cumplir con:
  - ? Máximo 500 caracteres
  - ? Ser una URL válida con esquema `http` o `https`

### Ejemplos de URLs Válidas

? **URLs VÁLIDAS:**
```
https://i.imgur.com/abc123.jpg
https://picsum.photos/200/300
https://example.com/images/products/laptop.png
http://localhost:3000/static/product-image.jpg
https://cdn.example.com/products/mouse-gamer.webp
```

? **URLs INVÁLIDAS:**
```
ftp://example.com/image.jpg        // Esquema no permitido
example.com/image.jpg              // Falta esquema
https://                           // URL incompleta
C:\images\product.jpg              // Ruta local
/images/product.jpg                // Ruta relativa
```

---

## ?? Servicios de Imágenes Recomendados (Gratuitos)

### Para Desarrollo/Testing:
1. **Imgur** - https://imgur.com/
   - Gratuito y sin registro requerido
   - Ideal para pruebas rápidas

2. **Picsum Photos** - https://picsum.photos/
   - Imágenes placeholder aleatorias
   - Ejemplo: `https://picsum.photos/200/300`

3. **Unsplash** - https://unsplash.com/
   - Imágenes de alta calidad gratuitas
   - Requiere registro

### Para Producción:
1. **Cloudinary** - Plan gratuito generoso
2. **AWS S3** - Con CloudFront CDN
3. **Azure Blob Storage** - Con Azure CDN

---

## ??? Base de Datos

### Columna Agregada
```sql
ALTER TABLE Products
ADD ImageUrl NVARCHAR(500) NULL;
```

### Query de Ejemplo
```sql
-- Ver productos con imágenes
SELECT Id, Name, ImageUrl 
FROM Products 
WHERE ImageUrl IS NOT NULL;

-- Actualizar ImageUrl de un producto existente
UPDATE Products 
SET ImageUrl = 'https://i.imgur.com/example.jpg'
WHERE Sku = 'LAP001';
```

---

## ?? Testing

### Test Manual con Swagger
1. Ejecutar la aplicación
2. Ir a `/swagger`
3. Probar el endpoint `POST /api/products` con el JSON de ejemplo
4. Verificar que la respuesta incluya el campo `imageUrl`

### Test con Postman/Insomnia
```json
{
  "sku": "TEST001",
  "internalCode": "TEST-001",
  "name": "Producto de Prueba",
  "description": "Descripción de prueba",
  "currentUnitPrice": 100.00,
  "stockQuantity": 10,
  "imageUrl": "https://picsum.photos/200/300"
}
```

---

## ?? Integración con Frontend

### React/Vue/Angular Example
```typescript
interface Product {
  id: string;
  sku: string;
  name: string;
  currentUnitPrice: number;
  imageUrl?: string; // Puede ser null
  isActive: boolean;
}

// Componente de producto
function ProductCard({ product }: { product: Product }) {
  const imageSrc = product.imageUrl || '/placeholder-image.png';
  
  return (
    <div className="product-card">
      <img 
        src={imageSrc} 
        alt={product.name}
        onError={(e) => {
          // Fallback si la imagen falla al cargar
          e.currentTarget.src = '/placeholder-image.png';
        }}
      />
      <h3>{product.name}</h3>
      <p>${product.currentUnitPrice}</p>
    </div>
  );
}
```

---

## ?? Próximos Pasos (Opcionales)

Si en el futuro quieres permitir **subir archivos** al servidor:

1. Crear endpoint de upload:
   ```csharp
   POST /api/products/{id}/upload-image
   ```

2. Guardar archivos en:
   - `wwwroot/images/products/`
   
3. Devolver la URL pública:
   ```
   https://tudominio.com/images/products/{productId}.jpg
   ```

4. Actualizar `ImageUrl` con la ruta generada

---

## ? Checklist de Implementación

- [x] Entidad `Product` actualizada
- [x] DTOs actualizados (Request, Response, Update, ListItem)
- [x] Validadores actualizados con validaciones de URL
- [x] Configuración de EF Core actualizada
- [x] ProductService actualizado (Create, Update, GetPaged)
- [x] Mapeos de AutoMapper actualizados
- [x] Migración creada y aplicada
- [x] Compilación exitosa
- [x] Base de datos actualizada

---

## ?? Soporte

Si tienes algún problema o necesitas implementar funcionalidades adicionales:
- Subir archivos al servidor
- Validación de formatos de imagen
- Optimización/redimensionamiento de imágenes
- Integración con servicios en la nube

¡La implementación está lista para usar! ??
