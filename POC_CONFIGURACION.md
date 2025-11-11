# Guía de Configuración del POC - GestionLlaves

## 1. Pruebas Unitarias con xUnit

### 1.1 Instalación y Configuración

El proyecto de pruebas unitarias ya está creado en `GestionLlaves.Tests`. Para configurarlo:

```bash
# Restaurar paquetes NuGet
dotnet restore

# Compilar el proyecto de pruebas
dotnet build GestionLlaves.Tests

# Ejecutar las pruebas
dotnet test GestionLlaves.Tests
```

### 1.2 Estructura del Proyecto de Pruebas

```
GestionLlaves.Tests/
├── Controllers/
│   └── AccountControllerTests.cs
├── Helpers/
│   └── PasswordHelperTests.cs
├── Models/
│   └── UsuarioTests.cs
└── GestionLlaves.Tests.csproj
```

### 1.3 Ejecución de Pruebas

**Desde Visual Studio:**
1. Abrir el Test Explorer (Test > Test Explorer)
2. Ejecutar todas las pruebas o seleccionar pruebas específicas

**Desde línea de comandos:**
```bash
dotnet test GestionLlaves.Tests --verbosity normal
```

**Con cobertura de código:**
```bash
dotnet test GestionLlaves.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### 1.4 Resultados Esperados

Las pruebas cubren:
- Login con credenciales válidas
- Login con credenciales inválidas
- Validación de campos vacíos
- Cambio de contraseña primera vez
- Validación de modelos

## 2. Calidad de Código con SonarAnalyzer

### 2.1 Configuración

SonarAnalyzer ya está configurado en `GestionLlaves.csproj`. El analizador se ejecuta automáticamente durante la compilación.

### 2.2 Verificación

**Desde Visual Studio:**
1. Compilar el proyecto (Build > Build Solution)
2. Revisar la ventana "Error List" para ver advertencias y sugerencias

**Desde línea de comandos:**
```bash
dotnet build GestionLlaves.csproj
```

### 2.3 Reglas Analizadas

SonarAnalyzer verifica:
- Problemas de seguridad (SQL injection, XSS, etc.)
- Code smells (código duplicado, complejidad ciclomática)
- Bugs potenciales (null reference, division por cero)
- Vulnerabilidades de seguridad
- Mantenibilidad del código

### 2.4 Configuración Avanzada (Opcional)

Crear archivo `Directory.Build.props` en la raíz del proyecto:

```xml
<Project>
  <PropertyGroup>
    <SonarQubeExclude>true</SonarQubeExclude>
  </PropertyGroup>
</Project>
```

Para excluir archivos específicos del análisis.

## 3. Pruebas de Carga con NBomber

### 3.1 Instalación y Configuración

El proyecto de pruebas de carga está en `GestionLlaves.LoadTests`.

### 3.2 Preparación

1. **Iniciar la aplicación:**
```bash
dotnet run --project GestionLlaves
```

2. **Asegurarse de que la aplicación esté ejecutándose en `https://localhost:7081`**

### 3.3 Ejecución de Pruebas de Carga

```bash
# Desde la raíz del proyecto
dotnet run --project GestionLlaves.LoadTests
```

### 3.4 Escenarios Configurados

#### Escenario 1: Carga de Login
- **Endpoint**: POST /Account/Login
- **Tasa**: 10 solicitudes/segundo
- **Duración**: 30 segundos
- **Warm-up**: 5 segundos

#### Escenario 2: Carga de Solicitar Llave
- **Endpoint**: GET /Docente/Solicitar
- **Tasa**: 5 solicitudes/segundo
- **Duración**: 30 segundos
- **Warm-up**: 5 segundos

#### Escenario 3: Carga de Estado
- **Endpoint**: GET /Docente/Estado
- **Tasa**: 8 solicitudes/segundo
- **Duración**: 30 segundos
- **Warm-up**: 5 segundos

### 3.5 Interpretación de Resultados

NBomber mostrará métricas como:
- **RPS (Requests Per Second)**: Solicitudes procesadas por segundo
- **Latency**: Tiempo de respuesta (p50, p95, p99)
- **Data Transfer**: Datos transferidos
- **Errors**: Cantidad y porcentaje de errores

### 3.6 Personalización de Escenarios

Editar `GestionLlaves.LoadTests/Program.cs` para modificar:
- Tasa de inyección
- Duración de la prueba
- Endpoints a probar
- Parámetros de las solicitudes

## 4. Integración en el Flujo del Proyecto

### 4.1 Pipeline CI/CD (Ejemplo)

```yaml
# .github/workflows/ci.yml
name: CI

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
      - name: Load Tests
        run: dotnet run --project GestionLlaves.LoadTests
```

### 4.2 Pre-commit Hooks (Opcional)

Crear script `.git/hooks/pre-commit`:

```bash
#!/bin/sh
dotnet build
dotnet test --no-build
```

## 5. Solución de Problemas

### Problema: Las pruebas no encuentran el contexto de base de datos
**Solución**: Asegurarse de que se está usando `UseInMemoryDatabase` en las pruebas.

### Problema: NBomber no puede conectar
**Solución**: Verificar que la aplicación esté ejecutándose y que la URL sea correcta.

### Problema: SonarAnalyzer muestra demasiadas advertencias
**Solución**: Configurar reglas específicas o excluir archivos no críticos.

## 6. Referencias

- [xUnit Documentation](https://xunit.net/)
- [SonarAnalyzer for .NET](https://www.sonarsource.com/products/code-analyzers/sonarcsharp/)
- [NBomber Documentation](https://nbomber.com/)

