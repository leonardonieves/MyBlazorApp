# 🎨 Diagrama Visual del Flujo de Stripe

## Arquitectura de la Integración

```
┌────────────────────────────────────────────────────────────────────┐
│                        USUARIO FINAL                               │
│                      (Navegador Web)                               │
└────────────────────────────────────────────────────────────────────┘
                              │
                              │ 1. Ingresa cantidad y producto
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                    TU APP BLAZOR (Cliente)                         │
│                    StripeCheckout.razor                            │
│                                                                    │
│  - Formulario: Monto, Producto                                    │
│  - Botón: "Proceed to Payment"                                    │
│  - onClick → ProcessPayment()                                      │
└────────────────────────────────────────────────────────────────────┘
                              │
                              │ 2. Envía request al servidor
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                    TU SERVIDOR BLAZOR                              │
│                    StripeService.cs                                │
│                                                                    │
│  CreateCheckoutSessionAsync(monto, producto)                      │
│  {                                                                 │
│    - Prepara datos de sesión                                      │
│    - Success URL: /payment-success?session_id=...                │
│    - Cancel URL: /payment-cancel                                  │
│    - Conecta a Stripe API                                         │
│  }                                                                 │
└────────────────────────────────────────────────────────────────────┘
                              │
                              │ 3. Llamada HTTPS a Stripe
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                   SERVIDORES DE STRIPE                             │
│                  (api.stripe.com/v1)                               │
│                                                                    │
│  POST /v1/checkout/sessions                                       │
│  {                                                                 │
│    "amount": 5000,                                                │
│    "currency": "usd",                                             │
│    "success_url": "...",                                          │
│    "cancel_url": "...",                                           │
│    ...                                                            │
│  }                                                                │
│                                                                    │
│  Response:                                                        │
│  {                                                                │
│    "id": "cs_test_a1b2c3d4e5f6...",                             │
│    "url": "https://checkout.stripe.com/pay/cs_test_a1b2c3d4..." │
│  }                                                                │
└────────────────────────────────────────────────────────────────────┘
                              │
                              │ 4. Devuelve sessionId
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                    TU APP BLAZOR (Cliente)                         │
│                    StripeCheckout.razor                            │
│                                                                    │
│  Recibe: sessionId = "cs_test_a1b2c3d4e5f6"                       │
│  Ejecuta: Navigation.NavigateTo(                                  │
│    "https://checkout.stripe.com/pay/cs_test_a1b2c3d4e5f6"        │
│  )                                                                │
│  → REDIRIGE AL USUARIO A STRIPE                                   │
└────────────────────────────────────────────────────────────────────┘
                              │
                              │ 5. Usuario en página de Stripe
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                   CHECKOUT.STRIPE.COM                              │
│              (Página de Pago Segura - HTTPS)                      │
│                                                                    │
│  ┌─────────────────────────────────────────────┐                 │
│  │ Producto: "Curso de Blazor"                 │                 │
│  │ Monto: $50.00                               │                 │
│  │                                             │                 │
│  │ Número de Tarjeta: [4242 4242 4242 4242]   │                 │
│  │ Fecha: [12 / 25]                            │                 │
│  │ CVC: [123]                                  │                 │
│  │                                             │                 │
│  │ [Botón: Pagar $50.00]                       │                 │
│  └─────────────────────────────────────────────┘                 │
│                                                                    │
│  Nota: Los datos de tarjeta NUNCA van a tu servidor               │
│        Stripe los procesa directamente                            │
└────────────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────┴─────────┐
                    │                   │
          Pago Exitoso        Pago Cancelado
                    │                   │
                    ▼                   ▼
┌────────────────┐      ┌────────────────────────┐
│ STRIPE API     │      │ STRIPE API             │
│ Procesa pago   │      │ Cancela sesión         │
│ Status: PAID   │      │ Status: NOT PAID       │
└────────────────┘      └────────────────────────┘
                    │                   │
                    └─────────┬─────────┘
                              │
              ┌───────────────┴───────────────┐
              │                               │
              ▼                               ▼
    Redirige a tu app        Redirige a tu app
    /payment-success         /payment-cancel
    ?session_id=...
              │                               │
              ▼                               ▼
┌────────────────────────────┐  ┌──────────────────────────┐
│  PaymentSuccess.razor      │  │ PaymentCancel.razor      │
│                            │  │                          │
│  1. Obtiene session_id     │  │ 1. Muestra:              │
│  2. Llama GetSessionAsync()│  │    "Pago cancelado"      │
│  3. Valida status="paid"   │  │                          │
│  4. Guarda en BD           │  │ 2. Opciones:             │
│  5. Muestra confirmación   │  │    - Reintentar          │
│                            │  │    - Volver al dashboard │
└────────────────────────────┘  └──────────────────────────┘
              │                               │
              │                               │
              └───────────────┬───────────────┘
                              │
                              ▼
                    ┌─────────────────────┐
                    │  TU BASE DE DATOS   │
                    │                     │
                    │ Tabla: Payments     │
                    │ ├─ Id: 1            │
                    │ ├─ Amount: 50.00    │
                    │ ├─ Status: completed│
                    │ ├─ SessionId: ...   │
                    │ └─ CreatedAt: now   │
                    └─────────────────────┘
```

---

## Flujo Paso a Paso en Código

### 1️⃣ Usuario hace click en "Proceed to Payment"

```csharp
// StripeCheckout.razor
private async Task ProcessPayment()
{
    // Validación básica
    if (amount <= 0) return;
    
    isProcessing = true;
    
    // Preparar URLs
    var baseUrl = Navigation.BaseUri.TrimEnd('/');
    var successUrl = $"{baseUrl}/payment-success";
    var cancelUrl = $"{baseUrl}/payment-cancel";
    
    // Crear sesión en Stripe
    var sessionId = await StripeService.CreateCheckoutSessionAsync(
        amount,
        productName,
        successUrl,
        cancelUrl
    );
    
    // Redirigir a Stripe
    Navigation.NavigateTo(
        $"https://checkout.stripe.com/pay/{sessionId}"
    );
}
```

### 2️⃣ Tu servidor crea sesión en Stripe

```csharp
// StripeService.cs
public async Task<string> CreateCheckoutSessionAsync(
    decimal amount, 
    string productName, 
    string successUrl, 
    string cancelUrl)
{
    // Preparar opciones de sesión
    var options = new SessionCreateOptions
    {
        PaymentMethodTypes = new List<string> { "card" },
        
        LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100),  // Convertir a centavos
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = productName,
                    },
                },
                Quantity = 1,
            },
        },
        
        Mode = "payment",  // Pago único (no suscripción)
        SuccessUrl = successUrl,
        CancelUrl = cancelUrl,
    };
    
    // Crear sesión
    var service = new SessionService();
    var session = await service.CreateAsync(options);
    
    // Devolver ID único
    return session.Id;
}
```

### 3️⃣ Usuario paga en Stripe

El usuario ve la página de Stripe (checkout.stripe.com), ingresa su tarjeta, y Stripe procesa el pago de forma segura.

### 4️⃣ Stripe redirige de vuelta a tu app

Después del pago (exitoso o cancelado), Stripe redirige al usuario a:

- **Éxito**: `tuapp.com/payment-success?session_id=cs_test_abc123`
- **Cancelado**: `tuapp.com/payment-cancel`

### 5️⃣ Tu app valida el pago

```csharp
// PaymentSuccess.razor
protected override async Task OnInitializedAsync()
{
    // Obtener el session_id de la URL
    var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
    var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
    var sessionId = queryParams.Get("session_id");
    
    if (string.IsNullOrEmpty(sessionId))
    {
        errorMessage = "No session ID provided";
        return;
    }
    
    try
    {
        // Obtener la sesión de Stripe
        var session = await StripeService.GetSessionAsync(sessionId);
        
        // Validar que el pago fue exitoso
        if (session.PaymentStatus == "paid")
        {
            // ✅ Pago validado
            // Guardar en tu base de datos
            await PaymentService.SavePaymentAsync(new Payment
            {
                UserId = currentUserId,
                ProductName = session.Metadata?["productName"] ?? "Unknown",
                Amount = (decimal)session.AmountTotal / 100m,
                Status = "completed",
                StripeSessionId = sessionId,
                CreatedAt = DateTime.UtcNow
            });
            
            successMessage = "¡Pago exitoso!";
            paymentConfirmed = true;
        }
        else
        {
            errorMessage = "Payment not completed";
        }
    }
    catch (Exception ex)
    {
        errorMessage = $"Error validating payment: {ex.Message}";
    }
}
```

### 6️⃣ Mostrar confirmación

```html
@if (paymentConfirmed)
{
    <div class="alert alert-success">
        <h4>✅ ¡Pago Exitoso!</h4>
        <p>Tu pago de $50.00 fue procesado correctamente.</p>
        <p>Session ID: @sessionId</p>
        <NavLink href="/dashboard">Volver al Dashboard</NavLink>
    </div>
}
else if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">
        @errorMessage
    </div>
}
```

---

## 🔐 Por qué Stripe es Seguro

```
┌─────────────────────────────────────────┐
│        Datos que maneja STRIPE          │
├─────────────────────────────────────────┤
│ ✅ Números de tarjeta                   │
│ ✅ Fecha de expiración                  │
│ ✅ CVC                                  │
│ ✅ Nombre del titular                   │
│                                         │
│ → Almacenados con encriptación PCI DSS │
│ → Tu servidor NUNCA los ve              │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│    Datos que recibe TU SERVIDOR         │
├─────────────────────────────────────────┤
│ ✅ Session ID                           │
│ ✅ Monto                                │
│ ✅ Estado de pago (paid/unpaid)         │
│ ✅ Timestamp                            │
│                                         │
│ → NO hay información sensible           │
│ → Seguro para guardar en BD             │
└─────────────────────────────────────────┘
```

---

## 💰 Cálculo de Montos

Stripe utiliza **centavos** en su API:

```
$50.00 USD = 5000 centavos

En código:
decimal amount = 50.00m;
long cents = (long)(amount * 100);  // 5000

En Stripe:
"amount": 5000,
"currency": "usd"
```

---

## 📱 Flujo Completo en 6 Pasos

```
┌──────────────────────────────────────────────────────────┐
│ 1. Usuario llena formulario de pago                      │
│    - Producto: "Curso de Blazor"                         │
│    - Cantidad: $50.00                                    │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 2. Tu app envía a Stripe: CreateCheckoutSessionAsync()   │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 3. Stripe devuelve ID de sesión                          │
│    ID: cs_test_a1b2c3d4e5f6g7h8i9j0k1l2m3               │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 4. Tu app redirige a checkout.stripe.com                │
│    URL: https://checkout.stripe.com/pay/cs_test_...     │
│    Usuario ingresa tarjeta (EN STRIPE)                   │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 5. Stripe redirige a tu app con resultado               │
│    SUCCESS: /payment-success?session_id=cs_test_...      │
│    Tu app valida: PaymentStatus == "paid"                │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 6. Guardar pago en BD y mostrar confirmación             │
│    - Tabla Payments: INSERT nuevo registro               │
│    - Tabla Users: Actualizar si necesario                │
│    - Mostrar: "¡Pago completado!"                        │
└──────────────────────────────────────────────────────────┘
```

---

Este es el flujo completo. La clave es entender que **Stripe maneja la seguridad**, tú solo coordinas.
