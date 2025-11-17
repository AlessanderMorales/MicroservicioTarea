using MicroservicioTarea.Application.Services;
using MicroservicioTarea.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicioTarea.Controllers
{
    public class AsignarUsuariosRequest
    {
        public int IdTarea { get; set; }
        public List<int> UsuariosIds { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TareaController : ControllerBase
    {
        private readonly TareaService _service;
        private readonly TareaUsuarioService _usuarioService;

        public TareaController(TareaService service, TareaUsuarioService usuarioService)
        {
            _service = service;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id) => Ok(_service.GetById(id));

        // Obtener tareas asignadas a un usuario
        [HttpGet("usuario/{idUsuario}")]
        public IActionResult GetByUsuario(int idUsuario)
        {
            return Ok(_service.GetByUsuarioAsignado(idUsuario));
        }

        [HttpGet("{id}/usuarios")]
        public IActionResult GetUsuarios(int id)
        {
            return Ok(_usuarioService.GetByTareaId(id));
        }

        [HttpPost]
        public IActionResult Create(Tarea t)
        {
            _service.Add(t);
            return Ok("Tarea creada.");
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Dictionary<string, object> body)
        {
            var tarea = _service.GetById(id);
            if (tarea == null)
                return NotFound();

            if (body.ContainsKey("titulo"))
                tarea.Titulo = body["titulo"]?.ToString();

            if (body.ContainsKey("descripcion"))
                tarea.Descripcion = body["descripcion"]?.ToString();

            if (body.ContainsKey("prioridad"))
                tarea.Prioridad = body["prioridad"]?.ToString();

            if (body.ContainsKey("estado") && int.TryParse(body["estado"].ToString(), out int e))
                tarea.Estado = e;

            if (body.ContainsKey("idProyecto") && int.TryParse(body["idProyecto"].ToString(), out int p))
                tarea.IdProyecto = p;

            if (body.ContainsKey("idUsuarioAsignado") && int.TryParse(body["idUsuarioAsignado"].ToString(), out int u))
                tarea.IdUsuarioAsignado = u;

            if (body.ContainsKey("status"))
                tarea.Status = body["status"]?.ToString();

            tarea.UltimaModificacion = DateTime.Now;

            _service.Update(tarea);

            return Ok("Tarea actualizada.");
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var tarea = _service.GetById(id);
            if (tarea == null)
                return NotFound();

            tarea.Estado = 0; // Eliminación lógica
            tarea.UltimaModificacion = DateTime.Now;

            _service.Update(tarea);

            return Ok("Tarea marcada como eliminada.");
        }

        // ✔ Recibe un request simple en lugar de DTOs
        [HttpPost("asignar-usuarios")]
        public IActionResult AsignarUsuarios([FromBody] AsignarUsuariosRequest req)
        {
            _usuarioService.AssignUsers(req.IdTarea, req.UsuariosIds);
            return Ok("Usuarios asignados.");
        }

        // ============================================
        // TAREAS POR PROYECTO
        // ============================================
        [HttpGet("proyecto/{idProyecto:int}")]
        public IActionResult GetByProyecto(int idProyecto)
        {
            return Ok(_service.GetByProyecto(idProyecto));
        }

        public class CambiarEstadoRequest
        {
            public string NuevoEstado { get; set; } = "";
        }

        [HttpPut("{id}/estado")]
        public IActionResult CambiarEstado(int id, [FromBody] CambiarEstadoRequest req)
        {
            var tarea = _service.GetById(id);
            if (tarea == null)
                return NotFound("Tarea no encontrada");

            tarea.Status = req.NuevoEstado;
            tarea.UltimaModificacion = DateTime.Now;

            _service.Update(tarea);

            return Ok("Estado actualizado.");
        }

        [HttpPost("{id}/usuarios")]
        public IActionResult AsignarUsuariosRest(int id, [FromBody] List<int> usuariosIds)
        {
            if (usuariosIds == null || usuariosIds.Count == 0)
                return BadRequest("Debe enviar al menos un usuario.");

            _usuarioService.AssignUsers(id, usuariosIds);

            return Ok("Usuarios asignados correctamente.");
        }
    }
}
