# AsistCC - SaaS Cuentas Corrientes

Sistema SaaS de gestión de cuentas corrientes para comercios y sus clientes, desarrollado con .NET 8 y Blazor WebAssembly.

## 📋 Descripción

AsistCC es una plataforma que permite a comercios gestionar las cuentas corrientes de sus clientes, registrar movimientos (debe/haber), establecer límites de crédito, y proporcionar a los clientes acceso para consultar su estado de cuenta.

## 🚀 Tecnologías

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - API RESTful
- **Entity Framework Core** - ORM para acceso a datos
- **SQL Server LocalDB** - Base de datos
- **JWT Bearer Authentication** - Autenticación y autorización
- **Swagger/OpenAPI** - Documentación de API

### Frontend
- **Blazor WebAssembly** - SPA Framework
- **MudBlazor** - Componentes Material Design
- **Blazored.LocalStorage** - Almacenamiento local
- **HttpClient** - Comunicación con API

## 📦 Estructura del Proyecto

```
SaaSCuentasCorrientes/
├── SaasACC.Model/              # Entidades y DTOs
│   ├── Entities/               # Modelos de dominio
│   └── Servicios/Login/        # DTOs de autenticación
├── SaasACC.Domain/             # Lógica de dominio
├── SaasACC.Application/        # Servicios y casos de uso
│   ├── Services/               # AuthService, ClienteService
│   └── Interfaces/             # Interfaces de repositorios
├── SaasACC.Infrastructure/     # Acceso a datos
│   └── Repositories/           # Implementación de repositorios
├── SaasACCAPI.api/             # Web API
│   ├── Controllers/            # Endpoints REST
│   └── Program.cs              # Configuración
└── SaasACC.BlazorWasm/         # Frontend SPA
    ├── Pages/                  # Páginas Razor
    ├── Components/             # Componentes reutilizables
    └── Services/               # Servicios del cliente
```

## ✨ Funcionalidades Implementadas

### 🔐 Autenticación y Registro

#### Sistema de Login con Multicomercio
- Login con email y password (único por usuario)
- **Un usuario puede ser cliente de múltiples comercios**
- Generación de JWT Token (válido por 24 horas)
- Token incluye lista de comercios activos (`ComercioIds`)
- Roles: Admin, UsuarioComercio, Cliente
- Almacenamiento seguro del token en localStorage
- Redirección automática según rol del usuario
- **Para clientes**: Selector de comercio después del login si tiene múltiples vinculaciones

#### Sistema de Registro con Dos Botones Separados
- **Página de selección**: Dos botones claramente diferenciados
  - 🏢 **Registrar Comercio**: Para negocios
  - 👤 **Registrar Cliente**: Para clientes de comercios existentes

##### Registro de Comercio
- Formulario completo dividido en dos secciones:
  - **Datos del Comercio**: Nombre, email, teléfono, dirección
  - **Datos del Administrador**: Nombre, email, password
- Validaciones en tiempo real
- Creación automática de usuario administrador
- Login automático después del registro
- Generación de token JWT
- Redirección a dashboard de administración

##### Registro de Cliente (Autogestión)
- Selector de comercio (dropdown con lista de comercios activos)
- Formulario de datos personales:
  - Nombre, apellido, email, teléfono, password
  - DNI (opcional)
  - Dirección (opcional)
- **Flujo de Aprobación**:
  - Cliente queda en estado "Pendiente" al registrarse
  - Creación de usuario con rol "Cliente" (si no existe)
  - **Si usuario ya existe**: Valida contraseña y crea solo la vinculación al comercio
  - NO puede hacer login en ese comercio hasta ser aprobado
  - Cuenta corriente se crea solo cuando el comercio lo aprueba
- Mensaje de confirmación y redirección a login

### 👥 Gestión de Clientes (Admin)

#### Sistema de Aprobación de Clientes
- **Diferenciación por Origen**:
  - 🏢 **Administración**: Clientes creados por el admin (auto-aprobados)
  - 👤 **Autogestión**: Clientes auto-registrados (requieren aprobación)
- **Estados de Cliente**:
  - ⏳ Pendiente: Esperando aprobación del comercio
  - ✅ Activo: Aprobado y con acceso al sistema
  - ❌ Inactivo: Rechazado o desactivado
- **Flujo de Aprobación**:
  - Clientes auto-registrados quedan pendientes
  - Admin puede aprobar o rechazar desde interfaz dedicada
  - Página específica para revisar pendientes
  - Filtros por estado en lista de clientes
  - Cuenta corriente se crea solo al aprobar
- **Auditoría**: Registro de quién y cuándo aprobó/rechazó cada cliente

#### Gestión General
- CRUD completo de clientes
- Listado de clientes por comercio
- Búsqueda y filtrado por estado
- Soft delete (desactivación sin borrado físico)
- Creación automática de cuenta corriente (al aprobar o para admin-created)
- **Detección de clientes existentes**: Al agregar cliente, si el email ya existe en el sistema, reutiliza el usuario y crea solo la vinculación

### 🔗 Cliente Multicomercio ⭐ NUEVO

#### Modelo de Vinculación
- **Un usuario puede ser cliente de múltiples comercios**
- Email único global (un solo login para todos los comercios)
- Cada vinculación (Cliente) tiene su propia:
  - Estado de aprobación independiente
  - Cuenta corriente separada
  - Historial de movimientos independiente
- Cliente actúa como **tabla intermedia** entre Usuario y Comercio

#### Vinculación de Usuario a Nuevo Comercio
- Cliente autenticado puede vincularse a otros comercios
- Flujo de autogestión con aprobación del comercio
- Mantiene su email y contraseña únicos
- Acceso centralizado a múltiples cuentas

#### Comercio Agrega Cliente Existente
- Al crear cliente, el sistema detecta si el email ya existe
- Si existe: Reutiliza el usuario y crea solo la vinculación al comercio
- Si no existe: Crea nuevo usuario + vinculación
- Evita duplicación de usuarios en el sistema

### 💰 Cuentas Corrientes
- Creación automática al registrar cliente
- Gestión de límite de crédito
- Cálculo automático de saldo
- Estado de cuenta (bloqueada/activa)
- Observaciones

### 📊 Movimientos
- Registro de movimientos tipo Debe y Haber
- Cálculo automático de saldo
- Historial de movimientos por cliente

## 🌐 Endpoints API

### Autenticación
```
POST   /api/auth/login                  # Iniciar sesión
POST   /api/auth/register/comercio      # Registrar comercio nuevo
POST   /api/auth/register/cliente       # Registrar cliente
```

### Comercios
```
GET    /api/comercios                   # Listar comercios activos
```

### Clientes
```
GET    /api/clientes?estadoId={id}      # Listar clientes (filtro opcional por estado)
GET    /api/clientes/{id}               # Obtener cliente por ID
GET    /api/clientes/pendientes         # Listar solo clientes pendientes de aprobación
GET    /api/clientes/mis-comercios      # Listar comercios del usuario autenticado ⭐ NUEVO
POST   /api/clientes                    # Crear cliente (admin - auto-aprobado, detecta usuarios existentes)
POST   /api/clientes/vincular           # Vincular usuario a nuevo comercio ⭐ NUEVO
POST   /api/clientes/{id}/aprobar       # Aprobar cliente pendiente
POST   /api/clientes/{id}/rechazar      # Rechazar cliente pendiente
PUT    /api/clientes/{id}               # Actualizar cliente
DELETE /api/clientes/{id}               # Eliminar cliente (soft delete)
```

## 🗄️ Modelo de Base de Datos

### Entidades Principales

**Comercio**
- Información del negocio
- Configuración de notificaciones
- Relación 1:N con Usuarios y Clientes

**Usuario**
- Administradores, operadores del comercio y clientes
- Autenticación con email/password (único global)
- Roles: Admin, UsuarioComercio, Cliente
- **ComercioId nullable**: NULL para clientes, NOT NULL para Admin/UsuarioComercio
- **Relación 1:N con Cliente**: Un usuario puede tener múltiples vinculaciones

**EstadoCliente** ⭐ NUEVO
- Sistema extensible de estados
- Estados: Pendiente, Activo, Inactivo
- Permite agregar nuevos estados en el futuro

**Cliente** ⭐ REFACTORIZADO
- **Tabla intermedia** entre Usuario y Comercio
- **UsuarioId (required)**: FK a Usuario (NO nullable)
- Información de contacto (mantenida por compatibilidad)
- Estado (Pendiente/Activo/Inactivo) - **Específico por vinculación**
- Origen (Administración/Autogestión)
- **Alias y NotasComercio**: Datos específicos de esta vinculación
- Índice único: `{UsuarioId, ComercioId}` - Un usuario solo puede vincularse una vez por comercio
- Tiene una CuentaCorriente (1:1, creada al aprobar)

**CuentaCorriente**
- Límite de crédito
- Saldo calculado desde movimientos
- Estado (bloqueada/activa)

**Movimiento**
- Tipo: Debe/Haber
- Importe
- Fecha
- Descripción

## 🔒 Seguridad

### Implementado
- ✅ Passwords hasheados con SHA256
- ✅ JWT Token con expiración
- ✅ Claims personalizados (UserId, Email, Role, ComercioId)
- ✅ CORS configurado
- ✅ Validación de datos (Data Annotations)
- ✅ Soft delete en todas las entidades
- ✅ Multi-tenancy por ComercioId

### Recomendaciones Futuras
- ⚠️ Migrar de SHA256 a BCrypt/Argon2
- ⚠️ Implementar rate limiting
- ⚠️ Añadir verificación de email
- ⚠️ CAPTCHA en registro
- ⚠️ Refresh tokens
- ⚠️ Two-factor authentication

## 🚀 Cómo Ejecutar

### Prerequisitos
- .NET 8 SDK
- SQL Server LocalDB
- Visual Studio 2022 (recomendado) o VS Code

### Pasos

1. **Clonar el repositorio**
```bash
git clone <repository-url>
cd SaaSCuentasCorrientes
```

2. **Restaurar paquetes**
```bash
dotnet restore
```

3. **Configurar la base de datos**
```bash
cd SaasACC.Infrastructure
dotnet ef database update
```

4. **Ejecutar la API**
```bash
cd SaasACCAPI.api
dotnet run
```
La API estará disponible en: `https://localhost:7201`

5. **Ejecutar el Frontend (en otra terminal)**
```bash
cd SaasACC.BlazorWasm
dotnet run
```
El frontend estará disponible en: `https://localhost:7163`

### Configuración

**appsettings.json** (API):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SaasAccDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "TuClaveSecretaMuyLarga",
    "Issuer": "SaasACC.API",
    "Audience": "SaasACC.Client",
    "ExpiryHours": 24
  }
}
```

**wwwroot/appsettings.json** (Blazor):
```json
{
  "ApiBaseUrl": "https://localhost:7201"
}
```

## 📱 Flujo de Usuario

### Para Comercios
1. **Registro**: Completar formulario de comercio + administrador
2. **Login automático**: Se genera token JWT
3. **Dashboard**: Acceso a gestión de clientes
4. **Gestionar clientes**: Crear, editar, ver cuentas corrientes
5. **Registrar movimientos**: Debe/Haber en cuentas de clientes

### Para Clientes (Autogestión)
1. **Registro**: Seleccionar comercio + completar datos + crear password
2. **Espera de Aprobación**: Estado pendiente, no puede hacer login
3. **Notificación**: Comercio revisa y aprueba/rechaza (futuro: email)
4. **Login**: Iniciar sesión después de aprobación
5. **Ver cuenta**: Consultar saldo y movimientos (futuro)
6. **Historial**: Ver movimientos históricos (futuro)

## 📝 Validaciones

### Backend (Data Annotations)
- Campos requeridos
- Formato de email
- Formato de teléfono
- Longitud mínima/máxima
- Comparación de passwords

### Backend (Lógica de Negocio)
- Email único de comercio
- Email único de usuario
- Email único de cliente por comercio
- Comercio debe existir
- Creación automática de cuenta corriente

### Frontend
- Validación en tiempo real con MudBlazor
- Mensajes de error personalizados
- Validación de formato
- Confirmación de passwords

## 🎯 Próximas Funcionalidades

### Sistema de Aprobación
- [ ] Notificaciones por email al cliente cuando es aprobado/rechazado
- [ ] Badge en menú con cantidad de clientes pendientes
- [ ] Razón de rechazo (campo adicional)
- [ ] Confirmación antes de rechazar cliente
- [ ] Aprobación masiva (bulk approval)
- [ ] Re-activación de clientes rechazados
- [ ] Historial de aprobaciones/rechazos

### Funcionalidades Generales
- [ ] Verificación de email
- [ ] Recuperación de contraseña
- [ ] Dashboard para clientes (autogestión)
- [ ] Notificaciones por email/WhatsApp para movimientos
- [ ] Exportación de movimientos (PDF, Excel)
- [ ] Reportes y estadísticas
- [ ] Configuración de comercio
- [ ] Gestión de usuarios por comercio
- [ ] Two-factor authentication
- [ ] Límites de crédito dinámicos
- [ ] Alertas de vencimiento

## 🧪 Testing

### Estado Actual
- ✅ Compilación exitosa
- ⏳ Unit tests pendientes
- ⏳ Integration tests pendientes
- ⏳ UI tests pendientes

### Testing Manual Recomendado
1. Registrar comercio nuevo
2. Verificar creación de usuario admin
3. Login con comercio registrado
4. Crear clientes
5. Verificar creación automática de cuenta corriente
6. Registrar cliente desde formulario público
7. Verificar validaciones de email duplicado

## 📄 Documentación

- **resumenDesarrollo.txt**: Resumen detallado de cada implementación
- **claude.md**: Guía para mantener documentación actualizada
- **Swagger UI**: Disponible en `https://localhost:7201/swagger`

## 🤝 Contribución

Este es un proyecto en desarrollo activo. Para contribuir:

1. Crear un branch desde `main`
2. Implementar la funcionalidad
3. Crear Pull Request
4. Actualizar documentación

## 📜 Licencia

[Especificar licencia]

## 👨‍💻 Autor

Pablo - [GitHub](https://github.com/PolaccoPablo)

## 📞 Soporte

Para reportar bugs o solicitar funcionalidades, crear un issue en GitHub.

---

**Última actualización**: 2025-12-04
**Branch actual**: hardcore-wright
**Versión**: 1.2.0-beta

## 📋 Changelog

### v1.2.0-beta (2025-12-04) ⭐ NUEVO
- ✨ **Cliente Multicomercio**: Un usuario puede ser cliente de múltiples comercios
- ✨ **Vinculación de Usuario**: Clientes pueden vincularse a nuevos comercios
- ✨ **Detección de Usuarios Existentes**: Al agregar cliente, reutiliza usuarios existentes
- ✨ **Login Multicomercio**: Token incluye lista de comercios activos del usuario
- 🔧 **Usuario.ComercioId nullable**: NULL para clientes, NOT NULL para Admin/UsuarioComercio
- 🔧 **Cliente como tabla intermedia**: UsuarioId required, índice único {UsuarioId, ComercioId}
- 🔧 **Nuevos endpoints**: GET /api/clientes/mis-comercios, POST /api/clientes/vincular
- 🔧 **AuthService refactorizado**: Soporte para múltiples vinculaciones en login y registro
- 📊 **Nuevos DTOs**: MiComercioDto, ComercioInfo en LoginResponse

### v1.1.0-beta (2025-11-20)
- ✨ **Sistema de Aprobación de Clientes**: Diferenciación entre clientes creados por admin vs autogestión
- ✨ **Estados de Cliente**: Pendiente, Activo, Inactivo (extensible)
- ✨ **Página de Pendientes**: Interfaz dedicada para revisar y aprobar clientes
- ✨ **Filtros por Estado**: Filtrado de clientes por estado en lista principal
- ✨ **Validación de Login**: Clientes pendientes/inactivos no pueden iniciar sesión
- ✨ **Auditoría**: Registro de quién y cuándo aprobó cada cliente
- 🔧 **Arquitectura**: Usuario unificado con rol "Cliente" para autogestión
- 🔧 **Cuenta Corriente**: Se crea solo al aprobar cliente (no al registrar)

### v1.0.0-beta (2025-11-13)
- ✨ Sistema de registro con dos botones separados (Comercio/Cliente)
- ✨ CRUD completo de clientes
- ✨ Gestión de cuentas corrientes
- ✨ Sistema de movimientos
- ✨ Autenticación JWT
- ✨ Multi-tenancy por comercio
