-- ========================================
-- Script de Creación de Base de Datos
-- BMTECHRD.Pos - PostgreSQL Setup
-- ========================================

-- NOTA: Ejecuta este script conectado a la base de datos 'postgres'
-- Ejemplo: psql -U postgres -d postgres -f setup-database.sql

-- Verificar si la base de datos existe y crearla si no existe
SELECT 'Verificando base de datos bmtechrd_pos...' as status;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'bmtechrd_pos') THEN
        RAISE NOTICE 'Base de datos bmtechrd_pos no existe. Creándola...';
        -- No se puede crear DB dentro de un bloque, se debe hacer fuera
    ELSE
        RAISE NOTICE 'Base de datos bmtechrd_pos ya existe.';
    END IF;
END $$;

-- Crear base de datos si no existe (ejecutar fuera del bloque)
CREATE DATABASE bmtechrd_pos
    WITH 
    ENCODING = 'UTF8'
    LC_COLLATE = 'Spanish_Spain.1252'
    LC_CTYPE = 'Spanish_Spain.1252'
    TEMPLATE = template0;

-- Mensaje
SELECT 'Base de datos bmtechrd_pos creada/verificada exitosamente' as status;

-- ========================================
-- SIGUIENTE PASO:
-- Conectarse a la base de datos bmtechrd_pos
-- y ejecutar las migraciones de Entity Framework:
--   cd BMTECHRD.Pos.Api
--   dotnet ef database update --project ../BMTECHRD.Pos.Infrastructure
-- ========================================
