# MyBlazorApp - Arquitectura de la Solución

## Estructura de Proyectos

```
MyBlazorApp/
├── MyBlazorApp.Api/          # Backend - ASP.NET Core Web API
│   ├── Controllers/          # Controladores API REST
│   ├── Data/                 # DbContext y acceso a datos
│   ├── Hubs/                 # SignalR Hubs
│   ├── Migrations/           # Migraciones de EF Core
│   ├── Models/               # Entidades de dominio
│   ├── Services/             # Lógica de negocio
│   └── appsettings.json      # Configuración (BD, Stripe, JWT)
│
├── MyBlazorApp.Web/          # Frontend - Blazor WebAssembly
│   ├── Components/           # Componentes reutilizables
│   ├── Pages/                # Páginas/Rutas
│   ├── Services/             # Servicios cliente (ApiService, AuthState)
│   └── wwwroot/              # Archivos estáticos y appsettings.json
│
├── MyBlazorApp.Shared/       # Biblioteca compartida
│   └── Models/               # DTOs compartidos entre API y Web
│
└── MyBlazorApp/              # [LEGACY - No usar]
    └── ...                   # Proyecto original, mantener solo como referencia
```

## Arquitectura

```
┌─────────────────────┐     HTTP/REST      ┌─────────────────────┐
│   MyBlazorApp.Web   │ ◄─────────────────► │   MyBlazorApp.Api   │
│   (Blazor WASM)     │     + SignalR       │   (ASP.NET Core)    │
│                     │                     │                     │
│  - UI/Componentes   │                     │  - Controllers      │
│  - AuthStateService │                     │  - Services         │
│  - ApiService       │                     │  - DbContext        │
│  - LocalStorage     │                     │  - Stripe SDK       │
└─────────────────────┘                     └─────────────────────┘
          │                                           │
          │                                           │
          ▼                                           ▼
┌─────────────────────┐                     ┌─────────────────────┐
│ MyBlazorApp.Shared  │                     │    MySQL Database   │
│   (DTOs/Models)     │                     │                     │
└─────────────────────┘                     └─────────────────────┘
```

## Comandos Útiles

### Base de Datos (Migraciones)

```bash
# Crear nueva migración
cd MyBlazorApp.Api
dotnet ef migrations add NombreMigracion --output-dir Migrations

# Aplicar migraciones
dotnet ef database update

# Revertir última migración
dotnet ef migrations remove

# Ver migraciones pendientes
dotnet ef migrations list
```

### Ejecución

```bash
# Ejecutar solo el API
cd MyBlazorApp.Api
dotnet run

# Ejecutar solo el Web (requiere API corriendo)
cd MyBlazorApp.Web
dotnet run

# Ejecutar ambos (desde VS: configurar startup múltiple)
```

### Compilación

```bash
# Compilar solución completa
dotnet build MyBlazorApp.Solution.slnx

# Publicar para producción
dotnet publish MyBlazorApp.Api -c Release -o ./publish/api
dotnet publish MyBlazorApp.Web -c Release -o ./publish/web
```

## Configuración

### MyBlazorApp.Api/appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=myblazorapp;User=root;Password=your_password;"
  },
  "Jwt": {
    "Key": "YourSecretKeyHere32CharsMin!!",
    "Issuer": "MyBlazorApp",
    "Audience": "MyBlazorApp"
  },
  "Stripe": {
    "SecretKey": "sk_test_xxx",
    "PublishableKey": "pk_test_xxx",
    "WebhookSecret": "whsec_xxx"
  }
}
```

### MyBlazorApp.Web/wwwroot/appsettings.json

```json
{
  "ApiBaseUrl": "https://localhost:7133/api/"
}
```

## Endpoints API Principales

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar usuario

### Raffles
- `GET /api/raffles` - Listar rifas activas
- `GET /api/raffles/{id}` - Detalle de rifa
- `GET /api/raffles/featured` - Rifas destacadas

### Tickets
- `POST /api/tickets/buy` - Comprar tickets (crea sesión Stripe)
- `POST /api/tickets/reserve` - Reservar tickets
- `POST /api/tickets/release` - Liberar reserva
- `GET /api/tickets/status/{raffleId}` - Estado de tickets (para selector visual)
- `GET /api/tickets/user/{userId}` - Tickets del usuario

### Sync (Admin)
- `POST /api/sync/stripe` - Sincronizar desde Stripe
- `GET /api/sync/status` - Estado de sincronización
- `GET /api/sync/stripe-products` - Debug: productos Stripe

### Admin
- `GET /api/admin/users` - Listar usuarios
- `GET /api/admin/payments` - Listar pagos
- `GET /api/admin/stats` - Estadísticas

### Webhooks
- `POST /api/webhook/stripe` - Webhook de Stripe

## Usuarios de Prueba

| Usuario | Password | Rol |
|---------|----------|-----|
| admin | admin123 | Admin |
| user | user123 | Basic |
