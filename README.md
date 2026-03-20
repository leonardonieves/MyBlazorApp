# 🎟️ MyBlazorApp - Sistema de Rifas

## 🎨 Últimas Actualizaciones

### ✨ Nuevas Características Implementadas:

#### 1. **Admin Dashboard Profesional** 🎯
   - Página `/admin` exclusiva para administradores
   - **Sidebar navegación** con panel elegante (gradiente navy)
   - **4 secciones principales:**
     - **Dashboard** - Vista general con estadísticas (Total Raffles, Usuarios, Revenue, Tickets Vendidos)
     - **Sync Stripe** - Sincronización de datos desde Stripe con interfaz premium
     - **Raffles Management** - Tabla para gestionar raffles existentes
     - **Users Management** - Gestión de usuarios (próximamente)

#### 2. **Logout Mejorado** 🔐
   - Modal de confirmación elegante
   - Mensaje personalizado: "¿Deseas cerrar sesión?"
   - Dos botones: "Sí, cerrar sesión" y "Cancelar"
   - Animación suave (fade in/out)
   - Icono rojo para claridad visual

#### 3. **Dropdown Menu en Navbar** 👤
   - Click en el avatar de usuario abre menú desplegable
   - Opciones:
     - 🔧 Admin Dashboard (solo para admins)
     - 🚪 Logout con confirmación
   - Estilo profesional con sombra y animación

#### 4. **Home Limpia** 🏠
   - Removido el "cintillo oscuro" (admin panel en navbar)
   - La página ahora es más limpia y enfocada en las raffles
   - Admin functions movidas al `/admin` dashboard

## Arquitectura

Este proyecto usa una arquitectura de **SPA (Single Page Application)** con API REST:

- **MyBlazorApp.Web** - Frontend Blazor WebAssembly (SPA)
- **MyBlazorApp.Api** - Backend API REST con .NET 8
- **MyBlazorApp.Shared** - Modelos compartidos entre frontend y backend

> ⚠️ **IMPORTANTE**: El proyecto `MyBlazorApp` (Blazor Server) está deprecado. 
> Usa únicamente `MyBlazorApp.Web` como frontend.

## 🚀 Cómo ejecutar

### Opción 1: Script PowerShell (Recomendado)

```powershell
.\start-app.ps1
```

Esto iniciará automáticamente:
- API en https://localhost:7133
- Frontend en https://localhost:7267

### Opción 2: Visual Studio (Múltiples proyectos)

1. Clic derecho en la **Solución** → **Configure Startup Projects...**
2. Seleccionar **Multiple startup projects**
3. Configurar:
   - `MyBlazorApp.Api` → **Start** (debe estar primero)
   - `MyBlazorApp.Web` → **Start**
   - `MyBlazorApp` → **None**
   - `MyBlazorApp.Shared` → **None**
4. Presionar **F5**

### Opción 3: Terminal (Manual)

Terminal 1 - API:
```bash
cd MyBlazorApp.Api
dotnet run --launch-profile https
```

Terminal 2 - Frontend:
```bash
cd MyBlazorApp.Web
dotnet run --launch-profile https
```

## 📁 Estructura del proyecto

```
MyBlazorApp/
├── MyBlazorApp.Api/          # API REST (Backend)
│   ├── Controllers/          # Endpoints de la API
│   ├── Services/             # Lógica de negocio
│   ├── Models/               # Entidades de BD
│   └── Data/                 # DbContext
│
├── MyBlazorApp.Web/          # Blazor WebAssembly (Frontend)
│   ├── Pages/                # Páginas/Componentes Razor
│   ├── Services/             # Servicios del cliente
│   └── wwwroot/              # Archivos estáticos
│
├── MyBlazorApp.Shared/       # Modelos compartidos
│   └── Models/               # DTOs y modelos de auth
│
└── MyBlazorApp/              # ⚠️ DEPRECADO - No usar
```

## 🔑 Credenciales de prueba

| Usuario | Contraseña | Rol |
|---------|------------|-----|
| admin | admin123 | Admin |
| user | user123 | Basic |

## 📡 Endpoints de la API

### Autenticación
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/login` - Iniciar sesión
- `GET /api/auth/validate` - Validar token JWT

### Rifas
- `GET /api/raffles` - Listar rifas activas
- `GET /api/raffles/{id}` - Detalle de rifa
- `GET /api/raffles/featured` - Rifas destacadas

### Tickets
- `POST /api/tickets/buy` - Comprar tickets
- `GET /api/tickets/my-tickets?email={email}` - Mis tickets

### Admin
- `POST /api/sync/stripe` - Sincronizar con Stripe
