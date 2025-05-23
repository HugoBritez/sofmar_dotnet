# Sofmar Backend .NET

Backend desarrollado en .NET para el sistema Sofmar.

## Requisitos

- .NET 8.0 SDK
- MySQL Server
- Visual Studio 2022 o Visual Studio Code

## Configuración

1. Clona el repositorio:
```bash
git clone https://github.com/tu-usuario/sofmar_backend-dotnet.git
```

2. Crea un archivo `.env` en la raíz del proyecto con las siguientes variables:
```env
MYSQL_HOST=tu_host
MYSQL_DB=tu_base_de_datos
MYSQL_USER=tu_usuario
MYSQL_PASSWORD=tu_password
JWT_KEY=tu_clave_jwt
JWT_ISSUER=tu_issuer
JWT_AUDIENCE=tu_audience
```

3. Restaura los paquetes NuGet:
```bash
dotnet restore
```

## Ejecución

1. Ejecuta la aplicación:
```bash
dotnet run
```

2. La documentacion de los endpoints API estará disponible en:
   - Swagger UI: `https://localhost:5001/swagger`
3. La url de la API en local es: `https://localhost:5001/api`

## Autenticación

La API utiliza JWT para la autenticación. Para acceder a los endpoints protegidos:

1. Obtén un token mediante el endpoint de login
2. Incluye el token en el header de las peticiones:



# Documentacion de la Arquitectura - Sofmar Backend V1

## Estructura General

📁 Api
├── 📁 Controllers      # Endpoints de la API
├── 📁 Services        # Lógica de negocio
├── 📁 Repositories    # Acceso a datos
│   ├── 📁 Implementations # Implementaciones de repositorios
│   └── 📁 Interfaces     # Interfaces para mantenibilidad
├── 📁 Models
│   ├── 📁 Entities    # Entidades de la base de datos
│   ├── 📁 DTOs        # Objetos de transferencia de datos
│   └── 📁 ViewModels  # Modelos para vistas/respuestas
├── 📁 Data           # Configuración de base de datos
├── 📁 Middleware     # Middlewares personalizados
└── 📁 Auth           # Autenticación y autorización
└── 📁 Aufit           # Manejo de la auditoria

## Capas de la aplicacion

### 1. Auth
Responsabilidad: Manejo completo de autenticación y autorización
Características:
Gestión de tokens JWT
Validación de credenciales
Manejo de roles y permisos
Políticas de seguridad

### 2. Middleware

Responsabilidad: Procesamiento de requests/responses
Características:
Manejo de errores global
Logging
Validación de requests
Transformación de respuestas

### 3. Repositories
Responsabilidad: Acceso a datos
Estructura:
Implementations/: Implementaciones concretas
Interfaces/: Contratos para mantenibilidad

### 4. Services
Responsabilidad: Lógica de negocio
Características:
   - Orquesta operaciones entre repositorios
   - Maneja transacciones
   - Implementa reglas de negocio

### 5. Controllers
Responsabilidad: Endpoints de la API
Características:
   - Expone endpoints REST
   - Valida requests
   - Maneja respuestas HTTP 


## Extensibilidad
La arquitectura permite:
   Agregar nuevas entidades fácilmente
   Implementar nuevas funcionalidades
   Modificar lógica de negocio
   Agregar nuevos endpoints
