## 🚀 GUÍA RÁPIDA DE INICIO

### Paso 1: Restaurar Paquetes
```powershell
dotnet restore MyBlazorApp.Solution.slnx
```

### Paso 2: Configurar BD (MySQL)
Edita `MyBlazorApp.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=MyBlazorApp;Uid=root;Pwd=YOUR_PASSWORD;"
}
```

### Paso 3: Aplicar Migraciones
```powershell
cd MyBlazorApp.Api
dotnet ef database update
cd ..
```

### Paso 4: Ejecutar Proyectos

**Opción A - Visual Studio:**
- Clic derecho en solución → Set Startup Projects
- Multiple startup projects → Start en Api y Web
- F5

**Opción B - Terminal:**

Terminal 1:
```powershell
cd MyBlazorApp.Api
dotnet run
```

Terminal 2:
```powershell
cd MyBlazorApp.Web
dotnet run
```

**Opción C - PowerShell Script:**
```powershell
.\run-both.ps1
```

---

## 📍 Acceso

| Aplicación | URL | Puerto |
|-----------|-----|--------|
| Web | https://localhost:7200 | 7200 |
| API | https://localhost:7100 | 7100 |
| API Swagger | https://localhost:7100/swagger | 7100 |

---

## 👤 Demo Credentials

```
Usuario: admin
Contraseña: admin123
Rol: Admin
```

```
Usuario: user
Contraseña: user123
Rol: Basic
```

---

## 📂 Proyectos

### MyBlazorApp.Api
- Backend Web API
- Entity Framework Core + MySQL
- Controllers: Auth, Raffles, Tickets, Sync
- Services: Auth, Raffle, Stripe

### MyBlazorApp.Web
- Frontend Blazor WebAssembly
- Pages: Home, Login, Register
- Components: RaffleCard, RaffleNavbar, RaffleFooter
- Services: ApiService, AuthStateService

### MyBlazorApp.Shared
- Modelos DTOs compartidos
- UserDto, RaffleDto, TicketDto, ApiModels

---

## ⚙️ Configuración Stripe (Opcional)

1. Ve a https://stripe.com → Developers → API Keys
2. Copia claves de test
3. Edita `MyBlazorApp.Api/appsettings.json`:

```json
"Stripe": {
  "PublishableKey": "pk_test_YOUR_KEY",
  "SecretKey": "sk_test_YOUR_KEY",
  "WebhookSecret": "whsec_YOUR_SECRET"
}
```

---

## ❓ Problemas Comunes

### "Can't connect to server"
```powershell
# Verifica MySQL
mysql -u root -p
# En appsettings.json verifica la contraseña
```

### "API no responde desde Web"
1. Verifica que API esté corriendo en puerto 7100
2. Verifica `MyBlazorApp.Web/wwwroot/appsettings.json` tiene URL correcta

### "Error en migraciones"
```powershell
cd MyBlazorApp.Api
dotnet ef database update
```

---

**¿Necesitas ayuda?** Revisa el archivo DETAILED_SETUP.md para instrucciones detalladas.
