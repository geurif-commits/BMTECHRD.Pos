# Runbook: Performance Baseline

## Objetivo
Ejecutar una prueba de carga mínima recurrente para detectar degradación temprana en endpoints críticos.

## Herramienta
- k6

## Script inicial
- `scripts/load/k6_cashier_smoke.js`

## Ejecución
```bash
k6 run scripts/load/k6_cashier_smoke.js \
  -e BASE_URL=http://localhost:5000 \
  -e BUSINESS_ID=<guid>
```

## Criterios base actuales
- `http_req_failed < 5%`
- `p95 http_req_duration < 1200ms`

## Recomendación
- Ejecutar semanalmente y tras cambios en `Cashier`, `Orders`, `Shifts`.
- Versionar baseline por entorno (dev/staging/prod-like).
