# 🔍 DEBUGGEAR EL PROBLEMA - "NO PASA NADA AL CLICK"

## ⚠️ El botón no responde cuando hago click

Si el botón "Proceed to Payment" no hace nada, sigue estos pasos:

---

## PASO 1️⃣: Abre la Consola del Navegador

Presiona **F12** en tu navegador.

Verás algo así:
```
┌─────────────────────────────────────┐
│ Developer Tools (Inspector)         │
├─────────────────────────────────────┤
│ Elements | Console | Network | ... │
│                                     │
│ > (cursor aquí)                    │
└─────────────────────────────────────┘
```

Asegúrate de estar en la pestaña **"Console"**.

---

## PASO 2️⃣: Verifica que Stripe.js esté Cargado

En la consola, escribe:

```javascript
typeof Stripe
```

Presiona Enter.

**✅ Si ves:** `"function"`
→ Stripe.js está correctamente cargado ✓

**❌ Si ves:** `"undefined"`
→ Stripe.js NO está cargado ✗ Recarga la página (Ctrl+R)

---

## PASO 3️⃣: Haz Click en el Botón y Observa los Logs

1. En la consola, escribe:
```javascript
console.clear()
```
(Esto limpia los logs anteriores)

2. Haz click en **"Proceed to Payment"**

3. Observa la consola. Deberías ver mensajes como:

```
Starting payment process...
Creating session for Sample Product - $29.99
Session created: cs_test_b1jrKzwqvI...
PublishableKey exists: 87 chars
SessionId: cs_test_b1jrKzwqvI...
Stripe exists: true
redirectToCheckout called with sessionId: cs_test_b1jrKzwqvI...
redirectToCheckout called with publishableKey: pk_test_51TAy7NGRja...
Initializing Stripe...
Stripe initialized successfully
Redirecting to checkout with session: cs_test_b1jrKzwqvI...
Successfully redirected to Stripe Checkout
```

---

## 🐛 ERRORES COMUNES Y SOLUCIONES

### ❌ Error: "Stripe is not defined"

```
Uncaught ReferenceError: Stripe is not defined
```

**Solución:**
1. Recarga la página (Ctrl+R o Cmd+R)
2. Espera 2-3 segundos
3. Intenta nuevamente

**Si sigue fallando:**
- Verifica que en `App.razor` está: `<script src="https://js.stripe.com/v3/"></script>`
- Verifica tu conexión a Internet
- Intenta en otro navegador

---

### ❌ Error: "Stripe Publishable Key is not configured"

```
Error processing payment: Stripe Publishable Key is not configured. Check appsettings.json
```

**Solución:**
1. Abre `appsettings.json`
2. Verifica que `PublishableKey` NO esté vacío
3. Debe tener un valor como: `"pk_test_51TAy7NGRja..."`
4. Guarda el archivo
5. Recarga la página en el navegador (Ctrl+R)

---

### ❌ Error: "Session ID is empty"

```
Error processing payment: Session ID is empty
```

**Solución:**
1. Verifica que `SecretKey` en `appsettings.json` sea correcta
2. Debe empezar con `sk_test_`
3. No debe tener espacios extra
4. Guarda el archivo
5. Reinicia la aplicación: `dotnet run`

---

### ❌ Error: "Session creation failed"

```
Logs: Creating session for Sample Product - $29.99
      ERROR: ...something...
```

**Solución:**
1. Mira el error exacto en la consola
2. Si dice "Invalid API Key" → Tu `SecretKey` es inválida
3. Si dice "Invalid Publishable Key" → Tu `PublishableKey` es inválida
4. Ve a https://dashboard.stripe.com y copia las keys nuevamente
5. Actualiza `appsettings.json`
6. Reinicia: `dotnet run`

---

### ❌ Te redirige a Stripe pero dice "Error"

```
Error: Invalid or expired session_id
```

**Solución:**
1. Espera 1-2 minutos
2. Intenta crear una nueva sesión
3. Si sigue fallando, ve a Stripe Dashboard y verifica que tu API keys sean válidas

---

## ✅ LISTA DE VERIFICACIÓN - QUÉ DEBE ESTAR CORRECTO

Abre una por una estas cosas y verifica:

### 1. `appsettings.json`
```json
"Stripe": {
  "PublishableKey": "pk_test_51TAy7NGRjaQMHgPx...",  // ✅ Debe existir
  "SecretKey": "sk_test_51TAy7NGRjaQMHgPx..."        // ✅ Debe existir
}
```

Verifica:
- [ ] PublishableKey NO está vacío
- [ ] SecretKey NO está vacío
- [ ] Ambas empiezan con `pk_test_` y `sk_test_` respectivamente
- [ ] No hay espacios extras
- [ ] El JSON es válido (usa: https://jsonlint.com/)

---

### 2. `App.razor`
Debe tener en `<head>`:
```html
<script src="https://js.stripe.com/v3/"></script>
```

Y en `<body>` antes del cierre:
```html
<script src="_framework/blazor.web.js"></script>
<script src="js/stripe.js"></script>
```

---

### 3. La Aplicación Está Corriendo
```powershell
dotnet run
```

Debe decir:
```
Now listening on: https://localhost:7000
```

Si ves un error → Tu puerto está en uso. Intenta:
```powershell
dotnet run --urls https://localhost:7001
```

---

### 4. Estás en la Página Correcta

Debe estar en:
```
https://localhost:7000/stripe-checkout
```

NO en:
- `http://localhost:7000` (falta HTTPS)
- `localhost:7000` (falta HTTPS)
- Otra ruta

---

## 🎯 PASOS FINALES PARA DEBUGGEAR

Si **NADA** de lo anterior funciona:

### 1. Limpia la Caché del Navegador
```
F12 → Application → Clear storage → Clear all
```
Luego recarga (Ctrl+R)

### 2. Reinicia Todo
```powershell
# En PowerShell:
cd MyBlazorApp
dotnet clean
dotnet build
dotnet run
```

### 3. Abre Incógnito/Privado
El navegador en privado no usa caché.

### 4. Intenta Otro Navegador
Chrome, Firefox, Edge, Safari...

---

## 📝 INFORMACIÓN IMPORTANTE PARA REPORTAR EL PROBLEMA

Si todavía no funciona, copia toda la información de la consola (F12):

1. F12 → Console
2. Haz click en el botón
3. Clic derecho en los logs
4. "Copy all messages"
5. Pega aquí para referencia

---

¡Con estos pasos deberías identificar el problema! 🚀
