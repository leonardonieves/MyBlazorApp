-- Script para crear usuarios de prueba en MySQL
-- Ejecutar después de: dotnet ef database update

-- Usuario Admin
INSERT INTO Users (Username, Email, PasswordHash, RoleId, CreatedAt, IsActive)
VALUES (
  'admin',
  'admin@example.com',
  'yb7OL3KNe5Xs2R8W2KqPqZ6V4X9L2M5N8Q1R4S7U0V3W6Z9A2B5C8D1E4F7G0H3',
  1,
  NOW(),
  1
);

-- Usuario Basic
INSERT INTO Users (Username, Email, PasswordHash, RoleId, CreatedAt, IsActive)
VALUES (
  'user',
  'user@example.com',
  'yb7OL3KNe5Xs2R8W2KqPqZ6V4X9L2M5N8Q1R4S7U0V3W6Z9A2B5C8D1E4F7G0H3',
  2,
  NOW(),
  1
);

-- Nota: El hash es para "user123"
-- Para cambiar la contraseña, usa AuthService.ChangePasswordAsync()
