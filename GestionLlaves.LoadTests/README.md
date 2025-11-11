# Pruebas de Carga - GestionLlaves

## Configuración

1. Asegúrate de que la aplicación esté ejecutándose en `https://localhost:7081`
2. Ejecuta las pruebas de carga:

```bash
dotnet run --project GestionLlaves.LoadTests
```

## Escenarios de Prueba

### 1. Carga de Login
- **Tasa de inyección**: 10 solicitudes por segundo
- **Duración**: 30 segundos
- **Endpoint**: POST /Account/Login

### 2. Carga de Solicitar Llave
- **Tasa de inyección**: 5 solicitudes por segundo
- **Duración**: 30 segundos
- **Endpoint**: GET /Docente/Solicitar

### 3. Carga de Estado
- **Tasa de inyección**: 8 solicitudes por segundo
- **Duración**: 30 segundos
- **Endpoint**: GET /Docente/Estado

## Resultados

Los resultados se mostrarán en la consola con métricas de:
- Requests por segundo
- Latencia (p50, p95, p99)
- Errores
- Throughput

