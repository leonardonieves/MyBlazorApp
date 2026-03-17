# Running the Application with ngrok

## Prerequisites
- .NET 8 SDK installed
- MySQL running on localhost:3306
- ngrok installed (download from https://ngrok.com/download)

## Quick Start (Local Development - Without ngrok)

```powershell
cd C:\Users\djlei\source\repos\MyBlazorApp
dotnet run
```

App will run on `http://localhost:7184`

---

## Testing with Stripe (Using ngrok)

### Step 1: Start ngrok in a PowerShell terminal

```powershell
ngrok http 7184
```

You should see output like:
```
Forwarding                    https://a1b2c3d4e5f6.ngrok.io -> http://localhost:7184
```

Copy this URL.

### Step 2: Update `appsettings.json`

Edit `MyBlazorApp/appsettings.json`:

```json
"AppSettings": {
  "Port": 7184,
  "Ngrok": {
    "Enabled": true,
    "Url": "https://a1b2c3d4e5f6.ngrok.io"
  }
}
```

Replace the URL with your actual ngrok URL from Step 1.

### Step 3: Run the Application

```powershell
cd C:\Users\djlei\source\repos\MyBlazorApp
dotnet run
```

### Step 4: Test Payment Flow

1. Open `https://a1b2c3d4e5f6.ngrok.io` in your browser
2. Login with `admin` / `admin123`
3. Navigate to "New Payment"
4. Select a product, price, and quantity
5. Click "Proceed to Payment"
6. Use test card `4242 4242 4242 4242` with any future date and any 3-digit CVC

---

## Configuration Options

All settings are in `appsettings.json`:

```json
"AppSettings": {
  "Port": 7184,              // Your app's local port
  "Ngrok": {
    "Enabled": false,        // Set to true when using ngrok
    "Url": ""                // Your ngrok URL (leave empty if disabled)
  }
}
```

- **Enabled: false** → Uses `http://localhost:7184`
- **Enabled: true** → Uses your ngrok URL for Stripe callbacks

---

## Troubleshooting

### "Page not found" when clicking Stripe Checkout button

1. Verify ngrok is running: Look for "Forwarding" line in ngrok terminal
2. Verify ngrok URL is correct in `appsettings.json`
3. Make sure `Ngrok:Enabled` is set to `true`
4. Restart the app after updating `appsettings.json`

### Can't connect to ngrok URL

1. Verify port 7184 is correct in `appsettings.json`
2. Run `dotnet run` in the project directory to check the actual port
3. Update ngrok command to match: `ngrok http 7184`

### Payment shows error but ngrok is working

1. Check that Stripe keys in `appsettings.json` are correct
2. Verify Prices exist in Stripe dashboard
3. Check browser console (F12) for detailed error messages
