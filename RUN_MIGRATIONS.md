# 🚀 COMANDOS PARA CREAR MIGRACIONES Y BD

## Paso 1: Abre PowerShell y navega a la carpeta del proyecto

```powershell
cd MyBlazorApp
```

## Paso 2: Crear la Migration Inicial

```powershell
dotnet ef migrations add InitialCreate
```

**Resultado esperado:**
```
Build started...
Build completed.
Done. To undo this action, use 'dotnet ef migrations remove'
```

Esto creará la carpeta `Migrations/` con los archivos de migración.

## Paso 3: Aplicar la Migration a la BD (Crear BD y tablas)

```powershell
dotnet ef database update
```

**Resultado esperado:**
```
Build started...
Build completed.
Done. Connection string used: 'Server=localhost;Port=3306;Database=MyBlazorAppDb;Uid=root;Pwd=root;'
```

## Paso 4: Verificar que la BD se creó

```powershell
mysql -u root -p root -e "USE MyBlazorAppDb; SHOW TABLES;"
```

**Deberías ver 3 tablas:**
```
Tables_in_MyBlazorAppDb
Payments
Roles
Users
```

## Paso 5: Ejecutar la aplicación

```powershell
dotnet run
```

## Paso 6: Abrir en navegador

```
https://localhost:7000
```

---

# ✅ COMANDO RÁPIDO (TODO DE UNA VEZ)

Si quieres ejecutar todo junto:

```powershell
cd MyBlazorApp
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

---

# 🐛 SI HAY ERROR: "Unable to find project"

Asegúrate de estar en la carpeta correcta:

```powershell
# Esto debería mostrar MyBlazorApp.csproj
ls
```

---

**¿Ejecutaste los comandos? Cuéntame qué pasó** 🚀
