# GUION DE PRESENTACIÓN - POC GESTIÓN DE LLAVES
## Duración: 10 minutos

---

## DIAPOSITIVA 1: PORTADA
**Tiempo: 30 segundos**

**Contenido:**
- Título: "Proof of Concept: Herramientas de Calidad y Pruebas"
- Subtítulo: "Sistema de Gestión de Llaves - GestionLlaves"
- Nombre del equipo
- Fecha

**Narración:**
"Buenos días/tardes. Hoy presentamos el Proof of Concept que demuestra la integración de herramientas de calidad y pruebas en nuestro sistema de gestión de llaves."

---

## DIAPOSITIVA 2: OBJETIVO Y CONTEXTO
**Tiempo: 1 minuto**

**Contenido:**
- Objetivo: Integrar 3 herramientas de calidad
- Contexto del proyecto: Sistema web ASP.NET Core MVC
- Funcionalidades principales del sistema

**Narración:**
"El objetivo de este POC es demostrar la integración exitosa de tres herramientas fundamentales: pruebas unitarias con xUnit, análisis de calidad de código con SonarAnalyzer, y pruebas de carga con NBomber. Nuestro proyecto, GestionLlaves, es un sistema web que permite a los docentes gestionar el préstamo de llaves de aulas."

---

## DIAPOSITIVA 3: HERRAMIENTAS SELECCIONADAS
**Tiempo: 1 minuto**

**Contenido:**
- Lista de las 3 herramientas:
  1. xUnit - Pruebas Unitarias
  2. SonarAnalyzer - Calidad de Código
  3. NBomber - Pruebas de Carga
- Logo o icono de cada herramienta

**Narración:**
"Seleccionamos tres herramientas complementarias: xUnit para pruebas unitarias, SonarAnalyzer para análisis estático de código, y NBomber para pruebas de rendimiento. Cada una cubre un aspecto diferente de la calidad del software."

---

## DIAPOSITIVA 4: PRUEBAS UNITARIAS - xUnit
**Tiempo: 2 minutos**

**Contenido:**
- ¿Qué es xUnit?
- Justificación de la elección
- Captura de pantalla: Resultados de pruebas
- Métricas: 10 pruebas, 100% exitosas, 75% cobertura

**Narración:**
"xUnit es un framework de pruebas unitarias para .NET. Lo elegimos por su simplicidad y excelente integración con Visual Studio. Implementamos 10 pruebas que cubren funcionalidades críticas como login, cambio de contraseña y validaciones. Todas las pruebas pasaron exitosamente, logrando una cobertura del 75%."

**Visualización:**
```
[Mostrar captura de Test Explorer con pruebas pasadas]
```

---

## DIAPOSITIVA 5: CALIDAD DE CÓDIGO - SonarAnalyzer
**Tiempo: 2 minutos**

**Contenido:**
- ¿Qué es SonarAnalyzer?
- Tipos de problemas detectados:
  - Seguridad (2 críticos)
  - Code Smells (13)
  - Bugs (8)
- Captura: Lista de issues en Visual Studio
- Métricas clave

**Narración:**
"SonarAnalyzer analiza el código estáticamente durante la compilación. Detectó 23 problemas, incluyendo 2 críticos de seguridad relacionados con credenciales hard-coded. También identificó code smells y posibles bugs que mejoran la mantenibilidad del código."

**Visualización:**
```
[Mostrar ventana de Error List con advertencias de SonarAnalyzer]
Issues detectados:
- Critical: 2
- Major: 8
- Minor: 13
```

---

## DIAPOSITIVA 6: PRUEBAS DE CARGA - NBomber
**Tiempo: 2 minutos**

**Contenido:**
- ¿Qué es NBomber?
- Escenarios probados:
  1. Login: 10 RPS
  2. Solicitar Llave: 5 RPS
  3. Estado: 8 RPS
- Gráfico de métricas:
  - Latencia promedio: 120-210ms
  - RPS: 7.2 promedio
  - Tasa de éxito: 95.4%

**Narración:**
"NBomber nos permite simular múltiples usuarios concurrentes. Probamos tres escenarios críticos: login, solicitar llave y consultar estado. Los resultados muestran que el sistema maneja bien la carga con una latencia promedio de 120-210ms y una tasa de éxito del 95.4%."

**Visualización:**
```
[Mostrar gráfico de barras con métricas de rendimiento]
Escenario        RPS    Latencia    Éxito
Login           9.5     120ms      95%
Solicitar       4.7     210ms      94%
Estado          7.6     165ms      96%
```

---

## DIAPOSITIVA 7: RESULTADOS INTEGRADOS
**Tiempo: 1 minuto**

**Contenido:**
- Tabla resumen de resultados
- Beneficios de la integración
- Impacto en el proyecto

**Narración:**
"La integración de las tres herramientas nos proporciona una visión completa: las pruebas unitarias garantizan funcionalidad, SonarAnalyzer mejora la calidad y seguridad, y NBomber valida el rendimiento. Esto resulta en un código más confiable, seguro y performante."

**Visualización:**
```
Tabla de resultados:
┌─────────────────┬──────────────┬────────────┐
│ Herramienta     │ Métrica       │ Resultado  │
├─────────────────┼──────────────┼────────────┤
│ xUnit           │ Pruebas       │ 10/10 ✓    │
│ SonarAnalyzer   │ Issues        │ 23 (2 críticos)│
│ NBomber         │ RPS           │ 7.2        │
└─────────────────┴──────────────┴────────────┘
```

---

## DIAPOSITIVA 8: DEMOSTRACIÓN EN VIVO (OPCIONAL)
**Tiempo: 1 minuto**

**Contenido:**
- Ejecutar pruebas unitarias
- Mostrar análisis de SonarAnalyzer
- Ejecutar prueba de carga rápida

**Narración:**
"Ahora les muestro una demostración rápida. [Ejecutar dotnet test] Como pueden ver, todas las pruebas pasan. [Mostrar Error List] Aquí vemos las advertencias de SonarAnalyzer. [Ejecutar NBomber brevemente] Y estos son los resultados de las pruebas de carga."

---

## DIAPOSITIVA 9: CONCLUSIONES Y APRENDIZAJES
**Tiempo: 1 minuto**

**Contenido:**
- Conclusiones principales
- Aprendizajes del equipo
- Mejoras futuras

**Narración:**
"En conclusión, este POC demuestra que la integración de herramientas de calidad es fundamental para el desarrollo de software confiable. Aprendimos la importancia de detectar problemas temprano y validar tanto la funcionalidad como el rendimiento. Para el futuro, planeamos aumentar la cobertura de pruebas y optimizar los puntos identificados por las pruebas de carga."

---

## DIAPOSITIVA 10: PREGUNTAS
**Tiempo: 30 segundos**

**Contenido:**
- "¿Preguntas?"
- Información de contacto del equipo

**Narración:**
"Gracias por su atención. ¿Hay alguna pregunta?"

---

## NOTAS ADICIONALES PARA LA PRESENTACIÓN

### Tips de Presentación:
1. **Preparación**: Asegúrate de tener la aplicación ejecutándose para la demo
2. **Timing**: Respeta los tiempos asignados para cada diapositiva
3. **Visualización**: Usa capturas de pantalla reales cuando sea posible
4. **Interacción**: Invita a preguntas durante la presentación si el tiempo lo permite

### Material de Apoyo:
- Capturas de pantalla de:
  - Test Explorer con pruebas pasadas
  - Error List con advertencias de SonarAnalyzer
  - Consola de NBomber con resultados
  - Código de ejemplo de pruebas

### Preguntas Frecuentes Preparadas:
1. **¿Por qué xUnit y no NUnit?**
   - xUnit es más moderno y tiene mejor integración con .NET Core

2. **¿SonarAnalyzer es gratuito?**
   - Sí, SonarAnalyzer es gratuito. SonarQube Server tiene versiones de pago con más funcionalidades.

3. **¿Las pruebas de carga afectan el sistema en producción?**
   - No, las pruebas se ejecutan contra un entorno de desarrollo o staging.

4. **¿Qué tan difícil fue la integración?**
   - La integración fue relativamente sencilla, cada herramienta tiene buena documentación.

---

## ESTRUCTURA DE DIAPOSITIVAS PARA POWERPOINT

### Diseño Sugerido:
- **Fondo**: Color institucional (si aplica) o azul profesional
- **Fuente**: Calibri o Arial, tamaño mínimo 24pt para texto
- **Colores**: 
  - Títulos: Azul oscuro (#003366)
  - Texto: Negro o gris oscuro
  - Acentos: Verde para éxito, rojo para errores

### Plantilla de Diapositiva:
```
┌─────────────────────────────────────┐
│  [Logo/Imagen]    Título            │
├─────────────────────────────────────┤
│                                     │
│  Contenido principal                │
│  - Punto 1                          │
│  - Punto 2                          │
│  - Punto 3                          │
│                                     │
│  [Gráfico/Imagen]                  │
│                                     │
└─────────────────────────────────────┘
```

---

**Fin del Guion**

