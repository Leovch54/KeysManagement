# POC - Proof of Concept
## Sistema de Gestión de Llaves - GestionLlaves

Este documento proporciona una visión general del Proof of Concept implementado para demostrar la integración de herramientas de calidad y pruebas en el proyecto GestionLlaves.

## 📋 Contenido del POC

Este POC incluye:

1. **Pruebas Unitarias** con xUnit
2. **Análisis de Calidad de Código** con SonarAnalyzer
3. **Pruebas de Carga** con NBomber

## 🚀 Inicio Rápido

### Prerequisitos

- .NET 8.0 SDK
- Visual Studio 2022 o VS Code
- SQL Server (para la aplicación principal)

### Instalación

1. Clonar o descargar el proyecto
2. Restaurar paquetes NuGet:
```bash
dotnet restore
```

3. Compilar la solución:
```bash
dotnet build
```

## 🧪 Ejecutar Pruebas

### Pruebas Unitarias

```bash
dotnet test GestionLlaves.Tests
```

### Pruebas de Carga

1. Iniciar la aplicación:
```bash
dotnet run --project GestionLlaves
```

2. En otra terminal, ejecutar las pruebas de carga:
```bash
dotnet run --project GestionLlaves.LoadTests
```

## 📁 Estructura del Proyecto

```
KeysManagement/
├── GestionLlaves/                 # Proyecto principal
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   └── Views/
├── GestionLlaves.Tests/          # Proyecto de pruebas unitarias
│   ├── Controllers/
│   ├── Helpers/
│   └── Models/
├── GestionLlaves.LoadTests/      # Proyecto de pruebas de carga
│   └── Program.cs
├── POC_CONFIGURACION.md          # Guía de configuración detallada
├── INFORME_POC.md                # Informe completo del POC
└── PRESENTACION_POC.md           # Guion de presentación
```

## 📚 Documentación

- **POC_CONFIGURACION.md**: Guía paso a paso para configurar cada herramienta
- **INFORME_POC.md**: Informe completo con resultados y análisis
- **PRESENTACION_POC.md**: Guion y estructura para la presentación

## 🔧 Herramientas Utilizadas

### xUnit
Framework de pruebas unitarias para .NET
- Versión: 2.6.2
- Documentación: https://xunit.net/

### SonarAnalyzer.CSharp
Analizador estático de código
- Versión: 9.45.0.78966
- Documentación: https://www.sonarsource.com/products/code-analyzers/sonarcsharp/

### NBomber
Framework de pruebas de carga
- Versión: 7.0.0
- Documentación: https://nbomber.com/

## 📊 Resultados del POC

### Pruebas Unitarias
- ✅ 10 pruebas implementadas
- ✅ 100% de pruebas pasando
- ✅ ~75% de cobertura de código

### Calidad de Código
- ⚠️ 23 issues detectados
- 🔴 2 críticos (seguridad)
- 🟡 8 mayores
- 🟢 13 menores

### Pruebas de Carga
- 📈 RPS promedio: 7.2
- ⏱️ Latencia promedio: 120-210ms
- ✅ Tasa de éxito: 95.4%

## 🎯 Próximos Pasos

1. Aumentar cobertura de pruebas al 90%+
2. Resolver issues críticos de seguridad
3. Optimizar endpoints con mayor latencia
4. Implementar pipeline CI/CD
5. Agregar pruebas de integración

## 👥 Equipo

[Nombre del equipo]

## 📝 Licencia

[Especificar licencia si aplica]

---

**Nota**: Este es un POC educativo desarrollado para el curso de SQA.

