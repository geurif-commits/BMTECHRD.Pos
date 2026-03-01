# Plan de Actualización a .NET 10.0
## BMTECHRD.Pos Solution

---

## Tabla de Contenidos

- [Resumen Ejecutivo](#resumen-ejecutivo)
- [Estrategia de Migración](#estrategia-de-migración)
- [Análisis Detallado de Dependencias](#análisis-detallado-de-dependencias)
- [Planes Proyecto por Proyecto](#planes-proyecto-por-proyecto)
  - [BMTECHRD.Pos.Domain](#bmtechrdposdomain)
  - [BMTECHRD.Pos.Application](#bmtechrdposapplication)
  - [BMTECHRD.Pos.Infrastructure](#bmtechrdposinfrastructure)
  - [BMTECHRD.Pos.Auth.Core](#bmtechrdposauthcore)
  - [BMTECHRD.Pos.Api](#bmtechrdposapi)
  - [BMTECHRD.Pos.App](#bmtechrdposapp)
  - [BMTECHRD.Pos.Api.Tests](#bmtechrdposapitests)
- [Referencia de Actualización de Paquetes](#referencia-de-actualización-de-paquetes)
- [Catálogo de Cambios Importantes (Breaking Changes)](#catálogo-de-cambios-importantes-breaking-changes)
- [Gestión de Riesgos](#gestión-de-riesgos)
- [Estrategia de Pruebas y Validación](#estrategia-de-pruebas-y-validación)
- [Evaluación de Complejidad y Esfuerzo](#evaluación-de-complejidad-y-esfuerzo)
- [Estrategia de Control de Versiones](#estrategia-de-control-de-versiones)
- [Criterios de Éxito](#criterios-de-éxito)

---

## Resumen Ejecutivo

### Descripción del Escenario

Este plan describe la actualización de la solución **BMTECHRD.Pos** de **.NET 8.0 a .NET 10.0 (LTS)**. La solución incluye aplicaciones API REST, una aplicación WPF de escritorio, y bibliotecas de clase que implementan arquitectura limpia con capas de dominio, aplicación e infraestructura.

### Alcance

**Proyectos Afectados**: 7 proyectos

| Proyecto | Framework Actual | Framework Destino | Tipo |
|----------|------------------|-------------------|------|
| BMTECHRD.Pos.Domain | net8.0 | net10.0 | ClassLibrary |
| BMTECHRD.Pos.Application | net8.0 | net10.0 | ClassLibrary |
| BMTECHRD.Pos.Infrastructure | net8.0 | net10.0 | ClassLibrary |
| BMTECHRD.Pos.Auth.Core | net8.0 | net10.0 | ClassLibrary |
| BMTECHRD.Pos.Api | net8.0 | net10.0 | AspNetCore |
| BMTECHRD.Pos.App | net8.0-windows | net10.0-windows | Wpf |
| BMTECHRD.Pos.Api.Tests | net8.0 | net10.0 | DotNetCoreApp (Tests) |

**Estado Actual**: Todos los proyectos están en formato SDK-style, facilitando la actualización.

### Estrategia Seleccionada

**Estrategia Todo-a-la-Vez (All-At-Once)** - Todos los proyectos se actualizan simultáneamente en una sola operación.

**Justificación**:
- ✅ Solución pequeña (7 proyectos)
- ✅ Todos los proyectos actualmente en .NET 8.0 (moderno)
- ✅ Estructura de dependencias clara y simple (2-3 niveles)
- ✅ Todos los paquetes NuGet tienen versiones compatibles con .NET 10.0
- ✅ Sin vulnerabilidades de seguridad críticas
- ✅ Proyectos ya en formato SDK-style

**Ventajas**:
- Tiempo de finalización más rápido
- Sin complejidad de multi-targeting
- Resolución limpia de dependencias
- Coordinación simple

**Desafíos**:
- El proyecto WPF (BMTECHRD.Pos.App) tiene 1082+ líneas estimadas de código a revisar por incompatibilidades binarias
- 2 paquetes obsoletos requieren migración a alternativas modernas

### Métricas Descubiertas

| Métrica | Valor | Estado |
|---------|-------|--------|
| **Total de Proyectos** | 7 | Todos requieren actualización |
| **Total de Paquetes NuGet** | 26 | 15 necesitan actualización |
| **Paquetes Obsoletos** | 2 | Requieren reemplazo |
| **Archivos de Código** | 260 | |
| **Archivos con Incidentes** | 64 | 24.6% del código |
| **Líneas de Código Total** | 14,657 | |
| **LOC Estimadas a Modificar** | 1,131+ | 7.7% del código base |

### Evaluación de Compatibilidad

**Compatibilidad de Paquetes**:
- ✅ Compatible: 11 paquetes (42.3%)
- 🔄 Actualización Recomendada: 13 paquetes (50.0%)
- ⚠️ Incompatible/Obsoleto: 2 paquetes (7.7%)

**Compatibilidad de API**:
- 🔴 Incompatibilidades Binarias: 1,018 (Alto impacto - principalmente en proyecto WPF)
- 🟡 Incompatibilidades de Código Fuente: 15 (Impacto medio)
- 🔵 Cambios de Comportamiento: 98 (Impacto bajo - requieren pruebas)
- ✅ Compatible: 20,989

### Problemas Críticos

1. **Paquetes Obsoletos**:
   - `Microsoft.IdentityModel.Tokens` v7.6.0
   - `System.IdentityModel.Tokens.Jwt` v7.6.0
   - **Acción**: Actualizar a versiones modernas de la familia `Microsoft.IdentityModel.*`

2. **Proyecto WPF con Alta Complejidad**:
   - `BMTECHRD.Pos.App` tiene 1,018 incompatibilidades binarias relacionadas con APIs de WPF
   - La mayoría son relacionadas con tipos de WPF (`RoutedEventHandler`, `TextBox`, `Button`, etc.)
   - **Acción**: Cambiar TargetFramework a `net10.0-windows` y validar comportamiento en runtime

3. **Incompatibilidades de API**:
   - 15 incompatibilidades de código fuente en proyectos API y de pruebas
   - Principalmente relacionadas con cambios en ASP.NET Core y Entity Framework Core

### Enfoque Recomendado

**Operación Atómica Única**: Actualizar todos los archivos de proyecto + todas las referencias de paquetes + corregir todos los errores de compilación en una sola operación coordinada.

### Estrategia de Iteración

Dado que se trata de una solución de complejidad media con un punto de riesgo concentrado (proyecto WPF), se utilizará el siguiente enfoque iterativo para la generación del plan:

**Fase 1**: Fundamentos (completada)
**Fase 2**: Análisis de dependencias y estrategia (siguiente)
**Fase 3**: Detalles proyecto por proyecto con énfasis en proyecto WPF
**Fase 4**: Referencias de paquetes, breaking changes, riesgos y criterios de éxito

**Iteraciones Esperadas**: 6-8 iteraciones totales

---

## Estrategia de Migración

### Enfoque Seleccionado: Todo-a-la-Vez (All-At-Once)

#### Justificación Detallada

La estrategia **Todo-a-la-Vez** fue seleccionada basándose en las siguientes características de la solución:

**Condiciones Ideales Cumplidas**:
- ✅ Solución pequeña (7 proyectos, <30 proyectos)
- ✅ Todos los proyectos actualmente en .NET 8.0 o superior
- ✅ Código base homogéneo con patrones consistentes (arquitectura limpia)
- ✅ Complejidad de dependencias externas baja-media
- ✅ Todos los paquetes NuGet tienen versiones conocidas para el framework destino

**Ventajas Específicas para Esta Solución**:
1. **Tiempo de Finalización Más Rápido**: Un solo ciclo de actualización en lugar de múltiples fases
2. **Sin Complejidad de Multi-Targeting**: No es necesario mantener múltiples TargetFrameworks simultáneamente
3. **Beneficio Inmediato**: Todos los proyectos obtienen mejoras de .NET 10.0 al mismo tiempo
4. **Resolución Limpia de Dependencias**: Sin conflictos de versión entre proyectos
5. **Coordinación Simple**: Un solo punto de prueba y validación

**Desafíos Específicos**:
1. **Superficie de Prueba Más Grande**: Toda la solución debe probarse de una vez
2. **Mayor Riesgo Inicial**: El proyecto WPF con 1,018 incompatibilidades binarias
3. **Coordinación del Equipo**: Todos los desarrolladores deben adaptarse simultáneamente

#### Ordenamiento Basado en Dependencias

Aunque la actualización es atómica, el **orden de corrección de errores** sigue el flujo de dependencias:

**Secuencia de Corrección**:
1. **Domain** → Sin dependencias, corregir primero
2. **Application** → Depende solo de Domain
3. **Infrastructure + Auth.Core** → Dependen de Application/Domain
4. **Api + App** → Dependen de capas inferiores
5. **Api.Tests** → Depende de todo

**Justificación**: Resolver errores de compilación desde las capas base hacia arriba previene la confusión entre errores reales y errores en cascada.

#### Ejecución Paralela vs Secuencial

**Actualización de Archivos de Proyecto**: Paralela (todos los proyectos simultáneamente)
- Actualizar TargetFramework en todos los archivos .csproj
- Actualizar todas las referencias de paquetes en todos los proyectos
- Restaurar dependencias una sola vez para toda la solución

**Corrección de Errores de Compilación**: Secuencial (siguiendo dependencias)
- Resolver errores siguiendo el orden Domain → Application → Infrastructure → Apps → Tests
- Construir incrementalmente para verificar que cada capa se resuelve correctamente

**Ejecución de Pruebas**: Secuencial después de que toda la solución compile
- Ejecutar primero pruebas unitarias de capas base
- Luego pruebas de integración
- Finalmente pruebas end-to-end

#### Definición de Fases

Aunque es una operación atómica, el trabajo se organiza en fases lógicas:

**Fase 0: Preparación**
- ✅ Verificar instalación del SDK de .NET 10.0
- ✅ Validar que no existan cambios pendientes
- ✅ Crear rama de actualización (`upgrade-to-NET10`)

**Fase 1: Actualización Atómica**
- Actualizar todas las propiedades TargetFramework
- Actualizar todas las referencias de paquetes NuGet
- Restaurar dependencias
- Construir solución completa e identificar errores de compilación
- Corregir todos los errores de compilación encontrados (siguiendo orden de dependencias)
- Reconstruir y verificar 0 errores

**Entregable**: La solución completa compila sin errores

**Fase 2: Validación de Pruebas**
- Ejecutar todos los proyectos de pruebas
- Corregir fallos de pruebas
- Validar comportamiento de aplicaciones (especialmente proyecto WPF)

**Entregable**: Todas las pruebas pasan, aplicaciones funcionan correctamente

#### Consideraciones para Paquetes Obsoletos

Dos paquetes requieren atención especial por estar obsoletos:

1. **Microsoft.IdentityModel.Tokens** v7.6.0
2. **System.IdentityModel.Tokens.Jwt** v7.6.0

**Estrategia**: Estos paquetes están obsoletos pero Microsoft recomienda usar versiones más recientes de la misma familia. Se actualizarán a las versiones compatibles con .NET 10.0 como parte de la actualización atómica.

#### Consideraciones Específicas para Proyecto WPF

El proyecto **BMTECHRD.Pos.App** requiere atención especial:

**Cambios Necesarios**:
- Actualizar TargetFramework de `net8.0-windows` a `net10.0-windows`
- Las 1,018 "incompatibilidades binarias" reportadas son en su mayoría cambios de versión de ensamblados WPF, no errores de código
- La mayoría del código WPF es compatible a nivel de código fuente

**Validación Crítica**:
- Pruebas manuales de interfaz de usuario
- Verificar binding de datos XAML
- Validar comportamiento de controles personalizados
- Confirmar funcionamiento de diálogos y navegación

#### Control de Versiones

**Estrategia de Commit Único (Recomendada)**:
- Crear un solo commit con todos los cambios de la actualización atómica
- Mensaje de commit descriptivo: `chore: upgrade solution to .NET 10.0`
- Facilita rollback si es necesario
- Refleja la naturaleza atómica de la actualización

**Alternativa - Commits por Fase**:
- Commit 1: Actualización de archivos de proyecto y paquetes
- Commit 2: Correcciones de errores de compilación
- Commit 3: Correcciones de pruebas
- Proporciona más granularidad en el historial

---

## Análisis Detallado de Dependencias

### Resumen del Grafo de Dependencias

La solución BMTECHRD.Pos tiene una estructura de dependencias clara y bien organizada que sigue principios de arquitectura limpia:

```
Capa de Dominio (Sin dependencias)
    └── BMTECHRD.Pos.Domain

Capa de Aplicación (Depende de Dominio)
    └── BMTECHRD.Pos.Application → Domain

Capa de Infraestructura (Depende de Application y Domain)
    └── BMTECHRD.Pos.Infrastructure → Application, Domain
    └── BMTECHRD.Pos.Auth.Core → Application

Capa de Presentación (Depende de capas inferiores)
    ├── BMTECHRD.Pos.Api → Infrastructure, Application
    ├── BMTECHRD.Pos.App (WPF) → Application, Auth.Core
    └── BMTECHRD.Pos.Api.Tests → Api, Infrastructure, Application, Domain
```

### Agrupación de Proyectos por Fase de Migración

Dado que utilizamos la estrategia **Todo-a-la-Vez**, todos los proyectos se actualizarán simultáneamente. Sin embargo, el orden de validación seguirá el flujo de dependencias:

**Grupo 1: Proyectos Base (Sin dependencias de proyecto)**
- `BMTECHRD.Pos.Domain` - 0 dependencias de proyecto, 0 paquetes NuGet

**Grupo 2: Capa de Aplicación (Depende de Dominio)**
- `BMTECHRD.Pos.Application` - Depende de Domain, 0 paquetes NuGet

**Grupo 3: Capa de Infraestructura (Depende de Application/Domain)**
- `BMTECHRD.Pos.Infrastructure` - Depende de Application + Domain, 6 paquetes NuGet (4 actualizaciones + 2 obsoletos)
- `BMTECHRD.Pos.Auth.Core` - Depende de Application, 2 paquetes NuGet (2 actualizaciones)

**Grupo 4: Aplicaciones y API (Depende de todas las capas)**
- `BMTECHRD.Pos.Api` - Depende de Infrastructure + Application, 4 paquetes NuGet (2 actualizaciones)
- `BMTECHRD.Pos.App` - Depende de Application + Auth.Core, 7 paquetes NuGet (4 actualizaciones), **PROYECTO DE MAYOR RIESGO**

**Grupo 5: Pruebas (Depende de todo)**
- `BMTECHRD.Pos.Api.Tests` - Depende de Api + Infrastructure + Application + Domain, 7 paquetes NuGet (3 actualizaciones)

### Identificación del Camino Crítico

**Camino Crítico Principal**:
```
Domain → Application → Infrastructure → Api → Api.Tests
```

**Camino Crítico Secundario**:
```
Domain → Application → Auth.Core → App (WPF)
```

El camino crítico secundario contiene el **mayor riesgo** debido a las 1,018 incompatibilidades binarias del proyecto WPF.

### Detalles de Dependencias Circulares

✅ **No se detectaron dependencias circulares** en la solución. La arquitectura sigue un diseño limpio y unidireccional.

### Análisis de Impacto

| Proyecto | Dependencias Directas | Dependientes Directos | Nivel de Impacto |
|----------|----------------------|----------------------|------------------|
| BMTECHRD.Pos.Domain | 0 | 3 | 🔴 **Alto** - Base de toda la solución |
| BMTECHRD.Pos.Application | 1 | 5 | 🔴 **Alto** - Usada por casi todos |
| BMTECHRD.Pos.Infrastructure | 2 | 2 | 🟡 **Medio** - Usada por API y Tests |
| BMTECHRD.Pos.Auth.Core | 1 | 1 | 🟢 **Bajo** - Solo usada por App |
| BMTECHRD.Pos.Api | 2 | 1 | 🟡 **Medio** - Aplicación principal |
| BMTECHRD.Pos.App | 2 | 0 | 🟢 **Bajo** - No tiene dependientes |
| BMTECHRD.Pos.Api.Tests | 4 | 0 | 🟢 **Bajo** - Proyecto de pruebas |

### Principios de Ordenamiento para la Estrategia Todo-a-la-Vez

Aunque todos los proyectos se actualizan simultáneamente, el orden de **validación y corrección de errores** debe seguir el flujo de dependencias:

1. **Validar primero**: Proyectos sin dependencias (Domain)
2. **Validar segundo**: Proyectos que dependen solo de proyectos ya validados (Application)
3. **Validar tercero**: Capas de infraestructura (Infrastructure, Auth.Core)
4. **Validar cuarto**: Aplicaciones finales (Api, App)
5. **Validar último**: Proyectos de pruebas (Api.Tests)

**Justificación**: Este orden asegura que los errores de compilación se resuelvan de abajo hacia arriba, evitando confusión entre errores reales y errores en cascada por dependencias no resueltas.

---

## Planes Proyecto por Proyecto

### BMTECHRD.Pos.Domain

**Estado Actual**: 
- TargetFramework: `net8.0`
- Tipo: ClassLibrary (SDK-style)
- Dependencias de proyecto: 0
- Dependientes: 3 proyectos (Application, Infrastructure, Api.Tests)
- Paquetes NuGet: 0
- Líneas de código: 413
- Archivos con incidentes: 1
- LOC estimadas a modificar: 0+

**Estado Destino**: 
- TargetFramework: `net10.0`
- Paquetes a actualizar: Ninguno

#### Pasos de Migración

**1. Prerrequisitos**
- ✅ Ninguno (proyecto base sin dependencias)

**2. Actualización del Framework**

Archivo: `BMTECHRD.Pos.Domain\BMTECHRD.Pos.Domain.csproj`

Cambiar:
```xml
<TargetFramework>net8.0</TargetFramework>
```

A:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Actualizaciones de Paquetes/Módulos/Dependencias**

✅ **No hay paquetes NuGet** en este proyecto.

**4. Breaking Changes Esperados**

✅ **No se esperan breaking changes**. Este es un proyecto de dominio puro sin dependencias externas.

**5. Modificaciones de Código**

✅ **No se esperan modificaciones de código**. El proyecto contiene solo entidades de dominio y lógica de negocio que no dependen de características específicas del framework.

**6. Estrategia de Pruebas**

- ✅ Compilar el proyecto: `dotnet build BMTECHRD.Pos.Domain\BMTECHRD.Pos.Domain.csproj`
- ✅ Verificar que no hay errores de compilación
- ✅ Verificar que no hay advertencias

**7. Lista de Verificación**

- [ ] Archivo .csproj actualizado a net10.0
- [ ] Proyecto compila sin errores
- [ ] Proyecto compila sin advertencias
- [ ] Proyectos dependientes siguen compilando

**Nivel de Complejidad**: 🟢 **Baja** - Cambio trivial, solo actualización de TargetFramework.

---

### BMTECHRD.Pos.Application

**Estado Actual**: 
- TargetFramework: `net8.0`
- Tipo: ClassLibrary (SDK-style)
- Dependencias de proyecto: 1 (Domain)
- Dependientes: 5 proyectos (Api, App, Auth.Core, Infrastructure, Api.Tests)
- Paquetes NuGet: 0
- Líneas de código: 496
- Archivos con incidentes: 1
- LOC estimadas a modificar: 0+

**Estado Destino**: 
- TargetFramework: `net10.0`
- Paquetes a actualizar: Ninguno

#### Pasos de Migración

**1. Prerrequisitos**
- ✅ BMTECHRD.Pos.Domain debe estar actualizado a net10.0 primero

**2. Actualización del Framework**

Archivo: `BMTECHRD.Pos.Application\BMTECHRD.Pos.Application.csproj`

Cambiar:
```xml
<TargetFramework>net8.0</TargetFramework>
```

A:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Actualizaciones de Paquetes/Módulos/Dependencias**

✅ **No hay paquetes NuGet** en este proyecto.

**Dependencias de Proyecto** (se actualizan automáticamente):
- `BMTECHRD.Pos.Domain` (net10.0)

**4. Breaking Changes Esperados**

✅ **No se esperan breaking changes**. Este proyecto contiene interfaces, DTOs y lógica de aplicación que no dependen de características específicas del framework.

**5. Modificaciones de Código**

✅ **No se esperan modificaciones de código**. El proyecto implementa:
- Interfaces de repositorio
- Servicios de aplicación
- DTOs y modelos de vista
- Validaciones

Ninguno de estos componentes suele verse afectado por cambios de versión de framework.

**6. Estrategia de Pruebas**

- ✅ Compilar el proyecto: `dotnet build BMTECHRD.Pos.Application\BMTECHRD.Pos.Application.csproj`
- ✅ Verificar que no hay errores de compilación
- ✅ Verificar que no hay advertencias
- ✅ Verificar que la referencia a Domain funciona correctamente

**7. Lista de Verificación**

- [ ] Archivo .csproj actualizado a net10.0
- [ ] Proyecto compila sin errores
- [ ] Proyecto compila sin advertencias
- [ ] Referencia a Domain funciona correctamente
- [ ] Proyectos dependientes siguen compilando

**Nivel de Complejidad**: 🟢 **Baja** - Cambio simple, solo actualización de TargetFramework sin paquetes externos.

---

### BMTECHRD.Pos.Infrastructure

**Estado Actual**: 
- TargetFramework: `net8.0`
- Tipo: ClassLibrary (SDK-style)
- Dependencias de proyecto: 2 (Domain, Application)
- Dependientes: 2 proyectos (Api, Api.Tests)
- Paquetes NuGet: 9
- Líneas de código: 5,010
- Archivos con incidentes: 2
- LOC estimadas a modificar: 4+

**Estado Destino**: 
- TargetFramework: `net10.0`
- Paquetes a actualizar: 6 (incluye 2 paquetes obsoletos críticos)

#### Pasos de Migración

**1. Prerrequisitos**
- ✅ BMTECHRD.Pos.Domain debe estar actualizado a net10.0
- ✅ BMTECHRD.Pos.Application debe estar actualizado a net10.0

**2. Actualización del Framework**

Archivo: `BMTECHRD.Pos.Infrastructure\BMTECHRD.Pos.Infrastructure.csproj`

Cambiar:
```xml
<TargetFramework>net8.0</TargetFramework>
```

A:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Actualizaciones de Paquetes/Módulos/Dependencias**

| Paquete | Versión Actual | Versión Destino | Razón |
|---------|----------------|-----------------|-------|
| `Microsoft.EntityFrameworkCore` | 8.0.8 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.8 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.EntityFrameworkCore.Relational` | 8.0.8 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.Extensions.Configuration` | 8.0.0 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.IdentityModel.Tokens` | 7.6.0 | **8.2.1** | ⚠️ **Paquete obsoleto - actualizar a versión moderna** |
| `System.IdentityModel.Tokens.Jwt` | 7.6.0 | **8.2.1** | ⚠️ **Paquete obsoleto - actualizar a versión moderna** |

**Paquetes Compatibles (sin actualización)**:
- `BCrypt.Net-Next` 4.0.2 - ✅ Compatible con .NET 10.0
- `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.8 - ✅ Compatible con .NET 10.0

**4. Breaking Changes Esperados**

**a) Entity Framework Core 8.0 → 10.0**

Áreas a revisar:
- Cambios en métodos de consulta LINQ
- Cambios en configuración de DbContext
- Posibles cambios en migraciones
- Cambios en comportamiento de rastreo de entidades

Recursos:
- [Breaking changes in EF Core 9.0](https://learn.microsoft.com/ef/core/what-is-new/ef-core-9.0/breaking-changes)
- [Breaking changes in EF Core 10.0](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/breaking-changes)

**b) Paquetes de Identidad Obsoletos (CRÍTICO)**

`Microsoft.IdentityModel.Tokens` y `System.IdentityModel.Tokens.Jwt`:
- Versiones 7.x están obsoletas
- Actualizar a versión 8.x
- Posibles cambios en API de validación de tokens
- Posibles cambios en configuración de parámetros de validación

**Áreas de Código a Revisar**:
1. Código que genera tokens JWT
2. Código que valida tokens JWT
3. Configuración de TokenValidationParameters
4. Manejo de claims y identidades

**c) Incompatibilidades Binarias de IdentityModel (4 reportadas)**

El assessment reporta 4 incompatibilidades binarias relacionadas con paquetes de identidad. Revisar específicamente:
- Uso de tipos de `Microsoft.IdentityModel.*`
- Creación y validación de tokens
- Configuración de autenticación

**5. Modificaciones de Código**

**Archivos Potencialmente Afectados** (según assessment: 2 archivos con incidentes):

Buscar y revisar:
1. **Servicios de autenticación/token**:
   - Clases que implementan generación de JWT
   - Clases que validan tokens
   - Configuración de autenticación

2. **DbContext y configuraciones de EF**:
   - Clases que heredan de DbContext
   - Configuraciones de entidades
   - Cualquier uso de APIs específicas de EF

**Cambios Comunes en IdentityModel 8.x**:
```csharp
// Ejemplo: Posible cambio en validación de tokens
// ANTES (7.x):
var tokenHandler = new JwtSecurityTokenHandler();
var validationParameters = new TokenValidationParameters { ... };

// DESPUÉS (8.x): API puede haber cambiado
// Revisar documentación específica de cambios
```

**6. Estrategia de Pruebas**

**Pruebas Unitarias**:
- ✅ Compilar el proyecto
- ✅ Verificar que no hay errores o advertencias

**Pruebas de Integración** (Críticas):
- 🔐 **Pruebas de Autenticación JWT**:
  - Generar token JWT
  - Validar token JWT válido
  - Rechazar token JWT inválido
  - Validar claims en token
  - Probar expiración de tokens

- 💾 **Pruebas de Entity Framework**:
  - Conexión a base de datos
  - Consultas básicas (Select, Where, OrderBy)
  - Inserciones y actualizaciones
  - Verificar que migraciones funcionan
  - Validar comportamiento de rastreo

**Pruebas Manuales**:
- Login en aplicación
- Renovación de token
- Acceso a endpoints protegidos

**7. Lista de Verificación**

- [ ] Archivo .csproj actualizado a net10.0
- [ ] Todos los paquetes NuGet actualizados (especialmente IdentityModel)
- [ ] Proyecto compila sin errores
- [ ] Proyecto compila sin advertencias
- [ ] Generación de tokens JWT funciona
- [ ] Validación de tokens JWT funciona
- [ ] Consultas de Entity Framework funcionan
- [ ] Migraciones de base de datos compatibles
- [ ] Sin regresiones en autenticación

**Nivel de Complejidad**: 🟡 **Media-Alta** - Requiere atención especial a paquetes de identidad obsoletos.

**Criticidad**: 🔴 **Alta** - Este proyecto maneja autenticación y acceso a datos, componentes críticos de seguridad.

---

### BMTECHRD.Pos.Auth.Core

**Estado Actual**: 
- TargetFramework: `net8.0`
- Tipo: ClassLibrary (SDK-style)
- Dependencias de proyecto: 1 (Application)
- Dependientes: 1 proyecto (App)
- Paquetes NuGet: 2
- Líneas de código: 408
- Archivos con incidentes: 3
- LOC estimadas a modificar: 15+

**Estado Destino**: 
- TargetFramework: `net10.0`
- Paquetes a actualizar: 2

#### Pasos de Migración

**1. Prerrequisitos**
- ✅ BMTECHRD.Pos.Application debe estar actualizado a net10.0

**2. Actualización del Framework**

Archivo: `BMTECHRD.Pos.Auth.Core\BMTECHRD.Pos.Auth.Core.csproj`

Cambiar:
```xml
<TargetFramework>net8.0</TargetFramework>
```

A:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Actualizaciones de Paquetes/Módulos/Dependencias**

| Paquete | Versión Actual | Versión Destino | Razón |
|---------|----------------|-----------------|-------|
| `Microsoft.Extensions.Http` | 8.0.0 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.Extensions.Logging.Abstractions` | 8.0.0 | 10.0.3 | Compatibilidad con .NET 10.0 |

**4. Breaking Changes Esperados**

**Cambios de Comportamiento (15 reportados)**:

El assessment reporta 15 cambios de comportamiento en este proyecto. La mayoría probablemente relacionados con:
- Cambios en HttpClient y HttpClientFactory
- Cambios en logging

**Áreas a Revisar**:
1. **Uso de HttpClient**:
   - Creación de HttpClient
   - Configuración de headers
   - Manejo de respuestas
   - Timeout y retry policies

2. **Logging**:
   - Uso de ILogger
   - Niveles de log
   - Formatos de mensaje

**5. Modificaciones de Código**

**Archivos Potencialmente Afectados**: 3 archivos con incidentes

**Cambios Comunes**:
- Posibles ajustes en configuración de HttpClient
- Cambios menores en llamadas de logging
- Actualizaciones en manejo de excepciones de HTTP

**6. Estrategia de Pruebas**

**Pruebas de Compilación**:
- ✅ Compilar el proyecto
- ✅ Verificar que no hay errores o advertencias

**Pruebas Funcionales**:
- 🌐 Validar llamadas HTTP funcionan correctamente
- 📝 Verificar que logging funciona
- 🔄 Probar flujos de autenticación que usan este módulo

**7. Lista de Verificación**

- [ ] Archivo .csproj actualizado a net10.0
- [ ] Paquetes Microsoft.Extensions.* actualizados
- [ ] Proyecto compila sin errores
- [ ] Proyecto compila sin advertencias
- [ ] Llamadas HTTP funcionan correctamente
- [ ] Logging funciona correctamente
- [ ] Sin regresiones en autenticación

**Nivel de Complejidad**: 🟢 **Baja** - Cambios menores, principalmente actualizaciones de paquetes.

---

### BMTECHRD.Pos.Api.Tests

**Estado Actual**: 
- TargetFramework: `net8.0`
- Tipo: DotNetCoreApp (SDK-style, proyecto de pruebas)
- Dependencias de proyecto: 4 (Domain, Api, Infrastructure, Application)
- Dependientes: 0
- Paquetes NuGet: 7
- Líneas de código: 1,125
- Archivos con incidentes: 4
- LOC estimadas a modificar: 19+

**Estado Destino**: 
- TargetFramework: `net10.0`
- Paquetes a actualizar: 3

#### Pasos de Migración

**1. Prerrequisitos**
- ✅ TODOS los proyectos de los que depende deben estar actualizados primero:
  - BMTECHRD.Pos.Domain (net10.0)
  - BMTECHRD.Pos.Application (net10.0)
  - BMTECHRD.Pos.Infrastructure (net10.0)
  - BMTECHRD.Pos.Api (net10.0)

**2. Actualización del Framework**

Archivo: `BMTECHRD.Pos.Api.Tests\BMTECHRD.Pos.Api.Tests.csproj`

Cambiar:
```xml
<TargetFramework>net8.0</TargetFramework>
```

A:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Actualizaciones de Paquetes/Módulos/Dependencias**

| Paquete | Versión Actual | Versión Destino | Razón |
|---------|----------------|-----------------|-------|
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.8 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.EntityFrameworkCore.InMemory` | 8.0.8 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Newtonsoft.Json` | 13.0.3 | 13.0.4 | Actualización menor recomendada |

**Paquetes Compatibles (sin actualización)**:
- `Microsoft.NET.Test.Sdk` 18.3.0 - ✅ Compatible
- `Moq` 4.20.70 - ✅ Compatible
- `xunit` 2.5.3 - ✅ Compatible
- `xunit.runner.visualstudio` 2.5.0 - ✅ Compatible

**4. Breaking Changes Esperados**

**a) Incompatibilidad de Código Fuente (1 reportada)**

Probablemente relacionada con cambios en ASP.NET Core Testing

**b) Cambios de Comportamiento (18 reportados)**

Áreas comunes de cambio en pruebas:
- Comportamiento de WebApplicationFactory
- Configuración de TestServer
- Manejo de respuestas HTTP en pruebas
- Serialización/deserialización JSON

**c) Entity Framework InMemory**

Posibles cambios en:
- Configuración de base de datos en memoria
- Comportamiento de transacciones en pruebas
- Seed de datos de prueba

**5. Modificaciones de Código**

**Archivos Potencialmente Afectados**: 4 archivos con incidentes

**Áreas Comunes a Revisar**:

1. **Configuración de WebApplicationFactory**:
```csharp
// Posibles cambios en configuración de host de pruebas
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Revisar si API ha cambiado
    }
}
```

2. **Aserciones de Pruebas**:
- Verificar que asserts siguen funcionando
- Actualizar asserts si formatos de respuesta han cambiado

3. **Configuración de DbContext InMemory**:
```csharp
// Verificar configuración de base de datos en memoria
services.AddDbContext<PosDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
```

**6. Estrategia de Pruebas**

**Compilación**:
- ✅ Compilar proyecto de pruebas
- ✅ Verificar que no hay errores o advertencias

**Ejecución de Pruebas**:
- 🧪 Ejecutar todas las pruebas: `dotnet test BMTECHRD.Pos.Api.Tests`
- 🧪 Verificar que todas las pruebas pasan
- 🧪 Revisar pruebas fallidas y determinar si son:
  - Cambios legítimos de comportamiento → Actualizar prueba
  - Regresiones → Corregir código de producción

**Análisis de Fallos**:
- Categorizar fallos por tipo (asserts, configuración, datos)
- Priorizar corrección de pruebas críticas primero

**7. Lista de Verificación**

- [ ] Archivo .csproj actualizado a net10.0
- [ ] Todos los paquetes de pruebas actualizados
- [ ] Proyecto compila sin errores
- [ ] Proyecto compila sin advertencias
- [ ] Todas las pruebas se ejecutan (no fallan por configuración)
- [ ] Todas las pruebas pasan
- [ ] Cobertura de pruebas se mantiene
- [ ] Sin falsos positivos

**Nivel de Complejidad**: 🟡 **Media** - Requiere ejecutar y posiblemente ajustar pruebas.

---

### BMTECHRD.Pos.Api

**Estado Actual**: 
- TargetFramework: `net8.0`
- Tipo: AspNetCore (SDK-style)
- Dependencias de proyecto: 2 (Infrastructure, Application)
- Dependientes: 1 proyecto (Api.Tests)
- Paquetes NuGet: 4
- Líneas de código: 3,167
- Archivos con incidentes: 2
- LOC estimadas a modificar: 11+

**Estado Destino**: 
- TargetFramework: `net10.0`
- Paquetes a actualizar: 2

#### Pasos de Migración

**1. Prerrequisitos**
- ✅ BMTECHRD.Pos.Infrastructure debe estar actualizado a net10.0
- ✅ BMTECHRD.Pos.Application debe estar actualizado a net10.0

**2. Actualización del Framework**

Archivo: `BMTECHRD.Pos.Api\BMTECHRD.Pos.Api.csproj`

Cambiar:
```xml
<TargetFramework>net8.0</TargetFramework>
```

A:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Actualizaciones de Paquetes/Módulos/Dependencias**

| Paquete | Versión Actual | Versión Destino | Razón |
|---------|----------------|-----------------|-------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.8 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.8 | 10.0.3 | Herramientas de EF Core para migraciones |

**Paquetes Compatibles (sin actualización)**:
- `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.8 - ✅ Compatible
- `Swashbuckle.AspNetCore` 6.6.2 - ✅ Compatible

**4. Breaking Changes Esperados**

**a) Incompatibilidades de Código Fuente (11 reportadas)**

El assessment reporta 11 incompatibilidades de código fuente. Áreas comunes en ASP.NET Core:

1. **Program.cs y Startup.cs**:
   - Cambios en configuración de servicios
   - Cambios en middleware pipeline
   - Cambios en configuración de autenticación

2. **Autenticación JWT**:
   - Cambios en configuración de JwtBearer
   - Posibles cambios en validación de tokens
   - Cambios en manejo de claims

3. **Controladores**:
   - Cambios en atributos de ruta
   - Cambios en model binding
   - Cambios en validación de modelos

4. **Middleware**:
   - Orden de middleware puede ser importante
   - Algunos middleware pueden tener nueva configuración

**Breaking Changes Comunes en ASP.NET Core 9.0+ → 10.0**:
- Cambios en `WebApplication.CreateBuilder()`
- Cambios en configuración de autenticación
- Cambios en serialización JSON (System.Text.Json)
- Cambios en validación de modelos
- Cambios en manejo de CORS

**Recursos**:
- [Breaking changes in ASP.NET Core 9.0](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-9.0)
- [Breaking changes in ASP.NET Core 10.0](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0)

**5. Modificaciones de Código**

**Archivos Potencialmente Afectados**: 2 archivos con incidentes

**Áreas Prioritarias a Revisar**:

1. **Program.cs** (o Startup.cs):
```csharp
// Revisar configuración de servicios
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Verificar que esta configuración sigue siendo válida
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Parámetros de validación
        };
    });

// Revisar configuración de Swagger
builder.Services.AddSwaggerGen();

// Revisar configuración de DbContext
builder.Services.AddDbContext<PosDbContext>();
```

2. **Controladores**:
```csharp
// Verificar atributos y model binding
[ApiController]
[Route("api/[controller]")]
[Authorize] // Verificar que autenticación funciona
public class SomeController : ControllerBase
{
    // Revisar métodos de acción
}
```

3. **Middleware Pipeline**:
```csharp
// Verificar orden de middleware
app.UseHttpsRedirection();
app.UseAuthentication(); // Orden importante
app.UseAuthorization();  // Orden importante
app.MapControllers();
```

4. **Configuración de Swagger/OpenAPI**:
```csharp
// Verificar configuración de Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Cambios Comunes Necesarios**:
- Actualizar configuración de autenticación JWT si API cambió
- Ajustar orden de middleware si es necesario
- Actualizar configuración de Swagger si hubo cambios
- Revisar serialización JSON (opciones por defecto pueden haber cambiado)

**6. Estrategia de Pruebas**

**Pruebas de Compilación**:
- ✅ Compilar el proyecto
- ✅ Verificar que no hay errores o advertencias

**Pruebas Funcionales** (CRÍTICAS):

1. **Autenticación**:
   - 🔐 Login funciona (obtener token JWT)
   - 🔐 Endpoints protegidos requieren token
   - 🔐 Token válido permite acceso
   - 🔐 Token inválido deniega acceso
   - 🔐 Token expirado deniega acceso

2. **Endpoints de API**:
   - 🌐 GET endpoints funcionan
   - 🌐 POST endpoints funcionan
   - 🌐 PUT endpoints funcionan
   - 🌐 DELETE endpoints funcionan
   - 🌐 Validación de modelos funciona
   - 🌐 Manejo de errores funciona

3. **Swagger/OpenAPI**:
   - 📄 Swagger UI se carga correctamente
   - 📄 Documentación de API es correcta
   - 📄 Esquemas de modelos son correctos

4. **Base de Datos**:
   - 💾 Conexión a PostgreSQL funciona
   - 💾 Operaciones CRUD funcionan
   - 💾 Migraciones de EF Core funcionan

**Pruebas Automatizadas**:
- 🧪 Ejecutar Api.Tests (después de actualizarlo)
- 🧪 Verificar que todas las pruebas pasan

**7. Lista de Verificación**

- [ ] Archivo .csproj actualizado a net10.0
- [ ] Paquetes NuGet actualizados
- [ ] Proyecto compila sin errores
- [ ] Proyecto compila sin advertencias
- [ ] Aplicación inicia sin errores
- [ ] Autenticación JWT funciona
- [ ] Todos los endpoints de API funcionan
- [ ] Swagger UI funciona
- [ ] Conexión a base de datos funciona
- [ ] Pruebas automatizadas pasan
- [ ] Sin regresiones funcionales

**Nivel de Complejidad**: 🟡 **Media** - Requiere validación exhaustiva de autenticación y endpoints.

**Criticidad**: 🔴 **Alta** - API principal del sistema, debe funcionar correctamente.

---

### BMTECHRD.Pos.App

**Estado Actual**: 
- TargetFramework: `net8.0-windows`
- Tipo: Wpf (SDK-style)
- Dependencias de proyecto: 2 (Application, Auth.Core)
- Dependientes: 0
- Paquetes NuGet: 7
- Líneas de código: 4,038
- Archivos con incidentes: 51
- LOC estimadas a modificar: 1,082+ ⚠️ **PROYECTO DE MAYOR COMPLEJIDAD**

**Estado Destino**: 
- TargetFramework: `net10.0-windows`
- Paquetes a actualizar: 4

#### Pasos de Migración

**1. Prerrequisitos**
- ✅ BMTECHRD.Pos.Application debe estar actualizado a net10.0
- ✅ BMTECHRD.Pos.Auth.Core debe estar actualizado a net10.0

**2. Actualización del Framework**

Archivo: `BMTECHRD.Pos.App\BMTECHRD.Pos.App.csproj`

Cambiar:
```xml
<TargetFramework>net8.0-windows</TargetFramework>
```

A:
```xml
<TargetFramework>net10.0-windows</TargetFramework>
```

**Verificar que exista**:
```xml
<UseWPF>true</UseWPF>
<OutputType>WinExe</OutputType>
```

**3. Actualizaciones de Paquetes/Módulos/Dependencias**

| Paquete | Versión Actual | Versión Destino | Razón |
|---------|----------------|-----------------|-------|
| `Microsoft.AspNetCore.SignalR.Client` | 8.0.8 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.Extensions.Http` | 8.0.0 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.Extensions.Logging` | 8.0.0 | 10.0.3 | Compatibilidad con .NET 10.0 |
| `Microsoft.Extensions.Logging.Console` | 8.0.0 | 10.0.3 | Compatibilidad con .NET 10.0 |

**Paquetes Compatibles (sin actualización)**:
- `Serilog` 4.0.0 - ✅ Compatible
- `Serilog.Extensions.Logging` 3.0.0 - ✅ Compatible
- `Serilog.Sinks.Console` 4.0.0 - ✅ Compatible
- `Serilog.Sinks.File` 6.0.0 - ✅ Compatible

**4. Breaking Changes Esperados**

**⚠️ IMPORTANTE: Contexto sobre las 1,018 Incompatibilidades Binarias**

El assessment reporta **1,018 incompatibilidades binarias**, pero esto NO significa 1,018 errores de código:

**Realidad de las "Incompatibilidades Binarias" de WPF**:
- Son principalmente cambios de versión de ensamblado (assembly version)
- WPF en .NET 10.0 tiene nuevas versiones de ensamblados `PresentationCore.dll`, `PresentationFramework.dll`, etc.
- La mayoría del código fuente de WPF es **compatible a nivel de código fuente**
- Los tipos como `Button`, `TextBox`, `RoutedEventHandler` existen y funcionan igual

**Lo que realmente significa**:
- El proyecto compilará correctamente después de cambiar TargetFramework
- El riesgo real está en **comportamiento en runtime**, no en errores de compilación
- Necesitarás probar exhaustivamente la UI, no corregir miles de líneas de código

**Breaking Changes Reales a Considerar**:

**a) Cambios en WPF de .NET 8 → .NET 10**:

Áreas potencialmente afectadas:
1. **Renderizado y Gráficos**:
   - Cambios menores en rendering de controles
   - Posibles cambios en aceleración por hardware
   - Cambios en manejo de alta resolución (DPI)

2. **Data Binding**:
   - Generalmente estable, pero validar
   - Posibles cambios en binding de colecciones
   - Cambios en INotifyPropertyChanged

3. **Eventos y Comandos**:
   - Eventos routed generalmente estables
   - Validar comandos MVVM
   - Verificar comportamiento de eventos de input

4. **Recursos y Temas**:
   - Posibles cambios en temas por defecto
   - Validar estilos personalizados
   - Verificar diccionarios de recursos

**b) SignalR Client**:
- Posibles cambios en conexión a hubs
- Validar reconexión automática
- Verificar manejo de mensajes

**c) Cambios de Comportamiento (65 reportados)**:
- La mayoría relacionados con tipos de `System.Uri`, `HttpContent`, `JsonDocument`
- Validar llamadas HTTP a API
- Verificar serialización/deserialización JSON

**5. Modificaciones de Código**

**Archivos Potencialmente Afectados**: 51 archivos

**IMPORTANTE**: A pesar de que 51 archivos tienen "incidentes", la mayoría NO requerirán cambios de código. Los incidentes son referencias a tipos de WPF que han cambiado de versión de ensamblado, no errores de código.

**Áreas Reales a Revisar**:

1. **App.xaml y App.xaml.cs**:
```csharp
// Verificar inicialización de aplicación
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Configuración inicial
        // Configuración de logging (Serilog)
        // Configuración de DI si se usa
    }
}
```

2. **MainWindow.xaml y ViewModels**:
```csharp
// Verificar data binding
// Verificar comandos MVVM
// Verificar INotifyPropertyChanged
```

3. **Conexión SignalR**:
```csharp
// Revisar configuración de conexión
var connection = new HubConnectionBuilder()
    .WithUrl("https://api/hub")
    .WithAutomaticReconnect()
    .Build();

// Verificar que API no ha cambiado
```

4. **Llamadas HTTP a API**:
```csharp
// Verificar HttpClient
// Verificar serialización de requests/responses
// Verificar manejo de autenticación (tokens JWT)
```

5. **Controles Personalizados** (si existen):
```csharp
// Validar controles personalizados
// Verificar herencia de controles WPF
// Validar dependency properties
```

**Cambios Probables Necesarios**:
- **Pocos o ninguno** en archivos XAML (seguirán compilando)
- **Mínimos** en code-behind (API de WPF es estable)
- **Posibles** ajustes en conexión SignalR si API cambió
- **Posibles** ajustes en llamadas HTTP

**6. Estrategia de Pruebas**

**Pruebas de Compilación**:
- ✅ Compilar el proyecto
- ✅ Verificar que no hay errores o advertencias
- ✅ Verificar que recursos XAML se compilan correctamente

**Pruebas Funcionales de UI** (CRÍTICAS - REQUIEREN PRUEBA MANUAL EXHAUSTIVA):

**⚠️ Nivel de Esfuerzo Alto**: Estas pruebas manuales representan ~40% del esfuerzo total de la actualización

**1. Inicio y Navegación**:
- 🖥️ Aplicación inicia sin errores
- 🖥️ MainWindow se muestra correctamente
- 🖥️ Navegación entre vistas funciona
- 🖥️ Todos los menús son accesibles

**2. Controles de UI**:
- 🖱️ Todos los botones responden a clicks
- ⌨️ Todos los TextBox aceptan entrada
- 📋 Todos los ComboBox muestran opciones
- 📊 Todos los DataGrid muestran datos
- 🖼️ Todos los controles personalizados funcionan

**3. Data Binding**:
- 🔗 Binding de texto funciona
- 🔗 Binding de colecciones funciona
- 🔗 Comandos MVVM responden
- 🔗 INotifyPropertyChanged actualiza UI
- 🔗 Validación de datos funciona

**4. Diálogos y Ventanas**:
- 💬 Diálogos modales funcionan
- 💬 MessageBox se muestra correctamente
- 💬 Ventanas secundarias se abren/cierran
- 💬 Focus de controles funciona

**5. Autenticación**:
- 🔐 Login funciona
- 🔐 Token JWT se obtiene correctamente
- 🔐 Llamadas autenticadas a API funcionan
- 🔐 Logout funciona

**6. Comunicación con API**:
- 🌐 Conexión a API funciona
- 🌐 GET/POST/PUT/DELETE funcionan
- 🌐 Manejo de errores funciona
- 🌐 Timeout se maneja correctamente

**7. SignalR (si se usa)**:
- 📡 Conexión a hub funciona
- 📡 Recepción de mensajes funciona
- 📡 Envío de mensajes funciona
- 📡 Reconexión automática funciona

**8. Logging**:
- 📝 Logs se escriben correctamente (Serilog)
- 📝 Niveles de log funcionan
- 📝 Logs en archivo funcionan
- 📝 Logs en consola funcionan (si aplica)

**9. Renderizado y Visualización**:
- 🎨 Colores se muestran correctamente
- 🎨 Estilos personalizados funcionan
- 🎨 Imágenes se cargan correctamente
- 🎨 Animaciones funcionan (si las hay)
- 🎨 Escalado de alta resolución (DPI) funciona

**10. Funcionalidad de Negocio**:
- 💼 Todas las operaciones de negocio críticas funcionan
- 💼 Flujos de trabajo completos funcionan
- 💼 Validaciones de negocio funcionan
- 💼 Cálculos son correctos

**Metodología de Prueba Recomendada**:
1. **Smoke Test** (5-10 minutos): Iniciar app, hacer operación básica, cerrar
2. **Prueba Completa** (1-2 horas): Recorrer todos los flujos importantes
3. **Prueba Detallada** (según sea necesario): Probar cada control y vista

**7. Lista de Verificación**

- [ ] Archivo .csproj actualizado a net10.0-windows
- [ ] <UseWPF>true</UseWPF> presente en .csproj
- [ ] Todos los paquetes NuGet actualizados
- [ ] Proyecto compila sin errores
- [ ] Proyecto compila sin advertencias
- [ ] Recursos XAML se compilan correctamente
- [ ] **Aplicación inicia correctamente**
- [ ] **Todas las vistas se muestran correctamente**
- [ ] **Todos los controles interactivos funcionan**
- [ ] **Data binding funciona correctamente**
- [ ] **Autenticación funciona**
- [ ] **Comunicación con API funciona**
- [ ] **SignalR funciona (si aplica)**
- [ ] **Logging funciona**
- [ ] **Todas las funcionalidades de negocio funcionan**
- [ ] Sin regresiones visuales
- [ ] Sin regresiones funcionales

**Nivel de Complejidad**: 🔴 **Alta** - Requiere pruebas manuales exhaustivas de toda la UI.

**Criticidad**: 🔴 **Alta** - Aplicación principal de usuario, debe funcionar perfectamente.

**Estimación de Esfuerzo**: ~40% del esfuerzo total de la actualización se invertirá en probar este proyecto.

**Recomendación**: Asignar a desarrollador con fuerte experiencia en WPF para liderar la validación de este proyecto.

---

### BMTECHRD.Pos.Api.Tests

**Estado Actual**: 
- TargetFramework: `net8.0`
- Tipo: DotNetCoreApp (SDK-style, proyecto de pruebas)
- Dependencias de proyecto: 4 (Domain, Api, Infrastructure, Application)
- Dependientes: 0
- Paquetes NuGet: 7
- Líneas de código: 1,125
- Archivos con incidentes: 4
- LOC estimadas a modificar: 19+

**Estado Destino**: 
- TargetFramework: `net10.0`
- Paquetes a actualizar: 3

[Detalles por completar]

---

## Referencia de Actualización de Paquetes

### Resumen de Actualizaciones por Alcance

**Paquetes Comunes** (afectan múltiples proyectos):

| Paquete | Versión Actual | Versión Destino | Proyectos Afectados | Razón de Actualización |
|---------|----------------|-----------------|---------------------|------------------------|
| `Microsoft.EntityFrameworkCore.Design` | 8.0.8 | 10.0.3 | 2 proyectos (Api, Infrastructure) | Compatibilidad con .NET 10.0 |
| `Microsoft.Extensions.Http` | 8.0.0 | 10.0.3 | 2 proyectos (App, Auth.Core) | Compatibilidad con .NET 10.0 |

**Paquetes por Categoría**:

#### Entity Framework Core (3 proyectos)

| Paquete | Actual | Destino | Proyectos |
|---------|--------|---------|-----------|
| `Microsoft.EntityFrameworkCore` | 8.0.8 | 10.0.3 | Infrastructure |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.8 | 10.0.3 | Api, Infrastructure |
| `Microsoft.EntityFrameworkCore.Relational` | 8.0.8 | 10.0.3 | Infrastructure |
| `Microsoft.EntityFrameworkCore.InMemory` | 8.0.8 | 10.0.3 | Api.Tests |

**Razón**: Compatibilidad con .NET 10.0 y mejoras de rendimiento de EF Core 10.0

#### ASP.NET Core (2 proyectos)

| Paquete | Actual | Destino | Proyectos |
|---------|--------|---------|-----------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.8 | 10.0.3 | Api |
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.8 | 10.0.3 | Api.Tests |
| `Microsoft.AspNetCore.SignalR.Client` | 8.0.8 | 10.0.3 | App |

**Razón**: Compatibilidad con .NET 10.0 y nuevas características de ASP.NET Core 10.0

#### Microsoft.Extensions (4 proyectos)

| Paquete | Actual | Destino | Proyectos |
|---------|--------|---------|-----------|
| `Microsoft.Extensions.Configuration` | 8.0.0 | 10.0.3 | Infrastructure |
| `Microsoft.Extensions.Http` | 8.0.0 | 10.0.3 | App, Auth.Core |
| `Microsoft.Extensions.Logging` | 8.0.0 | 10.0.3 | App |
| `Microsoft.Extensions.Logging.Abstractions` | 8.0.0 | 10.0.3 | Auth.Core |
| `Microsoft.Extensions.Logging.Console` | 8.0.0 | 10.0.3 | App |

**Razón**: Compatibilidad con .NET 10.0

#### ⚠️ Paquetes de Identidad Obsoletos (1 proyecto - CRÍTICO)

| Paquete | Actual | Destino | Proyectos | Razón |
|---------|--------|---------|-----------|-------|
| `Microsoft.IdentityModel.Tokens` | 7.6.0 | 8.2.1 | Infrastructure | ⚠️ **Paquete obsoleto** - actualizar a versión moderna |
| `System.IdentityModel.Tokens.Jwt` | 7.6.0 | 8.2.1 | Infrastructure | ⚠️ **Paquete obsoleto** - actualizar a versión moderna |

**Razón**: Versiones 7.x están obsoletas. La versión 8.x es compatible con .NET 10.0 y contiene mejoras de seguridad.

**IMPORTANTE**: Estos paquetes son críticos para autenticación JWT. Requieren validación exhaustiva después de la actualización.

#### Otros Paquetes

| Paquete | Actual | Destino | Proyectos | Razón |
|---------|--------|---------|-----------|-------|
| `Newtonsoft.Json` | 13.0.3 | 13.0.4 | Api.Tests | Actualización menor recomendada |

### Paquetes Sin Actualización (Compatibles con .NET 10.0)

| Paquete | Versión | Proyectos | Estado |
|---------|---------|-----------|--------|
| `BCrypt.Net-Next` | 4.0.2 | Infrastructure | ✅ Compatible |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.8 | Api, Infrastructure | ✅ Compatible |
| `Serilog` | 4.0.0 | App | ✅ Compatible |
| `Serilog.Extensions.Logging` | 3.0.0 | App | ✅ Compatible |
| `Serilog.Sinks.Console` | 4.0.0 | App | ✅ Compatible |
| `Serilog.Sinks.File` | 6.0.0 | App | ✅ Compatible |
| `Swashbuckle.AspNetCore` | 6.6.2 | Api | ✅ Compatible |
| `Microsoft.NET.Test.Sdk` | 18.3.0 | Api.Tests | ✅ Compatible |
| `Moq` | 4.20.70 | Api.Tests | ✅ Compatible |
| `xunit` | 2.5.3 | Api.Tests | ✅ Compatible |
| `xunit.runner.visualstudio` | 2.5.0 | Api.Tests | ✅ Compatible |

### Matriz Completa: Proyecto × Paquete

| Proyecto | Paquetes a Actualizar | Total Paquetes |
|----------|----------------------|----------------|
| **Domain** | 0 | 0 |
| **Application** | 0 | 0 |
| **Infrastructure** | 6 (incluye 2 obsoletos) | 9 |
| **Auth.Core** | 2 | 2 |
| **Api** | 2 | 4 |
| **App** | 4 | 7 |
| **Api.Tests** | 3 | 7 |
| **TOTAL** | **15 actualizaciones** | **26 paquetes** |

### Prioridad de Actualización

**Prioridad Alta** (críticos, actualizar primero):
1. `Microsoft.IdentityModel.Tokens` (obsoleto, seguridad)
2. `System.IdentityModel.Tokens.Jwt` (obsoleto, seguridad)
3. `Microsoft.AspNetCore.Authentication.JwtBearer` (autenticación)

**Prioridad Media** (importantes, actualizar en bloque):
4. Todos los paquetes `Microsoft.EntityFrameworkCore.*`
5. Todos los paquetes `Microsoft.Extensions.*`
6. `Microsoft.AspNetCore.SignalR.Client`

**Prioridad Baja** (pueden actualizarse al final):
7. `Newtonsoft.Json`

**Nota**: En la estrategia Todo-a-la-Vez, todos se actualizan simultáneamente, pero esta priorización es útil para entender dependencias y para troubleshooting si hay problemas.

---

## Catálogo de Cambios Importantes (Breaking Changes)

### Resumen por Categoría

| Categoría | Cantidad | Proyectos Afectados | Nivel de Impacto |
|-----------|----------|---------------------|------------------|
| 🔴 Incompatibilidades Binarias | 1,018 | App (WPF) | **Alto** - Principalmente WPF, mayoría son cambios de versión de ensamblado |
| 🟡 Incompatibilidades de Código Fuente | 15 | Api, App, Api.Tests | **Medio** - Requieren corrección de código |
| 🔵 Cambios de Comportamiento | 98 | Todos excepto Domain y Application | **Bajo** - Requieren validación en runtime |

### 1. Breaking Changes del Framework

#### .NET 8.0 → .NET 10.0

**Cambios Generales**:
- Cambios en comportamiento de `System.Uri`
- Cambios en `System.Text.Json` (serialización/deserialización)
- Cambios en manejo de `HttpContent` y `HttpClient`
- Mejoras de rendimiento que pueden afectar timing

**Referencias**:
- [What's new in .NET 9](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9/overview)
- [Breaking changes in .NET 9](https://learn.microsoft.com/dotnet/core/compatibility/9.0)
- [What's new in .NET 10](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview) (cuando esté disponible)

### 2. Breaking Changes por Tecnología

#### ASP.NET Core 8.0 → 10.0

**Proyectos Afectados**: Api, Api.Tests

**Cambios Potenciales**:

1. **Autenticación y Autorización**:
   - Cambios en configuración de `JwtBearerOptions`
   - Posibles cambios en middleware de autenticación
   - Cambios en manejo de claims

2. **Model Binding y Validación**:
   - Cambios en validación automática de modelos
   - Cambios en deserialización de JSON
   - Cambios en binding de parámetros complejos

3. **Middleware Pipeline**:
   - Posibles cambios en orden recomendado de middleware
   - Cambios en configuración de CORS
   - Cambios en manejo de errores

4. **Minimal APIs** (si se usan):
   - Cambios en sintaxis de endpoints
   - Cambios en binding de parámetros

**Acciones Recomendadas**:
- Revisar documentación oficial de breaking changes de ASP.NET Core 9.0 y 10.0
- Probar todos los endpoints de API
- Validar autenticación exhaustivamente
- Verificar manejo de errores

**Referencias**:
- [Breaking changes in ASP.NET Core 9.0](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-9.0)

#### Entity Framework Core 8.0 → 10.0

**Proyectos Afectados**: Infrastructure, Api, Api.Tests

**Cambios Potenciales**:

1. **Consultas LINQ**:
   - Cambios en traducción de LINQ a SQL
   - Mejoras de rendimiento en consultas complejas
   - Cambios en comportamiento de `Include()` y `ThenInclude()`

2. **Migraciones**:
   - Cambios en generación de scripts de migración
   - Posibles cambios en convenciones de nombres
   - Cambios en tipos de datos mapeados

3. **Configuración**:
   - Cambios en API de configuración de entidades
   - Cambios en convenciones por defecto
   - Cambios en comportamiento de cascada

4. **Rastreo de Cambios**:
   - Mejoras de rendimiento en change tracking
   - Posibles cambios en comportamiento de `AsNoTracking()`

**Acciones Recomendadas**:
- Ejecutar todas las pruebas de repositorio
- Validar que consultas complejas funcionan correctamente
- Verificar que migraciones se generan correctamente
- Revisar cambios en comportamiento de lazy loading (si se usa)

**Referencias**:
- [Breaking changes in EF Core 9.0](https://learn.microsoft.com/ef/core/what-is-new/ef-core-9.0/breaking-changes)

#### WPF en .NET 8.0 → .NET 10.0

**Proyectos Afectados**: App

**Contexto Importante**:
Las 1,018 "incompatibilidades binarias" reportadas en el proyecto WPF son **mayormente cambios de versión de ensamblado**, no errores de código real. La API de WPF en .NET es muy estable.

**Cambios Potenciales (Reales)**:

1. **Renderizado**:
   - Posibles mejoras en rendering de alta resolución (DPI)
   - Cambios menores en aceleración por hardware
   - Mejoras de rendimiento en animaciones

2. **Data Binding**:
   - API de data binding generalmente estable
   - Posibles optimizaciones de rendimiento
   - Validar binding de colecciones complejas

3. **Controles**:
   - Cambios menores en estilos por defecto
   - Posibles mejoras de accesibilidad
   - Validar controles personalizados (si existen)

**Cambios Poco Probables**:
- Cambios en API de eventos routed
- Cambios en comandos MVVM
- Cambios en dependency properties

**Acciones Recomendadas**:
- **NO pánico** por el número de incompatibilidades reportadas
- Compilar el proyecto (debería compilar sin errores)
- Realizar pruebas manuales exhaustivas de UI
- Validar rendering en diferentes resoluciones
- Probar data binding y comandos

**Referencias**:
- [What's new in WPF for .NET 9](https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net90)

#### SignalR 8.0 → 10.0

**Proyectos Afectados**: App

**Cambios Potenciales**:
- Cambios en configuración de conexión
- Mejoras en reconexión automática
- Cambios en manejo de mensajes grandes
- Posibles cambios en serialización de mensajes

**Acciones Recomendadas**:
- Probar conexión a hubs
- Validar envío y recepción de mensajes
- Verificar comportamiento de reconexión

### 3. Breaking Changes de Paquetes Obsoletos

#### Microsoft.IdentityModel.Tokens 7.6.0 → 8.2.1

**Proyecto Afectado**: Infrastructure

**Cambios Significativos en Versión 8.x**:

1. **Validación de Tokens**:
   - Posibles cambios en `TokenValidationParameters`
   - Cambios en manejo de algoritmos de firma
   - Mejoras de seguridad en validación

2. **Configuración**:
   - API puede haber cambiado para configuración de validación
   - Posibles nuevos parámetros requeridos
   - Cambios en valores por defecto

**Áreas de Código Críticas a Revisar**:
```csharp
// Búsqueda de código que usa:
- TokenValidationParameters
- JwtSecurityTokenHandler
- SecurityKey y SigningCredentials
- Validación de issuer y audience
```

**Acciones Recomendadas**:
- Revisar documentación de migración de Microsoft.IdentityModel
- Probar exhaustivamente generación y validación de tokens
- Validar todos los flujos de autenticación
- Verificar configuración de seguridad

**Referencias**:
- [Microsoft.IdentityModel release notes](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/releases)

### 4. Cambios de Comportamiento por API

#### APIs con Mayor Número de Cambios de Comportamiento

| API | Ocurrencias | Impacto | Proyecto |
|-----|-------------|---------|----------|
| `System.Uri` | 34 | Medio | Varios |
| `System.Net.Http.HttpContent` | 23 | Medio | App, Auth.Core, Api |
| `System.Text.Json.JsonDocument` | 12 | Bajo | Varios |

**System.Uri**:
- Cambios en parsing de URLs
- Cambios en manejo de caracteres especiales
- Cambios en normalización de rutas

**Acciones**: Validar todas las URLs construidas dinámicamente

**HttpContent**:
- Cambios en lectura de contenido
- Cambios en manejo de encoding
- Cambios en streaming

**Acciones**: Probar todas las llamadas HTTP, especialmente con cuerpos grandes

**JsonDocument**:
- Cambios en parsing de JSON
- Mejoras de rendimiento
- Cambios en manejo de valores nulos

**Acciones**: Validar serialización/deserialización de DTOs

### 5. Guía de Corrección por Tipo de Breaking Change

#### Para Incompatibilidades de Código Fuente (15 casos)

1. **Identificar** el error de compilación exacto
2. **Buscar** en documentación oficial el breaking change correspondiente
3. **Aplicar** la corrección recomendada
4. **Probar** que la funcionalidad se mantiene

#### Para Cambios de Comportamiento (98 casos)

1. **No requerirán cambios de código** en la mayoría de los casos
2. **Requerirán validación en runtime**:
   - Ejecutar pruebas automatizadas
   - Realizar pruebas manuales de funcionalidades afectadas
   - Verificar logs para excepciones o warnings
3. **Si se detecta regresión**:
   - Investigar breaking change específico
   - Ajustar código si es necesario
   - Considerar workarounds temporales

#### Para Incompatibilidades Binarias WPF (1,018 casos)

1. **NO requieren corrección de código** en la mayoría de los casos
2. **Requieren validación exhaustiva de UI**:
   - Pruebas manuales de todas las vistas
   - Validación de controles interactivos
   - Verificación de data binding
   - Comprobación de estilos y temas
3. **Si se detectan problemas visuales o funcionales**:
   - Investigar cambio específico de WPF
   - Ajustar estilos o código si es necesario

### 6. Recursos de Referencia

**Documentación Oficial**:
- [.NET 9 Breaking Changes](https://learn.microsoft.com/dotnet/core/compatibility/9.0)
- [ASP.NET Core 9 Breaking Changes](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-9.0)
- [EF Core 9 Breaking Changes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-9.0/breaking-changes)
- [WPF for .NET 9](https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net90)

**Herramientas de Análisis**:
- Upgrade Assistant (ya se ejecutó en assessment)
- Roslyn Analyzers para detectar código obsoleto
- .NET API Port Analyzer

**Comunidad**:
- Stack Overflow (buscar por error específico + ".NET 10")
- GitHub Issues de repositorios oficiales de Microsoft
- .NET Blog para anuncios de breaking changes

---

## Gestión de Riesgos

### Evaluación de Riesgos Alto Nivel

| Proyecto | Nivel de Riesgo | Descripción | Mitigación |
|----------|-----------------|-------------|------------|
| BMTECHRD.Pos.App | 🔴 **Alto** | 1,018 incompatibilidades binarias de WPF, 51 archivos afectados, 1,082+ LOC a revisar | Pruebas exhaustivas de UI, validación manual de controles, revisión de data binding |
| BMTECHRD.Pos.Infrastructure | 🟡 **Medio** | 2 paquetes obsoletos de identidad críticos, 4 incompatibilidades binarias | Actualizar a versiones modernas de Microsoft.IdentityModel, probar autenticación |
| BMTECHRD.Pos.Api | 🟡 **Medio** | 11 incompatibilidades de código fuente en API | Revisar cambios de ASP.NET Core 10.0, probar endpoints |
| BMTECHRD.Pos.Auth.Core | 🟢 **Bajo** | 15 cambios de comportamiento | Validar flujos de autenticación |
| BMTECHRD.Pos.Api.Tests | 🟢 **Bajo** | 19 cambios de comportamiento en pruebas | Ejecutar y corregir pruebas |
| BMTECHRD.Pos.Application | 🟢 **Bajo** | Sin problemas detectados | Actualizar TargetFramework solamente |
| BMTECHRD.Pos.Domain | 🟢 **Bajo** | Sin problemas detectados | Actualizar TargetFramework solamente |

### Cambios de Alto Riesgo Detallados

#### 1. Proyecto WPF - Incompatibilidades Binarias (RIESGO ALTO)

**Descripción**:
- 1,018 incompatibilidades binarias reportadas
- Afectan tipos fundamentales de WPF: `RoutedEventHandler`, `TextBox`, `Button`, `Application`, etc.
- 51 archivos XAML y code-behind afectados

**Riesgo Real**:
- Las "incompatibilidades binarias" en WPF son mayormente cambios de versión de ensamblado
- La mayoría del código fuente es compatible
- Mayor riesgo: comportamiento en tiempo de ejecución, no errores de compilación

**Estrategia de Mitigación**:
1. Actualizar TargetFramework a `net10.0-windows`
2. Compilar y resolver cualquier error de compilación real
3. **Pruebas manuales exhaustivas**:
   - Navegar por todas las ventanas y vistas
   - Probar todos los controles interactivos
   - Validar data binding y comandos
   - Verificar diálogos y mensajes
4. Monitorear logs de runtime para excepciones
5. Tener plan de rollback listo

#### 2. Paquetes de Identidad Obsoletos (RIESGO MEDIO-ALTO)

**Paquetes Afectados**:
- `Microsoft.IdentityModel.Tokens` v7.6.0 (usado en Infrastructure)
- `System.IdentityModel.Tokens.Jwt` v7.6.0 (usado en Infrastructure)

**Riesgo**:
- Estos paquetes son críticos para autenticación JWT
- Versiones obsoletas pueden tener problemas de seguridad
- Cambios en API pueden romper flujos de autenticación

**Estrategia de Mitigación**:
1. Actualizar a versiones modernas compatibles con .NET 10.0
2. Revisar código que usa estos paquetes:
   - Generación de tokens JWT
   - Validación de tokens
   - Configuración de autenticación
3. Probar todos los flujos de autenticación:
   - Login
   - Renovación de tokens
   - Validación de permisos
4. Validar que no haya cambios de comportamiento de seguridad

#### 3. Incompatibilidades de Código Fuente en API (RIESGO MEDIO)

**Proyectos Afectados**:
- BMTECHRD.Pos.Api (11 incompatibilidades)

**Áreas de Riesgo**:
- Cambios en middleware de ASP.NET Core
- Cambios en configuración de servicios
- Posibles cambios en serialización JSON
- Cambios en manejo de respuestas HTTP

**Estrategia de Mitigación**:
1. Revisar breaking changes de ASP.NET Core 10.0
2. Compilar y resolver errores específicos
3. Ejecutar todas las pruebas de API
4. Validar contratos de API (request/response)
5. Probar manualmente endpoints críticos

### Vulnerabilidades de Seguridad

✅ **No se identificaron vulnerabilidades de seguridad** en el assessment.

Sin embargo, se recomienda:
- Actualizar los paquetes obsoletos de identidad por seguridad
- Revisar las notas de seguridad de .NET 10.0
- Validar configuraciones de seguridad después de la actualización

### Planes de Contingencia

#### Contingencia 1: Errores de Compilación Inesperados en WPF

**Situación**: El proyecto WPF no compila después de cambiar TargetFramework

**Alternativas**:
1. Revisar referencias a ensamblados específicos de Windows
2. Verificar que `<UseWPF>true</UseWPF>` esté presente
3. Agregar referencias explícitas a ensamblados de WPF si es necesario
4. Consultar breaking changes específicos de WPF en .NET 10.0

#### Contingencia 2: Problemas de Autenticación JWT

**Situación**: Autenticación falla después de actualizar paquetes de identidad

**Alternativas**:
1. Revisar documentación de migración de Microsoft.IdentityModel
2. Verificar configuración de validación de tokens
3. Confirmar que algoritmos de firma no hayan cambiado
4. Validar configuración de audiencia y emisor

#### Contingencia 3: Cambios de Comportamiento Inesperados

**Situación**: Aplicaciones compilan pero comportamiento es diferente

**Alternativas**:
1. Revisar logs de runtime para excepciones o advertencias
2. Comparar comportamiento con documentación de breaking changes
3. Activar logging detallado para diagnóstico
4. Probar en ambiente de desarrollo aislado primero

#### Contingencia 4: Problemas de Rendimiento

**Situación**: Degradación de rendimiento después de actualización

**Alternativas**:
1. Usar herramientas de profiling para identificar cuellos de botella
2. Revisar cambios de rendimiento documentados en .NET 10.0
3. Ajustar configuraciones de runtime si es necesario
4. Considerar cambios específicos de optimización

### Plan de Rollback

**Si se requiere revertir la actualización**:

1. **Revertir Cambios en Git**:
   ```bash
   git reset --hard <commit-antes-de-upgrade>
   # o
   git revert <commit-de-upgrade>
   ```

2. **Restaurar Paquetes**:
   ```bash
   dotnet restore
   ```

3. **Verificar**:
   ```bash
   dotnet build
   dotnet test
   ```

4. **Comunicar al Equipo**: Notificar decisión de rollback y razones

---

## Estrategia de Pruebas y Validación

### Enfoque Multi-Nivel

La estrategia de pruebas sigue un enfoque de validación en múltiples niveles para asegurar que la actualización es exitosa y no introduce regresiones.

### Nivel 1: Pruebas por Proyecto

Después de actualizar cada proyecto, validar individualmente:

#### Proyectos de Biblioteca de Clase (Domain, Application, Infrastructure, Auth.Core)

**Validación Rápida**:
- ✅ Compilación exitosa sin errores
- ✅ Compilación exitosa sin advertencias
- ✅ Sin conflictos de dependencias

**Comando**:
```bash
dotnet build [proyecto].csproj --no-incremental
```

#### Proyecto de API (Api)

**Validación de Compilación**:
- ✅ Compilación exitosa sin errores
- ✅ Sin advertencias

**Validación de Inicio**:
- 🚀 Aplicación inicia sin excepciones
- 🚀 Swagger UI se carga correctamente
- 🚀 Base de datos es accesible

**Comandos**:
```bash
dotnet build BMTECHRD.Pos.Api\BMTECHRD.Pos.Api.csproj
dotnet run --project BMTECHRD.Pos.Api\BMTECHRD.Pos.Api.csproj
```

**Smoke Test Manual**:
1. Abrir Swagger UI
2. Ejecutar endpoint de login
3. Obtener token JWT
4. Ejecutar un endpoint protegido con el token
5. Verificar respuesta exitosa

#### Proyecto WPF (App)

**Validación de Compilación**:
- ✅ Compilación exitosa sin errores
- ✅ Sin advertencias
- ✅ Recursos XAML compilados correctamente

**Validación de Inicio**:
- 🖥️ Aplicación inicia sin excepciones
- 🖥️ MainWindow se muestra
- 🖥️ Sin errores en logs

**Comandos**:
```bash
dotnet build BMTECHRD.Pos.App\BMTECHRD.Pos.App.csproj
dotnet run --project BMTECHRD.Pos.App\BMTECHRD.Pos.App.csproj
```

**Smoke Test Manual** (5-10 minutos):
1. Iniciar aplicación
2. Realizar login
3. Navegar a una vista principal
4. Realizar una operación básica
5. Cerrar aplicación

#### Proyecto de Pruebas (Api.Tests)

**Validación de Compilación**:
- ✅ Compilación exitosa sin errores
- ✅ Sin advertencias

**Ejecución de Pruebas**:
- 🧪 Todas las pruebas se ejecutan
- 🧪 Todas las pruebas pasan

**Comandos**:
```bash
dotnet build BMTECHRD.Pos.Api.Tests\BMTECHRD.Pos.Api.Tests.csproj
dotnet test BMTECHRD.Pos.Api.Tests\BMTECHRD.Pos.Api.Tests.csproj --verbosity normal
```

### Nivel 2: Pruebas por Fase

Después de completar cada fase lógica de la actualización:

#### Fase 1: Después de Actualización Atómica

**Objetivo**: Verificar que toda la solución compila y funciona básicamente

**Validaciones**:

1. **Compilación Completa**:
```bash
dotnet build BMTECHRD.Pos.slnx --no-incremental
```
- ✅ 0 errores de compilación
- ✅ 0 advertencias (o solo advertencias conocidas/aceptables)

2. **Pruebas Automatizadas**:
```bash
dotnet test BMTECHRD.Pos.slnx --verbosity normal
```
- ✅ Todas las pruebas se ejecutan
- ✅ % de pruebas pasando = 100% (o identificar fallos)

3. **Inicio de Aplicaciones**:
- API inicia correctamente
- App WPF inicia correctamente
- Sin excepciones en logs de inicio

**Criterio de Fase Completa**:
- Solución compila sin errores
- Aplicaciones inician sin errores críticos
- Se pueden realizar operaciones básicas

#### Fase 2: Validación de Pruebas

**Objetivo**: Asegurar que todas las pruebas automatizadas pasan y la funcionalidad es correcta

**Validaciones**:

1. **Pruebas Unitarias**:
- Ejecutar todas las pruebas unitarias
- Investigar y corregir fallos
- Re-ejecutar hasta que todas pasen

2. **Pruebas de Integración** (si existen):
- Ejecutar pruebas de integración contra servicios reales
- Validar conectividad a base de datos
- Validar llamadas entre servicios

3. **Pruebas Manuales de API**:
- Probar endpoints críticos manualmente
- Validar autenticación y autorización
- Verificar operaciones CRUD

4. **Pruebas Manuales de Aplicación WPF** (EXTENSIVAS):
Ver sección detallada más abajo

**Criterio de Fase Completa**:
- Todas las pruebas automatizadas pasan
- Pruebas manuales de API exitosas
- Pruebas manuales de WPF completas sin regresiones

### Nivel 3: Pruebas de Solución Completa

Después de que todos los proyectos estén migrados y todas las pruebas por fase pasen:

#### Pruebas End-to-End

**Escenarios Completos de Usuario**:

1. **Flujo de Autenticación Completo**:
   - Usuario inicia App WPF
   - Usuario hace login
   - Token JWT se obtiene de API
   - Token se usa para operaciones subsiguientes
   - Token expira y se renueva correctamente
   - Usuario hace logout

2. **Flujo de Operación de Negocio** (ejemplo genérico):
   - Usuario navega a módulo específico
   - Usuario consulta datos (GET a API)
   - Usuario crea nuevo registro (POST a API)
   - Usuario actualiza registro (PUT a API)
   - Usuario elimina registro (DELETE a API)
   - Datos se reflejan correctamente en UI

3. **Flujo de Comunicación SignalR** (si aplica):
   - App establece conexión SignalR con API
   - App recibe notificaciones en tiempo real
   - App envía mensajes al hub
   - Reconexión funciona si se pierde conexión

#### Pruebas de Rendimiento (Básicas)

**Objetivo**: Asegurar que no hay regresiones significativas de rendimiento

**Validaciones**:
- Tiempo de inicio de aplicaciones (no significativamente mayor)
- Tiempo de respuesta de API (no significativamente mayor)
- Uso de memoria (no significativamente mayor)
- Tiempo de renderizado de UI WPF (no significativamente mayor)

**Método**:
- Comparar métricas antes/después de la actualización
- Aceptar pequeñas variaciones (<10%)
- Investigar degradaciones significativas (>20%)

#### Pruebas de Regresión

**Objetivo**: Confirmar que funcionalidad existente no se rompió

**Método**:
- Ejecutar checklist de funcionalidades críticas
- Comparar comportamiento con versión anterior
- Documentar cualquier diferencia encontrada

### Pruebas Detalladas de Aplicación WPF

**⚠️ CRÍTICO**: Este es el área de mayor esfuerzo de pruebas (~40% del tiempo total)

#### Preparación

**Ambiente de Pruebas**:
- Ambiente de desarrollo limpio
- Conexión a API de desarrollo/pruebas
- Base de datos de pruebas con datos representativos
- Logs habilitados (Serilog)

**Documentación**:
- Lista de todas las vistas de la aplicación
- Lista de funcionalidades críticas de negocio
- Casos de prueba existentes (si los hay)

#### Checklist de Pruebas de UI WPF

**1. Inicio y Configuración** (5 minutos):
- [ ] Aplicación inicia sin excepciones
- [ ] MainWindow se muestra correctamente
- [ ] Splash screen funciona (si existe)
- [ ] Configuración inicial se carga correctamente
- [ ] Logs se escriben correctamente (verificar archivo/consola)

**2. Autenticación** (10 minutos):
- [ ] Pantalla de login se muestra correctamente
- [ ] Campos de usuario/contraseña aceptan entrada
- [ ] Login con credenciales válidas funciona
- [ ] Token JWT se obtiene correctamente
- [ ] Login con credenciales inválidas muestra error apropiado
- [ ] Validaciones de campos funcionan
- [ ] Botones y controles responden
- [ ] Navegación después de login exitoso funciona

**3. Navegación** (10 minutos):
- [ ] Menú principal se muestra correctamente
- [ ] Todos los ítems de menú son clickeables
- [ ] Navegación entre vistas funciona
- [ ] Breadcrumbs funcionan (si existen)
- [ ] Botones de navegación (atrás, adelante) funcionan
- [ ] Navegación por teclado funciona (Tab, Enter)
- [ ] Shortcuts de teclado funcionan (si existen)

**4. Controles de Entrada** (15 minutos):
- [ ] Todos los TextBox aceptan texto
- [ ] Todos los ComboBox muestran opciones y permiten selección
- [ ] Todos los CheckBox pueden marcarse/desmarcarse
- [ ] Todos los RadioButton pueden seleccionarse
- [ ] Todos los DatePicker permiten seleccionar fechas
- [ ] Controles numéricos aceptan solo números
- [ ] Validaciones de entrada funcionan
- [ ] Tooltips se muestran correctamente

**5. Botones y Comandos** (15 minutos):
- [ ] Todos los botones responden a clicks
- [ ] Comandos MVVM se ejecutan correctamente
- [ ] Botones deshabilitados no responden
- [ ] Feedback visual de hover funciona
- [ ] Feedback visual de click funciona
- [ ] Estados de botones (habilitado/deshabilitado) correctos

**6. Data Binding y Visualización de Datos** (20 minutos):
- [ ] DataGrid se carga con datos correctamente
- [ ] Columnas de DataGrid se muestran correctamente
- [ ] Sorting en DataGrid funciona
- [ ] Filtering en DataGrid funciona (si existe)
- [ ] Paginación funciona (si existe)
- [ ] ListView se carga correctamente
- [ ] ItemsControl se carga correctamente
- [ ] Binding bidireccional funciona (cambios en UI actualizan modelo)
- [ ] INotifyPropertyChanged funciona (cambios en modelo actualizan UI)
- [ ] ObservableCollection actualiza UI cuando cambia

**7. Operaciones CRUD** (25 minutos):
- [ ] **Crear**: Formulario de creación funciona
  - Campos se llenan correctamente
  - Validaciones funcionan
  - Guardar envía datos a API
  - Respuesta exitosa se maneja correctamente
  - UI se actualiza con nuevo registro
  - Errores se muestran apropiadamente
- [ ] **Leer/Consultar**: Consulta de datos funciona
  - Búsqueda funciona
  - Filtros funcionan
  - Resultados se muestran correctamente
- [ ] **Actualizar**: Edición funciona
  - Formulario se llena con datos existentes
  - Cambios se guardan correctamente
  - UI se actualiza
- [ ] **Eliminar**: Eliminación funciona
  - Confirmación se solicita
  - Eliminación se ejecuta en API
  - UI se actualiza (registro desaparece)

**8. Comunicación con API** (15 minutos):
- [ ] Llamadas GET funcionan
- [ ] Llamadas POST funcionan
- [ ] Llamadas PUT funcionan
- [ ] Llamadas DELETE funcionan
- [ ] Manejo de errores de API funciona (mostrar mensaje al usuario)
- [ ] Timeout de API se maneja correctamente
- [ ] Indicadores de carga se muestran durante llamadas
- [ ] Token JWT se incluye en headers
- [ ] Renovación de token funciona

**9. SignalR** (10 minutos, si aplica):
- [ ] Conexión a hub se establece
- [ ] Recepción de mensajes funciona
- [ ] Envío de mensajes funciona
- [ ] Notificaciones en tiempo real funcionan
- [ ] Reconexión automática funciona
- [ ] Manejo de desconexión funciona

**10. Diálogos y Ventanas** (10 minutos):
- [ ] MessageBox se muestra correctamente
- [ ] Diálogos modales funcionan
- [ ] Ventanas secundarias se abren correctamente
- [ ] Ventanas secundarias se cierran correctamente
- [ ] Datos se pasan entre ventanas correctamente
- [ ] Focus retorna a ventana principal

**11. Estilos y Temas** (10 minutos):
- [ ] Colores se muestran correctamente
- [ ] Fuentes se muestran correctamente
- [ ] Íconos se cargan correctamente
- [ ] Imágenes se cargan correctamente
- [ ] Estilos personalizados funcionan
- [ ] Temas (claro/oscuro) funcionan (si aplica)
- [ ] Animaciones funcionan (si existen)

**12. Resolución y Escalado** (10 minutos):
- [ ] UI se ve bien en resolución 1920x1080
- [ ] UI se ve bien en resolución 1366x768
- [ ] UI se ve bien en alta resolución (4K)
- [ ] Escalado de DPI funciona correctamente
- [ ] Textos son legibles en todas las resoluciones
- [ ] Controles no se superponen o se cortan

**13. Funcionalidades de Negocio Específicas** (Tiempo variable):
- [ ] [Listar funcionalidad 1]
- [ ] [Listar funcionalidad 2]
- [ ] [Listar funcionalidad 3]
- [ ] ...

**14. Manejo de Errores y Casos Edge** (15 minutos):
- [ ] Manejo de datos vacíos (listas vacías)
- [ ] Manejo de valores nulos
- [ ] Manejo de entrada inválida
- [ ] Manejo de pérdida de conexión a API
- [ ] Manejo de errores inesperados (no crash)
- [ ] Logs capturan errores correctamente

**15. Cierre de Aplicación** (5 minutos):
- [ ] Cerrar con X funciona
- [ ] Cerrar con menú/comando funciona
- [ ] Confirmación de cierre funciona (si existe)
- [ ] Recursos se liberan correctamente
- [ ] No quedan procesos huérfanos

**Tiempo Total Estimado de Pruebas Manuales de WPF**: **3-4 horas**

### Registro de Pruebas

**Documentar**:
- Qué se probó
- Resultado (Pasó / Falló)
- Si falló, descripción del problema
- Si falló, pasos para reproducir
- Prioridad del problema (Crítico / Alto / Medio / Bajo)

**Formato Sugerido**:
```
FECHA: [fecha]
PROBADOR: [nombre]
VERSIÓN: .NET 10.0

RESULTADOS:
- Inicio y Configuración: ✅ PASÓ
- Autenticación: ✅ PASÓ
- Navegación: ⚠️ FALLÓ PARCIALMENTE
  - Problema: Menú X no responde
  - Prioridad: Media
- ...

RESUMEN:
- Pruebas pasadas: 12/15
- Pruebas fallidas: 3/15
- Bloqueadores: 0
- Estado: REQUIERE CORRECCIONES
```

### Criterios de Aceptación de Pruebas

**Mínimo para considerar actualización exitosa**:
- ✅ 100% de proyectos compilan sin errores
- ✅ 100% de pruebas automatizadas pasan
- ✅ 100% de funcionalidades críticas de negocio funcionan
- ✅ 0 bloqueadores encontrados en pruebas manuales
- ⚠️ Problemas de baja prioridad pueden documentarse para corrección posterior

**Definición de Bloqueador**:
- Aplicación no inicia
- Autenticación no funciona
- Pérdida de datos
- Funcionalidad crítica de negocio no funciona
- Crash frecuente de aplicación

---

## Evaluación de Complejidad y Esfuerzo

### Complejidad Relativa por Proyecto

| Proyecto | Complejidad | Dependencias | Riesgo | Justificación |
|----------|-------------|--------------|--------|---------------|
| BMTECHRD.Pos.Domain | 🟢 **Baja** | 0 proyectos, 0 paquetes | Bajo | Sin dependencias, sin incidentes, solo cambio de TargetFramework |
| BMTECHRD.Pos.Application | 🟢 **Baja** | 1 proyecto, 0 paquetes | Bajo | Sin paquetes NuGet, sin incidentes, cambio simple |
| BMTECHRD.Pos.Auth.Core | 🟢 **Baja** | 1 proyecto, 2 paquetes | Bajo | Proyecto pequeño, 2 actualizaciones de paquetes, 15 cambios de comportamiento |
| BMTECHRD.Pos.Api.Tests | 🟡 **Media** | 4 proyectos, 7 paquetes | Bajo | Proyecto de pruebas, 3 actualizaciones, puede requerir ajustes en asserts |
| BMTECHRD.Pos.Api | 🟡 **Media** | 2 proyectos, 4 paquetes | Medio | API principal, 11 incompatibilidades de código fuente, cambios en ASP.NET Core |
| BMTECHRD.Pos.Infrastructure | 🟡 **Media-Alta** | 2 proyectos, 9 paquetes | Medio | 6 actualizaciones (incluye 2 paquetes obsoletos críticos), autenticación JWT |
| BMTECHRD.Pos.App | 🔴 **Alta** | 2 proyectos, 7 paquetes | Alto | **Proyecto WPF con 1,018 incompatibilidades binarias**, 51 archivos afectados, requiere pruebas manuales extensivas |

### Evaluación de Complejidad por Fase

#### Fase 1: Actualización Atómica

**Complejidad: Media-Alta**

**Componentes**:
1. Actualización de archivos de proyecto (7 archivos .csproj) - **Baja**
2. Actualización de referencias de paquetes (15 paquetes) - **Media**
3. Reemplazo de paquetes obsoletos (2 paquetes) - **Media**
4. Restauración de dependencias - **Baja**
5. Compilación y corrección de errores - **Alta** (especialmente WPF)

**Factores de Complejidad**:
- ✅ Todos los proyectos son SDK-style (simplifica edición)
- ✅ Estructura de dependencias clara (facilita orden de corrección)
- ⚠️ Proyecto WPF con muchas incompatibilidades (requiere atención)
- ⚠️ Paquetes de identidad obsoletos (requiere validación de seguridad)
- ⚠️ 11 incompatibilidades de código fuente en API

**Orden de Corrección Recomendado** (de más simple a más complejo):
1. Domain (sin cambios esperados)
2. Application (sin cambios esperados)
3. Auth.Core (cambios de comportamiento menores)
4. Infrastructure (paquetes obsoletos, requiere atención)
5. Api (incompatibilidades de código fuente)
6. Api.Tests (ajustes de pruebas)
7. App (WPF, mayor complejidad, muchas validaciones)

#### Fase 2: Validación de Pruebas

**Complejidad: Media**

**Componentes**:
1. Ejecución de pruebas unitarias - **Baja**
2. Corrección de fallos de pruebas - **Media** (pueden requerir ajustes por cambios de comportamiento)
3. Validación manual de aplicación WPF - **Alta** (requiere probar toda la UI)

**Factores de Complejidad**:
- ✅ Solo 1 proyecto de pruebas (Api.Tests)
- ⚠️ 19 cambios de comportamiento en pruebas
- 🔴 Proyecto WPF requiere pruebas manuales exhaustivas

### Requisitos de Recursos

#### Habilidades Requeridas

**Para Actualización Básica** (Domain, Application, Auth.Core):
- Nivel: Desarrollador Junior/Mid
- Conocimiento de archivos .csproj
- Comprensión de gestión de paquetes NuGet
- Habilidad para leer errores de compilación

**Para Actualización de API** (Api, Api.Tests):
- Nivel: Desarrollador Mid/Senior
- Conocimiento de ASP.NET Core
- Experiencia con Entity Framework Core
- Comprensión de breaking changes de framework

**Para Actualización de Infraestructura**:
- Nivel: Desarrollador Mid/Senior
- Conocimiento de autenticación JWT
- Experiencia con paquetes Microsoft.IdentityModel
- Comprensión de seguridad y tokens

**Para Actualización de WPF** (App):
- Nivel: Desarrollador Senior
- Experiencia sólida con WPF y XAML
- Conocimiento de data binding y comandos
- Habilidad para debugging de UI
- Experiencia con migraciones de WPF

#### Capacidad de Paralelización

**Actualización de Archivos**: ✅ Altamente paralelizable
- Diferentes desarrolladores pueden editar diferentes archivos .csproj simultáneamente

**Corrección de Errores**: ⚠️ Parcialmente paralelizable
- Requiere seguir orden de dependencias
- Proyectos sin dependencias entre sí pueden trabajarse en paralelo:
  - Auth.Core y Infrastructure (ambos dependen de Application)
  - Api y App (ambos dependen de capas inferiores pero no entre sí)

**Pruebas**: ⚠️ Secuencial recomendado
- Mejor ejecutar pruebas después de que toda la solución compile
- Pruebas manuales de WPF requieren atención dedicada

### Complejidad Total de la Solución

**Clasificación General: Media-Alta**

**Justificación**:
- ✅ Solución pequeña (7 proyectos)
- ✅ Arquitectura limpia y bien estructurada
- ✅ Sin dependencias circulares
- ⚠️ 15 paquetes a actualizar
- ⚠️ 2 paquetes obsoletos críticos
- 🔴 Proyecto WPF con 1,018 incompatibilidades
- 🔴 26.8% del código del proyecto WPF potencialmente afectado

**Comparación**:
- **Más simple que**: Soluciones con dependencias circulares, múltiples frameworks, o decenas de proyectos
- **Más compleja que**: Soluciones pequeñas sin UI o con pocos paquetes externos
- **Similar a**: Soluciones empresariales pequeñas con aplicación de escritorio y API

### Factores que Influyen en el Esfuerzo

#### Factores que Reducen Esfuerzo:
1. ✅ Proyectos ya en formato SDK-style
2. ✅ Todos en .NET 8.0 (salto menor a .NET 10.0)
3. ✅ Arquitectura bien diseñada
4. ✅ Dependencias claras sin ciclos
5. ✅ Sin vulnerabilidades de seguridad urgentes

#### Factores que Aumentan Esfuerzo:
1. 🔴 Proyecto WPF con muchas incompatibilidades binarias
2. ⚠️ Paquetes de identidad obsoletos (críticos para seguridad)
3. ⚠️ Incompatibilidades de código fuente en API
4. ⚠️ Necesidad de pruebas manuales extensivas de UI
5. ⚠️ 64 archivos con incidentes (24.6% del código)

### Estimación Relativa de Esfuerzo

**Nota**: No se proporcionan estimaciones de tiempo en horas/días debido a la alta variabilidad según experiencia del equipo, ambiente, y factores externos.

**Distribución Relativa de Esfuerzo por Actividad**:

1. **Actualización de archivos de proyecto y paquetes**: 5%
2. **Corrección de errores de compilación**: 25%
   - Domain + Application: 2%
   - Auth.Core: 3%
   - Infrastructure (paquetes obsoletos): 8%
   - Api (incompatibilidades): 7%
   - App (WPF): 5% (la mayoría son falsos positivos)
3. **Validación y pruebas automatizadas**: 20%
4. **Pruebas manuales de aplicación WPF**: 40%
5. **Corrección de problemas encontrados en pruebas**: 10%

**Mayor Inversión de Esfuerzo**: Pruebas manuales del proyecto WPF (40% del esfuerzo total estimado)

---

## Estrategia de Control de Versiones

### Configuración de Branch

**Branch de Origen**: `master`
**Branch de Actualización**: `upgrade-to-NET10` (ya creada)

### Estrategia de Commit - Opción Recomendada: Commit Único

Dado que se utiliza la estrategia **Todo-a-la-Vez**, se recomienda un **commit único** que refleje la naturaleza atómica de la actualización.

#### Ventajas del Commit Único:
- ✅ Refleja fielmente la estrategia Todo-a-la-Vez
- ✅ Simplifica rollback (un solo revert)
- ✅ Historial más limpio
- ✅ Facilita cherry-pick si es necesario
- ✅ Pull request más fácil de revisar como unidad

#### Flujo de Trabajo con Commit Único:

**1. Durante la Actualización**:
```bash
# Todos los cambios se van acumulando en working directory
# NO hacer commits intermedios
```

**2. Después de que Todo Compila y las Pruebas Pasan**:
```bash
# Stage todos los cambios
git add -A

# Crear commit único descriptivo
git commit -m "chore: upgrade solution to .NET 10.0

- Update all projects from net8.0 to net10.0
- Update 15 NuGet packages to versions compatible with .NET 10.0
- Replace obsolete identity packages (Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt)
- Fix compilation errors from breaking changes
- All tests passing
- Manual testing of WPF app completed

BREAKING CHANGES:
- .NET 10.0 SDK now required
- Updated minimum package versions

Resolves #[issue-number] (si aplica)"
```

**3. Push a Remote**:
```bash
git push origin upgrade-to-NET10
```

### Estrategia de Commit Alternativa: Commits por Fase

Si se prefiere más granularidad en el historial:

#### Commit 1: Actualización de Archivos de Proyecto
```bash
git add **/*.csproj
git commit -m "chore: update target framework to net10.0 in all projects"
```

#### Commit 2: Actualización de Paquetes NuGet
```bash
git add **/*.csproj
git commit -m "chore: update NuGet packages to .NET 10.0 compatible versions

- Update Microsoft.EntityFrameworkCore.* to 10.0.3
- Update Microsoft.AspNetCore.* to 10.0.3
- Update Microsoft.Extensions.* to 10.0.3
- Replace obsolete identity packages with v8.2.1"
```

#### Commit 3: Correcciones de Errores de Compilación
```bash
git add -A
git commit -m "fix: resolve compilation errors from .NET 10.0 upgrade

- Fix ASP.NET Core API incompatibilities
- Update authentication configuration
- Adjust EF Core queries"
```

#### Commit 4: Correcciones de Pruebas
```bash
git add -A
git commit -m "test: fix failing tests after .NET 10.0 upgrade

- Update test assertions for new behavior
- Fix test setup and teardown
- All tests now passing"
```

**Desventajas de Commits por Fase**:
- ⚠️ Commits intermedios pueden no compilar (trabajo incompleto)
- ⚠️ Más complejo para rollback (múltiples reverts)
- ⚠️ No refleja naturaleza atómica de la estrategia

### Convenciones de Mensajes de Commit

**Formato**: Conventional Commits

```
<tipo>: <descripción corta>

[cuerpo opcional]

[notas al pie opcionales]
```

**Tipos**:
- `chore`: Cambios de mantenimiento (actualización de framework)
- `fix`: Corrección de bugs
- `test`: Cambios en pruebas
- `docs`: Cambios en documentación

**Ejemplo de Mensaje Completo**:
```
chore: upgrade solution to .NET 10.0

- Updated all projects from net8.0/net8.0-windows to net10.0/net10.0-windows
- Updated 15 NuGet packages to .NET 10.0 compatible versions:
  - Microsoft.EntityFrameworkCore.* 8.0.8 → 10.0.3
  - Microsoft.AspNetCore.* 8.0.8 → 10.0.3
  - Microsoft.Extensions.* 8.0.0 → 10.0.3
- Replaced obsolete identity packages:
  - Microsoft.IdentityModel.Tokens 7.6.0 → 8.2.1
  - System.IdentityModel.Tokens.Jwt 7.6.0 → 8.2.1
- Fixed 11 source code incompatibilities in API project
- Fixed 4 binary incompatibilities in Infrastructure project
- All 7 projects compile successfully with 0 errors and 0 warnings
- All automated tests passing (X tests)
- Manual testing of WPF application completed successfully
- No functional regressions detected

BREAKING CHANGES:
- .NET 10.0 SDK is now required for development and deployment
- Minimum package versions updated (see above)
- PostgreSQL provider version compatible with EF Core 10.0 required

Testing:
- Automated tests: X/X passing
- Manual WPF testing: Complete (3-4 hours)
- API smoke testing: Complete
- Authentication flows validated
- No blocker issues found

Closes #[issue-number]
```

### Proceso de Revisión y Merge

#### Crear Pull Request

**Título**: `Upgrade solution to .NET 10.0`

**Descripción de PR**:
```markdown
## Descripción

Este PR actualiza toda la solución BMTECHRD.Pos de .NET 8.0 a .NET 10.0 LTS.

## Estrategia

Estrategia **Todo-a-la-Vez**: Todos los proyectos actualizados simultáneamente en una operación atómica.

## Cambios Incluidos

### Proyectos Actualizados (7)
- [x] BMTECHRD.Pos.Domain: net8.0 → net10.0
- [x] BMTECHRD.Pos.Application: net8.0 → net10.0
- [x] BMTECHRD.Pos.Infrastructure: net8.0 → net10.0
- [x] BMTECHRD.Pos.Auth.Core: net8.0 → net10.0
- [x] BMTECHRD.Pos.Api: net8.0 → net10.0
- [x] BMTECHRD.Pos.App: net8.0-windows → net10.0-windows
- [x] BMTECHRD.Pos.Api.Tests: net8.0 → net10.0

### Paquetes Actualizados (15)
- [x] Microsoft.EntityFrameworkCore.* 8.0.8 → 10.0.3
- [x] Microsoft.AspNetCore.* 8.0.8 → 10.0.3
- [x] Microsoft.Extensions.* 8.0.0 → 10.0.3
- [x] Microsoft.IdentityModel.Tokens 7.6.0 → 8.2.1 (obsoleto reemplazado)
- [x] System.IdentityModel.Tokens.Jwt 7.6.0 → 8.2.1 (obsoleto reemplazado)
- [x] Newtonsoft.Json 13.0.3 → 13.0.4

### Correcciones de Código
- [x] Resueltas 11 incompatibilidades de código fuente en API
- [x] Resueltas 4 incompatibilidades binarias en Infrastructure
- [x] Actualizada configuración de autenticación JWT
- [x] Ajustadas pruebas para nuevos comportamientos

## Pruebas Realizadas

### Compilación
- [x] Todos los proyectos compilan sin errores
- [x] 0 advertencias

### Pruebas Automatizadas
- [x] Todas las pruebas unitarias pasan (X/X)
- [x] Todas las pruebas de integración pasan (X/X)

### Pruebas Manuales
- [x] API inicia correctamente
- [x] Swagger UI funciona
- [x] Autenticación JWT funciona
- [x] Endpoints de API funcionan
- [x] Aplicación WPF inicia correctamente
- [x] Pruebas exhaustivas de UI WPF completadas (3-4 horas)
- [x] Todas las funcionalidades críticas validadas
- [x] SignalR funciona correctamente
- [x] No se detectaron regresiones

### Problemas Conocidos
- [ ] Ninguno / [Listar si existen]

## Breaking Changes

- ⚠️ Requiere .NET 10.0 SDK para desarrollo
- ⚠️ Requiere .NET 10.0 Runtime para deployment
- ⚠️ Versiones mínimas de paquetes actualizadas

## Checklist de Revisión

- [ ] Código revisado
- [ ] Cambios en archivos .csproj verificados
- [ ] Versiones de paquetes verificadas
- [ ] Build pipeline pasa
- [ ] Pruebas automatizadas pasan
- [ ] Documentación actualizada (si aplica)
- [ ] Changelog actualizado

## Notas para el Revisor

- Este es un upgrade de framework, no cambios funcionales
- La mayoría de cambios son mecánicos (versiones en .csproj)
- Foco de revisión: configuraciones de autenticación y correcciones de breaking changes
- WPF ha sido probado exhaustivamente de forma manual

## Plan de Deployment

1. Verificar que servidores tienen .NET 10.0 Runtime instalado
2. Actualizar aplicación API
3. Actualizar aplicación WPF para usuarios
4. Monitorear logs por 24-48 horas

## Rollback Plan

Si se requiere rollback:
```bash
git revert [commit-hash]
# o
git reset --hard [commit-anterior]
```

## Referencias

- [Plan de Actualización Completo](link-a-plan.md)
- [Assessment](link-a-assessment.md)
- [.NET 10.0 Release Notes](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview)
```

#### Checklist de Revisión

**Para el Revisor**:

1. **Archivos .csproj**:
   - [ ] Todos tienen TargetFramework actualizado correctamente
   - [ ] Versiones de paquetes son consistentes
   - [ ] No hay paquetes obsoletos (excepto los que ya se reemplazaron)

2. **Código**:
   - [ ] Correcciones de breaking changes son apropiadas
   - [ ] No hay comentarios de código o TODOs sin resolver
   - [ ] Configuraciones de autenticación son correctas

3. **Pruebas**:
   - [ ] Build pipeline pasa
   - [ ] Todas las pruebas automatizadas pasan
   - [ ] Evidencia de pruebas manuales documentada

4. **Documentación**:
   - [ ] README actualizado si es necesario
   - [ ] CHANGELOG.md actualizado
   - [ ] Comentarios de código actualizados si es necesario

#### Criterios de Aprobación

**Mínimo para aprobar PR**:
- ✅ Build pipeline verde
- ✅ Todas las pruebas automatizadas pasan
- ✅ Al menos 1 aprobación de desarrollador senior
- ✅ Aprobación de líder técnico
- ✅ Evidencia de pruebas manuales
- ✅ Sin conflictos de merge

### Merge a Master

**Método Recomendado**: Squash and Merge (si se usaron commits por fase)

Si se usó commit único, usar: **Merge Commit**

**Comando después de aprobación**:
```bash
# Actualizar branch local
git checkout upgrade-to-NET10
git pull origin upgrade-to-NET10

# Merge a master
git checkout master
git pull origin master
git merge --no-ff upgrade-to-NET10 -m "Merge branch 'upgrade-to-NET10' - Upgrade to .NET 10.0"

# Push a remote
git push origin master
```

### Tagging de Versión

Después de merge exitoso a master:

```bash
# Crear tag de versión
git tag -a v2.0.0-net10 -m "Release v2.0.0 - Upgraded to .NET 10.0

- Migrated all projects to .NET 10.0 LTS
- Updated all dependencies to latest compatible versions
- Replaced obsolete identity packages
- All tests passing
- Production ready"

# Push tag a remote
git push origin v2.0.0-net10
```

### Documentación en Control de Versiones

**Archivos a Actualizar en el Repositorio**:

1. **CHANGELOG.md**:
```markdown
## [2.0.0] - 2024-XX-XX

### Changed
- Upgraded entire solution to .NET 10.0 LTS
- Updated all NuGet packages to .NET 10.0 compatible versions

### Fixed
- Resolved source code incompatibilities from framework upgrade
- Updated authentication configuration for new IdentityModel packages

### Breaking Changes
- .NET 10.0 SDK required for development
- .NET 10.0 Runtime required for deployment
```

2. **README.md** (si existe):
```markdown
## Requirements

- .NET 10.0 SDK (for development)
- .NET 10.0 Runtime (for deployment)
- PostgreSQL 12+
- Visual Studio 2022 17.12+ or JetBrains Rider 2024.3+
```

3. **.github/workflows/*.yml** (si existen CI/CD pipelines):
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v3
  with:
    dotnet-version: '10.0.x'
```

### Estrategia de Branches Post-Merge

**Opción 1**: Eliminar branch de upgrade
```bash
# Localmente
git branch -d upgrade-to-NET10

# Remotamente
git push origin --delete upgrade-to-NET10
```

**Opción 2**: Mantener branch como referencia histórica
- Útil para consultar el proceso de upgrade
- Puede archivarse después de cierto tiempo

### Comunicación al Equipo

**Después de merge a master**:

1. **Notificación al equipo**:
```
📢 Actualización a .NET 10.0 Completa

La solución BMTECHRD.Pos ha sido actualizada exitosamente a .NET 10.0 LTS.

Acción Requerida para Desarrolladores:
1. Instalar .NET 10.0 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
2. Actualizar IDE si es necesario (VS 2022 17.12+ o Rider 2024.3+)
3. Pull de master: git checkout master && git pull
4. Limpiar y reconstruir: dotnet clean && dotnet build

Acción Requerida para Deployment:
1. Asegurar que servidores tengan .NET 10.0 Runtime instalado
2. Seguir proceso normal de deployment
3. Monitorear logs por 24-48 horas

Documentación:
- Plan completo: [link]
- Assessment: [link]
- PR: [link]

Preguntas: Contactar a [nombre del líder técnico]
```

2. **Actualización de documentación de proyecto**

3. **Entrada en wiki/knowledge base** (si existe)

---

## Criterios de Éxito

### Criterios Técnicos

#### 1. Compilación y Build

**Criterio**: Toda la solución debe compilar sin errores ni advertencias

**Validación**:
```bash
dotnet build BMTECHRD.Pos.slnx --configuration Release --no-incremental
```

**Métricas de Éxito**:
- ✅ 0 errores de compilación en todos los proyectos
- ✅ 0 advertencias (o solo advertencias pre-existentes documentadas y aceptadas)
- ✅ Build exitoso en primer intento después de correcciones

**Evidencia**:
- Captura de pantalla de output de build
- Log de compilación limpio

---

#### 2. Target Framework

**Criterio**: Todos los proyectos deben apuntar al framework correcto

**Validación**:
- BMTECHRD.Pos.Domain: `net10.0` ✅
- BMTECHRD.Pos.Application: `net10.0` ✅
- BMTECHRD.Pos.Infrastructure: `net10.0` ✅
- BMTECHRD.Pos.Auth.Core: `net10.0` ✅
- BMTECHRD.Pos.Api: `net10.0` ✅
- BMTECHRD.Pos.App: `net10.0-windows` ✅
- BMTECHRD.Pos.Api.Tests: `net10.0` ✅

**Evidencia**:
- Revisión de archivos .csproj
- Output de `dotnet --version` y `dotnet --list-sdks`

---

#### 3. Paquetes NuGet

**Criterio**: Todos los paquetes deben estar actualizados a versiones compatibles con .NET 10.0

**Validación**:
- ✅ Todos los paquetes que requerían actualización fueron actualizados
- ✅ No quedan paquetes obsoletos (Microsoft.IdentityModel.Tokens 7.x, System.IdentityModel.Tokens.Jwt 7.x reemplazados)
- ✅ No hay conflictos de dependencias
- ✅ `dotnet restore` se completa sin errores

**Métricas**:
- 15 paquetes actualizados exitosamente
- 2 paquetes obsoletos reemplazados
- 11 paquetes compatibles sin cambios

**Evidencia**:
- Archivo .csproj de cada proyecto
- Output de `dotnet list package --vulnerable` (sin vulnerabilidades)
- Output de `dotnet list package --deprecated` (sin paquetes obsoletos)

---

#### 4. Pruebas Automatizadas

**Criterio**: Todas las pruebas automatizadas deben pasar

**Validación**:
```bash
dotnet test BMTECHRD.Pos.slnx --configuration Release --verbosity normal
```

**Métricas de Éxito**:
- ✅ 100% de pruebas pasando
- ✅ 0 pruebas fallidas
- ✅ 0 pruebas omitidas (a menos que estén documentadas)
- ✅ Cobertura de pruebas mantenida o mejorada

**Evidencia**:
- Report de pruebas con porcentajes
- Captura de pantalla de test explorer
- Log de ejecución de pruebas

---

#### 5. Breaking Changes Resueltos

**Criterio**: Todos los breaking changes identificados deben estar resueltos

**Validación**:
- ✅ 11 incompatibilidades de código fuente en Api resueltas
- ✅ 4 incompatibilidades binarias en Infrastructure resueltas
- ✅ Cambios de comportamiento validados en pruebas
- ✅ Paquetes obsoletos reemplazados correctamente

**Evidencia**:
- Lista de breaking changes del assessment
- Documentación de cómo cada uno fue resuelto
- Código actualizado en commits

---

#### 6. Sin Vulnerabilidades de Seguridad

**Criterio**: No debe haber vulnerabilidades de seguridad conocidas

**Validación**:
```bash
dotnet list package --vulnerable
```

**Métricas de Éxito**:
- ✅ 0 paquetes con vulnerabilidades críticas
- ✅ 0 paquetes con vulnerabilidades altas
- ✅ Paquetes obsoletos de identidad reemplazados (mejora de seguridad)

**Evidencia**:
- Output del comando anterior
- Confirmación de versiones actualizadas de paquetes de seguridad

---

### Criterios Funcionales

#### 7. Aplicación API Funciona Correctamente

**Criterio**: La API debe iniciar y responder correctamente

**Validación**:

**Inicio**:
- ✅ `dotnet run` inicia sin excepciones
- ✅ Swagger UI se carga en `https://localhost:XXXX/swagger`
- ✅ Conexión a base de datos exitosa

**Autenticación**:
- ✅ Endpoint de login funciona
- ✅ Token JWT se genera correctamente
- ✅ Token JWT válido permite acceso a endpoints protegidos
- ✅ Token JWT inválido deniega acceso
- ✅ Token JWT expirado deniega acceso

**Endpoints**:
- ✅ Todos los endpoints GET responden correctamente
- ✅ Todos los endpoints POST funcionan
- ✅ Todos los endpoints PUT funcionan
- ✅ Todos los endpoints DELETE funcionan
- ✅ Validación de modelos funciona
- ✅ Manejo de errores apropiado (códigos de estado HTTP correctos)

**Base de Datos**:
- ✅ Operaciones CRUD funcionan
- ✅ Consultas complejas funcionan
- ✅ Transacciones funcionan
- ✅ Migraciones de EF Core son compatibles

**Evidencia**:
- Logs de inicio sin errores
- Screenshots de Swagger UI
- Resultados de smoke tests de endpoints
- Evidencia de autenticación exitosa

---

#### 8. Aplicación WPF Funciona Correctamente

**Criterio**: La aplicación de escritorio debe funcionar sin regresiones

**Validación Completa** (ver §Estrategia de Pruebas para detalles):

**Inicio**:
- ✅ Aplicación inicia sin excepciones
- ✅ MainWindow se muestra correctamente
- ✅ Logs de Serilog funcionan

**Autenticación**:
- ✅ Login funciona
- ✅ Token se obtiene de API
- ✅ Sesión se mantiene

**UI**:
- ✅ Todos los controles interactivos funcionan
- ✅ Navegación entre vistas funciona
- ✅ Data binding funciona correctamente
- ✅ Comandos MVVM funcionan
- ✅ Diálogos y ventanas funcionan
- ✅ Estilos y temas se aplican correctamente

**Comunicación**:
- ✅ Llamadas a API funcionan
- ✅ SignalR funciona (si aplica)
- ✅ Manejo de errores de red funciona

**Funcionalidad de Negocio**:
- ✅ Todas las operaciones CRUD funcionan
- ✅ Todas las funcionalidades críticas validadas
- ✅ Cálculos y lógica de negocio correctos

**Evidencia**:
- Checklist completo de pruebas de WPF (ver §Estrategia de Pruebas)
- Screenshots de vistas principales funcionando
- Documentación de pruebas manuales realizadas (3-4 horas)
- Firma de QA o desarrollador senior que realizó pruebas

---

#### 9. Sin Regresiones Funcionales

**Criterio**: Ninguna funcionalidad existente debe dejar de funcionar

**Validación**:
- ✅ Todas las funcionalidades documentadas funcionan como antes
- ✅ Todos los casos de uso críticos funcionan
- ✅ No hay comportamientos inesperados reportados
- ✅ Rendimiento comparable a versión anterior

**Métricas**:
- 0 funcionalidades rotas
- 0 bloqueadores funcionales
- Problemas menores (si existen) documentados como no-bloqueadores

**Evidencia**:
- Comparación lado a lado de funcionalidad antes/después
- Registro de pruebas de regresión
- Confirmación de stakeholders clave

---

### Criterios de Calidad

#### 10. Código Limpio y Mantenible

**Criterio**: El código debe mantener o mejorar la calidad

**Validación**:
- ✅ Sin código comentado no resuelto
- ✅ Sin TODOs críticos sin resolver
- ✅ Configuraciones correctamente externalizadas
- ✅ Sin hardcoded values introducidos durante upgrade
- ✅ Logging apropiado mantenido

**Evidencia**:
- Revisión de código en PR
- Análisis estático de código (si hay herramientas configuradas)

---

#### 11. Documentación Actualizada

**Criterio**: Documentación debe reflejar la nueva versión

**Validación**:
- ✅ README.md actualizado con requisitos de .NET 10.0
- ✅ CHANGELOG.md actualizado con entrada de upgrade
- ✅ Documentación de deployment actualizada
- ✅ Comentarios de código actualizados si es necesario
- ✅ Wiki/knowledge base actualizada (si existe)

**Evidencia**:
- Commits con cambios de documentación
- Revisión de archivos de documentación

---

### Criterios de Proceso

#### 12. Estrategia Todo-a-la-Vez Seguida

**Criterio**: La actualización debe seguir los principios de la estrategia seleccionada

**Validación**:
- ✅ Todos los proyectos actualizados simultáneamente
- ✅ No quedan proyectos en .NET 8.0
- ✅ Sin estados intermedios de multi-targeting
- ✅ Operación atómica completada exitosamente

**Evidencia**:
- Revisión de commits (único o serie coordinada)
- Todos los .csproj muestran net10.0 / net10.0-windows

---

#### 13. Control de Versiones Apropiado

**Criterio**: Cambios deben estar correctamente versionados

**Validación**:
- ✅ Branch `upgrade-to-NET10` creada y usada
- ✅ Commits con mensajes descriptivos siguiendo Conventional Commits
- ✅ Pull Request creado con descripción completa
- ✅ PR aprobado por al menos 1 reviewer senior
- ✅ Merge a `master` exitoso
- ✅ Tag de versión creado (ej: v2.0.0-net10)

**Evidencia**:
- Historial de Git
- Pull Request en repositorio
- Tags de versión

---

### Criterios de Deployment

#### 14. Preparación para Producción

**Criterio**: La solución debe estar lista para deployment a producción

**Validación**:
- ✅ Build de Release exitoso
- ✅ .NET 10.0 Runtime disponible en servidores objetivo
- ✅ Plan de deployment documentado
- ✅ Plan de rollback documentado
- ✅ Comunicación a stakeholders completada
- ✅ Documentación de deployment actualizada

**Evidencia**:
- Build de Release exitoso
- Confirmación de infraestructura lista
- Plan de deployment escrito
- Emails/mensajes a stakeholders

---

### Resumen de Criterios

**Para considerar la actualización EXITOSA, se deben cumplir**:

| # | Criterio | Prioridad | Estado |
|---|----------|-----------|--------|
| 1 | Compilación sin errores/advertencias | 🔴 Crítico | [ ] |
| 2 | Target Framework correcto en todos los proyectos | 🔴 Crítico | [ ] |
| 3 | Paquetes NuGet actualizados | 🔴 Crítico | [ ] |
| 4 | Todas las pruebas automatizadas pasan | 🔴 Crítico | [ ] |
| 5 | Breaking changes resueltos | 🔴 Crítico | [ ] |
| 6 | Sin vulnerabilidades de seguridad | 🔴 Crítico | [ ] |
| 7 | Aplicación API funciona | 🔴 Crítico | [ ] |
| 8 | Aplicación WPF funciona | 🔴 Crítico | [ ] |
| 9 | Sin regresiones funcionales | 🔴 Crítico | [ ] |
| 10 | Código limpio y mantenible | 🟡 Importante | [ ] |
| 11 | Documentación actualizada | 🟡 Importante | [ ] |
| 12 | Estrategia Todo-a-la-Vez seguida | 🟢 Deseable | [ ] |
| 13 | Control de versiones apropiado | 🟡 Importante | [ ] |
| 14 | Preparación para producción | 🔴 Crítico | [ ] |

**Criterios Mínimos para Aprobar**:
- 🔴 **Todos los criterios críticos** deben cumplirse (1-9, 14)
- 🟡 **Al menos 80% de criterios importantes** deben cumplirse (10, 11, 13)
- 🟢 Criterios deseables son opcionales pero recomendados (12)

**Definición de DONE**:
```
✅ Solución compila sin errores
✅ Todos los paquetes actualizados
✅ Todas las pruebas pasan
✅ API funciona correctamente
✅ WPF funciona correctamente
✅ Sin regresiones
✅ Documentación actualizada
✅ PR aprobado y merged
✅ Tag de versión creado
✅ Lista para deployment a producción
```

---

### Proceso de Sign-Off

**Aprobaciones Requeridas**:

1. **Desarrollador que ejecutó la actualización**: ✅
   - Confirma que todos los pasos se completaron
   - Confirma que pruebas técnicas pasaron

2. **Desarrollador Senior / Líder Técnico**: ✅
   - Revisa código y cambios
   - Valida decisiones técnicas
   - Aprueba PR

3. **QA / Tester** (si aplica): ✅
   - Valida pruebas manuales de WPF
   - Confirma sin regresiones
   - Documenta resultados de pruebas

4. **Product Owner / Stakeholder** (si aplica): ✅
   - Confirma que funcionalidad de negocio está intacta
   - Aprueba para deployment

**Documento de Sign-Off**:
```
ACTUALIZACIÓN A .NET 10.0 - SIGN-OFF

Proyecto: BMTECHRD.Pos
Fecha: [fecha]
Versión: 2.0.0-net10

RESUMEN:
- 7 proyectos actualizados exitosamente
- 15 paquetes actualizados
- 2 paquetes obsoletos reemplazados
- Todas las pruebas pasando
- Sin regresiones detectadas

APROBACIONES:

[✅] Desarrollador: [nombre] - [fecha]
     Comentarios: Actualización completada según plan. Todas las pruebas técnicas OK.

[✅] Líder Técnico: [nombre] - [fecha]
     Comentarios: Código revisado y aprobado. Cambios apropiados.

[✅] QA: [nombre] - [fecha]
     Comentarios: Pruebas manuales completadas. Sin bloqueadores encontrados.

[✅] Product Owner: [nombre] - [fecha]
     Comentarios: Aprobado para deployment a producción.

ESTADO: ✅ APROBADO PARA PRODUCCIÓN

Próximos pasos:
1. Deployment a ambiente de staging: [fecha planificada]
2. Validación en staging: [fecha planificada]
3. Deployment a producción: [fecha planificada]
```

---

## FIN DEL PLAN

Este plan proporciona una guía completa y accionable para actualizar la solución BMTECHRD.Pos de .NET 8.0 a .NET 10.0 LTS utilizando la estrategia Todo-a-la-Vez.

**Última actualización**: [fecha de generación]
**Autor**: GitHub Copilot - Planning Agent
**Versión del plan**: 1.0
