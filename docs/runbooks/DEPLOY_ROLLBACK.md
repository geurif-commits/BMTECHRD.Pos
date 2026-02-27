# Runbook: Deploy & Rollback

## Objetivo
Estandarizar despliegue y reversión rápida ante incidentes.

## Pre-checks
1. CI verde (build/test/coverage gate).
2. Migraciones revisadas.
3. Ventana de despliegue aprobada.

## Deploy
1. Publicar artefacto release.
2. Aplicar configuración por entorno (`ConnectionStrings`, `Jwt`, logging).
3. Ejecutar smoke check:
   - `/api/health`
   - login básico
   - endpoint crítico de caja/reportes.
4. Monitorear 15 minutos (errores 5xx, latencia, auth failures).

## Rollback
1. Revertir a artefacto previo estable.
2. Restaurar config previa.
3. Si hubo migración no compatible, ejecutar plan de rollback DB aprobado.
4. Repetir smoke check y comunicar estado.

## Evidencia mínima
- versión desplegada
- hora inicio/fin
- resultado smoke checks
- incidencias detectadas
