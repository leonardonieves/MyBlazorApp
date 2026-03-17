# 🛒 Guía Completa: Integración de Stripe con Blazor Server

## 📚 Índice
1. [¿Qué es Stripe?](#qué-es-stripe)
2. [Flujo de Pago](#flujo-de-pago)
3. [Componentes de la Integración](#componentes)
4. [Paso a Paso](#paso-a-paso)
5. [Ciclo de Vida Completo](#ciclo-de-vida-completo)
6. [Casos de Uso](#casos-de-uso)

---

## 🎯 ¿Qué es Stripe?

Stripe es una plataforma de procesamiento de pagos en línea que permite a tu aplicación aceptar pagos con tarjetas de crédito de forma segura. 

**Ventajas:**
- ✅ No almacenas datos de tarjetas (cumple PCI DSS automáticamente)
- ✅ API robusta y documentada
- ✅ Modo de prueba (Test Mode) para desarrollar sin dinero real
- ✅ Webhooks para eventos de pago
- ✅ Dashboard para ver todas las transacciones

---

## 🔄 Flujo de Pago en Stripe

```
┌─────────────────────────────────────────────────────────────┐
│                      USUARIO EN BLAZOR                       │
│                                                              │
│  1. Ingresa monto y producto                                │
│  2. Click en "Proceed to Payment"                           │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    TU SERVIDOR BLAZOR                        │
│                  (StripeService.cs)                         │
│                                                              │
│  3. Crea una "Checkout Session" en Stripe                   │
│     - Envía: monto, producto, URLs de success/cancel        │
│     - Recibe: ID de sesión único                            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  SERVIDOR DE STRIPE                          │
│                (checkout.stripe.com)                         │
│                                                              │
│  4. Stripe genera página de checkout segura                │
│  5. Usuario ingresa datos de tarjeta (EN STRIPE, NO EN TI) │
│  6. Procesa pago de forma segura                           │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    RESULTADO DEL PAGO                        │
│                                                              │
│  Éxito: Redirige a tuURL/success?session_id=...            │
│  Cancelado: Redirige a tuURL/cancel                        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                 VALIDAR PAGO EN TU SERVIDOR                  │
│                                                              │
│  7. Obtener sesión de Stripe por ID                         │
│  8. Verificar que payment_status == "paid"                  │
│  9. Guardar pago en tu BD                                   │
│  10. Mostrar confirmación al usuario                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Componentes de la Integración

### 1. **StripeService.cs** (Backend)
```
Responsabilidades:
├─ Configurar API Key de Stripe
├─ Crear Checkout Sessions
├─ Recuperar sesiones por ID
└─ Validar estado de pago
```

### 2. **StripeCheckout.razor** (Frontend)
```
Responsabilidades:
├─ Formulario de entrada (monto, producto)
├─ Llamar a StripeService
├─ Redirigir a checkout.stripe.com
└─ Mostrar estado
```

### 3. **PaymentSuccess.razor** (Frontend)
```
Responsabilidades:
├─ Recibir session_id de Stripe
├─ Validar pago con StripeService
├─ Guardar en base de datos
└─ Mostrar confirmación
```

### 4. **appsettings.json**
```
Contiene:
├─ Stripe:PublishableKey (pk_test_...)
└─ Stripe:SecretKey (sk_test_...)
```

---

## 📋 Paso a Paso de la Integración

### PASO 1: Obtener Keys de Stripe

1. Ve a https://dashboard.stripe.com/register
2. Crea una cuenta (usa email)
3. Verifica tu email
4. En el Dashboard, ve a "Developers" → "API Keys"
5. Copia:
   - **Publishable Key** (comienza con `pk_test_`)
   - **Secret Key** (comienza con `sk_test_`)

⚠️ **IMPORTANTE**: Secret Key es privada, NUNCA la compartas

### PASO 2: Configurar en appsettings.json

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_KEY_HERE",
    "SecretKey": "sk_test_YOUR_KEY_HERE"
  }
}
```

### PASO 3: Instalar Stripe NuGet Package

```bash
dotnet add package Stripe.net
```

### PASO 4: Crear StripeService

El servicio ya existe en tu app:

```csharp
public class StripeService
{
    public StripeService(IConfiguration configuration)
    {
        var secretKey = configuration["Stripe:SecretKey"];
        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        decimal amount, 
        string productName, 
        string successUrl, 
        string cancelUrl)
    {
        // Crea sesión de Stripe
        // Devuelve ID de sesión
    }
}
```

### PASO 5: Crear Página de Checkout

```razor
@page "/stripe-checkout"
@inject StripeService StripeService
@inject NavigationManager Navigation

<!-- Formulario con monto y producto -->
<button @onclick="ProcessPayment">Ir a Pagar</button>

@code {
    private async Task ProcessPayment()
    {
        // 1. Crear sesión en Stripe
        var sessionId = await StripeService.CreateCheckoutSessionAsync(
            amount: 50.00m,
            productName: "Mi Producto",
            successUrl: "https://miapp.com/payment-success",
            cancelUrl: "https://miapp.com/payment-cancel"
        );

        // 2. Redirigir a Stripe Checkout
        var checkoutUrl = $"https://checkout.stripe.com/pay/{sessionId}";
        Navigation.NavigateTo(checkoutUrl);
    }
}
```

### PASO 6: Validar Pago en Success

```razor
@page "/payment-success"

@code {
    protected override async Task OnInitializedAsync()
    {
        // Obtener session_id de la URL
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var sessionId = queryParams.Get("session_id");

        if (!string.IsNullOrEmpty(sessionId))
        {
            // Validar sesión con Stripe
            var session = await StripeService.GetSessionAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                // ✅ Pago completado
                // Guardar en BD
                await PaymentService.SavePaymentAsync(new Payment 
                { 
                    Amount = session.AmountTotal / 100m,
                    Status = "completed",
                    StripeSessionId = sessionId
                });
            }
        }
    }
}
```

---

## 🔁 Ciclo de Vida Completo

### Escenario: Usuario compra un producto de $50

**Paso 1: Usuario en Dashboard**
```
Dashboard.razor
├─ Ve botón "New Payment"
└─ Click → Va a /stripe-checkout
```

**Paso 2: Formulario de Pago**
```
StripeCheckout.razor
├─ Ingresa:
│  ├─ Product Name: "Curso de Blazor"
│  └─ Amount: $50.00
├─ Click "Proceed to Payment"
└─ Backend:
   ├─ StripeService.CreateCheckoutSessionAsync()
   ├─ Crea sesión en Stripe
   ├─ Recibe sessionId: "cs_test_a1b2c3d4e5f6"
   └─ Redirige a checkout.stripe.com/pay/cs_test_a1b2c3d4e5f6
```

**Paso 3: Pago en Stripe (checkout.stripe.com)**
```
Página Segura de Stripe
├─ Usuario ve:
│  ├─ Producto: "Curso de Blazor"
│  ├─ Monto: $50.00
│  └─ Formulario de tarjeta
├─ Usuario ingresa:
│  ├─ Números de tarjeta (TEST)
│  ├─ Fecha expiración
│  ├─ CVC
│  └─ Email
├─ Stripe procesa pago
└─ Redirige a:
   ├─ SUCCESS: tuapp.com/payment-success?session_id=cs_test_a1b2c3d4e5f6
   └─ CANCEL: tuapp.com/payment-cancel
```

**Paso 4: Validación en Tu App**
```
PaymentSuccess.razor
├─ Obtiene session_id de URL
├─ Llama: StripeService.GetSessionAsync(sessionId)
├─ Verifica: PaymentStatus == "paid"
├─ Si es verdad:
│  ├─ Guarda en BD:
│  │  ├─ Amount: 50.00
│  │  ├─ Status: "completed"
│  │  └─ StripeSessionId: "cs_test_a1b2c3d4e5f6"
│  └─ Muestra: "¡Pago exitoso!"
└─ Si es falso:
   └─ Muestra: "Error en validación"
```

**Paso 5: Confirmación**
```
Dashboard.razor
├─ Usuario vuelve al dashboard
├─ Ve nueva transacción en tabla
├─ Estado: "Completed"
└─ Monto: $50.00
```

---

## 💳 Tarjetas de Prueba en Stripe

Para probar pagos exitosos:

| Resultado | Número | Exp | CVC |
|-----------|--------|-----|-----|
| ✅ Éxito | 4242 4242 4242 4242 | 12/25 | 123 |
| ❌ Rechazado | 4000 0000 0000 0002 | 12/25 | 123 |
| 3D Secure | 4000 2500 0000 3010 | 12/25 | 123 |

---

## 🔧 Casos de Uso

### Caso 1: Pago Simple (Lo que tienes)
```
Usuario → Ingresa monto → Paga en Stripe → Validar → Guardar
```

### Caso 2: Productos Predefinidos
```
Usuario → Selecciona producto → Precio fijo → Pagar

// En StripeCheckout.razor
<select @bind="selectedProduct">
    <option value="course">Curso ($49)</option>
    <option value="ebook">eBook ($9)</option>
</select>
```

### Caso 3: Suscripción Recurrente
```
// Cambiar mode de "payment" a "subscription"
options.Mode = "subscription";

// En lugar de amount, usar recurring price
```

### Caso 4: Múltiples Items en Carrito
```
// Agregar múltiples items
LineItems = new List<SessionLineItemOptions>
{
    new() { PriceData = ..., Quantity = 1 },
    new() { PriceData = ..., Quantity = 2 }
}
```

---

## 🔐 Seguridad

### ✅ Lo que Stripe maneja por ti:
- Almacenar datos de tarjeta de forma segura
- Encriptación PCI DSS compliant
- Cumplimiento regulatorio

### ✅ Lo que TÚ debes hacer:
- Guardar Secret Key en `appsettings.json` (no en código)
- Validar SIEMPRE pagos en el servidor
- No confiar en cliente para validar pagos
- Usar HTTPS en producción

### ❌ Lo que NUNCA debes hacer:
- Compartir Secret Key
- Almacenar números de tarjeta
- Procesar pagos en JavaScript

---

## 📊 Base de Datos

Estructura para guardar pagos:

```sql
CREATE TABLE Payments (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT,
    ProductName VARCHAR(255),
    Amount DECIMAL(10,2),
    Status VARCHAR(50),           -- 'pending', 'completed', 'failed'
    StripeSessionId VARCHAR(255), -- ID de sesión de Stripe
    CreatedAt DATETIME,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

---

## 🚀 Próximos Pasos

1. **Obtén tus keys de Stripe**: https://dashboard.stripe.com
2. **Actualiza appsettings.json** con tus keys
3. **Prueba con tarjetas de prueba**
4. **Verifica pagos en Dashboard de Stripe**
5. **En producción, obtén keys LIVE**

---

## 📞 Recursos Útiles

- Documentación: https://stripe.com/docs
- Dashboard: https://dashboard.stripe.com
- API Reference: https://stripe.com/docs/api
- Stripe.NET: https://github.com/stripe/stripe-dotnet

---

**Resumen Simple:**
1. Usuario ingresa monto y producto
2. Tu app crea una "sesión de pago" en Stripe
3. Usuario es redirigido a Stripe para pagar (seguro)
4. Stripe redirige de vuelta con resultado
5. Tu app valida y guarda el pago en BD
6. Usuario ve confirmación

¡Eso es todo! Stripe maneja la parte complicada (pago seguro), tú solo coordinas el flujo.
