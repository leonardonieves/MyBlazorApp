-- Script para crear usuarios de prueba
USE MyBlazorAppDb;

-- Primero eliminamos usuarios existentes
DELETE FROM Users WHERE Username IN ('admin', 'user');

-- SHA256 Hash en Base64:
-- "admin123" → CxT1DSymjYMi0MOzQPLFO5oEofP/H6B+mOhufw+ieuM=
-- "user123" → pmWkWSBCL50Hfkh+79xPuKBKHzz/H6B+mOhufw+ieuM=

-- Insertar usuarios
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES 
  ('admin', 'admin@example.com', 'CxT1DSymjYMi0MOzQPLFO5oEofP/H6B+mOhufw+ieuM=', 1, 1, NOW()),
  ('user', 'user@example.com', 'pmWkWSBCL50Hfkh+79xPuKBKHzz/H6B+mOhufw+ieuM=', 2, 1, NOW());

SELECT * FROM Users;
SELECT * FROM Roles;
