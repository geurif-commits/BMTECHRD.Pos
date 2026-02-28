# Runbook: Incident Response (API)

## Severidad alta (S1)
Ejemplos: 500 masivos, fallos auth generalizados, corrupción de flujo de pagos.

## Checklist inmediata
1. Confirmar alcance (endpoint/rol/negocio impactado).
2. Capturar `traceId` de respuestas ProblemDetails.
3. Revisar logs por `traceId`.
4. Mitigar:
   - rollback a último deploy estable, o
   - feature flag / bloqueo temporal de endpoint crítico.
5. Comunicar estado y ETA.

## Postmortem mínimo
- Timeline de eventos.
- Causa raíz técnica.
- Acciones correctivas permanentes.
- Tests/reglas nuevas para prevenir recurrencia.
