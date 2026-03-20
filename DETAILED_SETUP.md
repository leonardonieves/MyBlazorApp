# 📚 DOCUMENTACIÓN DETALLADA

## 1. ESTRUCTURA DEL PROYECTO

```
MyBlazorApp/
│
├── MyBlazorApp.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs         # Login, Register
│   │   ├── RafflesController.cs      # GET raffles
│   │   ├── TicketsController.cs      # Buy, Reserve tickets
│   │   ├── SyncController.cs         # Sincronizar con Stripe
│   │   └── StripeWebhookController.cs
│   │
│   ├── Services/
│   │   ├── AuthService.cs            # Autenticación, Hash
│   │   ├── RaffleService.cs          # CRUD raffles
│   │   ├── StripeService.cs          # Integración Stripe
│   │   ├── StripeSyncService.cs      # Sincronizar productos
│   │   └── StripeWebhookService.cs   # Procesar webhooks
│   │
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Raffle.cs
│   │   ├── Ticket.cs
│   │   ├── Winner.cs
│   │   └── Payment.cs
│   │
│   ├── Data/
│   │   └── AppDbContext.cs           # Entity Framework DbContext
│   │
│   ├── Migrations/
│   │   └── [Migraciones automáticas]
│   │
│   ├── appsettings.json              # ⚠️ EDITAR CON TUS DATOS
│   └── Program.cs                    # Configuración servicios
│
├── MyBlazorApp.Web/
│   ├── Pages/
│   │   ├── Home.razor                # Landing page con rifas
│   │   ├── Login.razor               # Formulario login
│   │   └── Register.razor            # Formulario registro
│   │
│   ├── Components/
│   │   ├── RaffleCard.razor          # Tarjeta de rifa
│   │   ├── RaffleNavbar.razor        # Barra navegación
│   │   └── RaffleFooter.razor        # Pie de página
│   │
│   ├── Services/
│   │   ├── ApiService.cs             # Llamadas HTTP a API
│   │   └── AuthStateService.cs       # Estado autenticación
│   │
│   ├── Layout/
│   │   └── MainLayout.razor          # Layout principal
│   │
│   ├── wwwroot/
│   │   ├── appsettings.json          # ⚠️ URL API (7100)
│   │   ├── index.html                # HTML principal
│   │   └── css/app.css               # Estilos
│   │
│   └── Program.cs                    # Configuración servicios
│
├── MyBlazorApp.Shared/
│   └── Models/
│       ├── UserDto.cs
│       ├── RaffleDto.cs
│       ├── TicketDto.cs
│       └── ApiModels.cs
│
├── MyBlazorApp.Solution.slnx         # Solución
├── QUICK_START.md                    # Este archivo
├── run-both.ps1                      # Script lanzador
└── README.md                         # Documentación general
```

---

## 2. FLUJOS DE DATOS

### 2.1 Autenticación (Login)

```
Usuario escribe credenciales
         ↓
[Login.razor] valida campos
         ↓
ApiService.LoginAsync()
    ↓
   POST /api/auth/login
    ↓
[AuthController.Login()]
    ↓
[AuthService.LoginAsync()]
    ↓
Hash password + Compare
    ↓
Generar JWT token
    ↓
Retornar UserDto + Token
    ↓
[AuthStateService.LoginAsync()]
    ↓
Guardar en LocalStorage:
  - authToken
  - currentUser
    ↓
OnAuthStateChanged() dispara
    ↓
Componentes se re-renderean
    ↓
NavBar muestra usuario logueado
```

### 2.2 Visualizar Rifas

```
Home.razor OnInitialized
         ↓
ApiService.GetRafflesAsync()
    ↓
   GET /api/raffles
    ↓
[RafflesController.GetRaffles()]
    ↓
[RaffleService.GetActiveRafflesAsync()]
    ↓
DbContext.Raffles
  .Include(Prizes)
  .Include(Images)
    ↓
Retornar List<RaffleDto>
    ↓
Renderizar RaffleCard x cada rifa
```

### 2.3 Admin Sincroniza Stripe

```
Admin logueado ve Home.razor
         ↓
Aparece Admin Panel (IsAdmin = true)
         ↓
Click botón "Sync from Stripe"
    ↓
SyncFromStripe()
    ↓
ApiService.SyncFromStripeAsync()
    ↓
   POST /api/sync/stripe
    ↓
[SyncController.SyncFromStripe()]
    ↓
AuthHeader con JWT token
    ↓
[StripeSyncService.SyncProductsFromStripeAsync()]
    ↓
StripeService.GetProductsAsync()
    ↓
Obtiene de Stripe API
    ↓
Para cada producto Stripe:
  - Verifica si es rifa (metadata)
  - Crea Raffle en BD
  - Genera Tickets preGenerados
    ↓
Retorna List<Raffle>
    ↓
Home.razor recibe SyncResult
    ↓
Recarga rifas
    ↓
Renderiza RaffleCard nuevas
```

---

## 3. CONFIGURACIÓN DETALLADA

### 3.1 appsettings.json (API)

**Ubicación:** `MyBlazorApp.Api/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  // ⚠️ EDITAR CON TU BD
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=MyBlazorApp;Uid=root;Pwd=YOUR_PASSWORD;"
  },
  
  // ⚠️ EDITAR CON CLAVES STRIPE
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_KEY",
    "SecretKey": "sk_test_YOUR_KEY",
    "WebhookSecret": "whsec_YOUR_SECRET"
  },
  
  // ⚠️ CAMBIAR EN PRODUCCIÓN
  "Jwt": {
    "Key": "GeneraUnaClaveSuperSecuraDeMinimo32Caracteres!",
    "Issuer": "MyBlazorApp.Api",
    "Audience": "MyBlazorApp.Web",
    "ExpirationInMinutes": 60
  },
  
  "AllowedHosts": "*"
}
```

### 3.2 appsettings.json (Web)

**Ubicación:** `MyBlazorApp.Web/wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "https://localhost:7100"
}
```

---

## 4. GUÍA PASO A PASO

### Paso 1: Clonar Repositorio
```powershell
git clone https://github.com/leonardonieves/MyBlazorApp.git
cd MyBlazorApp
```

### Paso 2: Restaurar NuGet
```powershell
dotnet restore MyBlazorApp.Solution.slnx
```

### Paso 3: Crear Base de Datos

En MySQL:
```sql
CREATE DATABASE MyBlazorApp CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

O por línea de comandos:
```powershell
mysql -u root -p -e "CREATE DATABASE MyBlazorApp CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
```

### Paso 4: Editar appsettings.json

**Archivo:** `MyBlazorApp.Api/appsettings.json`

Cambia:
```json
"DefaultConnection": "Server=localhost;Port=3306;Database=MyBlazorApp;Uid=root;Pwd=YOUR_PASSWORD;"
```

Por tu contraseña de MySQL.

### Paso 5: Aplicar Migraciones

```powershell
cd MyBlazorApp.Api
dotnet ef database update
cd ..
```

Esto:
- Crea las tablas en MySQL
- Crea usuarios demo (admin/admin123, user/user123)
- Configura relaciones

### Paso 6: Ejecutar

**Opción A - Visual Studio:**
1. Abre `MyBlazorApp.Solution.slnx`
2. Clic derecho en solución
3. **Set Startup Projects**
4. **Multiple startup projects**
5. Selecciona **Start** en Api y Web
6. F5

**Opción B - Script PowerShell:**
```powershell
.\run-both.ps1
```

**Opción C - Manual (2 terminales):**

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

### Paso 7: Acceder

- Web: https://localhost:7200
- API: https://localhost:7100
- Swagger: https://localhost:7100/swagger

---

## 5. INTEGRACIÓN STRIPE

### 5.1 Crear Cuenta Stripe

1. Ve a https://stripe.com
2. Haz clic en **Sign in**
3. **Email** → **Crear cuenta** o **Login**

### 5.2 Obtener Claves API

1. En Dashboard, ve a **Developers**
2. Click **API keys**
3. Busca **Standard keys**:
   - **Publishable key**: pk_test_...
   - **Secret key**: sk_test_...

### 5.3 Obtener Webhook Secret

1. En **Developers** → **Webhooks**
2. Click **Add endpoint**
3. URL: `https://localhost:7100/api/stripe-webhook`
4. Eventos a enviar:
   - checkout.session.completed
   - checkout.session.async_payment_succeeded
   - charge.succeeded
   - payment_intent.succeeded
5. Click **Add endpoint**
6. Click en el endpoint nuevo
7. Busca **Signing secret**
8. Copia (empieza con `whsec_`)

### 5.4 Actualizar appsettings.json

```json
"Stripe": {
  "PublishableKey": "pk_test_TU_CLAVE",
  "SecretKey": "sk_test_TU_CLAVE",
  "WebhookSecret": "whsec_TU_SECRET"
}
```

### 5.5 Crear Producto Raffle

1. En Stripe Dashboard → **Products**
2. **Add product**
3. **Name**: "Argentina vs France Raffle"
4. **Description**: "World Cup raffle ticket"
5. **Pricing**: One-time → $25.00
6. En **Metadata** (abajo), agrega:
   - Key: `type` → Value: `raffle`
   - Key: `total_tickets` → Value: `500`
7. Click **Save product**

---

## 6. USUARIOS DEMO

Después de aplicar migraciones, puedes usar:

| Usuario | Password | Rol     |
|---------|----------|---------|
| admin   | admin123 | Admin   |
| user    | user123  | Basic   |

---

## 7. TROUBLESHOOTING

### Error: "Could not connect to server"

**Problema:** MySQL no está corriendo o contraseña incorrecta

**Solución:**
```powershell
# Verificar MySQL
mysql -u root -p
# EnterSQL y cierra (Ctrl+C)

# Edita appsettings.json con contraseña correcta
# Reinicia aplicación
```

### Error: "Access denied for user 'root'@'localhost'"

**Problema:** Contraseña MySQL incorrecta

**Solución:**
```powershell
# Reset MySQL password Windows
mysql -u root
mysql> ALTER USER 'root'@'localhost' IDENTIFIED BY 'newpassword';
mysql> FLUSH PRIVILEGES;
mysql> EXIT;

# Actualiza appsettings.json con newpassword
```

### Error: "The connection string 'DefaultConnection' was not found"

**Problema:** Falta o está mal escrito appsettings.json

**Solución:**
- Verifica que exista `MyBlazorApp.Api/appsettings.json`
- Verifica que tenga `ConnectionStrings`

### Error: "CORS policy: No 'Access-Control-Allow-Origin'"

**Problema:** Web no puede conectar a API

**Solución:**
1. Verifica que API esté corriendo (https://localhost:7100)
2. Verifica `MyBlazorApp.Web/wwwroot/appsettings.json` tiene:
   ```json
   "ApiBaseUrl": "https://localhost:7100"
   ```
3. Verifica CORS en `MyBlazorApp.Api/Program.cs`

### Error: "No migrations found"

**Problema:** Faltan migraciones

**Solución:**
```powershell
cd MyBlazorApp.Api
dotnet ef migrations add Initial
dotnet ef database update
```

---

## 8. RECURSOS

- [Documentación .NET 8](https://learn.microsoft.com/dotnet)
- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/webassembly)
- [Entity Framework Core](https://learn.microsoft.com/ef/core)
- [Stripe Docs](https://stripe.com/docs)
- [JWT](https://tools.ietf.org/html/rfc7519)

---

**¿Necesitas ayuda?** Abre un issue en GitHub.
