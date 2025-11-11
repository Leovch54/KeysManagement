# Inicio Rápido - POC GestionLlaves

## ⚡ Pasos Rápidos para Ejecutar el POC

### 1. Restaurar Dependencias (30 segundos)

```bash
dotnet restore
```

### 2. Compilar la Solución (1 minuto)

```bash
dotnet build
```

### 3. Ejecutar Pruebas Unitarias (30 segundos)

```bash
dotnet test GestionLlaves.Tests --verbosity normal
```

**Resultado esperado:**
```
Test Run Successful.
Total tests: 10
     Passed: 10
```

### 4. Verificar Análisis de Calidad (automático)

El análisis de SonarAnalyzer se ejecuta automáticamente al compilar. Revisa la ventana "Error List" en Visual Studio o los warnings en la consola.

### 5. Ejecutar Pruebas de Carga (2 minutos)

**Paso 5.1:** Iniciar la aplicación (en una terminal)
```bash
dotnet run --project GestionLlaves
```

**Paso 5.2:** Ejecutar pruebas de carga (en otra terminal)
```bash
dotnet run --project GestionLlaves.LoadTests
```

**Nota:** Asegúrate de que la aplicación esté ejecutándose en `https://localhost:7081`

## 📊 Ver Resultados

### Pruebas Unitarias
- Resultados en consola
- Cobertura: Ver archivo `coverage.cobertura.xml` (si se configuró)

### Calidad de Código
- Visual Studio: Ventana "Error List"
- Consola: Warnings durante compilación

### Pruebas de Carga
- Resultados en consola con métricas detalladas
- Ver `RESULTADOS_EJEMPLO.md` para ejemplos

## 🐛 Solución de Problemas Rápidos

**Error: "No se puede encontrar el proyecto"**
```bash
# Asegúrate de estar en la raíz del proyecto
cd C:\Users\jg012\Downloads\KeysManagement
```

**Error: "No se puede conectar a la base de datos"**
- Las pruebas unitarias usan base de datos en memoria, no requieren SQL Server
- Solo las pruebas de carga requieren la app ejecutándose

**Error: "NBomber no puede conectar"**
- Verifica que la app esté en `https://localhost:7081`
- Revisa `Properties/launchSettings.json` para el puerto correcto

## 📚 Documentación Completa

- **POC_CONFIGURACION.md**: Configuración detallada
- **INFORME_POC.md**: Informe completo
- **PRESENTACION_POC.md**: Guion de presentación
- **RESULTADOS_EJEMPLO.md**: Ejemplos de resultados

## ✅ Checklist de Verificación

- [ ] `dotnet restore` ejecutado sin errores
- [ ] `dotnet build` compila sin errores
- [ ] `dotnet test` ejecuta 10 pruebas exitosamente
- [ ] SonarAnalyzer muestra advertencias en Error List
- [ ] Aplicación inicia correctamente
- [ ] Pruebas de carga se ejecutan y muestran resultados

---

**Tiempo total estimado:** 5-10 minutos

