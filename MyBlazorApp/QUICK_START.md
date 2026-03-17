## 🚀 MyBlazorApp - Quick Start Guide

### Option 1: Run Locally Without ngrok (Development)

```powershell
# In PowerShell
cd C:\Users\djlei\source\repos\MyBlazorApp
dotnet run

# Open in browser: http://localhost:7184
# Login: admin / admin123
```

**appsettings.json should have:**
```json
"AppSettings": {
  "Ngrok": {
    "Enabled": false
  }
}
```

---

### Option 2: Run Locally WITH ngrok (Stripe Testing)

#### Terminal 1 - Start ngrok:
```powershell
ngrok http 7184
```

Wait for output:
```
Forwarding    https://a1b2c3d4e5f6.ngrok.io -> http://localhost:7184
```

#### Terminal 2 - Update config and run app:

Edit `appsettings.json`:
```json
"AppSettings": {
  "Port": 7184,
  "Ngrok": {
    "Enabled": true,
    "Url": "https://a1b2c3d4e5f6.ngrok.io"  // ← Use YOUR URL from Terminal 1
  }
}
```

Run app:
```powershell
cd C:\Users\djlei\source\repos\MyBlazorApp
dotnet run
```

#### Browser:
- Open: `https://a1b2c3d4e5f6.ngrok.io` (your ngrok URL)
- Login: `admin` / `admin123`
- Go to "New Payment"
- Select product → Select price → Enter quantity
- Click "Proceed to Payment"
- Use test card: `4242 4242 4242 4242` (any future date, any CVC)

---

### 📋 Configuration Explained

**appsettings.json:**
```json
"AppSettings": {
  "Port": 7184,                              // Your app port
  "Ngrok": {
    "Enabled": false,                        // Set to true for ngrok
    "Url": "https://xxx.ngrok.io"           // Your ngrok URL
  }
}
```

- **Enabled: false** → Stripe callbacks go to `http://localhost:7184`
- **Enabled: true** → Stripe callbacks go to your ngrok URL

No more hardcoding! Just update the JSON file.

---

### 🔧 Troubleshooting

**"Page not found" on Stripe checkout:**
1. Check ngrok is running in Terminal 1
2. Verify ngrok URL in appsettings.json matches Terminal 1 output
3. Make sure `Ngrok:Enabled` is `true`
4. Restart dotnet app after changing config

**ngrok not working:**
1. Make sure you ran `ngrok http 7184` (check port matches app)
2. Visit http://127.0.0.1:4040 to see ngrok dashboard
3. Install ngrok: https://ngrok.com/download

**Can't see ngrok URL:**
In Terminal 1, look for line starting with "Forwarding"

---

### ✅ What's Configured

- ✅ Port 7184 (update in appsettings.json if different)
- ✅ MySQL database (localhost:3306)
- ✅ Stripe test keys (in appsettings.json)
- ✅ User: admin / admin123
- ✅ ngrok support via UrlConfigurationService
- ✅ All comments in English
