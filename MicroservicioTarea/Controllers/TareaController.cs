using MicroservicioTarea.Application.Services;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize] // ✅ JWT: Todos los endpoints requieren autenticación
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

                // Validar y actualizar título
                if (body.ContainsKey("titulo") && body["titulo"] != null)
                {
                    var tituloStr = body["titulo"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(tituloStr))
                    {
                        tarea.Titulo = InputValidator.ValidateAndSanitize(tituloStr, "Título");
                    }
                }

                // Validar y actualizar descripción
                if (body.ContainsKey("descripcion"))
                {
                    var descripcionStr = body["descripcion"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(descripcionStr))
                    {
                        tarea.Descripcion = InputValidator.SanitizeText(descripcionStr);
                    }
                    else
                    {
                        tarea.Descripcion = null; // Permitir descripción vacía
                    }
                }

                // Validar y actualizar prioridad
                if (body.ContainsKey("prioridad") && body["prioridad"] != null)
                {
                    var prioridadStr = body["prioridad"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(prioridadStr))
                    {
                        tarea.Prioridad = InputValidator.ValidatePriority(prioridadStr);
                    }
                }

                // Validar y actualizar status
                if (body.ContainsKey("status") && body["status"] != null)
                {
                    var statusStr = body["status"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(statusStr))
                    {
                        tarea.Status = InputValidator.ValidateStatus(statusStr);
                    }
                }

                // Validar y actualizar estado
                if (body.ContainsKey("estado") && body["estado"] != null)
                {
                    var estadoStr = body["estado"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(estadoStr) && int.TryParse(estadoStr, out int e))
                    {
                        tarea.Estado = e;
                    }
                }

                // Validar y actualizar idProyecto
                if (body.ContainsKey("idProyecto") && body["idProyecto"] != null)
                {
                    var proyectoStr = body["idProyecto"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(proyectoStr) && int.TryParse(proyectoStr, out int p))
                    {
                        if (p <= 0)
                            return BadRequest(new { error = true, message = "El ID del proyecto debe ser mayor a 0." });
                        tarea.IdProyecto = p;
                    }
                }

                // Validar y actualizar idUsuarioAsignado
                if (body.ContainsKey("idUsuarioAsignado") && body["idUsuarioAsignado"] != null)
                {
                    var usuarioStr = body["idUsuarioAsignado"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(usuarioStr) && int.TryParse(usuarioStr, out int u))
                    {
                        tarea.IdUsuarioAsignado = u > 0 ? u : (int?)null;
                    }
                }

                tarea.UltimaModificacion = DateTime.Now;

                _service.Update(tarea);
                _logger.LogInformation($"Tarea actualizada: {id} - {tarea.Titulo}");

                return Ok(new { error = false, message = "Tarea actualizada." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validación fallida al actualizar tarea {id}: {ex.Message}");
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al actualizar tarea {id}: {ex.Message}");
                return StatusCode(500, new { error = true, message = "Error interno al actualizar la tarea." });
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

                // ✅ CORREGIDO: Permitir lista vacía para desasignar todos los empleados
                if (usuariosIds == null)
                    usuariosIds = new List<int>();

                // Validar que los IDs sean válidos (solo si hay elementos)
                if (usuariosIds.Any(uid => uid <= 0))
                    return BadRequest(new { error = true, message = "Todos los IDs de usuario deben ser mayores a 0." });

                _usuarioService.AssignUsers(id, usuariosIds);
                
                if (usuariosIds.Count == 0)
                {
                    _logger.LogInformation($"Todos los usuarios desasignados de tarea {id}");
                    return Ok(new { error = false, message = "Todos los empleados han sido desasignados de la tarea." });
                }
                else
                {
                    _logger.LogInformation($"Usuarios asignados a tarea {id}: {usuariosIds.Count} empleados");
                    return Ok(new { error = false, message = "Usuarios asignados correctamente." });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }
    }
}
