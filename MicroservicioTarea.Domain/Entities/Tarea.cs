using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicioTarea.Domain.Entities
{
    public class Tarea
    {
        public int IdTarea { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Prioridad { get; set; }
        public int Estado { get; set; } = 1;
        public DateTime FechaRegistro { get; set; }
        public DateTime UltimaModificacion { get; set; }
        public int? IdProyecto { get; set; }
        public int? IdUsuarioAsignado { get; set; }
        public string Status { get; set; } = "SinIniciar";
    }
}

