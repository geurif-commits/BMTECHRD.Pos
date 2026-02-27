# Definition of Done (PR)

Un PR se considera completo si cumple:

1. **Arquitectura**
   - Controladores delgados (sin lógica de dominio relevante).
   - Lógica en servicios/casos de uso con interfaces claras.

2. **Errores**
   - Contrato uniforme de errores (`ApiProblemException` + `ProblemDetails`).
   - Códigos de error estables para front.

3. **Pruebas**
   - Tests unitarios para reglas nuevas o modificadas.
   - Si toca endpoints críticos, agregar/ajustar prueba de integración.

4. **Calidad CI**
   - Build y tests en verde.
   - Gate de cobertura cumplido.

5. **Documentación**
   - README/runbook/checklist/bitácora actualizados cuando aplique.

6. **Seguridad y resiliencia**
   - Revisar efectos en auth, permisos e idempotencia.
