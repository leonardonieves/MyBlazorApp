# 💳 Guía Completa: Cómo Hacer un Pago con Stripe en Blazor

## ✅ Verificación Previa

Antes de hacer el primer pago, verifica que todo esté configurado correctamente:

### 1. Verifica las credenciales en `appsettings.json`

```json
"Stripe": {
  "PublishableKey": "pk_test_...",  // Debe empezar con pk_test_
  "SecretKey": "sk_test_..."         // Debe empezar con sk_test_
}
```

✅ **Tu configuración actual es correcta** (credenciales de sandbox)

### 2. Verifica que el proyecto esté compilando

```powershell
cd MyBlazorApp
dotnet build
```

Debe mostrar: ✅ **Build successful**

---

## 🚀 Pasos para Hacer tu Primer Pago

### **Paso 1: Ejecutar la aplicación**

```powershell
cd MyBlazorApp
dotnet run
```

Esperarás un mensaje similar a:
```
Now listening on: https://localhost:7000
```

### **Paso 2: Abrir el navegador**

Ve a: **https://localhost:7000**

### **Paso 3: Navegar a la página de pagos**

En el menú izquierdo, haz clic en **"Stripe Payment"** o ve directamente a:
```
https://localhost:7000/stripe-checkout
```

### **Paso 4: Completar el formulario**

Verás un formulario con dos campos:

| Campo | Valor por defecto | ¿Puedo cambiar? |
|-------|-------------------|-----------------|
| **Product Name** | "Sample Product" | ✅ Sí |
| **Amount ($)** | 29.99 | ✅ Sí |

**Ejemplo:** 
- Product: "Learning Book"
- Amount: 19.99

### **Paso 5: Hacer clic en "Proceed to Payment"**

Verás un spinner que dice "Processing payment..." por unos segundos.

### **Paso 6: Serás redirigido a Stripe Checkout**

Se abrirá una página de Stripe (blanca) donde necesitarás:

1. **Ingresar datos de tarjeta:**
   - Tarjeta: `4242 4242 4242 4242`
   - Fecha: `12/25` (o cualquier futura)
   - CVC: `123` (cualquier 3 dígitos)

2. **Ingresar datos de email y nombre:**
   - Email: `test@example.com` (cualquier email)
   - Nombre: Cualquier nombre

3. **Haz clic en "Pay"**

### **Paso 7: Confirmación**

Después de completar el pago, serás redirigido a una de estas páginas:

✅ **Si fue exitoso:**
- URL: `https://localhost:7000/payment-success`
- Verás: "Payment Successful!" con un ícono de ✓

❌ **Si fue cancelado:**
- URL: `https://localhost:7000/payment-cancel`
- Verás: "Payment Cancelled"

---

## 🧪 Tarjetas de Prueba (Test Cards)

Usa estas tarjetas para probar diferentes escenarios en modo SANDBOX:

### Tarjetas que funcionan:

| Escenario | Tarjeta | Resultado |
|-----------|---------|-----------|
| ✅ **Pago exitoso** | `4242 4242 4242 4242` | Aprobado siempre |
| ❌ **Tarjeta rechazada** | `4000 0000 0000 0002` | Rechazado siempre |
| 🔐 **3D Secure** | `4000 0025 0000 3155` | Requiere autenticación |
| 📍 **ZIP incorrecto** | `4000 0000 0000 0010` | Rechazado por AVS |

**Parámetros para TODAS las tarjetas de prueba:**
- **Fecha de expiración:** Cualquiera en el futuro (ej: 12/25, 06/27)
- **CVC:** Cualquier número de 3 dígitos (ej: 123, 456, 999)
- **Nombre:** Cualquier nombre
- **Email:** Cualquier email válido

---

## 🐛 Si algo NO funciona...

### **Problema 1: "Stripe is not defined"**
**Solución:**
- Abre la consola del navegador (F12 → Console)
- Recarga la página (Ctrl + R)
- Verifica que no haya errores rojos

### **Problema 2: "Session ID is invalid"**
**Solución:**
- Verifica que tu `SecretKey` en `appsettings.json` sea correcta
- No puede tener espacios extras
- Debe empezar con `sk_test_`

### **Problema 3: "Publishable Key is not configured"**
**Solución:**
- Abre `appsettings.json`
- Verifica que `PublishableKey` NO esté vacío
- Debe empezar con `pk_test_`

### **Problema 4: No me redirige a Stripe**
**Solución:**
1. Abre la consola del navegador (F12)
2. Mira si hay errores
3. Verifica que ambas keys estén configuradas
4. Intenta recargar la página

---

## 🔍 Depuración - Ver los logs

Para ver qué está pasando internamente, abre la consola del navegador:

1. Presiona **F12** (o Ctrl + Shift + I)
2. Ve a la pestaña **"Console"**
3. Haz clic en "Proceed to Payment"
4. Busca mensajes como:
   - "Creating session for Sample Product - $29.99"
   - "Session created: cs_test_..."
   - "Redirecting to checkout with session: cs_test_..."

Si ves estos mensajes, significa que TODO está funcionando correctamente.

---

## 📊 Verificar Pagos en Stripe Dashboard

1. Ve a https://dashboard.stripe.com
2. Inicia sesión con tu cuenta
3. En el menú, haz clic en **"Payments"**
4. Verás todos tus pagos de prueba listados
5. Haz clic en uno para ver los detalles

---

## 💡 Ejemplo Completo - Paso a Paso Visual

```
1. Tu navegador
   ↓ Haces clic en "Stripe Payment"
   
2. Ves el formulario
   ├─ Product Name: "My Book"
   ├─ Amount: $49.99
   └─ Botón: "Proceed to Payment"
   
3. Haces clic en "Proceed to Payment"
   ↓ (spinner aparece)
   
4. Backend (C#)
   ├─ Recibe la solicitud
   ├─ Crea una sesión en Stripe
   ├─ Recibe Session ID: cs_test_123abc...
   └─ Retorna al frontend
   
5. Frontend (JavaScript)
   ├─ Recibe Session ID
   ├─ Llama a Stripe.redirectToCheckout()
   └─ Te redirige a Stripe Checkout
   
6. Página de Stripe
   ├─ Ingresas tarjeta: 4242 4242 4242 4242
   ├─ Ingresas fecha: 12/25
   ├─ Ingresas CVC: 123
   └─ Haces clic en "Pay"
   
7. Stripe procesa
   ├─ Valida la tarjeta
   ├─ Procesa el pago
   └─ Te redirige a success o cancel URL
   
8. Ves el resultado
   ├─ ✅ "Payment Successful!" 
   └─ O ❌ "Payment Cancelled"
```

---

## 🎓 Recursos Adicionales

- 📖 [Documentación de Stripe](https://docs.stripe.com)
- 📖 [Stripe.js Reference](https://stripe.com/docs/stripe-js)
- 📖 [.NET Stripe Library](https://github.com/stripe/stripe-dotnet)
- 📖 [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor)

---

## ⚠️ Notas Importantes

> ✋ **NO uses tarjetas REALES en modo test**
> - El modo test solo acepta tarjetas de prueba
> - Las transacciones no son reales
> - Los cargos no aparecerán en tarjetas reales

> 🔒 **Nunca compartas tu Secret Key**
> - Es como una contraseña
> - Siempre debe estar en `appsettings.json` (servidor)
> - Nunca en JavaScript o frontend

> 🚀 **Cuando vayas a producción:**
> - Reemplaza las keys test con keys LIVE
> - Las keys live empiezan con `pk_live_` y `sk_live_`
> - Asegúrate de usar HTTPS
> - Configura los webhooks de Stripe

---

¿Necesitas ayuda con algo específico? 🚀
