using System;

namespace MicroservicioTarea.Domain.Entities
{
    public class TareaUsuario
    {
        public int Id { get; set; }
        public int IdTarea { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;
        public int Estado { get; set; } = 1;
    }
}
