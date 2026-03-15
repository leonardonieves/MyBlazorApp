# 🎯 GUÍA RÁPIDA - HACER UN PAGO CON STRIPE EN 5 MINUTOS

## ✅ Prerequisitos (YA COMPLETADOS)

- ✅ Credenciales de Stripe en `appsettings.json` 
- ✅ NuGet Package Stripe.net instalado
- ✅ Componentes Razor creados
- ✅ Servicio StripeService configurado
- ✅ Scripts JavaScript listos

---

## 🚀 PASOS PARA HACER UN PAGO

### **1️⃣ Ejecutar la aplicación**

Abre PowerShell en la carpeta del proyecto:

```powershell
cd MyBlazorApp
dotnet run
```

Espera hasta ver:
```
Now listening on: https://localhost:7000
```

### **2️⃣ Abrir el navegador**

Dirígete a:
```
https://localhost:7000
```

### **3️⃣ Ir a "Stripe Payment"**

En el menú izquierdo, haz clic en **"Stripe Payment"**

O ve directamente a:
```
https://localhost:7000/stripe-checkout
```

### **4️⃣ Rellenar el formulario**

Verás dos campos:

```
┌─────────────────────────────────┐
│ Product Name                    │
│ [Sample Product          ]      │  ← Puedes cambiar esto
│                                 │
│ Amount ($)                      │
│ [29.99                   ]      │  ← Puedes cambiar esto
│                                 │
│ [ Proceed to Payment ]          │
└─────────────────────────────────┘
```

**Ejemplo:**
- Product Name: `Learn Blazor & Stripe`
- Amount: `9.99`

### **5️⃣ Haz clic en "Proceed to Payment"**

Verás un spinne que dice "Processing payment..."

### **6️⃣ Se abre Stripe Checkout**

Te redirigirá a una página de Stripe donde necesitarás:

**Ingresar datos de tarjeta:**

```
┌────────────────────────────┐
│ Card details               │
│ Card number: 4242 4242 ... │
│ Expiry date: 12 / 25       │
│ CVC: 123                   │
│                            │
│ Cardholder name: Test      │
│ Email: test@example.com    │
│                            │
│    [ Pay ]                 │
└────────────────────────────┘
```

### **7️⃣ Resultado**

**✅ Si fue exitoso:**
```
¡Pago Exitoso!
Tu pago ha sido procesado correctamente.
Recibirás un email de confirmación en breve.

[ Make Another Payment ]
```

**❌ Si cancelaste:**
```
Pago Cancelado
Tu pago fue cancelado. No se realizó ningún cargo.

[ Try Again ]
```

---

## 💳 TARJETAS DE PRUEBA PARA DIFERENTES ESCENARIOS

Usa estas tarjetas en la página de Stripe Checkout:

### ✅ Pago Exitoso (USA ESTA)
```
Tarjeta: 4242 4242 4242 4242
Fecha: 12/25
CVC: 123
```

### ❌ Tarjeta Rechazada
```
Tarjeta: 4000 0000 0000 0002
Fecha: 12/25
CVC: 123
```

### 🔐 Requiere Autenticación 3D Secure
```
Tarjeta: 4000 0025 0000 3155
Fecha: 12/25
CVC: 123
```

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### ❌ "No se abre Stripe Checkout"

**Solución:**
1. Abre la consola del navegador (F12)
2. Ve a la pestaña "Console"
3. Busca errores rojos
4. Si ves: "Stripe is not defined" → Recarga la página

### ❌ "Error processing payment"

**Solución:**
1. Verifica que `appsettings.json` tenga las keys correctas
2. La `PublishableKey` debe empezar con `pk_test_`
3. La `SecretKey` debe empezar con `sk_test_`
4. No deben tener espacios

### ❌ "Publishable Key is not configured"

**Solución:**
```json
// En appsettings.json
"Stripe": {
  "PublishableKey": "pk_test_51TAy7NGRjaQMHgPx...",  // ✅ NO vacío
  "SecretKey": "sk_test_51TAy7NGRjaQMHgPx..."        // ✅ NO vacío
}
```

---

## 📊 VER LOS PAGOS EN STRIPE DASHBOARD

1. Ve a https://dashboard.stripe.com
2. Inicia sesión
3. Haz clic en **"Payments"**
4. Verás todos tus pagos de prueba
5. Haz clic en uno para ver detalles

---

## 🔍 DEBUG - VER LOS LOGS

Abre la consola (F12) y verás mensajes como:

```javascript
Creating session for Sample Product - $29.99
Session created: cs_test_b1234567890...
Redirecting to checkout with session: cs_test_b1234567890...
```

Si ves estos mensajes, **ESTÁ FUNCIONANDO CORRECTAMENTE** ✅

---

## 📌 CHECKLIST FINAL

Antes de hacer un pago, verifica:

- [ ] El proyecto está corriendo (`dotnet run`)
- [ ] Puedes acceder a https://localhost:7000
- [ ] El menú tiene "Stripe Payment"
- [ ] `appsettings.json` tiene las keys de Stripe
- [ ] Abriste la consola del navegador para ver logs (F12)
- [ ] Tienes lista la tarjeta de prueba: `4242 4242 4242 4242`

---

¡**Ahora estás listo para hacer tu primer pago con Stripe en Blazor!** 🚀

¿Necesitas ayuda? Ve al archivo `PAYMENT_GUIDE.md` para más detalles.
