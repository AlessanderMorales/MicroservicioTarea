using MicroservicioTarea.Application.Services;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Domain.Validators;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicioTarea.Controllers
{
    public class AsignarUsuariosRequest
    {
        public int IdTarea { get; set; }
        public List<int> UsuariosIds { get; set; } = new List<int>();
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TareaController : ControllerBase
    {
        private readonly TareaService _service;
        private readonly TareaUsuarioService _usuarioService;
        private readonly ILogger<TareaController> _logger;

        public TareaController(TareaService service, TareaUsuarioService usuarioService, ILogger<TareaController> logger)
        {
            _service = service;
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });

            var tarea = _service.GetById(id);
            if (tarea == null)
                return NotFound(new { error = true, message = "Tarea no encontrada." });

            return Ok(tarea);
        }

        [HttpGet("usuario/{idUsuario}")]
        public IActionResult GetByUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                return BadRequest(new { error = true, message = "El ID del usuario debe ser mayor a 0." });

            return Ok(_service.GetByUsuarioAsignado(idUsuario));
        }

        [HttpGet("{id}/usuarios")]
        public IActionResult GetUsuarios(int id)
        {
            if (id <= 0)
                return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });

            return Ok(_usuarioService.GetByTareaId(id));
        }

        [HttpPost]
        public IActionResult Create(Tarea t)
        {
            try
            {
                if (t == null)
                    return BadRequest(new { error = true, message = "Los datos de la tarea son requeridos." });

                t.Titulo = InputValidator.ValidateAndSanitize(t.Titulo, "Título");
                if (!string.IsNullOrWhiteSpace(t.Descripcion))
                    t.Descripcion = InputValidator.SanitizeText(t.Descripcion);

                t.Prioridad = InputValidator.ValidatePriority(t.Prioridad);
                t.Status = InputValidator.ValidateStatus(t.Status);

                if (t.IdProyecto <= 0)
                    return BadRequest(new { error = true, message = "Debe especificar un proyecto válido." });

                t.Estado = 1;
                t.FechaRegistro = DateTime.Now;
                t.UltimaModificacion = DateTime.Now;

                _service.Add(t);
                _logger.LogInformation($"Tarea creada: {t.Titulo}");

                return Ok(new { error = false, message = "Tarea creada.", data = t });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validación fallida al crear tarea: {ex.Message}");
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Dictionary<string, object> body)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });

                if (body == null || body.Count == 0)
                    return BadRequest(new { error = true, message = "Debe proporcionar datos para actualizar." });

                var tarea = _service.GetById(id);
                if (tarea == null)
                    return NotFound(new { error = true, message = "Tarea no encontrada." });

                if (body.ContainsKey("titulo"))
                    tarea.Titulo = InputValidator.ValidateAndSanitize(body["titulo"]?.ToString(), "Título");

                if (body.ContainsKey("descripcion"))
                    tarea.Descripcion = InputValidator.SanitizeText(body["descripcion"]?.ToString());

                if (body.ContainsKey("prioridad"))
                    tarea.Prioridad = InputValidator.ValidatePriority(body["prioridad"]?.ToString());

                if (body.ContainsKey("status"))
                    tarea.Status = InputValidator.ValidateStatus(body["status"]?.ToString());

                if (body.ContainsKey("estado") && int.TryParse(body["estado"].ToString(), out int e))
                    tarea.Estado = e;

                if (body.ContainsKey("idProyecto") && int.TryParse(body["idProyecto"].ToString(), out int p))
                {
                    if (p <= 0)
                        return BadRequest(new { error = true, message = "El ID del proyecto debe ser mayor a 0." });
                    tarea.IdProyecto = p;
                }

                if (body.ContainsKey("idUsuarioAsignado") && int.TryParse(body["idUsuarioAsignado"].ToString(), out int u))
                    tarea.IdUsuarioAsignado = u;

                tarea.UltimaModificacion = DateTime.Now;

                _service.Update(tarea);
                _logger.LogInformation($"Tarea actualizada: {id}");

                return Ok(new { error = false, message = "Tarea actualizada." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validación fallida al actualizar tarea: {ex.Message}");
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });

            var tarea = _service.GetById(id);
            if (tarea == null)
                return NotFound(new { error = true, message = "Tarea no encontrada." });

            tarea.Estado = 0;
            tarea.UltimaModificacion = DateTime.Now;

            _service.Update(tarea);
            _logger.LogInformation($"Tarea eliminada: {id}");

            return Ok(new { error = false, message = "Tarea marcada como eliminada." });
        }

        [HttpPost("asignar-usuarios")]
        public IActionResult AsignarUsuarios([FromBody] AsignarUsuariosRequest req)
        {
            try
            {
                if (req == null || req.IdTarea <= 0)
                    return BadRequest(new { error = true, message = "Debe especificar una tarea válida." });

                if (req.UsuariosIds == null || req.UsuariosIds.Count == 0)
                    return BadRequest(new { error = true, message = "Debe especificar al menos un usuario." });

                if (req.UsuariosIds.Any(id => id <= 0))
                    return BadRequest(new { error = true, message = "Todos los IDs de usuario deben ser mayores a 0." });

                _usuarioService.AssignUsers(req.IdTarea, req.UsuariosIds);
                _logger.LogInformation($"Usuarios asignados a tarea {req.IdTarea}");

                return Ok(new { error = false, message = "Usuarios asignados." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpGet("proyecto/{idProyecto:int}")]
        public IActionResult GetByProyecto(int idProyecto)
        {
            if (idProyecto <= 0)
                return BadRequest(new { error = true, message = "El ID del proyecto debe ser mayor a 0." });

            return Ok(_service.GetByProyecto(idProyecto));
        }

        public class CambiarEstadoRequest
        {
            public string NuevoEstado { get; set; } = "";
        }

        [HttpPut("{id}/estado")]
        public IActionResult CambiarEstado(int id, [FromBody] CambiarEstadoRequest req)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { error = true, message = "El ID debe ser mayor a 0." });

                if (req == null || string.IsNullOrWhiteSpace(req.NuevoEstado))
                    return BadRequest(new { error = true, message = "Debe especificar el nuevo estado." });

                var tarea = _service.GetById(id);
                if (tarea == null)
                    return NotFound(new { error = true, message = "Tarea no encontrada" });

                tarea.Status = InputValidator.ValidateStatus(req.NuevoEstado);
                tarea.UltimaModificacion = DateTime.Now;

                _service.Update(tarea);
                _logger.LogInformation($"Estado de tarea {id} cambiado a {tarea.Status}");

                return Ok(new { error = false, message = "Estado actualizado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpPost("{id}/usuarios")]
        public IActionResult AsignarUsuariosRest(int id, [FromBody] List<int> usuariosIds)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { error = true, message = "El ID de la tarea debe ser mayor a 0." });

                if (usuariosIds == null || usuariosIds.Count == 0)
                    return BadRequest(new { error = true, message = "Debe enviar al menos un usuario." });

                if (usuariosIds.Any(uid => uid <= 0))
                    return BadRequest(new { error = true, message = "Todos los IDs de usuario deben ser mayores a 0." });

                _usuarioService.AssignUsers(id, usuariosIds);
                _logger.LogInformation($"Usuarios asignados a tarea {id}");

                return Ok(new { error = false, message = "Usuarios asignados correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }
    }
}
