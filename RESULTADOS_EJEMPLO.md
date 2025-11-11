# Resultados de Ejecución del POC
## Ejemplos de Salidas y Métricas

---

## 1. RESULTADOS DE PRUEBAS UNITARIAS (xUnit)

### Salida de Consola

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    10, Skipped:     0, Total:    10, Duration: 2 s
```

### Detalle de Pruebas

```
Test Run Successful.
Total tests: 10
     Passed: 10
     Failed: 0
 Total time: 2.345s
```

### Cobertura de Código

```
Calculating coverage result...
  Generating report 'coverage.cobertura.xml'

+----------------------+--------+--------+--------+
| Module               | Line   | Branch | Method |
|----------------------|--------|--------|--------|
| GestionLlaves        | 75.2%  | 68.5%  | 72.3%  |
|  Controllers         | 78.5%  | 71.2%  | 75.8%  |
|  Models              | 60.0%  | 55.0%  | 58.3%  |
|  Helpers             | 90.0%  | 85.0%  | 88.5%  |
+----------------------+--------|--------|--------+
```

### Lista de Pruebas Ejecutadas

```
✓ Login_ConCredencialesValidas_DeberiaRedirigirADashboard [45ms]
✓ Login_ConCredencialesInvalidas_DeberiaRetornarViewConMensajeError [32ms]
✓ Login_ConCamposVacios_DeberiaRetornarViewConMensajeError [28ms]
✓ Login_UsuarioInactivo_DeberiaRetornarViewConMensajeError [35ms]
✓ CambiarPasswordPrimeraVez_ConDatosValidos_DeberiaActualizarPassword [67ms]
✓ CambiarPasswordPrimeraVez_ConPasswordActualIncorrecta_DeberiaRetornarError [41ms]
✓ Logout_DeberiaLimpiarSesionYRedirigirALogin [15ms]
✓ HashPassword_ConMismaPassword_DeberiaGenerarMismoHash [8ms]
✓ HashPassword_ConDiferentesPasswords_DeberiaGenerarDiferentesHashes [7ms]
✓ Usuario_EmailInvalido_DeberiaFallarValidacion [12ms]
```

---

## 2. RESULTADOS DE SONARANALYZER

### Resumen de Issues

```
Total Issues: 23
  Critical: 2
  Major: 8
  Minor: 13
```

### Issues Críticos Detectados

```
1. AccountController.cs (línea 234)
   Rule: S2068 (Credentials should not be hard-coded)
   Severity: CRITICAL
   Message: Remove this hard-coded credential.
   
2. AccountController.cs (línea 75)
   Rule: S2259 (Null pointers should not be dereferenced)
   Severity: CRITICAL
   Message: 'usuario.Rol' is null on at least one execution path.
```

### Issues Mayores

```
3. DocenteController.cs (línea 41)
   Rule: S3776 (Cognitive Complexity of functions should not be too high)
   Severity: MAJOR
   Message: Refactor this function to reduce its Cognitive Complexity from 15 to the 15 allowed.
   
4. AccountController.cs (línea 93)
   Rule: S4790 (Hashing data is security-sensitive)
   Severity: MAJOR
   Message: Make sure that hashing data is safe here.
```

### Métricas Generales

```
Lines of Code: 3,450
Code Smells: 13
Bugs: 8
Vulnerabilities: 2
Security Hotspots: 2
Technical Debt: 4.5 hours
Maintainability Rating: B
Reliability Rating: B
Security Rating: C
```

---

## 3. RESULTADOS DE PRUEBAS DE CARGA (NBomber)

### Escenario 1: Carga de Login

```
scenario: carga_login
  ok count: 285
  fail count: 15
  RPS: 9.5
  min: 45ms
  mean: 120ms
  max: 450ms
  p50: 95ms
  p95: 280ms
  p99: 380ms
  data transfer: 1.2 MB
  status codes:
    200: 285
    500: 15
```

### Escenario 2: Carga de Solicitar Llave

```
scenario: carga_solicitar_llave
  ok count: 142
  fail count: 8
  RPS: 4.7
  min: 120ms
  mean: 210ms
  max: 680ms
  p50: 180ms
  p95: 450ms
  p99: 620ms
  data transfer: 2.1 MB
  status codes:
    200: 142
    401: 8
```

### Escenario 3: Carga de Estado

```
scenario: carga_estado
  ok count: 228
  fail count: 12
  RPS: 7.6
  min: 80ms
  mean: 165ms
  max: 520ms
  p50: 140ms
  p95: 380ms
  p99: 480ms
  data transfer: 1.8 MB
  status codes:
    200: 228
    500: 12
```

### Resumen General

```
All scenarios finished
Total requests: 655
Successful: 625 (95.4%)
Failed: 30 (4.6%)
Average RPS: 7.2
Total data transfer: 5.1 MB
Test duration: 30s
```

### Gráfico de Latencia (Representación)

```
Latency Distribution (ms)
─────────────────────────
p0   (min):     45
p50  (median):  120
p75:            180
p90:            280
p95:            380
p99:            450
p100 (max):     680
```

---

## 4. INTERPRETACIÓN DE RESULTADOS

### Pruebas Unitarias
✅ **Estado**: Exitoso
- Todas las pruebas pasan
- Cobertura del 75% es aceptable pero mejorable
- Tiempo de ejecución rápido (2.3s)

### Calidad de Código
⚠️ **Estado**: Requiere atención
- 2 issues críticos deben resolverse inmediatamente
- Code smells indican necesidad de refactorización
- Rating de seguridad (C) necesita mejoras

### Pruebas de Carga
✅ **Estado**: Aceptable con mejoras recomendadas
- Sistema maneja bien la carga normal
- Algunos timeouts requieren optimización
- Latencia promedio dentro de rangos aceptables

---

## 5. RECOMENDACIONES BASADAS EN RESULTADOS

1. **Inmediatas**:
   - Resolver issues críticos de seguridad
   - Mover credenciales a configuración segura

2. **Corto Plazo**:
   - Aumentar cobertura de pruebas al 85%+
   - Refactorizar métodos con alta complejidad ciclomática
   - Optimizar consultas de base de datos

3. **Mediano Plazo**:
   - Implementar caché para endpoints frecuentes
   - Agregar más pruebas de integración
   - Configurar pipeline CI/CD completo

---

**Nota**: Estos son resultados de ejemplo. Los resultados reales pueden variar según el entorno y la configuración.

