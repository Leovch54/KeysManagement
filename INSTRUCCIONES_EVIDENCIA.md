# INSTRUCCIONES PARA GENERAR EVIDENCIA DEL POC
## Guía Paso a Paso para Obtener Resultados de Cada Herramienta

Este documento proporciona instrucciones detalladas para ejecutar cada herramienta del POC y capturar los resultados como evidencia.

---

## 📋 PREREQUISITOS

Antes de comenzar, asegúrate de tener:

- ✅ .NET 8.0 SDK instalado
- ✅ Visual Studio 2022 o VS Code
- ✅ Proyecto compilado sin errores
- ✅ Base de datos configurada (para pruebas de carga)
- ✅ Aplicación ejecutándose (para pruebas de carga)

**Verificar instalación:**
```bash
dotnet --version
# Debe mostrar: 8.0.x o superior
```

---

## 1. PRUEBAS UNITARIAS CON xUnit

### 1.1 Ejecutar Pruebas Unitarias

**Paso 1: Abrir terminal en la raíz del proyecto**

```bash
cd C:\Users\jg012\Downloads\KeysManagement
```

**Paso 2: Restaurar paquetes (si es necesario)**

```bash
dotnet restore
```

**Paso 3: Compilar el proyecto de pruebas**

```bash
dotnet build GestionLlaves.Tests
```

**Paso 4: Ejecutar todas las pruebas**

```bash
dotnet test GestionLlaves.Tests --verbosity normal
```

### 1.2 Capturar Resultados

**Opción A: Salida en Consola (Básica)**

Ejecuta el comando y copia la salida completa:

```bash
dotnet test GestionLlaves.Tests --verbosity normal > resultados_pruebas_unitarias.txt
```

Luego abre `resultados_pruebas_unitarias.txt` y copia el contenido.

**Opción B: Salida Detallada con Logger**

```bash
dotnet test GestionLlaves.Tests --logger "console;verbosity=detailed" > resultados_detallados.txt
```

**Opción C: Formato TRX (Recomendado para Evidencia)**

```bash
dotnet test GestionLlaves.Tests --logger "trx;LogFileName=resultados_pruebas.trx"
```

El archivo `resultados_pruebas.trx` se genera en: `GestionLlaves.Tests\TestResults\`

### 1.3 Generar Reporte de Cobertura

**Paso 1: Instalar coverlet (si no está instalado)**

```bash
dotnet tool install -g coverlet.console
```

**Paso 2: Ejecutar pruebas con cobertura**

```bash
dotnet test GestionLlaves.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./cobertura/
```

**Paso 3: Generar reporte HTML (opcional)**

```bash
dotnet tool install -g reportgenerator
reportgenerator -reports:"./cobertura/coverage.opencover.xml" -targetdir:"./cobertura/report" -reporttypes:Html
```

El reporte HTML estará en: `./cobertura/report/index.html`

### 1.4 Evidencia a Capturar

**Archivos a guardar:**
1. ✅ `resultados_pruebas.trx` - Resultados en formato XML
2. ✅ `cobertura/coverage.opencover.xml` - Datos de cobertura
3. ✅ Captura de pantalla de la consola con resultados
4. ✅ Reporte HTML de cobertura (opcional pero recomendado)

**Información clave a documentar:**
- Total de pruebas ejecutadas
- Pruebas pasadas vs fallidas
- Tiempo de ejecución
- Cobertura por módulo
- Lista de pruebas con estado

**Ejemplo de salida esperada:**
```
Test Run Successful.
Total tests: 20
     Passed: 20
     Failed: 0
 Total time: 2.345s
```

---

## 2. CALIDAD DE CÓDIGO CON SONARANALYZER

### 2.1 Verificar Instalación

**Paso 1: Verificar que SonarAnalyzer está en el proyecto**

Abre `GestionLlaves.csproj` y verifica que contiene:

```xml
<PackageReference Include="SonarAnalyzer.CSharp" Version="9.45.0.78966">
```

**Paso 2: Compilar el proyecto**

```bash
dotnet build GestionLlaves.csproj
```

SonarAnalyzer se ejecuta automáticamente durante la compilación.

### 2.2 Capturar Resultados en Visual Studio

**Método 1: Error List (Recomendado)**

1. Abre el proyecto en Visual Studio
2. Compila la solución (Build > Build Solution o Ctrl+Shift+B)
3. Abre la ventana "Error List" (View > Error List o Ctrl+\, E)
4. Filtra por "Warnings" (Advertencias)
5. Captura pantalla de la lista completa

**Método 2: Output Window**

1. Abre Output Window (View > Output o Ctrl+Alt+O)
2. Selecciona "Build" en el dropdown
3. Busca líneas que contengan "warning" o "SonarAnalyzer"
4. Copia el contenido relevante

### 2.3 Capturar Resultados desde Línea de Comandos

**Paso 1: Compilar con salida detallada**

```bash
dotnet build GestionLlaves.csproj /v:detailed > build_output.txt
```

**Paso 2: Filtrar advertencias de SonarAnalyzer**

```bash
# En PowerShell:
Select-String -Path build_output.txt -Pattern "warning|SonarAnalyzer" > sonar_warnings.txt

# O manualmente abre build_output.txt y busca "warning"
```

### 2.4 Generar Reporte con SonarQube (Opcional - Avanzado)

Si tienes acceso a SonarQube Server:

**Paso 1: Instalar SonarScanner**

```bash
dotnet tool install -g dotnet-sonarscanner
```

**Paso 2: Iniciar análisis**

```bash
dotnet sonarscanner begin /k:"GestionLlaves" /d:sonar.host.url="http://localhost:9000"
dotnet build
dotnet sonarscanner end
```

**Paso 3: Acceder al reporte**

Abre el navegador en: `http://localhost:9000` y busca el proyecto.

### 2.5 Evidencia a Capturar

**Archivos a guardar:**
1. ✅ Captura de pantalla de Error List con todas las advertencias
2. ✅ `build_output.txt` con warnings filtrados
3. ✅ Lista manual de issues críticos, mayores y menores
4. ✅ Captura de métricas (si usas SonarQube)

**Información clave a documentar:**
- Total de issues detectados
- Issues críticos (con ubicación exacta)
- Issues mayores
- Issues menores
- Métricas: Code Smells, Bugs, Vulnerabilidades
- Ratings: Mantenibilidad, Confiabilidad, Seguridad

**Ejemplo de formato para documentar:**

```
ISSUES DE SONARANALYZER
=======================
Total: 23
  Críticos: 2
  Mayores: 8
  Menores: 13

ISSUES CRÍTICOS:
1. Hard-coded Credentials
   Archivo: AccountController.cs
   Línea: 234
   Regla: S2068

2. Possible Null Reference
   Archivo: AccountController.cs
   Línea: 75
   Regla: S2259
```

---

## 3. PRUEBAS DE CARGA Y STRESS CON NBOMBER

### 3.1 Preparación

**Paso 1: Verificar que la aplicación está ejecutándose**

```bash
# Terminal 1: Iniciar la aplicación
dotnet run --project GestionLlaves
```

Verifica que la aplicación esté en: `https://localhost:7081`

**Paso 2: Verificar configuración de NBomber**

Abre `GestionLlaves.LoadTests/Program.cs` y verifica que las URLs sean correctas.

### 3.2 Ejecutar Pruebas de Carga

**Paso 1: Compilar el proyecto de pruebas de carga**

```bash
dotnet build GestionLlaves.LoadTests
```

**Paso 2: Ejecutar las pruebas**

```bash
dotnet run --project GestionLlaves.LoadTests
```

**Paso 3: Capturar la salida completa**

```bash
dotnet run --project GestionLlaves.LoadTests > resultados_carga.txt 2>&1
```

### 3.3 Ejecutar Escenarios Individuales (Opcional)

Si quieres probar un escenario específico, puedes modificar temporalmente `Program.cs` para comentar los demás escenarios.

### 3.4 Capturar Resultados Detallados

**Método 1: Salida en Consola**

La salida de NBomber muestra:
- Métricas por escenario
- RPS (Requests Per Second)
- Latencia (min, mean, max, p50, p95, p99)
- Tasa de éxito/error
- Data transfer

**Método 2: Reportes HTML (si NBomber los genera)**

Algunas versiones de NBomber generan reportes HTML automáticamente. Revisa la carpeta de salida.

**Método 3: Captura de Pantalla**

Ejecuta las pruebas y captura la pantalla completa de la consola con todos los resultados.

### 3.5 Evidencia a Capturar

**Archivos a guardar:**
1. ✅ `resultados_carga.txt` - Salida completa de la consola
2. ✅ Captura de pantalla de la consola con métricas
3. ✅ Tabla resumen de todos los escenarios

**Información clave a documentar por escenario:**

```
ESCENARIO: carga_login_autenticacion
------------------------------------
ok count: [número]
fail count: [número]
RPS: [número]
min: [tiempo]ms
mean: [tiempo]ms
max: [tiempo]ms
p50: [tiempo]ms
p95: [tiempo]ms
p99: [tiempo]ms
data transfer: [tamaño]
status codes:
  200: [número]
  500: [número]
```

**Ejemplo de salida esperada:**

```
scenario: carga_login_autenticacion
  ok count: 1140
  fail count: 60
  RPS: 19.0
  min: 45ms
  mean: 420ms
  max: 1200ms
  p50: 350ms
  p95: 750ms
  p99: 1100ms
  data transfer: 2.4 MB
```

---

## 4. ORGANIZACIÓN DE EVIDENCIAS

### 4.1 Estructura de Carpetas Recomendada

Crea la siguiente estructura para organizar todas las evidencias:

```
Evidencias_POC/
├── 1_Pruebas_Unitarias/
│   ├── resultados_pruebas.trx
│   ├── resultados_consola.txt
│   ├── cobertura/
│   │   ├── coverage.opencover.xml
│   │   └── report/
│   │       └── index.html
│   └── capturas/
│       └── pantalla_resultados.png
│
├── 2_Calidad_Codigo/
│   ├── build_output.txt
│   ├── sonar_warnings.txt
│   ├── lista_issues.md
│   └── capturas/
│       ├── error_list.png
│       └── metricas.png
│
└── 3_Pruebas_Carga/
    ├── resultados_carga.txt
    ├── resumen_escenarios.md
    └── capturas/
        ├── escenario_login.png
        ├── escenario_solicitar.png
        ├── escenario_stress.png
        └── resumen_general.png
```

### 4.2 Crear Resumen Ejecutivo

Crea un archivo `RESUMEN_EVIDENCIAS.md` con:

```markdown
# RESUMEN DE EVIDENCIAS - POC GESTIÓN DE LLAVES

## Fecha de Ejecución: [FECHA]

## 1. PRUEBAS UNITARIAS
- Total de pruebas: [NÚMERO]
- Pasadas: [NÚMERO]
- Fallidas: [NÚMERO]
- Cobertura: [PORCENTAJE]%
- Archivo de evidencia: [RUTA]

## 2. CALIDAD DE CÓDIGO
- Total de issues: [NÚMERO]
- Críticos: [NÚMERO]
- Mayores: [NÚMERO]
- Menores: [NÚMERO]
- Archivo de evidencia: [RUTA]

## 3. PRUEBAS DE CARGA
- Total de escenarios: 6
- Total de solicitudes: [NÚMERO]
- RPS promedio: [NÚMERO]
- Tasa de éxito: [PORCENTAJE]%
- Archivo de evidencia: [RUTA]
```

---

## 5. COMANDOS RÁPIDOS - RESUMEN

### Pruebas Unitarias
```bash
# Ejecutar pruebas
dotnet test GestionLlaves.Tests --verbosity normal

# Con cobertura
dotnet test GestionLlaves.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Guardar resultados
dotnet test GestionLlaves.Tests --logger "trx;LogFileName=resultados.trx"
```

### Calidad de Código
```bash
# Compilar y ver warnings
dotnet build GestionLlaves.csproj /v:detailed > build_output.txt

# Filtrar warnings (PowerShell)
Select-String -Path build_output.txt -Pattern "warning" > sonar_warnings.txt
```

### Pruebas de Carga
```bash
# Terminal 1: Iniciar aplicación
dotnet run --project GestionLlaves

# Terminal 2: Ejecutar pruebas de carga
dotnet run --project GestionLlaves.LoadTests > resultados_carga.txt 2>&1
```

---

## 6. TROUBLESHOOTING

### Problema: Las pruebas no se ejecutan
**Solución:**
```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

### Problema: SonarAnalyzer no muestra warnings
**Solución:**
1. Verifica que el paquete está instalado en `.csproj`
2. Compila en modo Release: `dotnet build -c Release`
3. Revisa Error List en Visual Studio

### Problema: NBomber no puede conectar
**Solución:**
1. Verifica que la aplicación está ejecutándose
2. Verifica la URL en `Program.cs` (debe ser `https://localhost:7081`)
3. Verifica el puerto en `Properties/launchSettings.json`
4. Acepta el certificado SSL si es necesario

### Problema: No se genera reporte de cobertura
**Solución:**
```bash
# Instalar coverlet manualmente
dotnet add package coverlet.collector
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 7. CHECKLIST DE EVIDENCIAS

Antes de considerar completa la evidencia, verifica:

### Pruebas Unitarias
- [ ] Archivo TRX con resultados
- [ ] Captura de pantalla de consola
- [ ] Reporte de cobertura (XML o HTML)
- [ ] Lista de todas las pruebas con estado

### Calidad de Código
- [ ] Captura de Error List con warnings
- [ ] Lista de issues críticos con ubicación
- [ ] Métricas consolidadas (total, críticos, mayores, menores)
- [ ] Ratings (Mantenibilidad, Confiabilidad, Seguridad)

### Pruebas de Carga
- [ ] Salida completa de consola
- [ ] Métricas de cada escenario (RPS, latencia, éxito)
- [ ] Captura de pantalla de resultados
- [ ] Tabla resumen de todos los escenarios

---

## 8. EJEMPLO DE EJECUCIÓN COMPLETA

### Sesión Completa de Evidencia (Tiempo estimado: 30 minutos)

```bash
# 1. Preparación (5 min)
cd C:\Users\jg012\Downloads\KeysManagement
dotnet restore
dotnet build

# 2. Pruebas Unitarias (5 min)
dotnet test GestionLlaves.Tests --logger "trx;LogFileName=resultados.trx" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
# Capturar pantalla de resultados

# 3. Calidad de Código (5 min)
dotnet build GestionLlaves.csproj /v:detailed > build_output.txt
# Abrir Visual Studio y capturar Error List

# 4. Pruebas de Carga (15 min)
# Terminal 1:
dotnet run --project GestionLlaves

# Terminal 2 (esperar a que la app inicie):
dotnet run --project GestionLlaves.LoadTests > resultados_carga.txt 2>&1
# Capturar pantalla de resultados
```

---

## 9. NOTAS FINALES

- **Tiempo total estimado:** 30-45 minutos para obtener toda la evidencia
- **Espacio requerido:** ~50 MB para todas las evidencias
- **Formato recomendado:** Guardar todo en carpeta `Evidencias_POC/` con fecha
- **Backup:** Hacer copia de seguridad de las evidencias antes de entregar

---

**Última actualización:** [Fecha]
**Versión:** 1.0

