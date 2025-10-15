using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionLlaves.Data;
using GestionLlaves.Models.ViewModels;
using System;
using System.Linq;

namespace GestionLlaves.Controllers
{
    public class EstadoController : Controller
    {
        private readonly AppDbContext _context;

        public EstadoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult MiEstado()
        {
            var docente = _context.Usuario.FirstOrDefault(u => u.Email == User.Identity.Name);
            int docenteId = docente?.Id ?? 0;

            if (docenteId == 0)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View(new EstadoViewModel());
            }

            var ahora = DateTime.Now;

            // 🔹 1️⃣ Buscar préstamo activo primero (prioridad)
            var prestamoActivo = _context.Prestamo
                .Include(p => p.Aula)
                    .ThenInclude(a => a.Edificio)
                .Include(p => p.HorarioAcademico)
                    .ThenInclude(h => h.Materia)
                .Where(p => p.PersonaId == docenteId && p.EstadoPrestamo == "ACTIVO" && p.Estado)
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefault();

            if (prestamoActivo != null)
            {
                var horaInicio = prestamoActivo.FechaInicio;
                var horaFin = prestamoActivo.FechaFinProgramada;
                var tiempoTotal = horaFin - horaInicio;
                var tiempoTranscurrido = ahora - horaInicio;

                var modelPrestamo = new EstadoViewModel
                {
                    AulaNombre = prestamoActivo.Aula?.Codigo ?? "Sin aula",
                    MateriaNombre = prestamoActivo.HorarioAcademico?.Materia?.Nombre
                        ?? prestamoActivo.Reserva?.Proposito
                        ?? "Préstamo de Llave",
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                    PorcentajeTranscurrido = Math.Min(100, (tiempoTranscurrido.TotalMinutes / tiempoTotal.TotalMinutes) * 100),
                    TiempoRestante = horaFin - ahora,
                    TotalPrestamos = _context.Prestamo.Count(),
                    DevolucionesATiempo = _context.Prestamo.Count(p => p.EstadoPrestamo == "DEVUELTO"),
                    PromedioUso = TimeSpan.FromMinutes(105),
                    PrestamosSinRetraso = 23,
                    RetrasoPromedio = TimeSpan.FromMinutes(5),
                    RachaDias = 12
                };

                return View(modelPrestamo);
            }

            // 🔹 2️⃣ Si no hay préstamo activo, usar horario académico actual
            DayOfWeek diaSemana = DateTime.Now.DayOfWeek;
            string diaColumna = diaSemana switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miercoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sabado",
                _ => "Lunes"
            };

            TimeSpan horaActual = DateTime.Now.TimeOfDay;

            var horarioActual = _context.HorariosAcademico
                .Include(h => h.Aula)
                .Include(h => h.Materia)
                .AsEnumerable()
                .FirstOrDefault(h =>
                    h.DocenteId == docenteId &&
                    (h.GetType().GetProperty(diaColumna,
                        System.Reflection.BindingFlags.IgnoreCase |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance)
                        ?.GetValue(h) as bool? == true) &&
                    horaActual >= h.HoraInicio &&
                    horaActual <= h.HoraFin
                );

            if (horarioActual == null)
            {
                ViewBag.Mensaje = "No tienes clases activas ni préstamos activos en este momento.";
                return View(new EstadoViewModel());
            }

            var inicioClase = DateTime.Today.Add(horarioActual.HoraInicio);
            var finClase = DateTime.Today.Add(horarioActual.HoraFin);
            var total = finClase - inicioClase;
            var transcurrido = ahora - inicioClase;

            var modelHorario = new EstadoViewModel
            {
                AulaNombre = horarioActual.Aula?.Codigo ?? "Sin aula",
                MateriaNombre = horarioActual.Materia?.Nombre ?? "Sin materia",
                HoraInicio = inicioClase,
                HoraFin = finClase,
                PorcentajeTranscurrido = Math.Min(100, (transcurrido.TotalMinutes / total.TotalMinutes) * 100),
                TiempoRestante = finClase - ahora,
                TotalPrestamos = _context.Prestamo.Count(),
                DevolucionesATiempo = _context.Prestamo.Count(p => p.EstadoPrestamo == "DEVUELTO"),
                PromedioUso = TimeSpan.FromMinutes(105),
                PrestamosSinRetraso = 23,
                RetrasoPromedio = TimeSpan.FromMinutes(5),
                RachaDias = 12
            };

            return View(modelHorario);
        }
    }
}
