## Resumen
-

## Checklist DoD (obligatorio)
- [ ] Controladores delgados / lógica en servicios.
- [ ] Contrato de error consistente (`ApiProblemException` + `ProblemDetails`).
- [ ] Tests actualizados (unit/integration según impacto).
- [ ] Cobertura no cae por debajo del umbral CI.
- [ ] Documentación/runbook actualizado si aplica.
- [ ] Consideración de idempotencia/reintentos en operaciones críticas.

## Validación
- [ ] `dotnet test BMTECHRD.Pos.slnx`
