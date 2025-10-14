using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionLlaves.Data;
using GestionLlaves.Models.ViewModels;
using System;

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

            // 🔹 Obtener el día actual
            DayOfWeek diaSemana = DateTime.Now.DayOfWeek;

            // 🔹 Traducir el día actual a la propiedad real del modelo
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

            // 🔹 Si no hay clase activa
            if (horarioActual == null)
            {
                ViewBag.Mensaje = "No tienes clases activas en este momento.";
                return View(new EstadoViewModel());
            }

            // 🔹 Calcular tiempo restante real
            var horaInicio = DateTime.Today.Add(horarioActual.HoraInicio);
            var horaFin = DateTime.Today.Add(horarioActual.HoraFin);

            var tiempoTotal = horaFin - horaInicio;
            var tiempoTranscurrido = DateTime.Now - horaInicio;
            var tiempoRestante = horaFin - DateTime.Now;

            // 🔹 Crear modelo para la vista
            var model = new EstadoViewModel
            {
                AulaNombre = horarioActual.Aula?.Codigo ?? "Sin aula",
                MateriaNombre = horarioActual.Materia?.Nombre ?? "Sin materia",
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                PorcentajeTranscurrido = Math.Min(100, (tiempoTranscurrido.TotalMinutes / tiempoTotal.TotalMinutes) * 100),
                TiempoRestante = tiempoRestante,
                TotalPrestamos = _context.Prestamo.Count(),
                DevolucionesATiempo = _context.Prestamo.Count(p => p.EstadoPrestamo == "DEVUELTO"),
                PromedioUso = TimeSpan.FromMinutes(105),
                PrestamosSinRetraso = 23,
                RetrasoPromedio = TimeSpan.FromMinutes(5),
                RachaDias = 12
            };

            return View(model);
        }
    }
}
