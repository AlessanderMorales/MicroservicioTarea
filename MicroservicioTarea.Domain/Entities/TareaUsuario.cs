using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicioTarea.Domain.Entities
{
    public class TareaUsuario
    {
        public int Id { get; set; }
        public int IdTarea { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public int Estado { get; set; }
    }
}

