# Senior-ready checklist (ejecución)

## 1) Arquitectura de casos de uso
- [x] Extraer caso de uso de creación de órdenes por lote a servicio dedicado (`OrderBatchService`).
- [ ] Extraer pagos, turnos, inventario y reportes a casos de uso dedicados.

## 2) Error handling estándar
- [x] Middleware global en formato `ProblemDetails`.
- [x] Códigos de error de negocio iniciales para órdenes.
- [ ] Unificar todos los controladores al mismo contrato de error.

## 3) Pruebas
- [x] Tests de auth flow existentes.
- [ ] Unit tests por dominio (orden, inventario, pago, turno).
- [ ] Integration tests API+DB para endpoints críticos.

## 4) Documentación operativa
- [x] README base con setup/build/run/test.
- [ ] Runbooks operativos por entorno.

## 5) Cliente WPF
- [ ] Reducir lógica de orquestación en `MainWindow` y mover a servicios/viewmodels.

## 6) CI/CD quality gates
- [ ] Pipeline obligatorio con build+tests+análisis estático+cobertura.

## 7) Resiliencia/performance
- [ ] Idempotencia y reglas anti-reintentos duplicados para operaciones críticas.
- [ ] Pruebas de carga base para endpoints clave.
