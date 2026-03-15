# 🎯 GUÍA COMPLETA: SISTEMA DE PAGOS CON STRIPE, MYSQL Y AUTENTICACIÓN

## 📋 ARQUITECTURA

```
┌─────────────────┐
│   Blazor App    │
├─────────────────┤
│ Pages:          │
│ - Login.razor   │
│ - Register.razor│
│ - Dashboard     │
│ - Checkout      │
└────────┬────────┘
         │
┌────────▼────────┐
│  Services       │
├─────────────────┤
│ - AuthService   │
│ - PaymentService│
│ - StripeService │
└────────┬────────┘
         │
┌────────▼────────┐
│    MySQL        │
├─────────────────┤
│ Tables:         │
│ - Users         │
│ - Roles         │
│ - Payments      │
└─────────────────┘
```

---

## 🚀 INSTALACIÓN Y CONFIGURACIÓN

### Paso 1: Instalar MySQL

**En Windows:**
1. Descargar desde https://dev.mysql.com/downloads/mysql/
2. Instalar con credenciales por defecto
3. Puerto: 3306
4. Usuario: root
5. Contraseña: root

**Verificar que MySQL está corriendo:**
```bash
mysql -u root -p
# Ingresar contraseña: root
```

### Paso 2: Actualizar la Cadena de Conexión (opcional)

En `appsettings.json`, si tus credenciales son diferentes:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=MyBlazorAppDb;Uid=root;Pwd=tucontraseña;"
}
```

### Paso 3: Crear la Base de Datos

Ejecuta estos comandos en tu terminal:

```powershell
cd MyBlazorApp

# Limpiar y reconstruir
dotnet clean
dotnet build

# Crear migration
dotnet ef migrations add InitialCreate

# Aplicar migration (crea la BD)
dotnet ef database update

# Ejecutar la aplicación
dotnet run
```

---

## 🔐 FLUJO DE AUTENTICACIÓN

### 1. Registro
```
Usuario → /register
  ↓
Llena formulario (username, email, password)
  ↓
AuthService.RegisterAsync()
  ↓
Hash de contraseña con SHA256
  ↓
Guardado en tabla Users
  ↓
Redirige a Login
```

### 2. Login
```
Usuario → /login
  ↓
Ingresa username y password
  ↓
AuthService.LoginAsync()
  ↓
Verifica contraseña
  ↓
Si válido → Redirige a /dashboard
Si no → Muestra error
```

### 3. Dashboard (Protegido)
```
Usuario logueado → /dashboard
  ↓
Muestra estadísticas de pagos
  ↓
Opción de hacer nuevo pago → /stripe-checkout
  ↓
Botón de logout → /login
```

---

## 💳 FLUJO DE PAGOS

### 1. Crear Pago
```
Usuario → /stripe-checkout
  ↓
Ingresa Producto y Monto
  ↓
Haz click "Proceed to Payment"
  ↓
Backend crea Stripe Session
  ↓
PaymentService guarda en BD (status: "pending")
  ↓
Redirige a Stripe Checkout (URL directa)
```

### 2. En Stripe Checkout
```
Se abre página de Stripe
  ↓
Usuario ingresa:
- Tarjeta: 4242 4242 4242 4242
- Fecha: 12/25
- CVC: 123
  ↓
Completa el pago
  ↓
Redirige a /payment-success
```

### 3. Ver Pagos en Dashboard
```
Dashboard muestra:
- Total de pagos
- Pagos completados
- Pagos pendientes
- Monto total
- Tabla con últimos 10 pagos
```

---

## 📊 TABLAS DE BASE DE DATOS

### Tabla: Roles
```sql
CREATE TABLE Roles (
  Id INT PRIMARY KEY,
  Name VARCHAR(50) NOT NULL
);

-- Datos iniciales
INSERT INTO Roles VALUES (1, 'Admin');
INSERT INTO Roles VALUES (2, 'Basic');
```

### Tabla: Users
```sql
CREATE TABLE Users (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Username VARCHAR(100) NOT NULL UNIQUE,
  Email VARCHAR(100) NOT NULL UNIQUE,
  PasswordHash VARCHAR(255) NOT NULL,
  RoleId INT NOT NULL,
  CreatedAt DATETIME NOT NULL,
  IsActive BOOL NOT NULL,
  FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
```

### Tabla: Payments
```sql
CREATE TABLE Payments (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  StripeSessionId VARCHAR(255) NOT NULL,
  ProductName VARCHAR(255) NOT NULL,
  Amount DECIMAL(18,2) NOT NULL,
  Status VARCHAR(50) NOT NULL,
  CreatedAt DATETIME NOT NULL,
  CustomerEmail VARCHAR(100)
);
```

---

## 🔐 CREDENCIALES DE PRUEBA

Después de ejecutar `dotnet ef database update`, crea usuarios de prueba. Por ahora, crea manualmente yendo a `/register`:

**Admin:**
- Username: `admin`
- Email: `admin@example.com`
- Password: `admin123`

**Usuario:**
- Username: `user`
- Email: `user@example.com`
- Password: `user123`

---

## 🎯 PASOS PARA EJECUTAR

### 1. Verificar MySQL está corriendo
```bash
mysql -u root -p -e "SELECT 1"
```

### 2. Abrir PowerShell en la carpeta del proyecto
```powershell
cd C:\Users\tunombre\source\repos\MyBlazorApp
```

### 3. Crear BD y ejecutar
```powershell
dotnet clean
dotnet build
dotnet ef database update
dotnet run
```

### 4. Abrir en navegador
```
https://localhost:7000
```

### 5. Deberías ver la página de Login

---

## 📱 FLUJO COMPLETO DE USUARIO

```
1. Abres https://localhost:7000
   ↓
2. Ves página de Login
   ↓
3. Haces click en "Create Account"
   ↓
4. Rellenas formulario de registro
   ↓
5. Eres redirigido a Login
   ↓
6. Ingresas tus credenciales
   ↓
7. Ves el Dashboard
   ↓
8. Haces click en "New Payment"
   ↓
9. Rellenas Producto y Monto
   ↓
10. Haces click "Proceed to Payment"
    ↓
11. Se abre Stripe Checkout
    ↓
12. Ingresas datos de tarjeta (4242 4242 4242 4242)
    ↓
13. Completas el pago
    ↓
14. Ves "Payment Successful"
    ↓
15. Vuelves a Dashboard
    ↓
16. Ves tu pago en la tabla con status "pending"
    ↓
17. (Con webhooks): Status cambia a "completed"
```

---

## 🐛 TROUBLESHOOTING

### Error: "Cannot connect to MySQL"
**Solución:**
- Verifica que MySQL está corriendo
- Verifica credenciales en `appsettings.json`
- Verifica puerto 3306 está disponible

### Error: "Database does not exist"
**Solución:**
```powershell
dotnet ef database update
```

### Error: "Migrations have not been applied"
**Solución:**
```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Usuario no puede loguear
**Solución:**
- Ve a `/register` y crea un nuevo usuario
- Verifica que la BD tiene datos

---

## 🔄 PROXIMOS PASOS CON WEBHOOKS

Para actualizar automáticamente el estado de los pagos:

1. Ir a Stripe Dashboard
2. Developers → Webhooks
3. Agregar endpoint: `https://tuapp.com/api/webhooks/stripe`
4. Seleccionar eventos: `checkout.session.completed`, `checkout.session.expired`
5. Implementar endpoint en Program.cs que actualice BD

---

¡**Ahora tienes un sistema completo de pagos con Blazor, MySQL y Autenticación!** 🚀
