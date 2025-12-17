using System;
using System.Collections.Generic;

namespace MicroservicioTarea.Domain.Events
{
    public class TareaAsignadaEvent
    {
        public int TareaId { get; set; }
        public List<int> EmpleadosIds { get; set; } = new List<int>();
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; } = DateTime.UtcNow;
        public string TareaTitulo { get; set; } = string.Empty;
    }
}
